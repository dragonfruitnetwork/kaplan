// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using Avalonia.Controls;
using Avalonia.Threading;
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
            if (DataContext is ICanCloseWindow windowCloser)
            {
                windowCloser.CloseRequested += () => Dispatcher.UIThread.InvokeAsync(Close);
            }

            (DataContext as IExecutesTaskPostLoad)?.Perform();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            (DataContext as IHandlesClosingEvent)?.OnClose(e);
        }
    }
}