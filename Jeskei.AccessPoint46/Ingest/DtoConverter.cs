namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class DtoConverter
    {
        public static List<IngestFileInfoDto> ConvertToDto(List<FileEntryChange> entries)
        {
            return entries
                .Select(a => IngestFileDtoFromFileEntryChange(a))
                .ToList();
        }

        public static List<IngestFileInfoDto> ConvertToDto(List<FileEntry> entries)
        {
            return entries
                .Select(a => IngestFileDtoFromFileEntry(a))
                .ToList();
        }

        public static IngestFileInfoDto IngestFileDtoFromFileEntryChange(FileEntryChange fileEntry)
        {
            return new IngestFileInfoDto()
            {
                ByteCount = fileEntry.Length,
                FileContentsChecksum = null,
                FileNameHash = fileEntry.HashId,
                FileName = Path.GetFileName(fileEntry.FullPath),
                Path = Path.GetDirectoryName(fileEntry.FullPath),
                LastModified = fileEntry.LastWriteTimeUtc,
            };
        }

        public static IngestFileInfoDto IngestFileDtoFromFileEntry(FileEntry fileEntry)
        {
            return new IngestFileInfoDto()
            {
                ByteCount = fileEntry.Length,
                FileContentsChecksum = null,
                FileNameHash = fileEntry.HashId,
                FileName = fileEntry.FullPath,
                LastModified = fileEntry.LastWriteTimeUtc,
            };
        }
    }
}
