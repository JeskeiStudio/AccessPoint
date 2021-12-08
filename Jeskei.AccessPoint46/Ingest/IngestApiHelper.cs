namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Core.Logging;
    using Jeskei.AccessPoint.Core;
    using Newtonsoft.Json;

    public class IngestApiHelper : ApiHelperBase<IngestApiHelper>
    {
        #region constructors

        public IngestApiHelper()
        {
        }

        public IngestApiHelper(string baseUri, string id) : base(baseUri, id)
        {
        }

        #endregion

        #region public methods

        public async Task PostFullFileListAsync(List<FileEntryChange> clientFiles)
        {
            Guard.NotNull(clientFiles, nameof(clientFiles));

            if (clientFiles.Count == 0)
                return;

            var dto = DtoConverter.ConvertToDto(clientFiles);
            var content = GetRequestContentAsJson(dto);
            var requestUri = BuildIngestFilePostAllUri();

            using (var client = BuildHttpClient())
            {
                var response = await client.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task PutIngestFileAsync(FileEntryChange fileEntry)
        {
            Guard.NotNull(fileEntry, nameof(fileEntry));

            await PutIngestFileAsync(new List<FileEntryChange> { fileEntry });
        }

        public async Task PutIngestFileAsync(List<FileEntryChange> fileEntries)
        {
            Guard.NotNull(fileEntries, nameof(fileEntries));
            fileEntries.ForEach(a => Guard.NotNull(a.HashId, nameof(a.HashId)));

            var dto = DtoConverter.ConvertToDto(fileEntries);

            var content = GetRequestContentAsJson(dto);
            var requestUri = BuildIngestFilePostUri();

            using (var client = BuildHttpClient())
            {
                var response = await client.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task PostIngestFileAsync(FileEntryChange fileEntry)
        {
            Guard.NotNull(fileEntry, nameof(fileEntry));

            await PostIngestFileAsync(new List<FileEntryChange> { fileEntry });
        }

        public async Task PostIngestFileAsync(List<FileEntryChange> fileEntries)
        {
            Guard.NotNull(fileEntries, nameof(fileEntries));
            fileEntries.ForEach(a => Guard.NotNull(a.HashId, nameof(a.HashId)));

            var dto = DtoConverter.ConvertToDto(fileEntries);

            var singleUpdate = dto.Count == 1;
            var content = singleUpdate ? GetRequestContentAsJson(dto[0]) : GetRequestContentAsJson(dto);
            var requestUri = singleUpdate ? BuildIngestFilePostUri(dto[0].FileNameHash) : BuildIngestFilePostUri();

            using (var client = BuildHttpClient())
            {
                var response = await client.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task DeleteIngestFileAsync(FileEntryChange fileEntry)
        {
            Guard.NotNull(fileEntry, nameof(fileEntry));
            Guard.NotNull(fileEntry.HashId, nameof(fileEntry.HashId));

            var requestUri = BuildIngestFileDeleteUri(fileEntry.HashId);

            using (var client = BuildHttpClient())
            {
                var response = await client.DeleteAsync(requestUri);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<List<IngestFileInfoDto>> GetFilesForChecksumCalculation()
        {
            var requestUri = BuildFileChecksumUri();

            var files = new List<IngestFileInfoDto>();

            using (var client = BuildHttpClient())
            {

                Logger.Debug($"Calling api for checksums ({requestUri})");

                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    Logger.Debug($"Response content {content}");

                    if (!String.IsNullOrWhiteSpace(content))
                    {
                        files = JsonConvert.DeserializeObject<List<IngestFileInfoDto>>(content);
                    }
                }
            }

            return files;
        }

        public async Task SetChecksumOnFile(IngestFileInfoDto file)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(file.FileContentsChecksum, nameof(file.FileContentsChecksum));

            var requestUri = BuildIngestFileChecksumPutUri(file.FileNameHash, file.FileContentsChecksum);

            using (var client = BuildHttpClient())
            {
                Logger.Debug($"Calling api to set checksum value ({requestUri})");
                var response = await client.PutAsync(requestUri, null);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IngestDetail> GetFileToIngest()
        {
            var requestUri = BuildIngestFileGetUri();

            IngestDetail ingestDetail = null;
            
            using (var client = BuildHttpClient(requestUri))
            {
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (!String.IsNullOrWhiteSpace(content))
                    {
                        ingestDetail = JsonConvert.DeserializeObject<IngestDetail>(content);
                    }
                }
            }

            return ingestDetail;
        }

        public async Task NotifyIngestComplete(IngestDetail ingestDetail, long bytesUploaded, bool final)
        {
            var requestUri = BuildIngestCompleteUri(ingestDetail.FileNameHash, bytesUploaded, final);

            using (var client = BuildHttpClient())
            {
                var response = await client.PostAsync(requestUri, null);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task PostStatisticsAsync(IngestStatistics stats)
        {
            var requestUri = BuildIngestStatisticsUri(stats.ContainerName);

            using (var client = BuildHttpClient())
            {
                var content = GetRequestContentAsJson(stats);

                var response = await client.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        #endregion

        #region private methods


        private string BuildIngestFilePostAllUri()
        {
            return $"{_baseUri}/ingest/{_id}/deltas";
        }

        private string BuildIngestFilePostUri()
        {
            return $"{_baseUri}/ingest/{_id}/deltas";
        }

        private string BuildIngestFileDeleteUri(string fileId)
        {
            var encodedFileId = UriEncodeHelper.EncodeUriPart(fileId);
            return $"{_baseUri}/ingest/{_id}/files/{encodedFileId}";
        }

        private string BuildIngestFileChecksumPutUri(string fileId, string checksum)
        {
            var encodedFileId = UriEncodeHelper.EncodeUriPart(fileId);
            var encodedChecksum = UriEncodeHelper.EncodeUriPart(checksum);

            return $"{_baseUri}/ingest/{_id}/files/{encodedFileId}/{encodedChecksum}";
        }

        private string BuildIngestFileGetUri()
        {
            return $"{_baseUri}/ingest/{_id}/files/ingest";
        }

        private string BuildFileChecksumUri()
        {
            return $"{_baseUri}/ingest/{_id}/files/checksums";
        }

        private string BuildIngestFilePostUri(string fileId)
        {
            return $"{_baseUri}/ingest/{_id}/files/{fileId}";
        }

        private string BuildIngestCompleteUri(string fileId, long bytesUploaded, bool final)
        {
            var encodedFileId = UriEncodeHelper.EncodeUriPart(fileId);

            return $"{_baseUri}/ingest/{_id}/files/ingest/{encodedFileId}/{bytesUploaded}/{final}";
        }

        private string BuildIngestStatisticsUri(string containerName)
        {
            return $"{_baseUri}/ingest/{_id}/statistics/{containerName}";
        }

        #endregion
    }
}
