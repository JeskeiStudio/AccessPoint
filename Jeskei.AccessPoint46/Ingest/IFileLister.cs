namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;
    using System.IO;

    public interface IFileLister
    {
        List<FileEntry> ListFiles(string[] paths);
    }
}
