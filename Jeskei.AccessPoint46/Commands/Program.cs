namespace Flix.AccessPoint.Modules
{
    using System;
    using Flix.AccessPoint.Modules.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.PlatformAbstractions;
    using Microsoft.Framework.Runtime.Common.CommandLine;
    using Serilog;

    public class Program
    {
        #region properties

        public IConfiguration Configuration { get; set; }

        #endregion

        #region public methods

        public int Main(string[] args)
        {
        }

        #endregion

        #region private methods 

        private void FolderMonCommand(CommandLineApplication command)
        {
            command.Description = "Monitors a set of folders for content and changes and provides information to Flix";
            command.HelpOption("-?|-h|--help");

            command.OnExecute(() =>
            {
                var settingsBuilder = new CommandSettingsBuilder(this.Configuration);
                var settings = settingsBuilder.BuildCommandSettings();

                var cmd = new FolderMonitorCommand();
                cmd.Run(settings);
                return 0;
            });
        }

        private void AssetMonCommand(CommandLineApplication command)
        {
            command.Description = "Monitors the configured asset library and calculates checksums as required";
            command.HelpOption("-?|-h|--help");

            command.OnExecute(() =>
            {
                var settingsBuilder = new CommandSettingsBuilder(this.Configuration);
                var settings = settingsBuilder.BuildCommandSettings();

                var cmd = new AssetMonitorCommand();
                cmd.Run(settings);
                return 0;
            });
        }

        private void PulseCommand(CommandLineApplication command)
        {
            command.Description = "Sends the Flix Pulse to the central api";
            command.HelpOption("-?|-h|--help");

            command.OnExecute(() =>
            {
                var settingsBuilder = new CommandSettingsBuilder(this.Configuration);
                var settings = settingsBuilder.BuildCommandSettings();

                var cmd = new PulseCommand();
                cmd.Run(settings);
                return 0;
            });
        }

        private void ChecksumGeneratorCommand(CommandLineApplication command)
        {
            command.Description = "Calculates checksums for ingest files";
            command.HelpOption("-?|-h|--help");

            command.OnExecute(() =>
            {
                var settingsBuilder = new CommandSettingsBuilder(this.Configuration);
                var settings = settingsBuilder.BuildCommandSettings();

                var cmd = new ChecksumCommand();
                cmd.Run(settings);
                return 0;
            });
        }

        private void IngestFileCommand(CommandLineApplication command)
        {
            command.Description = "Ingests a file";
            command.HelpOption("-?|-h|--help");
            
            command.OnExecute(() =>
            {
                var settingsBuilder = new CommandSettingsBuilder(this.Configuration);
                var settings = settingsBuilder.BuildCommandSettings();

                var cmd = new IngestCommand();
                cmd.Run(settings);
                return 0;
            });
        }

        private void BuildConfiguration(params string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(PlatformServices.Default.Application.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddFlixConfigService();

            this.Configuration = builder.Build();
        }

        #endregion
    }
}