﻿// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Avalonia.Media;
using DragonFruit.Kaplan.ViewModels.Enums;
using DragonFruit.Kaplan.ViewModels.Messages;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using ReactiveUI;

namespace DragonFruit.Kaplan.ViewModels
{
    public class RemovalProgressViewModel : ReactiveObject, IHandlesClosingEvent, IExecutesTaskPostLoad, ICanCloseWindow
    {
        private readonly ILogger _logger = App.GetLogger<RemovalProgressViewModel>();
        private readonly AsyncLock _lock = new();
        private readonly PackageInstallationMode _mode;
        private readonly CancellationTokenSource _cancellation = new();
        private readonly ObservableAsPropertyHelper<ISolidColorBrush> _progressColor;

        private OperationState _status;
        private int _currentPackageNumber;
        private PackageRemovalTask _current;

        public RemovalProgressViewModel(IEnumerable<Package> packages, PackageInstallationMode mode)
        {
            _mode = mode;
            _status = OperationState.Pending;
            _progressColor = this.WhenValueChanged(x => x.Status).Select(x => x switch
            {
                OperationState.Pending => Brushes.Gray,
                OperationState.Running => Brushes.DodgerBlue,
                OperationState.Errored => Brushes.Red,
                OperationState.Completed => Brushes.Green,
                OperationState.Canceled => Brushes.DarkGray,

                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToProperty(this, x => x.ProgressColor);

            var canCancelOperation = this.WhenAnyValue(x => x.CancellationRequested, x => x.Status)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => !x.Item1 && x.Item2 == OperationState.Running);

            Packages = packages.ToList();
            RequestCancellation = ReactiveCommand.Create(CancelOperation, canCancelOperation);
        }

        public event Action CloseRequested;

        public PackageRemovalTask Current
        {
            get => _current;
            private set => this.RaiseAndSetIfChanged(ref _current, value);
        }

        public int CurrentPackageNumber
        {
            get => _currentPackageNumber;
            private set => this.RaiseAndSetIfChanged(ref _currentPackageNumber, value);
        }

        public OperationState Status
        {
            get => _status;
            private set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public bool CancellationRequested => _cancellation.IsCancellationRequested;

        public ISolidColorBrush ProgressColor => _progressColor.Value;

        public IReadOnlyList<Package> Packages { get; }

        public ICommand RequestCancellation { get; }

        private void CancelOperation()
        {
            _cancellation.Cancel();
            this.RaisePropertyChanged(nameof(CancellationRequested));
        }

        void IHandlesClosingEvent.OnClose(CancelEventArgs args)
        {
            args.Cancel = Status == OperationState.Running;
        }

        async Task IExecutesTaskPostLoad.Perform()
        {
            _logger.LogInformation("Removal process started");
            _logger.LogDebug("Waiting for lock access");

            using (await _lock.LockAsync(_cancellation.Token).ConfigureAwait(false))
            {
                Status = OperationState.Running;

                var manager = new PackageManager();

                for (var i = 0; i < Packages.Count; i++)
                {
                    if (CancellationRequested)
                    {
                        break;
                    }

                    CurrentPackageNumber = i + 1;
                    Current = new PackageRemovalTask(manager, Packages[i], _mode);

                    try
                    {
                        _logger.LogInformation("Starting removal of {packageId}", Current.Package.Id);

#if DRY_RUN
                        await Task.Delay(1000, _cancellation.Token).ConfigureAwait(false);
#else
                        await Current.RemoveAsync(_cancellation.Token).ConfigureAwait(false);
#endif
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Package removal cancelled by user (stopped at {packageId})", Current.Package.Id);
                    }
                    catch (Exception ex)
                    {
                        Status = OperationState.Errored;
                        _logger.LogError(ex, "Package removal failed: {err}", ex.Message);

                        break;
                    }
                }
            }

            Status = CancellationRequested ? OperationState.Canceled : OperationState.Completed;
            MessageBus.Current.SendMessage(new PackageRefreshEventArgs());

            _logger.LogInformation("Package removal process ended: {state}", Status);

            await Task.Delay(1000).ConfigureAwait(false);
            CloseRequested?.Invoke();
        }
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