namespace Jeskei.AccessPoint.Modules.AssetMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core;
    using Core.Logging;

    public class AssetMonitorApiHelper : ApiHelperBase<AssetMonitorApiHelper>
    {
        #region constructors

        public AssetMonitorApiHelper()
        {
        }

        public AssetMonitorApiHelper(string baseUri, string id) : base(baseUri, id)
        {
        }

        #endregion

        #region public methods

        public async Task<List<AssetInfo>> GetAssets()
        {
            var requestUri = BuildAssetInfoUri();

            var assets = new List<AssetInfo>();

            using (var client = BuildHttpClient())
            {
                Logger.Debug($"Calling api {requestUri}");
                var response = await client.GetAsync(requestUri);
                Logger.Debug("Back from api call");
                response.EnsureSuccessStatusCode();

                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    Logger.Debug($"Response content {content}");

                    if (!String.IsNullOrWhiteSpace(content))
                    {
                        assets = SerialisationHelper.FromJson<List<AssetInfo>>(content);
                    }
                }
                else
                {
                    Logger.Debug("No content returned from api call");
                }
            }

            return assets;
        }

        public async Task PutAssetChecksum(AssetInfo asset)
        {
            var requestUri = BuildAssetInfoUri(asset);

            var content = GetRequestContentAsJson(asset);

            using (var client = BuildHttpClient())
            {
                Logger.Debug($"Http PUT of {content} to {requestUri}");
                var response = await client.PutAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        #endregion

        #region private methods

        private string BuildAssetInfoUri() => $"{this._baseUri}/assets/{this._id}";

        private string BuildAssetInfoUri(AssetInfo assetInfo) => $"{this._baseUri}/assets/{this._id}/asset/{assetInfo.AssetVersionInLocation}";

        #endregion
    }
}
