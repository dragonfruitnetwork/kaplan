// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using DragonFruit.Kaplan.ViewModels;
using FluentAvalonia.UI.Windowing;
using ReactiveUI;

namespace DragonFruit.Kaplan.Views
{
    public partial class MainWindow : ReactiveAppWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            TransparencyLevelHint = Program.TransparencyLevels;

            TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

            this.WhenActivated(action => action(ViewModel!.AboutPageInteraction.RegisterHandler(OpenAboutPage)));
            this.WhenActivated(action => action(ViewModel!.BeginRemovalInteraction.RegisterHandler(OpenProgressDialog)));
        }

        private async Task OpenAboutPage(InteractionContext<Unit, Unit> ctx)
        {
            await new About().ShowDialog(this).ConfigureAwait(false);

            ctx.SetOutput(Unit.Default);
        }

        private async Task OpenProgressDialog(InteractionContext<RemovalProgressViewModel, PackageRemover.OperationState> ctx)
        {
            var window = new RemovalProgress
            {
                DataContext = ctx.Input
            };

            await window.ShowDialog(this);
            ctx.SetOutput(ctx.Input.CurrentState);
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
    }
}