namespace Jeskei.AccessPoint.Ingest.Pulse
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Core;
    using Modules;

    public class PulseCoordinator : CommandCoordinatorBase<PulseCoordinator>
    {
        #region private fields

        private PulseApiHelper _apiHelper;
        private string lastAssetLibraryHash;
        private string lastDriveDetailsHash;

        #endregion

        #region constructors

        public PulseCoordinator(IConfigurationService configService, PulseApiHelper apiHelper)
            : base("Pulse", configService)
        {
            Guard.NotNull(apiHelper, nameof(apiHelper));

            this._apiHelper = apiHelper;
            base.CoordinatorBody = SendPulse;
        }

        #endregion

        #region public methods

        public async Task SendPulse()
        {
            // this method gets invoked every interval by the coordinator...

            var assetLibraryDrive = _configService.ReadConfigurationItem<string>(AccessPointSettingsNames.AssetLibraryDrive);
            var assetLibraryFolder = _configService.ReadConfigurationItem<string>(AccessPointSettingsNames.AssetLibraryFolders);
            var accessPointId = _configService.ReadConfigurationItem<string>(AccessPointSettingsNames.AccessPointId);

            // get asset library listing
            // TODO: support multiple folders??
            string assetLibraryHash;
            var assetLibraryEl = GetAssetLibraryIfChanged(assetLibraryFolder, out assetLibraryHash);

            // get drive details
            // TODO: support multiple drives
            string driveDetailsHash;
            var driveDetailsEl = GetDriveDetailsIfChanged(assetLibraryDrive, out driveDetailsHash);

            // generate pulse
            var pulse = this.GeneratePulse(accessPointId, driveDetailsEl, assetLibraryEl);

            // send pulse
            await this._apiHelper.PostPulse(pulse);

            // set hash values for next time
            this.lastDriveDetailsHash = driveDetailsHash;
            this.lastAssetLibraryHash = assetLibraryHash;
        }

        #endregion

        #region private methods

        private string GeneratePulse(string accessPointId, XElement driveDetailsEl, XElement assetLibraryEl)
        {
            var pulseXml = new XElement("JeskeiPulse",
                         new XAttribute("generated-at-utc", DateTime.UtcNow.ToString("s")),
                         new XElement("AccessPointId", accessPointId));

            if (driveDetailsEl != null)
                pulseXml.Add(driveDetailsEl);

            if (assetLibraryEl != null)
                pulseXml.Add(assetLibraryEl);

            return pulseXml.ToString();
        }

        private XElement GetAssetLibraryIfChanged(string assetLibraryFolder, out string hash)
        {
            var assetLibraryEl = GetAssetLibraryDetails(assetLibraryFolder);
            hash = HashHelper.MD5FromString(assetLibraryEl.ToString());

            return hash == this.lastAssetLibraryHash ? null : assetLibraryEl;
        }

        private XElement GetDriveDetailsIfChanged(string driveName, out string hash)
        {
            var driveEl = GetVolumeDetails(driveName);
            hash = HashHelper.MD5FromString(driveEl.ToString());

            return hash == this.lastDriveDetailsHash ? null : driveEl;
        }

        private XElement GetAssetLibraryDetails(string assetLibraryPath)
        {
            Guard.NotNullOrEmpty(assetLibraryPath, nameof(assetLibraryPath));

            var di = new DirectoryInfo(assetLibraryPath);
            return di.ToFolderXml();
        }

        private XElement GetVolumeDetails(string driveName)
        {
            Guard.NotNullOrEmpty(driveName, nameof(driveName));

            var di = new DriveInfo(driveName);
            return di.ToDriveXml();
        }

        #endregion
    }
}
