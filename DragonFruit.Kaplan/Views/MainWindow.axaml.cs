using System;
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

        private void OpenProgressDialog(UninstallEventArgs args)
        {
            var window = new RemovalProgress
            {
                DataContext = new RemovalProgressViewModel(args.Packages)
            };

            window.ShowDialog(this);
            
            MessageBus.Current.SendMessage(new PackageRefreshEventArgs());
        }
    }
}