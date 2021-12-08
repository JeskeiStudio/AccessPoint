namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Logging;
    using Jeskei.AccessPoint.Core;

    public class FolderSyncCoordinator : CommandCoordinatorBase<FolderSyncCoordinator>
    {
        #region private fields

        private FolderMonitor folderMonitor;
        private IFileChangeHandler fileChangeHandler;
        private Queue<FileChangeSet> fileChanges;

        #endregion

        #region constructors

        public FolderSyncCoordinator(
            IConfigurationService configService,
            FolderMonitor folderMonitor,
            IFileChangeHandler fileChangeHandler) : 
            base("Folder Sync", configService)
        {
            Guard.NotNull(configService, nameof(configService));
            Guard.NotNull(folderMonitor, nameof(folderMonitor));
            Guard.NotNull(fileChangeHandler, nameof(fileChangeHandler));

            this.fileChanges = new Queue<FileChangeSet>();
            this.folderMonitor = folderMonitor;
            this.folderMonitor.FileChanges += QueueFileChange;
            this.fileChangeHandler = fileChangeHandler;
            base.CoordinatorBody = SendChanges;
        }

        #endregion

        #region public methods

        public override void Start()
        {
            Logger.Debug("Starting folder monitor");
            this.folderMonitor.Start();

            base.Start();
        }

        public override void Stop()
        {
            Logger.Debug("Stopping folder monitor");
            this.folderMonitor.Stop();

            base.Stop();
        }

        #endregion

        #region private methods

        private async Task SendChanges()
        {
            if (this.fileChanges.Count > 0)
            {
                // dequeue next batch
                var changeSet = this.fileChanges.Peek();

                try
                {
                    if (changeSet.IsFullList)
                    {
                        Logger.Debug("Routing full set of changes");
                        this.fileChangeHandler.RouteFull(changeSet.Changes);
                    }
                    else
                    {
                        Logger.Debug("Routing file additions");
                        this.fileChangeHandler.RouteAdditions(ExtractChanges(changeSet.Changes, FileEntryChangeType.Add));
                        Logger.Debug("Routing file updates");
                        this.fileChangeHandler.RouteUpdates(ExtractChanges(changeSet.Changes, FileEntryChangeType.Update));
                        Logger.Debug("Routing file removals");
                        this.fileChangeHandler.RouteRemovals(ExtractChanges(changeSet.Changes, FileEntryChangeType.Remove));
                    }

                    // done so remove from queue
                    this.fileChanges.Dequeue();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Failed to route changes to the server", ex);
                }
            }
        }

        private List<FileEntryChange> ExtractChanges(List<FileEntryChange> changes, FileEntryChangeType changeType)
        {
            return changes
                .Where(a => a.Change == changeType)
                .ToList();
        }

        private void QueueFileChange(object sender, FileChangeEventArgs args)
        {
            var changeSet = new FileChangeSet
            {
                Changes = args.FileChanges,
                IsFullList = args.IsInitial
            };

            this.fileChanges.Enqueue(changeSet);
        }

        #endregion

        #region classes

        private class FileChangeSet
        {
            public bool IsFullList { get; set; }
            public List<FileEntryChange> Changes { get; set; }
        }

        #endregion
    }
}
