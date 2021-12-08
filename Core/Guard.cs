namespace Jeskei.AccessPoint.Core
{
    using System;

    public static class Guard
    {
        public static void NotNull<T>(T value, string name) where T : class
       {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        public static void NotNullOrEmpty(string value, string name)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Parameter {name} cannot be null or empty");
        }
    }
}
