// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using DragonFruit.Kaplan.ViewModels.Enums;

namespace DragonFruit.Kaplan
{
    public class PackageRemover
    {
        private readonly RemovalOptions _mode;
        private readonly PackageManager _manager;
        private readonly IReadOnlyList<Package> _packages;

        private Package _currentPackage;
        private DeploymentProgress _currentPackageRemovalProgress;

        private OperationState _state;
        private Task<int> _currentRemovalTask;

        public PackageRemover(PackageInstallationMode mode, PackageManager manager, IReadOnlyList<Package> packages)
        {
            _manager = manager;
            _packages = packages;

            _mode = mode switch
            {
                PackageInstallationMode.Machine => RemovalOptions.RemoveForAllUsers,
                PackageInstallationMode.User => RemovalOptions.None,

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// The total number of packages to be removed
        /// </summary>
        public int TotalPackages => _packages.Count;

        /// <summary>
        /// The index of the package currently being removed
        /// </summary>
        public int CurrentIndex { get; private set; }

        public OperationState State
        {
            get => _state;
            private set
            {
                if (_state == value) return;

                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }

        public Package CurrentPackage
        {
            get => _currentPackage;
            private set
            {
                _currentPackage = value;
                CurrentPackageChanged?.Invoke(this, value);
            }
        }

        public DeploymentProgress CurrentPackageRemovalProgress
        {
            get => _currentPackageRemovalProgress;
            private set
            {
                _currentPackageRemovalProgress = value;
                CurrentPackageRemovalProgressChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<OperationState> StateChanged;
        public event EventHandler<Package> CurrentPackageChanged;
        public event EventHandler<DeploymentProgress> CurrentPackageRemovalProgressChanged;

        /// <summary>
        /// Iterates through the provided packages, removing them from the system.
        /// If previously cancelled, will continue from the last package.
        /// </summary>
        public Task<int> RemovePackagesAsync(CancellationToken cancellation = default)
        {
            // prevent multiple removal tasks from running at once
            if (_currentRemovalTask?.Status is TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun or TaskStatus.Running or TaskStatus.Created)
            {
                return _currentRemovalTask;
            }

            return _currentRemovalTask = RemovePackagesAsyncImpl(cancellation);
        }

        private async Task<int> RemovePackagesAsyncImpl(CancellationToken cancellation)
        {
            var removed = 0;
            State = OperationState.Pending;

            for (int i = CurrentIndex; i < _packages.Count; i++)
            {
                if (cancellation.IsCancellationRequested)
                {
                    State = OperationState.Canceled;
                    break;
                }

                CurrentIndex = i;
                CurrentPackage = _packages[i];
                CurrentPackageRemovalProgress = default;

                try
                {
                    State = OperationState.Running;
#if !DEBUG
                    var progress = new Progress<DeploymentProgress>(p => CurrentPackageRemovalProgress = p);
                    await _manager.RemovePackageAsync(_packages[i].Id.FullName, _mode).AsTask(cancellation, progress).ConfigureAwait(false);
#else
                    // dummy removal progress
                    for (uint j = 0; j < 50; j++)
                    {
                        await Task.Delay(50, cancellation);
                        CurrentPackageRemovalProgress = new DeploymentProgress(DeploymentProgressState.Processing, j * 2);
                    }
#endif
                    removed++;
                }
                catch (OperationCanceledException)
                {
                    State = OperationState.Canceled;
                    return removed;
                }
                catch
                {
                    State = OperationState.Errored;
                    return removed;
                }
            }

            State = OperationState.Completed;
            return removed;
        }

        public enum OperationState
        {
            Pending,
            Running,
            Errored,
            Completed,
            Canceled
        }
    }
}