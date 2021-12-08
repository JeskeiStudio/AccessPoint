namespace Jeskei.AccessPoint.Core
{
    using System;
    using System.Threading.Tasks;
    using Jeskei.AccessPoint.Core.Logging;

    public static class RetryHelper
    {
        public static async Task ExecuteWithRetryAndBackoff(Func<Task> action, Action<Exception> exceptionHandler, ILog logger = null, int maxAttempts = 8, int baseRetryIntervalMs = 500)
        {
            int retryInterval = baseRetryIntervalMs;

            bool success = false;
            while (!success)
            {
                try
                {
                    await action();
                    success = false;
                    break;
                }
                catch (Exception ex)
                {
                    if (exceptionHandler != null) { exceptionHandler(ex); }
                }

                logger?.Debug($"Call failed, waiting for {retryInterval} ms");

                await Task.Delay(retryInterval);

                retryInterval *= 2;

                logger?.Debug($"Increasing wait time to {retryInterval} ms");
            }
        }
    }
}
