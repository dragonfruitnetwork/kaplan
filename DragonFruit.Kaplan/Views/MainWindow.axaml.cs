// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using Avalonia;
using Avalonia.Controls;
using DragonFruit.Kaplan.ViewModels;
using DragonFruit.Kaplan.ViewModels.Messages;
using ReactiveUI;

namespace DragonFruit.Kaplan.Views
{
    public partial class MainWindow : Window
    {
        private readonly IDisposable _uninstallMessageListener;

        public MainWindow()
        {
            InitializeComponent();

            _uninstallMessageListener = MessageBus.Current.Listen<UninstallEventArgs>().Subscribe(OpenProgressDialog);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _uninstallMessageListener.Dispose();
        }

        private async void OpenProgressDialog(UninstallEventArgs args)
        {
            var window = new RemovalProgress
            {
                DataContext = new RemovalProgressViewModel(args.Packages, args.Mode)
            };

            await window.ShowDialog(this).ConfigureAwait(false);
            MessageBus.Current.SendMessage(new PackageRefreshEventArgs());
        }

        private void PackageListPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name != nameof(ListBox.ItemsSource))
            {
                return;
            }

            // when the item source changes
            if (sender is ListBox box && box.Scroll != null)
            {
                box.Scroll.Offset = Vector.Zero;
            }
        }
    }
}