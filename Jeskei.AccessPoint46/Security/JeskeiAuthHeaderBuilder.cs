using System.Configuration;
using System.Net.Mime;
using System.Web;

namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Net.Http.Headers;
    using Core;
    using Thinktecture.IdentityModel.Client;

    public class JeskeiAuthHeaderBuilder
    {
        #region private fields

        private Uri _authUri;
        private string _clientId;
        private string _clientSecret;
        private string _scope;
        private TokenResponse _cachedToken;
        private OAuth2Client _client;
        private IConfigurationService _config;

        #endregion

        #region constructors

        public JeskeiAuthHeaderBuilder(string authUri, string clientId, string clientSecret, string scope, IConfigurationService config)
        {
            Guard.NotNullOrEmpty(authUri, nameof(authUri));
            Guard.NotNullOrEmpty(clientId, nameof(clientId));
            Guard.NotNullOrEmpty(clientId, nameof(clientSecret));
            Guard.NotNullOrEmpty(scope, nameof(scope));
            Guard.NotNull(config, nameof(config));

            _authUri = new Uri(authUri);
            _clientId = clientId;
            _clientSecret = clientSecret;
            _scope = scope;
            _config = config;
        }

        #endregion

        #region public methods

        public AuthenticationHeaderValue CreateJeskeiAuthHeader()
        {
            if (_cachedToken == null)
            {
                _cachedToken = GetAccessToken();
                // Open App.Config of executable
                System.Configuration.Configuration config =
                 ConfigurationManager.OpenExeConfiguration
                            (ConfigurationUserLevel.None);

                config.AppSettings.Settings.Remove("ModificationDate");
                // Add an Application Setting.
                config.AppSettings.Settings.Add("ModificationDate",
                               DateTime.Now.ToString() + " ");

                // Save the changes in App.config file.
                config.Save(ConfigurationSaveMode.Modified);

                // Force a reload of a changed section.
                ConfigurationManager.RefreshSection("appSettings");


              
                    string value = ConfigurationManager.AppSettings["ModificationDate"];
                    Console.WriteLine("Key: {0}, Value: {1}", "ModificationDate", value);
               

            }
            else
            {
                string value = ConfigurationManager.AppSettings["ModificationDate"];
                Console.WriteLine("Key: {0}, Value: {1}", "ModificationDate", value);

                DateTime dt = Convert.ToDateTime(value);
                var seconds = (DateTime.Now - dt).TotalSeconds;

                var tokenSeconds = _config.ReadConfigurationItem<int>(AccessPointSettingsNames.AuthTokenSeconds);

                if (seconds >= tokenSeconds)
                {
                    _cachedToken = GetRefreshToken(_cachedToken.RefreshToken);
                    // Open App.Config of executable
                    System.Configuration.Configuration config =
                     ConfigurationManager.OpenExeConfiguration
                                (ConfigurationUserLevel.None);

                                    config.AppSettings.Settings.Remove("ModificationDate");

                    // Add an Application Setting.
                    config.AppSettings.Settings.Add("ModificationDate",
                                   DateTime.Now.ToString() + " ");

                    // Save the changes in App.config file.
                    config.Save(ConfigurationSaveMode.Modified);

                    // Force a reload of a changed section.
                    ConfigurationManager.RefreshSection("appSettings");
                }
                else
                {
                    
                }
            }

            var header = new AuthenticationHeaderValue("Bearer", _cachedToken.AccessToken);

            return header;
        }

        public static JeskeiAuthHeaderBuilder Create(IConfigurationService config, string scope)
        {
            var authUri = config.ReadConfigurationItem<string>(AccessPointSettingsNames.JeskeiAuthUri);
            var clientId = config.ReadConfigurationItem<string>(AccessPointSettingsNames.AccessPointId);
            var secret = config.ReadConfigurationItem<string>(AccessPointSettingsNames.AccessPointSecret);

            return new JeskeiAuthHeaderBuilder(authUri, clientId, secret, scope, config);
        }

        #endregion

        #region private methods

        private TokenResponse GetAccessToken()
        {
            _client = GetAuthClient(_authUri, _clientId, _clientSecret);
            
            var response = _client.RequestResourceOwnerPasswordAsync("imax.user", "IMAX@te5tAcct!", _scope).Result;

            if (response.IsError)
                throw new Exception($"The call to obtain an authorisation token from {_authUri} failed. Error: {response.Error}");

            return response;
        }

        private TokenResponse GetRefreshToken(string refreshToken)
        {
            return _client.RequestRefreshTokenAsync(refreshToken).Result;
        }

        private OAuth2Client GetAuthClient(Uri authUri, string clientId, string clientSecret)
        {
             var client = new OAuth2Client(authUri, clientId, clientSecret);
            return client;
        }

        #endregion
    }
}
