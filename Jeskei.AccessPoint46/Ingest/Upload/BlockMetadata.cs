namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Text;

    public class BlockMetadata
    {
        public BlockMetadata()
        {
        }

        public BlockMetadata(int id, long length, int bytesPerBlock)
        {
            this.Id = id;
            this.BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(id.ToString("000000")));
            this.Index = (long)id * (long)bytesPerBlock;
            var remainingBytes = length - this.Index;
            this.Length = (int)Math.Min(remainingBytes, (long)bytesPerBlock);
        }

        public long Index { get; set; }
        public int Id { get; set; }
        public string BlockId { get; set; }
        public int Length { get; set; }
        public string BlockHash { get; set; }
        public bool UploadCompleted { get; set; } = false;
        public bool Committed { get; set; } = false;
    }
}
