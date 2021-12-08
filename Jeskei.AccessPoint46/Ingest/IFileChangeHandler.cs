namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFileChangeHandler
    {
        void RouteFull(List<FileEntryChange> fullList);

        void RouteAdditions(List<FileEntryChange> additions);

        void RouteUpdates(List<FileEntryChange> updates);

        void RouteRemovals(List<FileEntryChange> deletions);
    }
}
