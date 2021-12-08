namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;

    public class IngestDetail
    {
        public string Container { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public string FileNameHash { get; set; }

        public string ContainerUri { get; set; }

        public List<IngestBlobDetail> AssetLayout { get; set; }

        public string GetFullPath()
        {
            return System.IO.Path.Combine(Path, FileName);
        }
    }

    public class IngestBlobDetail
    {
        public string BlobName { get; set; }

        public long Offset { get; set; }

        public long Length { get; set; }

    }
}
