﻿// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace DragonFruit.Kaplan
{
    internal class Program
    {
        internal static Version MinWindowsVersion = new("10.0.19041");

        public static string Version { get; } = typeof(Program).Assembly.GetName().Version!.ToString(3);
        public static string AppTitle { get; } = $"DragonFruit Kaplan v{Version}";

        internal static WindowTransparencyLevel[] TransparencyLevels =
        {
            WindowTransparencyLevel.Mica,
            WindowTransparencyLevel.AcrylicBlur,
            WindowTransparencyLevel.None
        };

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}