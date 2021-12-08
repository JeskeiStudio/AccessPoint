namespace Jeskei.AccessPoint.Modules
{
    using Core;

    public static class ApiHelperBuilder
    {
        public static T BuildApiHelper<T>(IConfigurationService config, string scopes, string baseUri) where T : ApiHelperBase<T>, new()
        {
            var authHeaderBuilder = JeskeiAuthHeaderBuilder.Create(config, scopes);
            var token = authHeaderBuilder.CreateJeskeiAuthHeader();

            var clientId = config.ReadConfigurationItem<string>(AccessPointSettingsNames.AccessPointId);

            var apiHelper = new T();
            apiHelper.Init(baseUri, clientId);
            apiHelper.BuildAuthHeader = authHeaderBuilder.CreateJeskeiAuthHeader;

            return apiHelper;
        }
    }
}
