namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Ingest;

    public class ChecksumCommand
    {
        private FileChecksumCoordinator coordinator = null;

        public void Run(IDictionary<string, object> settings)
        {
            var config = new ConfigurationService(settings);
            var ingestBaseUri = config.ReadConfigurationItem<string>(IngestSettingsNames.IngestApiUri);
            var ingestApiHelper = ApiHelperBuilder.BuildApiHelper<IngestApiHelper>(config, ApiScopes.IngestScope, ingestBaseUri);

            Console.WriteLine("Starting checksum processing");

            coordinator = new FileChecksumCoordinator(config, ingestApiHelper);
            coordinator.Start();

            Console.WriteLine("Checksum starting");
        }

        public void Stop()
        {
            coordinator.Stop();
            Console.WriteLine("Checksum stopped");
        }
    }
}
