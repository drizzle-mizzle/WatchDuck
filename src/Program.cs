using NLog;
using WatchDuck.Exceptions;
using WatchDuck.Helpers;

namespace WatchDuck
{
    internal static class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();


        private static async Task Main(string[] args)
        {
            var nlogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "NLog.config");
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlogPath);

            _log.Info("[ Starting WatchDuck ]");

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                _log.Info("[ WatchDuck Stopped ]");
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                _log.Error($"Unhandled exception: {sender}\n{e.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                if (e.Exception.InnerException is UserFriendlyException)
                {
                    return;
                }

                _log.Error($"Unobserved task exception: {sender}\n{e.Exception}");
            };

            BotConfig.Initialize();
            await WatchDuckBot.RunAsync();
        }

    }
}
