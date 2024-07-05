// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using DragonFruit.Kaplan.ViewModels;
using DragonFruit.Kaplan.ViewModels.Messages;
using FluentAvalonia.UI.Windowing;
using ReactiveUI;

namespace DragonFruit.Kaplan.Views
{
    public partial class MainWindow : ReactiveAppWindow<MainWindowViewModel>
    {
        private readonly IEnumerable<IDisposable> _messageListeners;

        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(action => action(ViewModel!.BeginRemovalInteraction.RegisterHandler(OpenProgressDialog)));

            TransparencyLevelHint = Program.TransparencyLevels;

            TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

            _messageListeners = new[]
            {
                MessageBus.Current.Listen<ShowAboutWindowEventArgs>().Subscribe(_ => new About().ShowDialog(this))
            };
        }

        private async Task OpenProgressDialog(InteractionContext<RemovalProgressViewModel, PackageRemover.OperationState> args)
        {
            var window = new RemovalProgress
            {
                DataContext = args.Input
            };

            await window.ShowDialog(this);
            args.SetOutput(args.Input.CurrentState);
        }

        private void PackageListPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name != nameof(ListBox.ItemsSource))
            {
                return;
            }

            // when the item source changes, scroll to the top
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