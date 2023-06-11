// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

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
        }

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