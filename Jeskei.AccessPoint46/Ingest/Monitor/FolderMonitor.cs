namespace Jeskei.AccessPoint.Modules.Ingest
{
    using Jeskei.AccessPoint.Core;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class FolderMonitor
    {
        #region private fields

        private IConfigurationService configService;
        private FileEntryManager fileEntryManager;
        private IFileLister folderFileLister;
        private CancellationTokenSource cts;
        private string[] pathsToMonitor;

        #endregion

        #region events

        public EventHandler<FileChangeEventArgs> FileChanges;

        #endregion

        #region public methopds

        public FolderMonitor(IConfigurationService configService, IFileLister fileLister)
        {
            Guard.NotNull(configService, nameof(configService));
            Guard.NotNull(fileLister, nameof(fileLister));

            this.EnsureConfigurationSet(configService);
            this.configService = configService;
            this.folderFileLister = fileLister;
        }

        public void Start()
        {
            this.pathsToMonitor = this.configService.ReadConfigurationItem<string[]>(IngestSettingsNames.IngestFolders);

            var initialState = this.folderFileLister.ListFiles(pathsToMonitor);
            this.fileEntryManager = new FileEntryManager(initialState);

            // raise event for initial state
            var initialChanges = initialState
                .Select(a => FileEntryChange.FromFileEntry(FileEntryChangeType.Add, a))
                .ToList();

            RaiseChangeEvent(initialChanges, isInitial: true);

            this.cts = new CancellationTokenSource();
            new Task(() => RefreshFolderListing(), this.cts.Token, TaskCreationOptions.LongRunning).Start();
        }

        public void Stop()
        {
            this.cts?.Cancel();
            // TODO: wait for the cancel??
        }

        #endregion

        #region private methods

        private void RaiseChangeEvent(List<FileEntryChange> changes, bool isInitial = false)
        {
            if (changes.Count > 0)
                this.FileChanges?.Invoke(this, new FileChangeEventArgs(changes) { IsInitial = isInitial });
        }

        private void RefreshFolderListing()
        {
            do
            {
                var newState = this.folderFileLister
                    .ListFiles(this.pathsToMonitor);

                var changes = this.fileEntryManager
                    .ProcessChangeset(newState);

                RaiseChangeEvent(changes);

                // work done for now...wait for check period or until canceled, whichever is sooner
                this.cts.Token
                    .WaitHandle
                    .WaitOne(this.configService.ReadConfigurationItem<TimeSpan>(IngestSettingsNames.MonitorInterval));
            }
            while (!this.cts.Token.IsCancellationRequested);
        }

        private void EnsureConfigurationSet(IConfigurationService configService)
        {
            var requiredConfigItems = new string[]
            {
                IngestSettingsNames.IngestFolders,
                IngestSettingsNames.MonitorInterval,
            };

            string[] missing;
            if (!configService.TryValidateConfiguration(requiredConfigItems, out missing))
            {
                throw new Exception($"FolderMonitor configuration requires the {missing.AggregateIntoString()} value(s) be specified.");
            }
        }

        #endregion
    }
}
