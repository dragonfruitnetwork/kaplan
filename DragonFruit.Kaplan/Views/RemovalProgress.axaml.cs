using System;
using System.ComponentModel;
using Avalonia.Controls;
using DragonFruit.Kaplan.ViewModels;

namespace DragonFruit.Kaplan.Views
{
    public partial class RemovalProgress : Window
    {
        public RemovalProgress()
        {
            InitializeComponent();
        }
        
        protected override void OnOpened(EventArgs e)
        {
            (DataContext as IExecutesTaskPostLoad)?.Perform();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as IHandlesClosingEvent)?.OnClose(e);
        }
    }
}