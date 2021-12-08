namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;

    public class BlockStat 
    {
        public BlockMetadata Metadata { get; set; }

        public List<TransferStat> TransferHistory { get; set; } = new List<TransferStat>();
    }
}
