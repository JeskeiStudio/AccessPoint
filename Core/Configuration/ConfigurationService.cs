namespace Jeskei.AccessPoint.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ConfigurationService : IConfigurationService
    {
        #region private fields

        private IDictionary<string, object> configuration;

        #endregion

        #region configuration

        public ConfigurationService(IDictionary<string, object> configuration)
        {
            Guard.NotNull(configuration, nameof(configuration));
            this.configuration = configuration;
        }

        #endregion

        #region public methods

        public bool TryValidateConfiguration(string[] expectedKeys, out string[] missingKeys)
        {
            missingKeys = new string[0];

            if (expectedKeys == null)
                return false;

            missingKeys = expectedKeys
                .Except(this.configuration.Keys)
                .ToArray();

            return missingKeys.Count() == 0;
        }

        public T ReadConfigurationItem<T>(string key, T defaultValue = default(T))
        {
            Guard.NotNull(key, nameof(key));

            if (!configuration.ContainsKey(key))
                return defaultValue;

            var value = configuration[key];

            if (value == null)
                return defaultValue;

            try
            {
                return CastAs<T>(value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to read the configuration setting Key:'{key}'", ex);
            }
        }

        #endregion

        #region private methods

        private T CastAs<T>(object value)
        {
            // coerce a string into a string[] if required
            if (typeof(T) == typeof(string[]) && value is string)
                value = new string[] { value as string };

            if (typeof(T) == typeof(TimeSpan) && value is string)
                value = TimeSpan.Parse((string)value);

            return (T)Convert.ChangeType(value, typeof(T));
        }

        #endregion
    }
}
