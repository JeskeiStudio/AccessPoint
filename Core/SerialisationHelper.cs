namespace Jeskei.AccessPoint.Core
{
    using Newtonsoft.Json;

    public static class SerialisationHelper
    {
        public static string ToJson<T>(T objectToSerialize) where T : class
        {
            Guard.NotNull(objectToSerialize, nameof(objectToSerialize));

            var serialised = JsonConvert.SerializeObject(objectToSerialize, Formatting.None);
            return serialised;
        }

        public static T FromJson<T>(string jsonContent)
        {
            Guard.NotNullOrEmpty(jsonContent, nameof(jsonContent));

            var deserialized = JsonConvert.DeserializeObject<T>(jsonContent);
            return deserialized;
        }
    }
}
