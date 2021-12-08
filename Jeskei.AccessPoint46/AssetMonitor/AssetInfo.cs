namespace Jeskei.AccessPoint.Modules.AssetMonitor
{
    using System;
    using System.IO;
    using Core;

    public class AssetInfo
    {
        #region constructors

        public AssetInfo()
        {
        }

        public AssetInfo(string assetPath)
        {
            this.AssetPath = assetPath;
            this.AssetVersionInLocation = this.GetAssetVersionInLocationId(assetPath);
        }

        #endregion

        #region properties

        public string AssetPath { get; set; }

        public string AssetVersionInLocation { get; set; }

        public string Checksum { get; set; }

        public DateTimeOffset ChecksumGeneratedAt { get; set; }

        #endregion

        #region private methods

        private string GetAssetVersionInLocationId(string assetPath) => Path.GetFileNameWithoutExtension(assetPath);

        #endregion
    }
}
