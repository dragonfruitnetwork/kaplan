// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DragonFruit.Kaplan.Views;

namespace DragonFruit.Kaplan
{
    public partial class App : Application
    {
        public bool BugReportingEnabled { get; set; } = true;

        public static App Instance => (App)Current;

        public override void Initialize()
        {
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