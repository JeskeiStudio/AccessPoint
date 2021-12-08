namespace Jeskei.AccessPoint.Core
{
    public interface IConfigurationService
    {
        bool TryValidateConfiguration(string[] expectedKeys, out string[] missingKeys);
        T ReadConfigurationItem<T>(string key, T defaultValue = default(T));
    }
}
