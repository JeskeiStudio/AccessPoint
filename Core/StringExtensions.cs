namespace Jeskei.AccessPoint.Core
{
    using System;
    using System.Text;

    public static class StringExtensions
    {
        public static string RemoveSpaces(this string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return value;

            return value.Replace(" ", String.Empty);
        }
        public static string ToCamelCase(this string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return value;

            return (value.Length > 1) ? Char.ToLowerInvariant(value[0]) + value.Substring(1) : value;
        }
    }
}
