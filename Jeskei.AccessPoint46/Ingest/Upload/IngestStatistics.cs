namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core;

    public class IngestStatistics
    {
        public string ContainerName { get; set; }

        public List<BlobStatistics> BlobTransfers = new List<BlobStatistics>();

        public List<TimeBlock> TimeBlocks = new List<TimeBlock>();

        public double TotalTransferMbps { get; set;  }

        public long TotalTransferSeconds { get; set; }

        public long TotalTransferBytes { get; set; }
    }

    public class BlobStatistics
    {
        public string BlobName { get; set; }

        public long BlobLength { get; set; }

        public List<BlockStatistcs> BlockTransfers { get; set; } = new List<BlockStatistcs>();
    }

    public class BlockStatistcs
    {
        public int Id { get; set; }

        public long Length { get; set; }

        public List<BlockTransferStatistics> BlockAttempts { get; set; } = new List<BlockTransferStatistics>();
    }

    public class BlockTransferStatistics
    {
        public DateTimeOffset Start { get; set; }

        public long ElapsedMs { get; set; }
    }

    public static class IngestStaticsBuilder
    {
        public static IngestStatistics CreateFromUploadState(UploadState state)
        {
            Guard.NotNull(state, nameof(state));

            var stats = new IngestStatistics();
            stats.ContainerName = state.ContainerName;

            var currentBlock = new TimeBlock();
            stats.TimeBlocks.Add(currentBlock);

            foreach (var blob in state.Blobs)
            {
                var blobstats = new BlobStatistics() { BlobName = blob.BlobName, BlobLength = blob.BlobLength };

                foreach (var block in blob.Blocks)
                {
                    var blockstats = new BlockStatistcs() { Id = block.Metadata.Id, Length = block.Metadata.Length };

                    foreach (var transfer in block.TransferHistory)
                    {
                        blockstats.BlockAttempts.Add(new BlockTransferStatistics() { Start = transfer.StartTime, ElapsedMs = transfer.ElapsedMs });
                    }

                    blobstats.BlockTransfers.Add(blockstats);

                    try
                    {
                        var uploadedBlock = block.TransferHistory.Last();
                        if (currentBlock.IsInBlock(uploadedBlock.StartTime, uploadedBlock.EndTime))
                        {
                            currentBlock.Add(uploadedBlock.StartTime, uploadedBlock.EndTime);
                        }
                        else
                        {
                            stats.TimeBlocks.Add(currentBlock);
                            currentBlock = new TimeBlock();
                        }
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }

                stats.BlobTransfers.Add(blobstats);
            }

            stats.TotalTransferBytes = stats.BlobTransfers.Sum(a => a.BlobLength);
            stats.TotalTransferSeconds = (long)stats.TimeBlocks.Sum(a => a.GetBlockElapsed().TotalSeconds);
            stats.TotalTransferMbps = ((((double)(stats.TotalTransferBytes * 8)) / stats.TotalTransferSeconds) / 1000000);
            
            return stats;
        }
    }
}
