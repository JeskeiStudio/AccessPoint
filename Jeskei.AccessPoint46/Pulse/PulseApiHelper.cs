namespace Jeskei.AccessPoint.Ingest.Pulse
{
    using System.Threading.Tasks;
    using Core;
    using Modules;

    public class PulseApiHelper : ApiHelperBase<PulseApiHelper>
    {
        #region constructors

        public PulseApiHelper()
        {

        }

        public PulseApiHelper(string baseUri, string id) : base(baseUri, id)
        {
        }

        #endregion

        #region public methods

        public async Task PostPulse(string pulse)
        {
            Guard.NotNull(pulse, nameof(pulse));

            var content = GetRequestContentAsXml(pulse);
            var requestUri = BuildPulsePostUri();

            using (var client = BuildHttpClient())
            {
                var response = await client.PostAsync(requestUri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        #endregion

        #region private methods

        private string BuildPulsePostUri()
        {
            return $"{this._baseUri}/pulse/{this._id}";
        }

        #endregion
    }
}
