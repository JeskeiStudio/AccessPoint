namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Core;
    public static class IngestExtensions
    {
        public static async Task<byte[]> GetFileContentAsync(this FileInfo file, long offset, long length)
        {
            var remainder = new byte[0];

            try
            {
                using (var fs = file.OpenRead())
                {
                    fs.Seek(offset, SeekOrigin.Begin);

                    var contents = new byte[length];
                    var bytesRead = await fs.ReadAsync(contents, 0, contents.Length);

                    if (bytesRead == length)
                        return contents;

                    remainder = new byte[bytesRead];
                    Array.Copy(contents, remainder, bytesRead);

                    return remainder;
                }
            }
            catch (Exception e)
            {
                remainder = null;
                return remainder;
            }
        }

        public static async Task<string> ComputeMD5ChecksumAsync(this FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                var result = await HashHelper.MD5FromStreamAsync(fs);
                return result;
            }
        }
    }
}
