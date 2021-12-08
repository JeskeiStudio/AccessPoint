namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;

    public class FileEntryByHashIdEqualityComparer : IEqualityComparer<FileEntry>
    {
        public bool Equals(FileEntry x, FileEntry y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            return x.HashId.Equals(y.HashId);
        }

        public int GetHashCode(FileEntry obj)
        {
            return obj.HashId.GetHashCode();
        }
    }
}
