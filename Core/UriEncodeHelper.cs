namespace Jeskei.AccessPoint.Core
{
    using System.Net;

    public static class UriEncodeHelper
    {
        public static string EncodeUriPart(string par)
        {
            var encoded = par
                .Replace("+", "-")
                .Replace("/", "_");

            encoded = WebUtility.UrlEncode(encoded);

            return encoded;
        }

        public static string DecodeUriPart(string par)
        {
            // no need to urldecode webapi has url-decoded the parameters by the time they are routed to the controller 
            var decoded = par
                .Replace("-", "+")
                .Replace("_", "/");

            return decoded;
        }
    }
}
