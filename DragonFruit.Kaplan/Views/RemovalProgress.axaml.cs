// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.ComponentModel;
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
            (DataContext as IExecutesTaskPostLoad)?.Perform();

            if (DataContext is ICanCloseWindow windowCloser)
            {
                windowCloser.CloseRequested += () => Dispatcher.UIThread.InvokeAsync(Close);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            (DataContext as IHandlesClosingEvent)?.OnClose(e);
        }
    }
}