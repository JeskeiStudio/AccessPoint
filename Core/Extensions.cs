namespace Jeskei.AccessPoint.Core
{
    using System.Collections.Generic;
    using System.Text;

    public static class Extensions
    {
        public static string AggregateIntoString(this IEnumerable<string> values, string separator = ",")
        {
            var sb = new StringBuilder();

            foreach (var s in values)
            {
                sb.Append(s);
                sb.Append(separator);
            }

            sb.Remove(sb.Length-1, 1);

            return sb.ToString();
        }
    }
}
