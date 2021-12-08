namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Collections.Generic;
    using Core;
    using Ingest;

    public class IngestCommand
    {
        private FileUploadCoordinator coordinator = null;

        public void Run(IDictionary<string, object> settings)
        {
            var config = new ConfigurationService(settings);

            var ingestBaseUri = config.ReadConfigurationItem<string>(IngestSettingsNames.IngestApiUri);
            var ingestApiHelper = ApiHelperBuilder.BuildApiHelper<IngestApiHelper>(config, ApiScopes.IngestScope, ingestBaseUri);

            Console.WriteLine("Starting ingest processing");

            coordinator = new FileUploadCoordinator(config, ingestApiHelper);
            coordinator.Start();

            Console.WriteLine("ingest process started");
        }

        public void Stop()
        {
            coordinator.Stop();
            Console.WriteLine("Ingest stopped");
        }
    }
}
