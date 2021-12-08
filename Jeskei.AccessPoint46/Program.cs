using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Jeskei.AccessPoint.Modules;
using System.Configuration;
using Jeskei.AccessPoint.Modules.Configuration;
using System.Threading;
using Jeskei.AccessPoint.Core;
using Jeskei.AccessPoint.Modules.Ingest;

namespace Jeskei.AccessPoint46
{
    class Program
    {
        public static Dictionary<string, object> Configuration { get; set; }

        private static FolderMonitorCommand foldermonProcessor = null;
        private static ChecksumCommand checksumdProcessor = null;
        private static IngestCommand ingestProcessor = null;
        private static AssetMonitorCommand assetProcessor = null;
        private static PulseCommand pulseProcessor = null;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.TextWriter(Console.Out)
                .CreateLogger();

            BuildConfiguration(args);

            bool bIngest = false;
            bool bFolderMon = false;
            bool bChecksum = false;
            bool bAssetMon = false;
            bool bPulse = false;

            if (!string.IsNullOrEmpty(args.SingleOrDefault(arg => arg.StartsWith("ingestfiles"))))
            {
                bIngest = true;
            }

            if (!string.IsNullOrEmpty(args.SingleOrDefault(arg => arg.StartsWith("foldermon"))))
            {
                bFolderMon = true;
            }

            if (!string.IsNullOrEmpty(args.SingleOrDefault(arg => arg.StartsWith("checksum"))))
            {
                bChecksum = true;
            }

            if (!string.IsNullOrEmpty(args.SingleOrDefault(arg => arg.StartsWith("assetmon"))))
            {
                bAssetMon = true;
            }

            if (!string.IsNullOrEmpty(args.SingleOrDefault(arg => arg.StartsWith("pulse"))))
            {
                bPulse = true;
            }

            if (args.Count() == 0)
            {
                bIngest = bFolderMon = bChecksum = true;
            }

            bool bChecksumRun = true;
            var config = new ConfigurationService(Configuration);
            
            if (bFolderMon)
            {
                FolderMonCommand();
            }

            if (bChecksum &&
                bChecksumRun)
            {
                ChecksumGeneratorCommand();
                bChecksumRun = false;
            }

            if (bIngest)
            {
                IngestFileCommand();
            }
            
            if (bAssetMon)
            {
                AssetMonCommand();
            }

            if (bPulse)
            {
                PulseCommand();
            }

            Console.ReadLine();
            if (foldermonProcessor != null)
            {
                foldermonProcessor.Stop();
                Console.Write("Stopped foldermon");
            }
            if (checksumdProcessor != null)
            {
                checksumdProcessor.Stop();
                Console.Write("Stopped checksumd");
            }
            if (ingestProcessor != null)
            {
                ingestProcessor.Stop();
                Console.Write("Stopped ingest");
            }
        }

        #region private methods 

        private static void FolderMonCommand()
        {
            foldermonProcessor = new FolderMonitorCommand();

            new Thread(delegate ()
           {
               foldermonProcessor.Run(Configuration);
           }).Start();
        }

        private static void AssetMonCommand()
        {
            assetProcessor = new AssetMonitorCommand();

            new Thread(delegate ()
            {
                assetProcessor.Run(Configuration);
            }).Start();
        }

        private static void PulseCommand()
        {
            pulseProcessor = new PulseCommand();

            new Thread(delegate ()
            {
                pulseProcessor.Run(Configuration);
            }).Start();
        }

        private static void ChecksumGeneratorCommand()
        {
            checksumdProcessor = new ChecksumCommand();
            new Thread(delegate ()
            {
                checksumdProcessor.Run(Configuration);
            }).Start();
        }

        private static void IngestFileCommand()
        {
            ingestProcessor = new IngestCommand();
            new Thread(delegate ()
            {
                ingestProcessor.Run(Configuration);
            }).Start();
        }

        private static void BuildConfiguration(params string[] args)
        {
            Configuration = LocalConfigSettingHelper.GetValues();
        }

        #endregion
    }
}
