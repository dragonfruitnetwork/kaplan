// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.IO;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using SukiUI.Controls;

namespace DragonFruit.Kaplan.Views
{
    public partial class About : SukiWindow
    {
        public About()
        {
            InitializeComponent();
        }

        private async void OnLoad(object sender, RoutedEventArgs e)
        {
            using var licenseFileStream = AssetLoader.Open(new Uri("avares://DragonFruit.Kaplan/Assets/licences.txt"));
            using var streamReader = new StreamReader(licenseFileStream);

            var contents = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            Dispatcher.UIThread.InvokeAsync(() => LicenseContents.Text = contents);
        }
    }
}