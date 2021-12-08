namespace Jeskei.AccessPoint.Modules.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Logging;
    using Newtonsoft.Json;

    public class ConfigApiHelper : ApiHelperBase<ConfigApiHelper>
    {
        #region constructors

        public ConfigApiHelper()
        {
        }

        public ConfigApiHelper(string baseUri, string id) : base(baseUri, id)
        {
        }

        #endregion

        #region public methods

        public async Task<List<ConfigSetting>> GetSettings()
        {
            var requestUri = BuildGetConfigUri();

            var settings = new List<ConfigSetting>();

            using (var client = BuildHttpClient())
            {
                Logger.Debug("Calling api to get configuration");

                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (!String.IsNullOrWhiteSpace(content))
                    {
                        settings = JsonConvert.DeserializeObject<List<ConfigSetting>>(content);

                        Logger.DebugFormat("Settings: {@Settings}", settings);
                    }
                }
            }

            return settings;
        }

        #endregion

        #region private methods

        private string BuildGetConfigUri()
        {
            return $"{this._baseUri}/configuration/{this._id}";
        }

        #endregion
    }
}
