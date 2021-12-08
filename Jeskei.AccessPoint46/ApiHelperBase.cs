namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Core.Logging;
    using Jeskei.AccessPoint.Core;
    using Newtonsoft.Json;

    public abstract class ApiHelperBase<T>
    {
        #region fields

        internal protected static readonly ILog Logger = LogProvider.For<T>();

        protected Uri _baseUri;
        protected string _id;

        #endregion

        #region constructors

        public ApiHelperBase()
        {

        }

        public ApiHelperBase(string baseUri, string id)
        {
            Init(baseUri, id);
        }

        #endregion

        #region properties

        public Func<HttpMessageHandler> BuildMessageHandler { get; set; }

        internal protected Func<AuthenticationHeaderValue> BuildAuthHeader { get; set; }

        #endregion

        #region methods

        public void Init(string baseUri, string id)
        {
            Guard.NotNull(baseUri, nameof(baseUri));
            Guard.NotNull(id, nameof(id));

            _baseUri = new Uri(baseUri);
            _id = id;

            BuildMessageHandler = () => { return new HttpClientHandler(); };
            BuildAuthHeader = () => { return null; };
        }

        protected HttpClient BuildHttpClient()
        {
            var messageHandler = this.BuildMessageHandler();

            var httpClient = new HttpClient(messageHandler);

            var authHeader = BuildAuthHeader();
            if (authHeader != null)
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

            return httpClient;
        }

        protected HttpClient BuildHttpClient(string requestUri)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(requestUri)
            };

            var authHeader = BuildAuthHeader();
            if (authHeader != null)
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

            return httpClient;
        }

        protected StringContent GetRequestContentAsJson(object value)
        {
            var jsonBody = JsonConvert.SerializeObject(value, Formatting.Indented);
            return new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        protected StringContent GetRequestContentAsXml(string value)
        {
            return new StringContent(value, Encoding.UTF8, "application/xml");
        }

        #endregion
    }
}
