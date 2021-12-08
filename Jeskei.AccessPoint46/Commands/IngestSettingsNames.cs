namespace Jeskei.AccessPoint.Modules
{
    public static class IngestSettingsNames
    {
        public const int DefaultFolderSyncChangeIntervalMs = 1000;
        public const int DefaultFileChecksumIntervalMs = 1000;
        public const int DefaultIngestCheckIntervalMs = 1000;

        public const string IngestApiUri = "ingest.ingestApiUri";
        public const string IngestFolders = "ingest.folders";
        public const string MonitorInterval = "ingest.monitorInterval";
        public const string FolderSyncChangeInterval = "ingest.folderSyncChangeInterval";
        public const string FileChecksumInterval = "ingest.fileChecksumInterval";
        public const string IngestCheckInterval = "ingest.ingestCheckInterval";
        public const string DeleteFileAfterIngest = "ingest.deleteFileAfterIngest";
        public const string AzureUploadParallelism = "ingest.azureUploadParallelism";
        public const string AzureCommitBlockCount = "ingest.azureCommitBlockCount";
        public const string AssetMonitorApiUri = "assetMonitor.apiUri";
        
    }
}
