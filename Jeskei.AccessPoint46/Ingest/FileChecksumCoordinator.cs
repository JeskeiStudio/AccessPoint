namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Logging;
    using Jeskei.AccessPoint.Core;

    public class FileChecksumCoordinator
    {
        #region private fields

        private static readonly ILog Logger = LogProvider.For<FileChecksumCoordinator>();

        private readonly IConfigurationService configService;
        private readonly IngestApiHelper apiHelper;
        private CancellationTokenSource cts;
        private int sleepPeriodMs;

        #endregion

        #region constructors

        public FileChecksumCoordinator(IConfigurationService configService, IngestApiHelper apiHelper)
        {
            Guard.NotNull(configService, nameof(configService));
            Guard.NotNull(apiHelper, nameof(apiHelper));

            this.configService = configService;
            this.apiHelper = apiHelper;
            this.cts = new CancellationTokenSource();

            this.sleepPeriodMs = this.configService.ReadConfigurationItem<int>(IngestSettingsNames.FileChecksumInterval, IngestSettingsNames.DefaultFileChecksumIntervalMs);
            Logger.Info($"Sleep period set to {sleepPeriodMs}");
        }

        #endregion

        #region public methods

        public void Start()
        {
            Logger.Debug("Launching checksum task");
            new Task(() => HandleChecksums(), this.cts.Token, TaskCreationOptions.LongRunning).Start();
        }

        public void Stop()
        {
            Logger.Debug("Canceling task");
            this.cts?.Cancel();
        }

        public async Task HandleChecksums()
        {
            do
            {
                try
                {
                    var filesToChecksum = await this.apiHelper.GetFilesForChecksumCalculation();

                    foreach (var file in filesToChecksum)
                    {
                        Logger.InfoFormat("Calculating checksum for {@File}", file);

                        try
                        {
                            await CalculateFileChecksumAsync(file, this.configService.ReadConfigurationItem<string[]>(IngestSettingsNames.IngestFolders));
                            Logger.InfoFormat("Checksum calculated {@Checksum}", file.FileContentsChecksum);
                            await this.apiHelper.SetChecksumOnFile(file);
                        }
                        catch (Exception ex)
                        {
                            // TODO: what if file not found but server still thinks its present??  (should the folder monitoring pick this up??)
                    //        Logger.ErrorException("Failed to calculate checksum.", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Failed to route changes to the server", ex);
                }

                await Task.Delay(this.sleepPeriodMs);

            } while (!this.cts.IsCancellationRequested);

            Logger.Debug("Folder sync task canceled");
        }

        #endregion

        #region private methods

        private static async Task<IngestFileInfoDto> CalculateFileChecksumAsync(IngestFileInfoDto ingestFileInfo, string[] directories)
        {
            // TODO: What if there are multiple directories?
            var file = Path.Combine(directories[0] + ingestFileInfo.Path, ingestFileInfo.FileName);

            if (!File.Exists(file))
            {
                throw new FileNotFoundException("File not found. Cannot calculate checksum for file.", file);
            }

            var hash = await Task.Run(() => HashHelper.SHA1FromFile(file));

            ingestFileInfo.FileContentsChecksum = hash;

            return ingestFileInfo;
        }

        #endregion
    }
}
