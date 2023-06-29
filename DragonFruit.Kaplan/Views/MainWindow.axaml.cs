// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using DragonFruit.Kaplan.ViewModels;
using DragonFruit.Kaplan.ViewModels.Messages;
using FluentAvalonia.UI.Windowing;
using ReactiveUI;

namespace DragonFruit.Kaplan.Views
{
    public partial class MainWindow : AppWindow
    {
        private readonly IEnumerable<IDisposable> _messageListeners;

        public MainWindow()
        {
            InitializeComponent();

            TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

            TransparencyLevelHint = new[]
            {
                WindowTransparencyLevel.Mica,
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.None
            };

            _messageListeners = new[]
            {
                MessageBus.Current.Listen<UninstallEventArgs>().Subscribe(OpenProgressDialog),
                MessageBus.Current.Listen<ShowAboutWindowEventArgs>().Subscribe(_ => new About().ShowDialog(this))
            };
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            foreach (var messageListener in _messageListeners)
            {
                messageListener.Dispose();
            }
        }
    }
}