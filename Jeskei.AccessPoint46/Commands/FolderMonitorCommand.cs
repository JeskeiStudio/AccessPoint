namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Ingest;

    public class FolderMonitorCommand
    {
        FolderSyncCoordinator coordinator = null;

        public void Run(IDictionary<string, object> settings)
        {
            var config = new ConfigurationService(settings);
            var folderMonitor = new FolderMonitor(config, new FolderFileLister());

            var ingestBaseUri = config.ReadConfigurationItem<string>(IngestSettingsNames.IngestApiUri);
            var ingestApiHelper = ApiHelperBuilder.BuildApiHelper<IngestApiHelper>(config, ApiScopes.IngestScope, ingestBaseUri);

            var fileChangeHandler = new IngestApiFileChangeHandler(ingestApiHelper);

            Console.WriteLine("Starting monitoring of folders.");

            coordinator = new FolderSyncCoordinator(config, folderMonitor, fileChangeHandler);
            coordinator.Start();
            
            Console.WriteLine("Foldermon running");
        }

        public void Stop()
        {
            coordinator.Stop();
            Console.WriteLine("Foldermon stopped");
        }
    }
}