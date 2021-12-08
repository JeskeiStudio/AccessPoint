namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System.IO;
    using Core;
    using Newtonsoft.Json;

    public static class StateHelper
    {
        #region private fields

        private static object _sync = new object();

        #endregion

        #region public methods

        public static void SaveState(UploadState state)
        {
            Guard.NotNull(state, nameof(state));

            var fullPath = BuildStateFullPath(state.ContainerName);

            lock (_sync)
            {
                var serialised = JsonConvert.SerializeObject(state);
                File.WriteAllText(fullPath, serialised);
            }
        }

        public static UploadState LoadState(string containerName)
        {
            Guard.NotNullOrEmpty(containerName, nameof(containerName));

            var fullPath = BuildStateFullPath(containerName);

            if (!File.Exists(fullPath))
                return null;

            var serialised = File.ReadAllText(fullPath);

            var uploadState = JsonConvert.DeserializeObject<UploadState>(serialised);

            // TODO: handle deserialize failures (delete file??)

            return uploadState;
        }

        #endregion

        #region private methods

        private static string BuildStateFullPath(string containerName)
        {
            var folder = GetStorageFolder();
            var fileName = BuildFileName(containerName);

            return Path.Combine(folder, fileName);
        }

        private static string GetStorageFolder()
        {
            var tempPath = Path.GetTempPath();
            return tempPath;
        }

        private static string BuildFileName(string containerName)
        {
            return $"{containerName}_ingestState.json";
        }

        #endregion
    }
}
