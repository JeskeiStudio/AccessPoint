namespace Jeskei.AccessPoint.Core
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    public static class HashHelper
    {
        public static string SHA1FromString(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException(nameof(data), "The data to hash cannot be null or empty.");

            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        public static string SHA1FromFile(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path), "The path to hash cannot be null or empty.");

            const int bufferSize = 8192;

            // TODO: create an interruptable stream??
            // TODO: should user be able to delete file while checksum being calculated?
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            using (var sha1 = SHA1.Create())
            {
                fs.Position = 0;
                var hash = sha1.ComputeHash(fs);
                return Convert.ToBase64String(hash);
            }
        }

        public static string MD5FromString(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                throw new ArgumentNullException(nameof(data), "The data to hash cannot be null or empty.");

            return MD5FromBytes(Encoding.UTF8.GetBytes(data));
        }

        public static string MD5FromBytes(byte[] data)
        {
            Guard.NotNull(data, nameof(data));

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }

        public static string MD5FromFile(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path), "The path to hash cannot be null or empty.");

            const int bufferSize = 8192;

            // TODO: create an interruptable stream??
            // TODO: should user be able to delete file while checksum being calculated?
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(fs);
                return Convert.ToBase64String(hash);
            }
        }

        public static async Task<string> MD5FromStreamAsync(Stream stream)
        {
            var task = Task.Factory.StartNew<string>(() =>
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(stream);
                    return Convert.ToBase64String(hash);
                }
            });

            return await task;
        }
    }
}
