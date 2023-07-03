// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using DragonFruit.Kaplan.ViewModels;

namespace DragonFruit.Kaplan.Views
{
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool ShowCompatabilityMessage => !WindowsVersionCompatible;
        public bool WindowsVersionCompatible => Environment.OSVersion.Version >= Program.MinWindowsVersion;

        public string CompatabilityErrorMessage => $"Windows {Program.MinWindowsVersion.ToString()} is required to use this program. This machine is currently running version {Environment.OSVersion.Version.ToString()}";

        private async void OpenAboutDialog(object sender, RoutedEventArgs e)
        {
            await new About().ShowDialog(this).ConfigureAwait(false);
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            window.Show();

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = window;
            }

            Close();
        }
    }
}