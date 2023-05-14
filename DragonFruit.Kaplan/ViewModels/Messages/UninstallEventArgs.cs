// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using Windows.ApplicationModel;
using DragonFruit.Kaplan.ViewModels.Enums;

namespace DragonFruit.Kaplan.ViewModels.Messages
{
    public class UninstallEventArgs
    {
        public UninstallEventArgs(IEnumerable<Package> packages, PackageInstallationMode mode)
        {
            Packages = packages;
            Mode = mode;
        }

        public IEnumerable<Package> Packages { get; }
        public PackageInstallationMode Mode { get; }
    }
}