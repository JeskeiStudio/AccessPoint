namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;

    public class BlobStat
    {
        public string BlobName { get; set; }

        public string BlobMD5Checksum { get; set; }

        public long BlobLength { get; set; }

        public List<BlockStat> Blocks { get; set; } = new List<BlockStat>();
    }
}
