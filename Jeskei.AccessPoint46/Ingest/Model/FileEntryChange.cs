namespace Jeskei.AccessPoint.Modules.Ingest
{
    using Jeskei.AccessPoint.Core;
    using System;

    public class FileEntryChange
    {
        public string HashId { get; set; }
        public string FullPath { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public long Length { get; set; }
        public string Checksum { get; set; }
        public FileEntryChangeType Change { get; set; }

        public static FileEntryChange FromFileEntry(FileEntryChangeType changeType, FileEntry entry)
        {
            Guard.NotNull(entry, nameof(entry));

            return new FileEntryChange()
            {
                Change = changeType,
                Checksum = entry.Checksum,
                FullPath = entry.FullPath,
                HashId = entry.HashId,
                LastWriteTimeUtc = entry.LastWriteTimeUtc,
                Length = entry.Length,
            };
        }
    }
}
