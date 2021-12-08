namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;

    public class IngestFileInfoDto
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public string FileNameHash { get; set; }
        public long ByteCount { get; set; }
        public DateTime LastModified { get; set; }
        public string FileContentsChecksum { get; set; }
    }
}
