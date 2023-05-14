// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace DragonFruit.Kaplan.ViewModels
{
    /// <summary>
    /// A wrapper for a <see cref="Package"/>, exposing the Logo as a usable property to the UI.
    /// </summary>
    public class PackageViewModel : ReactiveObject
    {
        private IImage _logo;
        private Task _logoLoadTask;

        public PackageViewModel(Package package)
        {
            Package = package;
        }

        public Package Package { get; }

        public IImage Logo
        {
            get
            {
                // defer image loading until someone requests it.
                _logoLoadTask ??= LoadIconStream();

                return _logo;
            }
            private set => this.RaiseAndSetIfChanged(ref _logo, value);
        }

        public string Id => Package.Id.Name;
        public string Name => Package.DisplayName;
        public string Publisher => Package.PublisherDisplayName;

        public bool IsSearchMatch(string query)
        {
            return Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                   Publisher.Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        private async Task LoadIconStream()
        {
            var loader = Package.GetLogoAsRandomAccessStreamReference(new Size(64, 64));
            using var proxy = await loader.OpenReadAsync();

            Logo = new Bitmap(proxy.AsStreamForRead());
        }
    }
}