namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;

    public class ConsoleFileChangeHandler : IFileChangeHandler
    {
        public void RouteFull(List<FileEntryChange> fullList)
        {
            fullList.ForEach(a => Write('*', a));
        }

        public void RouteAdditions(List<FileEntryChange> additions)
        {
            additions.ForEach(a => Write('+', a));
        }

        public void RouteRemovals(List<FileEntryChange> deletions)
        {
            deletions.ForEach(a => Write('-', a));
        }

        public void RouteUpdates(List<FileEntryChange> updates)
        {
            updates.ForEach(a => Write('~', a));
        }

        private void Write(char prefix, FileEntryChange change)
        {
            Console.WriteLine($"{prefix} {change.FullPath} ({change.Length} bytes) {change.LastWriteTimeUtc:s}");
        }
    }
}
