namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Jeskei.AccessPoint.Core;

    public class IngestApiFileChangeHandler : IFileChangeHandler
    {
        #region private fields

        private IngestApiHelper apiHelper;

        #endregion

        #region constructors

        public IngestApiFileChangeHandler(IngestApiHelper ingestApi)
        {
            Guard.NotNull(ingestApi, nameof(ingestApi));
            this.apiHelper = ingestApi;
        }

        #endregion

        #region public methods

        public void RouteFull(List<FileEntryChange> fullList)
        {
            this.RouteFullAsync(fullList).Wait();
        }

        public void RouteAdditions(List<FileEntryChange> additions)
        {
            this.RouteAdditionsAsync(additions).Wait();
        }

        public void RouteRemovals(List<FileEntryChange> deletions)
        {
            this.RouteRemovalsAsync(deletions).Wait();
        }

        public void RouteUpdates(List<FileEntryChange> updates)
        {
            this.RouteUpdatesAsync(updates).Wait();
        }

        public async Task RouteFullAsync(List<FileEntryChange> fullList)
        {
            Guard.NotNull(fullList, nameof(fullList));

            if (fullList.Count > 0)
            {
                await this.apiHelper.PostFullFileListAsync(fullList);
            }
        }

        public async Task RouteAdditionsAsync(List<FileEntryChange> additions)
        {
            Guard.NotNull(additions, nameof(additions));

            if (additions.Count > 0)
            {
                await this.apiHelper.PutIngestFileAsync(additions);
            }
        }

        public async Task RouteRemovalsAsync(List<FileEntryChange> deletions)
        {
            Guard.NotNull(deletions, nameof(deletions));

            foreach (var deletion in deletions)
            {
                await this.apiHelper.DeleteIngestFileAsync(deletion);
            }
        }

        public async Task RouteUpdatesAsync(List<FileEntryChange> updates)
        {
            Guard.NotNull(updates, nameof(updates));

            if (updates.Count > 0)
            {
                await this.apiHelper.PutIngestFileAsync(updates);
            }
        }

        #endregion
    }
}
