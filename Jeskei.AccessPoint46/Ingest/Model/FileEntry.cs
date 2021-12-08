namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    public class FileEntry
    {
        public string HashId { get; set; }
        public string FullPath { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public long Length { get; set; }
        public string Checksum { get; set; }

        public FileEntry Clone()
        {
            return new FileEntry()
            {
                Checksum = this.Checksum,
                FullPath = this.FullPath,
                HashId = this.HashId,
                LastWriteTimeUtc = this.LastWriteTimeUtc,
                Length = this.Length,
            };
        }
    }
}
