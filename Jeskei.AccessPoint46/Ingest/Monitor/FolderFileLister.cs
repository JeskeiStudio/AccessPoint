namespace Jeskei.AccessPoint.Modules.Ingest
{
    using Jeskei.AccessPoint.Core;
    using System.Collections.Generic;
    using System.IO;

    public class FolderFileLister : IFileLister
    {
        public List<FileEntry> ListFiles(string[] paths)
        {
            Guard.NotNull(paths, nameof(paths));

            var files = new List<FileEntry>();

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                    continue;

                var entries = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                
                foreach (var entry in entries)
                {
                    var fi = new FileInfo(entry);

                    if (fi.Exists == true &&
                        fi.Length == 0)
                    {
                        // delete file
                        File.Delete(entry);
                    }
                    else
                    {
                        if (fi.Exists == true)
                        {
                            files.Add(new FileEntry()
                            {
                                FullPath = fi.FullName.Remove(0, path.Length),
                                LastWriteTimeUtc = fi.LastWriteTimeUtc,
                                Length = fi.Length,
                            });
                        }
                    }
                }
            }

            return files;
        }
    }
    
}
