namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;

    public class FileChangeEventArgs
    {
        public FileChangeEventArgs(List<FileEntryChange> fileChanges)
        {
            this.FileChanges = fileChanges;
        }

        public List<FileEntryChange> FileChanges { get; set; }
        public bool IsInitial { get; set; }
    }
}
