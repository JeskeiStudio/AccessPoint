namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class AzureBlobUpload
    {
        #region private fields

        private static readonly ILog Logger = LogProvider.For<AzureBlobUpload>();

        private const int MaxBlockSize = 1024 * 1024 * 4;
        private const int DefaultParallelism = 4;
        private const int DefaultBlockCommit = 100;

        private string _securityToken;
        private string _blobName;
        private Barrier _barrier;

        #endregion
        
        #region constructors

        public AzureBlobUpload(string securityToken, string blobName)
        {
            Guard.NotNull(securityToken, nameof(securityToken));
            Guard.NotNull(blobName, nameof(blobName));

            _securityToken = securityToken;
            _blobName = blobName;
        }

        #endregion

        #region properties

        public int ParallelUploadCount { get; set; } = DefaultParallelism;

        public int BlockCommitLimit { get; set; } = DefaultBlockCommit;
        
        public bool Working { get; set; } = true;

        #endregion

        #region public methods

        public async Task<bool> Upload(
            IngestApiHelper apiHelper,
            IngestDetail ingestDetail,
            Func<long, int, Task<byte[]>> fetchData,
            FileInfo sourceFile,
            long? offset = null,
            long? maxUpload = null)

        {
            Logger.DebugFormat("Starting upload of {0} from offset {1} for {2} bytes.", sourceFile.FullName, offset, maxUpload);
            
            var container = GetContainer(_securityToken);
            var blob = container.GetBlockBlobReference(_blobName);

            // trigger the hash calculation in the background whilst the upload takes place
            var md5hashTask = sourceFile.ComputeMD5ChecksumAsync();

            var blobLength = CalculateUploadLength(offset, sourceFile.Length, maxUpload);

            var state = GetOrCreateState(container.Name);
            state.InitBlob(blob.Name, blobLength);

            var blocksToProcess = state.GetBlocksToProcess(blob.Name);
            var missingBlocks = await GetMissingBlocks(container, blob, blocksToProcess);
            state.UpdateBlobStatus(blob.Name, missingBlocks);

            _barrier = new Barrier(0, barrier => CommitBlocksIfRequired(state, blob).Wait());

            if (ParallelUploadCount == 0)
                ParallelUploadCount = DefaultParallelism;

            // perform the upload
            Logger.Debug($"Initiating upload of missing blocks (parallelism={ParallelUploadCount})");

            var maxThreads = new SemaphoreSlim(ParallelUploadCount);
            var taskList = new List<Task>();

            for (int i = 0; i < missingBlocks.Count; i++)
            {
                    maxThreads.Wait();

                    int blockId = i; // NOTE: need to work from a copy of the counter 'i' (otherwise wrong blocks are passed upload)

                    var task = Task.Factory.StartNew(() =>
                    {
                        _barrier.AddParticipant();
                        UploadBlockAsync(fetchData, state, blob, missingBlocks[blockId]).Wait();
                    }, TaskCreationOptions.LongRunning)
                    .ContinueWith((tsk) =>
                    {
                        maxThreads.Release();
                        _barrier.RemoveParticipant();
                    });

                if (this.Working == false)
                {
                    return false;
                }
                    taskList.Add(task);

                    // 4096 KM * 1024 bytes = 4194304 bytes per block
                    await apiHelper.NotifyIngestComplete(ingestDetail, 4194304, false);
            }

            Logger.Debug("Waiting for final uploads to complete");
            await Task.WhenAll(taskList);
            Logger.Debug("All tasks complete; finished upload of blocks");

            blob.Properties.ContentMD5 = await md5hashTask;

            // commit the block list to finalize the blob
            var blocksToCommit = state.GetAllBlocksToCommit(blob.Name);
            await CommitBlockList(blob, blocksToCommit);

            return true;
        }

        #endregion

        #region private methods

        private UploadState GetOrCreateState(string containerName)
        {
            // get previous stats if available
            var state = StateHelper.LoadState(containerName);

            if (state == null)
            {
                state = new UploadState(containerName);
            }

            return state;
        }

        private async Task<List<BlockMetadata>> GetMissingBlocks(CloudBlobContainer container, CloudBlockBlob blob, List<BlockMetadata> blocksToProcess)
        {
            // determine the blocks 'missing' from the storage blob
            // this will be 'all of them' if we've not attempted to upload before
            // otherwise it will be the ones not yet uploaded

            List<BlockMetadata> missingBlocks = null;

            try
            {
                Logger.DebugFormat("Obtaining list of existing blocks for container {0}", container.Name);

                var blobs = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.UncommittedBlobs, null, null, null, null);

                List<ListBlockItem> blocksUploaded = new List<ListBlockItem>();

                if (blobs.Results.Any(b => b.Uri.ToString().EndsWith(_blobName)))
                {
                    blocksUploaded = (await blob.DownloadBlockListAsync(
                        BlockListingFilter.All,
                        AccessCondition.GenerateEmptyCondition(),
                        new BlobRequestOptions(),
                        new OperationContext()))
                    .ToList();
                }

                missingBlocks = blocksToProcess.Where(blockToProcess => !blocksUploaded.Any(blockUploaded =>
                        blockUploaded.Name == blockToProcess.BlockId &&
                        blockUploaded.Length == blockToProcess.Length))
                    .ToList();

                Logger.DebugFormat("Found {0} blocks missing from container", missingBlocks.Count);
            }
            catch (StorageException)
            {
                missingBlocks = blocksToProcess;
            }

            return missingBlocks;
        }

        private async Task UploadBlockAsync(
            Func<long, int, Task<byte[]>> fetchData,
            UploadState state,
            CloudBlockBlob blob,
            BlockMetadata block)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            Logger.DebugFormat("Loading data for block {0}/{1} (tid:{2})", block.Id, block.BlockId, threadId);
            var blockData = await fetchData(block.Index, block.Length);

            if (blockData == null)
            {
                Working = false;
                return;
            }

            Logger.DebugFormat("Computing MD5 hash of block (tid:{0})", threadId);
            block.BlockHash = HashHelper.MD5FromBytes(blockData);

            var startAt = DateTimeOffset.UtcNow;
            var sw = new Stopwatch();
            sw.Start();

            Logger.DebugFormat("Starting upload of block {0}/{1} (tid:{2})", block.Id, block.BlockId, threadId);

            // TODO: make this a retry with backoff and eventual failure instead of brute forcing the request through
            await RetryHelper.ExecuteWithRetryAndBackoff(async () =>
            {
                await blob.PutBlockAsync(
                    block.BlockId,
                    new MemoryStream(blockData),
                    block.BlockHash,
                    AccessCondition.GenerateEmptyCondition(),
                    new BlobRequestOptions()
                    {
                        StoreBlobContentMD5 = true,
                        UseTransactionalMD5 = true,
                    },
                    new OperationContext());

                block.UploadCompleted = true;

            }, ExceptionHandler, Logger);

            sw.Stop();
            var endAt = DateTimeOffset.UtcNow;

            state.Record(blob.Name, block, startAt, endAt, sw.Elapsed);

            StateHelper.SaveState(state);

            Logger.DebugFormat("Completed upload of block {0}/{1} in {2} ms (tid:{3})", block.Id, block.BlockId, sw.ElapsedMilliseconds, threadId);

            var completedUncommitted = state.GetNumberOfUncommittedBlocks(blob.Name);

            if (BlockCommitLimit == 0)
                BlockCommitLimit = DefaultBlockCommit;

            Logger.Debug($"There are {completedUncommitted} blocks to commit, block limit is {BlockCommitLimit}");

            if (completedUncommitted >= BlockCommitLimit)
            {
                Logger.Debug($"Need to commit blocks..waiting for upload synchronization (tid:{threadId})");
                _barrier.SignalAndWait();
                Logger.Debug($"Finished commit blocks..released signal for other uploads (tid:{threadId})");
            }
        }

        private async Task CommitBlocksIfRequired(UploadState state, CloudBlockBlob blob)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            var completedUncommitted = state.GetNumberOfUncommittedBlocks(blob.Name);

            Logger.Debug($"There are now {completedUncommitted} blocks to commit (tid:{threadId})");

            // we really are ok to do the commit now
            var blocksToCommit = state.GetAllBlocksToCommit(blob.Name);
            await CommitBlockList(blob, blocksToCommit);

            // mark the blocks as committed
            state.MarkBlocksCommitted(blob.Name, blocksToCommit);

            Logger.Debug($"Blocks committed (tid:{threadId})");
        }

        private async static Task CommitBlockList(CloudBlockBlob blob, List<string> blocksToCommit)
        {
            Logger.DebugFormat("Initiating commit of {0} blocks {blocksToCommit}", blocksToCommit.Count, blocksToCommit);

            await RetryHelper.ExecuteWithRetryAndBackoff(async () =>
            {
                await blob.PutBlockListAsync(blocksToCommit);
            }, ExceptionHandler, Logger);

            Logger.Debug("Finished commit of block list");
        }

        private static long CalculateUploadLength(long? currentOffset, long totalLength, long? maxUploadLength)
        {
            var remainingLength = totalLength - (currentOffset.HasValue ? currentOffset.Value : 0);

            if (!maxUploadLength.HasValue)
                return remainingLength;
            else
                return Math.Min(maxUploadLength.Value, remainingLength);
        }

        private static CloudBlobContainer GetContainer(string securityToken)
        {
            var container = new CloudBlobContainer(new Uri(securityToken));
            return container;
        }
        private static void ExceptionHandler(Exception ex)
        {
            Logger.ErrorException("Error occurred attempting to execute an operation with retry and backoff.", ex);
        }

        #endregion
    }
}
