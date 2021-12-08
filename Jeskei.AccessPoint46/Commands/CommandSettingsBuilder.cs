namespace Flix.AccessPoint.Modules
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public class CommandSettingsBuilder
    {
        #region private fields

        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, object> _settings;

        #endregion

        #region constructors

        public CommandSettingsBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
            _settings = new Dictionary<string, object>();

            foreach (var child in configuration.GetChildren())
            {
                _settings.Add(child.Key, child.Value);    
            }
        }

        #endregion

        #region public methods

        public IDictionary<string, object> BuildCommandSettings()
        {
            return _settings;
        }

        public void AddValueFromConfiguration(string name)
        {
            var value = _configuration[name];
            _settings.Add(name, value);
        }

        #endregion

    }
}
