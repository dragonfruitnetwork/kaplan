// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Avalonia.Media;
using ReactiveUI;

namespace DragonFruit.Kaplan.ViewModels
{
    public class RemovalProgressViewModel : ReactiveObject, IHandlesClosingEvent, IExecutesTaskPostLoad, ICanCloseWindow, IDisposable
    {
        private readonly PackageRemover _remover;
        private readonly CancellationTokenSource _cancellation;

        private readonly ObservableAsPropertyHelper<float> _progressValue;
        private readonly ObservableAsPropertyHelper<string> _progressText;
        private readonly ObservableAsPropertyHelper<ISolidColorBrush> _progressColor;

        private readonly ObservableAsPropertyHelper<PackageViewModel> _currentPackage;
        private readonly ObservableAsPropertyHelper<PackageRemover.OperationState> _currentState;

        public RemovalProgressViewModel(PackageRemover remover, CancellationTokenSource cts = null)
        {
            _remover = remover;
            _cancellation = cts ?? new CancellationTokenSource();

            var currentPackage = Observable.FromEventPattern<EventHandler<Package>, Package>(h => remover.CurrentPackageChanged += h, h => remover.CurrentPackageChanged -= h)
                .Select(static x => x.EventArgs);

            var currentPackageProgress = Observable.FromEventPattern<EventHandler<DeploymentProgress>, DeploymentProgress>(h => remover.CurrentPackageRemovalProgressChanged += h, h => remover.CurrentPackageRemovalProgressChanged -= h)
                .StartWith(new EventPattern<DeploymentProgress>(null, remover.CurrentPackageRemovalProgress))
                .Select(static x => x.EventArgs);

            _currentPackage = currentPackage
                .Select(static x => new PackageViewModel(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.Current);

            var state = Observable.FromEventPattern<EventHandler<PackageRemover.OperationState>, PackageRemover.OperationState>(h => remover.StateChanged += h, h => remover.StateChanged -= h)
                .StartWith(new EventPattern<PackageRemover.OperationState>(null, remover.State))
                .Select(static x => x.EventArgs);

            _currentState = state.ObserveOn(RxApp.MainThreadScheduler).ToProperty(this, x => x.CurrentState);
            _progressValue = currentPackage.CombineLatest(currentPackageProgress)
                // don't add to index, we only want processed packages up until this point
                .Select(x =>
                {
                    var singlePackagePercentage = 1f / remover.TotalPackages;
                    return remover.CurrentIndex * singlePackagePercentage + x.Second.percentage / 100f * singlePackagePercentage;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.ProgressValue);

            _progressColor = state.Select(static x => x switch
                {
                    PackageRemover.OperationState.Pending => Brushes.Gray,
                    PackageRemover.OperationState.Running => Brushes.DodgerBlue,
                    PackageRemover.OperationState.Errored => Brushes.Red,
                    PackageRemover.OperationState.Completed => Brushes.Green,
                    PackageRemover.OperationState.Canceled => Brushes.DarkGray,

                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.ProgressColor);

            _progressText = currentPackageProgress.CombineLatest(currentPackage).Select(static x => x.First.state switch
                {
                    DeploymentProgressState.Queued => $"Removing {x.Second.DisplayName}: Pending",
                    DeploymentProgressState.Processing when x.First.percentage == 100 => $"Removing {x.Second.DisplayName} Complete",
                    DeploymentProgressState.Processing when x.First.percentage > 0 => $"Removing {x.Second.DisplayName} ({x.First.percentage}% Complete)",

                    _ => $"Removing {x.Second.DisplayName}"
                })
                .ToProperty(this, x => x.ProgressText);

            var canCancelOperation = this.WhenAnyValue(x => x.CancellationRequested)
                .CombineLatest(state)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(static x => !x.Item1 && x.Item2 == PackageRemover.OperationState.Running);

            RequestCancellation = ReactiveCommand.Create(CancelOperation, canCancelOperation);
        }

        public event Action CloseRequested;

        public PackageViewModel Current => _currentPackage.Value;
        public PackageRemover.OperationState CurrentState => _currentState.Value;

        public float ProgressValue => _progressValue.Value;
        public string ProgressText => _progressText.Value;
        public ISolidColorBrush ProgressColor => _progressColor.Value;

        public bool CancellationRequested => _cancellation.IsCancellationRequested;
        public ICommand RequestCancellation { get; }

        private void CancelOperation()
        {
            _cancellation.Cancel();
            this.RaisePropertyChanged(nameof(CancellationRequested));
        }

        void IHandlesClosingEvent.OnClose(CancelEventArgs args)
        {
            args.Cancel = _remover.State == PackageRemover.OperationState.Running;
        }

        async Task IExecutesTaskPostLoad.Perform()
        {
            // waits for the process to end
            await _remover.RemovePackagesAsync(_cancellation.Token);

            // auto close if completed and not cancelled
            if (!_cancellation.IsCancellationRequested && _remover.State == PackageRemover.OperationState.Completed)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                CloseRequested?.Invoke();
            }
        }

        public void Dispose()
        {
            _cancellation?.Dispose();
        }
    }
}