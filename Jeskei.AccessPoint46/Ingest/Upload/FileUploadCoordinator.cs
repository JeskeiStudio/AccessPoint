namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Core;
    using Core.Logging;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class FileUploadCoordinator : CommandCoordinatorBase<FileUploadCoordinator>
    {
        #region private fields

        private readonly IngestApiHelper _apiHelper;

        #endregion

        #region constructors

        public FileUploadCoordinator(IConfigurationService configService, IngestApiHelper apiHelper) 
            : base("File Upload", configService)
        {
            Guard.NotNull(apiHelper, nameof(apiHelper));
            this._apiHelper = apiHelper;
            this.CoordinatorBody = HandleUploads;
        }

        #endregion

        #region public methods

        public async Task HandleUploads()
        {
            // call the ingest api to obtain details of next file to ingest
            Logger.Debug("Calling api for next file to ingest");
            var fileToIngest = await this._apiHelper.GetFileToIngest();

            if (fileToIngest != null)
            {
                Logger.DebugFormat("Attempting to ingest file {@FileToIngest}", fileToIngest);
                var uploadedInfo = await IngestFileAsync(fileToIngest);

                // TODO: need a better way of handling this....
                if (uploadedInfo > 0)
                {
                    Logger.Debug("File ingested.  Notifying api of completion");
                    await this._apiHelper.NotifyIngestComplete(fileToIngest, uploadedInfo, true);

                    Logger.Debug("Running local completion processing");
                    RunLocalCompletionProcessing(fileToIngest);
                }

                Logger.Debug("Ingest of file complete");
            }
        }

        #endregion

        #region private methods

        private async Task<long> IngestFileAsync(IngestDetail ingestDetail)
        {
            var fullPath = _configService.ReadConfigurationItem<string[]>(IngestSettingsNames.IngestFolders)[0] +  ingestDetail.GetFullPath();
            var fi = new FileInfo(fullPath);

            if (!fi.Exists)
            {
                throw new FileNotFoundException($"Failed to ingest file '{fullPath}' the file does not exist on disk.");
            }

            long uploadedBytes = 0;

            foreach (var blobEntry in ingestDetail.AssetLayout)
            {
                Logger.DebugFormat("Processing blob entry {@BlobEntry}", blobEntry);
                var uploader = new AzureBlobUpload(ingestDetail.ContainerUri, blobEntry.BlobName);
                uploader.ParallelUploadCount = _configService.ReadConfigurationItem<int>(IngestSettingsNames.AzureUploadParallelism);
                uploader.BlockCommitLimit = _configService.ReadConfigurationItem<int>(IngestSettingsNames.AzureCommitBlockCount);
                
                bool bSuccess = await uploader.Upload(
                                this._apiHelper,
                                ingestDetail,
                                (offset, length) => fi.GetFileContentAsync(offset, length),
                                fi,
                                blobEntry.Offset,
                                blobEntry.Length);
                
                if (bSuccess == true)
                {
                    uploadedBytes += blobEntry.Length;
                }
                else
                {
                    break;
                }
            }

            // send checksum file and transfer statistics
            var container = new CloudBlobContainer(new Uri(ingestDetail.ContainerUri));
            var state = StateHelper.LoadState(container.Name);

            await UploadChecksumJsonFileAsync(state, container);

            await ProcessIngestStatisticsAsync(state);

            // TODO: return the upload stats
            return uploadedBytes;
        }

        private void RunLocalCompletionProcessing(IngestDetail ingestDetail)
        {
            var ingestFolder = _configService.ReadConfigurationItem<string>(IngestSettingsNames.IngestFolders);

            var fullPath = ingestFolder + ingestDetail.GetFullPath();

            if (_configService.ReadConfigurationItem<bool>(IngestSettingsNames.DeleteFileAfterIngest, false))
            {
                
                if (File.Exists(fullPath))
                {
                    Logger.InfoFormat("Local completion processing, deleting file {0}", fullPath);
                    File.Delete(fullPath);

                    processDirectory(ingestFolder);
                }
            }
        }
        
        private async Task UploadChecksumJsonFileAsync(UploadState state, CloudBlobContainer container)
        {
            Logger.Debug("Initiating upload of checksum json file");
            var blobChecksumJson = state.BuildChecksumJson();

            await RetryHelper.ExecuteWithRetryAndBackoff(async () =>
            {
                var csblobname = container.Name + ".checkusm.json";
                var csBlob = container.GetBlockBlobReference(csblobname);
                await csBlob.UploadTextAsync(blobChecksumJson);

            }, rex => { Logger.ErrorException("Error occurred attempting to execute an operation until successful.", rex); }, Logger);

            Logger.Debug("Finished upload of checksum json file");
        }

        private async Task ProcessIngestStatisticsAsync(UploadState state)
        {
            try
            {
                var stats = IngestStaticsBuilder.CreateFromUploadState(state);
            
                Logger.Info($"Uploaded total of {stats.TotalTransferBytes} bytes in {stats.TotalTransferSeconds} seconds at {stats.TotalTransferMbps} Mbps");

                Logger.Debug("Initiating upload of statistics");

                await RetryHelper.ExecuteWithRetryAndBackoff(async () =>
                {
                    await _apiHelper.PostStatisticsAsync(stats);
                }, rex => { Logger.ErrorException("Error occurred attempting to execute an operation until successful.", rex); }, Logger);

                Logger.Debug("Finished upload of statistics");
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);
            }
        }

        private void processDirectory(string startLocation)
        {
            var ingestFolder = _configService.ReadConfigurationItem<string>(IngestSettingsNames.IngestFolders);

            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0 &&
                    Directory.GetParent(directory).FullName + "\\" != ingestFolder)
                {
                    var test = Directory.GetParent(directory).FullName;
                    Directory.Delete(directory, false);
                }
            }
        }

        #endregion
    }
}
