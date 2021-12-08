namespace Jeskei.AccessPoint.Modules.AssetMonitor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.Logging;

    public class AssetMonitorCoordinator : CommandCoordinatorBase<AssetMonitorCoordinator>
    {
        #region private fields

        private readonly AssetMonitorApiHelper _apiHelper;
        private List<AssetInfo> _lastAssetSnapshot;
        private DateTimeOffset _lastFullRefresh;
        private TimeSpan _fullReloadInterval;

        #endregion

        #region constructors
        public AssetMonitorCoordinator(IConfigurationService config, AssetMonitorApiHelper apiHelper)
            : base("Asset Monitor", config)
        {
            Guard.NotNull(apiHelper, nameof(apiHelper));

            _fullReloadInterval = GetCoordinatorConfigurationValue<TimeSpan>("fullReloadInterval", TimeSpan.FromMinutes(10));
            _apiHelper = apiHelper;
            base.CoordinatorBody = AssetMonitorBody;
        }

        #endregion

        #region public methods

        public async Task AssetMonitorBody()
        {
            // this method gets invoked every interval by the coordinator...
            Logger.Info($"Starting {nameof(AssetMonitorBody)}");
            
            var folder = _configService.ReadConfigurationItem<string>(AccessPointSettingsNames.AssetLibraryFolders);
            var localAssets = GetAssetsInFolder(folder);

            if (localAssets.Count > 0 && 
                LocalAssetsChanged(_lastAssetSnapshot, localAssets))
            {
                Logger.Info("Calling api to obtain list of central assets");

                UseCachedChecksumsIfRequired(localAssets);
         
                var centralAssets = await _apiHelper.GetAssets();
                var toChecksum = GetAssetsRequiringChecksum(localAssets, centralAssets);

                foreach (var asset in toChecksum)
                {
                    Logger.Info($"Calculating checksum for {asset.AssetPath}");
                    asset.Checksum = HashHelper.SHA1FromFile(asset.AssetPath);
                    asset.ChecksumGeneratedAt = DateTimeOffset.UtcNow;

                    Logger.Info("Checksum calculated; notifying central service of checksum value");
                    await _apiHelper.PutAssetChecksum(asset);
                }

                // save checkpoint
                _lastAssetSnapshot = localAssets;
            }
            else
            {
                Logger.Debug("No change in asset state");
            }

            Logger.Info($"Finished {nameof(AssetMonitorBody)}");

            return;
        }

        #endregion

        #region private methods

        private void UseCachedChecksumsIfRequired(List<AssetInfo> newLocalAssets)
        {
            // copy any already calculated checksums to the new local assets list
            // this is done to cover the case where checksums have been sent for processing 
            // but not yet persisted to the database
            if (DateTimeOffset.UtcNow < _lastFullRefresh.Add(_fullReloadInterval))
            {
                CopyChecksumState(_lastAssetSnapshot, newLocalAssets);
            }
            else
            {
                _lastFullRefresh = DateTimeOffset.UtcNow;
            }
        }

        private static void CopyChecksumState(List<AssetInfo> oldAssets, List<AssetInfo> newAssets)
        {
            foreach (var asset in oldAssets.Where(a => !String.IsNullOrWhiteSpace(a.Checksum)))
            {
                var newAsset = newAssets.SingleOrDefault(a => a.AssetVersionInLocation == asset.AssetVersionInLocation);

                if (newAsset != null)
                {
                    newAsset.Checksum = asset.Checksum;
                    newAsset.ChecksumGeneratedAt = asset.ChecksumGeneratedAt;
                }
            }
        }

        private static bool LocalAssetsChanged(List<AssetInfo> oldAssets, List<AssetInfo> newAssets)
        {
            if (oldAssets == null)
                return true;

            if (oldAssets.Count != newAssets.Count)
                return true;

            var joinedList = oldAssets.Join(newAssets, o => o.AssetVersionInLocation, n => n.AssetVersionInLocation, (o, n) => o)
                .ToList();

            if (joinedList.Count != oldAssets.Count)
                return true;

            return false;
        }

        private static List<AssetInfo> GetAssetsRequiringChecksum(List<AssetInfo> localAssets, List<AssetInfo> centralAssets)
        {
            var centralAssetsRequiringChecksum = centralAssets
                .Where(a => String.IsNullOrWhiteSpace(a.Checksum))
                .ToList();

            var toChecksum = localAssets.Join(centralAssetsRequiringChecksum, lcl => lcl.AssetVersionInLocation, ctrl => ctrl.AssetVersionInLocation, (lcl, ctrl) => lcl)
                .ToList();
            
            return toChecksum;
        }

        private static List<AssetInfo> GetAssetsInFolder(string assetFolder) => 
            GetAssetFiles(assetFolder).Select(a => new AssetInfo(a.FullName)).ToList();

        private static List<FileInfo> GetAssetFiles(string assetFolder)
        {
            if (!Directory.Exists(assetFolder))
                return new List<FileInfo>();

            DirectoryInfo di = new DirectoryInfo(assetFolder);
            return di.GetFiles("*.*", SearchOption.AllDirectories).ToList();
        }

        #endregion
    }
}
