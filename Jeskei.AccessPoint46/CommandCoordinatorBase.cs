namespace Jeskei.AccessPoint.Modules
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Logging;

    public abstract class CommandCoordinatorBase<T>
    {
        #region private fields

        private static readonly TimeSpan DefaultSleepIntervalTimeSpan = TimeSpan.FromSeconds(60);

        protected static readonly ILog Logger = LogProvider.For<T>();
        protected string _coordinatorName;
        protected IConfigurationService _configService;

        private CancellationTokenSource _cts;
        private TimeSpan _sleepPeriod;

        #endregion

        #region constructors

        public CommandCoordinatorBase(string coordinatorName, IConfigurationService configService)
        {
            Guard.NotNullOrEmpty(coordinatorName, nameof(coordinatorName));
            Guard.NotNull(configService, nameof(configService));

            _coordinatorName = coordinatorName;
            _configService = configService;
            _cts = new CancellationTokenSource();

            this._sleepPeriod = this._configService.ReadConfigurationItem<TimeSpan>(BuildConfigItemName("sleepInterval"), DefaultSleepIntervalTimeSpan);
            Logger.Info($"Sleep period set to {this._sleepPeriod}");
        }

        #endregion

        public Func<Task> CoordinatorBody { get; set; }

        #region public methods

        public string GetCoordinatorConfigurationValue(string itemName)
        {
            return GetCoordinatorConfigurationValue<string>(itemName);
        }

        public T GetCoordinatorConfigurationValue<T>(string itemName, T defaultValue = default(T))
        {
            Guard.NotNullOrEmpty(itemName, nameof(itemName));
            var fullItemName = BuildConfigItemName(itemName);
            var value = _configService.ReadConfigurationItem<T>(fullItemName);
            return value;
        }

        public virtual void Start()
        {
            Logger.Debug($"Starting {_coordinatorName} coordinator; launching task");
            new Task(() => CoordinatorTask(), _cts.Token, TaskCreationOptions.LongRunning).Start();
        }

        public virtual void Stop()
        {
            Logger.Debug($"Stopping {_coordinatorName} coordinator; canceling task");
            _cts?.Cancel();
        }

        #endregion

        #region private methods

        private async Task CoordinatorTask()
        {
            do
            {
                try
                {
                    await CoordinatorBody();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException($"Failed in {_coordinatorName} coordinator", ex);
                }

                await Task.Delay(_sleepPeriod);

            } while (!_cts.IsCancellationRequested);
        }

        private string BuildConfigItemName(string configItem)
        {
            var configKeyName = _coordinatorName
                .RemoveSpaces()
                .ToCamelCase();
            
            return $"{configKeyName}.{configItem}";
        }

        #endregion
    }
}
