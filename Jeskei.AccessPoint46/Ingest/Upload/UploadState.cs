namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core;
    using Newtonsoft.Json.Linq;

    public class UploadState
    {
        #region constructors

        public UploadState(string containerName)
        {
            Guard.NotNull(containerName, nameof(containerName));
            ContainerName = containerName;
        }

        #endregion

        #region properties

        public string ContainerName { get; set; }

        public List<BlobStat> Blobs { get; set; } = new List<BlobStat>();


        #endregion

        #region public methods

        public void InitBlob(string blobName, long blobLength)
        {
            Guard.NotNullOrEmpty(blobName, nameof(blobName));

            var blobStat = GetBlobStat(blobName);

            if (blobStat == null)
            {
                blobStat = new BlobStat() { BlobName = blobName, BlobLength = blobLength };
                blobStat.Blocks = CalculateBlocksToProcess(blobLength);
                Blobs.Add(blobStat);
            }
        }

        public List<BlockMetadata> GetBlocksToProcess(string blobName)
        {
            var blobStat = GetBlobStat(blobName);

            var blockMetadata = blobStat.Blocks
                .Select(a => a.Metadata)
                .ToList();

            return blockMetadata;
        }

        public void UpdateBlobStatus(string blobName, List<BlockMetadata> missingBlocks)
        {
            var blocksToProcess = GetBlocksToProcess(blobName);

            blocksToProcess.Except(missingBlocks)
               .ToList()
               .ForEach(a =>
               {
                   a.UploadCompleted = true;
                   a.Committed = true;
               });
        }

        public void Record(string blobName, BlockMetadata block, DateTimeOffset startAt, DateTimeOffset endAt, TimeSpan elapsed)
        {
            var blobStat = GetBlobStat(blobName);

            var blockStat = blobStat.Blocks.FirstOrDefault(a => a.Metadata.BlockId == block.BlockId);

            // should never be null??
            var transferStat = new TransferStat()
            {
                StartTime = startAt,
                EndTime = endAt,
                ElapsedMs = (long)elapsed.TotalMilliseconds,
            };

            blockStat.TransferHistory.Add(transferStat);
        }

        public int GetNumberOfUncommittedBlocks(string blobName)
        {
            var blobStat = GetBlobStat(blobName);
            var count = blobStat.Blocks.Count(a => a.Metadata.UploadCompleted == true && a.Metadata.Committed == false);
            return count;
        }

        public List<string> GetAllBlocksToCommit(string blobName)
        {
            var blobStat = GetBlobStat(blobName);

            // can only commit all blocks in sequence up to the first one that has not been uploaded
            // i.e. if other blocks A through F are uploaded and committed, G is still pending and H through K are uploaded and committed 
            //      we can't commit H through K until G is done otherwise the blob will be malformed and look like A B C D E F H I J K (note no G!)
            var indexOfFirstUncomitted = blobStat.Blocks.FindIndex(a => a.Metadata.Committed == false && a.Metadata.UploadCompleted == false);

            if (indexOfFirstUncomitted == 0)
                return null;

            if (indexOfFirstUncomitted == -1)
                indexOfFirstUncomitted = blobStat.Blocks.Count;

            var blocksToCommit = blobStat.Blocks.Take(indexOfFirstUncomitted)
                .Select(b => b.Metadata.BlockId)
                .ToList();

            return blocksToCommit;
        }

        public void MarkBlocksCommitted(string blobName, List<string> blocksToCommit)
        {
            var blobStat = GetBlobStat(blobName);
            blocksToCommit.ForEach(id => blobStat.Blocks.Single(b => b.Metadata.BlockId == id).Metadata.Committed = true);
        }

        public string BuildChecksumJson()
        {
            var json = new JObject();
            var blobArray = new JArray();
            json.Add(new JProperty("containerId", ContainerName));
            json.Add(new JProperty("blobs", blobArray));

            Blobs.ForEach(bl =>
            {
                var blob = new JObject(
                    new JProperty("blobName", bl.BlobName),
                    new JProperty("blobMd5", bl.BlobMD5Checksum),
                    new JProperty("length", bl.BlobLength));

                var blockArray = new JArray();
                bl.Blocks.ForEach(blk =>
                    {
                        blockArray.Add(new JObject(
                            new JProperty("blockNumber", blk.Metadata.Id),
                            new JProperty("blockId", blk.Metadata.BlockId),
                            new JProperty("offset", blk.Metadata.Index),
                            new JProperty("length", blk.Metadata.Length),
                            new JProperty("md5", blk.Metadata.BlockHash)));
                    });

                blob.Add(new JProperty("blocks", blockArray));
                blobArray.Add(blob);
            });

            return json.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        #endregion

        #region private methods

        private BlobStat GetBlobStat(string blobName)
        {
            var blobStat = Blobs.FirstOrDefault(a => a.BlobName == blobName);

            return blobStat;
        }

        private List<BlockStat> CalculateBlocksToProcess(long blobLength)
        {
            var blocksToUpload = (int)Math.Ceiling((double)blobLength / (double)UploadConstants.MaxBlockSize);

            var blocksToProcess = Enumerable
               .Range(0, blocksToUpload)
               .Select(r => new BlockStat() { Metadata = new BlockMetadata(r, blobLength, UploadConstants.MaxBlockSize) })
               .Where(b => b.Metadata.Length > 0)
               .ToList();

            return blocksToProcess;
        }

        #endregion
    }
}
