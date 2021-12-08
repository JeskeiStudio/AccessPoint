namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Collections.Generic;
    using AccessPoint.Ingest.Pulse;
    using Core;

    public class PulseCommand
    {
        private PulseCoordinator coordinator = null;

        public void Run(IDictionary<string, object> settings)
        {
            var config = new ConfigurationService(settings);

            var pulseApiUri = config.ReadConfigurationItem<string>(AccessPointSettingsNames.PulseApiUri);
            var apiHelper = ApiHelperBuilder.BuildApiHelper<PulseApiHelper>(config, ApiScopes.AccessPointBaseScope, pulseApiUri);

            Console.WriteLine("Starting pulse processing");

            coordinator = new PulseCoordinator(config, apiHelper);
            coordinator.Start();
            
            Console.WriteLine("Pulse started");
        }

        public void Stop()
        {
            coordinator.Stop();
            Console.WriteLine("Pulse stopped");
        }
    }
}
