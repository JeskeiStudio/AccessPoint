namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Collections.Generic;
    using AssetMonitor;
    using Core;

    public class AssetMonitorCommand
    {
        private AssetMonitorCoordinator coordinator = null;

        public void Run(IDictionary<string, object> settings)
        {
            var config = new ConfigurationService(settings);
            var assetMonitorApiUri = config.ReadConfigurationItem<string>(AccessPointSettingsNames.AssetMonitorApiUri);
            var apiHelper = ApiHelperBuilder.BuildApiHelper<AssetMonitorApiHelper>(config, ApiScopes.IngestScope, assetMonitorApiUri);

            Console.WriteLine("Starting asset monitor processing");

            var coordinator = new AssetMonitorCoordinator(config, apiHelper);
            coordinator.Start();
            
            Console.WriteLine("Asset monitor started");
        }

        public void Stop()
        {
            coordinator.Stop();
            Console.WriteLine("Asset monitor stopped");
        }
    }
}
