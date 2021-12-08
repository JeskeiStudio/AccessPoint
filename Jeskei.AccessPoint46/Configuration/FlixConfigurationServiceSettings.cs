namespace Flix.AccessPoint.Modules.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Flix.AccessPoint.Core;
    using System.Configuration;


    public static class FlixConfigurationServiceSettings
    {
        public static IConfigurationBuilder AddFlixConfigService(this IConfigurationBuilder configurationBuilder)
        {
            var configUri = ConfigurationManager.AppSettings["flix.configUri"];

            var settings = new Dictionary<string, object>()
            {
                { "ap.id", ConfigurationManager.AppSettings["ap.id"] },
                { "ap.secret",  ConfigurationManager.AppSettings["ap.secret"] },
                { "flix.authUri", ConfigurationManager.AppSettings["flix.authUri"] },
            };

            var cfgService = new ConfigurationService(settings);

            var apiHelper = ApiHelperBuilder.BuildApiHelper<ConfigApiHelper>(cfgService, ApiScopes.AccessPointBaseScope, configUri);
            var apSettings = apiHelper.GetSettings().Result;

            var settingsDict = apSettings.ToDictionary(k => k.Name, v => v.Value);

            configurationBuilder.AddInMemoryCollection(settingsDict);

            return configurationBuilder;
        }
    }
}
