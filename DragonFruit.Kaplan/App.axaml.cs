// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DragonFruit.Kaplan.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace DragonFruit.Kaplan
{
    public partial class App : Application
    {
        public static App Instance => (App)Current;

        private ILoggerFactory Logger { get; set; }

        public bool BugReportingEnabled { get; set; } = true;

        public static ILogger GetLogger<T>()
        {
            return Instance.Logger.CreateLogger<T>();
        }

        public override void Initialize()
        {
            Logger = LoggerFactory.Create(o =>
            {
                o.ClearProviders();

                o.AddEventLog(new EventLogSettings
                {
                    SourceName = Program.AppTitle,
                    Filter = (_, level) => level is LogLevel.Warning or LogLevel.Error or LogLevel.Critical
                });

                o.AddSentry(s =>
                {
                    s.Release = Program.Version;
                    s.Dsn = "https://7818b48e8de14c9f8b1e32df36e7417b@o97031.ingest.sentry.io/4505465657294848";

                    s.MaxBreadcrumbs = 200;
                    s.MinimumEventLevel = LogLevel.Warning;

                    s.SetBeforeSend(e => BugReportingEnabled && typeof(Program).Assembly.GetName().Version?.Major > 1 ? e : null);
                });
            });

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new Welcome();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}