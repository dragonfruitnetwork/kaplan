﻿// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using DragonFruit.Kaplan.ViewModels.Enums;
using ReactiveUI;

namespace DragonFruit.Kaplan.ViewModels
{
    public class PackageRemovalTask : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string> _statusString;
        private readonly PackageInstallationMode _mode;
        private readonly PackageManager _manager;

        private DeploymentProgress? _progress;

        public PackageRemovalTask(PackageManager manager, Package package, PackageInstallationMode mode)
        {
            Package = new PackageViewModel(package);

            _mode = mode;
            _manager = manager;
            _statusString = this.WhenAnyValue(x => x.Progress)
                .Select(x => x?.state switch
                {
                    DeploymentProgressState.Queued => $"Removing {Package.Name}: Pending",
                    DeploymentProgressState.Processing when x.Value.percentage == 100 => $"Removing {Package.Name} Complete",
                    DeploymentProgressState.Processing when x.Value.percentage > 0 => $"Removing {Package.Name}: {x.Value.percentage}% Complete",

                    _ => $"Removing {Package.Name}"
                })
                .ToProperty(this, x => x.Status);
        }

        private DeploymentProgress? Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public PackageViewModel Package { get; }

        public string Status => _statusString.Value;

        public async Task RemoveAsync(CancellationToken cancellation = default)
        {
            var progressCallback = new Progress<DeploymentProgress>(p => Progress = p);
            var options = _mode == PackageInstallationMode.Machine ? RemovalOptions.RemoveForAllUsers : RemovalOptions.None;

            await _manager.RemovePackageAsync(Package.Package.Id.FullName, options).AsTask(cancellation, progressCallback).ConfigureAwait(false);
        }
    }
}