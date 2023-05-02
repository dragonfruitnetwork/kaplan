using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Avalonia.Media;
using DynamicData.Binding;
using Nito.AsyncEx;
using ReactiveUI;

namespace DragonFruit.Kaplan.ViewModels
{
    public class RemovalProgressViewModel : ReactiveObject, IHandlesClosingEvent, IExecutesTaskPostLoad, ICanCloseWindow
    {
        private readonly AsyncLock _lock = new();
        private readonly ObservableAsPropertyHelper<ISolidColorBrush> _progressColor;

        private int _currentPackageNumber;
        private bool _cancellationRequested;
        private PackageViewModel _currentPackage;
        private OperationState _status = OperationState.Pending;

        public RemovalProgressViewModel(IEnumerable<Package> packages)
        {
            Packages = packages.Select(x => new PackageViewModel(x)).ToList();

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
                .Select(x => !x.Item1 && x.Item2 == OperationState.Running);

            RequestCancellation = ReactiveCommand.Create(() => CancellationRequested = true, canCancelOperation);
        }

        public event Action CloseRequested;

        public OperationState Status
        {
            get => _status;
            private set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public bool CancellationRequested
        {
            get => _cancellationRequested;
            private set => this.RaiseAndSetIfChanged(ref _cancellationRequested, value);
        }

        public PackageViewModel CurrentPackage
        {
            get => _currentPackage;
            private set => this.RaiseAndSetIfChanged(ref _currentPackage, value);
        }

        public int CurrentPackageNumber
        {
            get => _currentPackageNumber;
            private set => this.RaiseAndSetIfChanged(ref _currentPackageNumber, value);
        }

        public ISolidColorBrush ProgressColor => _progressColor.Value;

        public IReadOnlyList<PackageViewModel> Packages { get; }

        public ICommand RequestCancellation { get; }

        void IHandlesClosingEvent.OnClose(CancelEventArgs args)
        {
            args.Cancel = Status == OperationState.Running;
        }

        async Task IExecutesTaskPostLoad.Perform()
        {
            using (await _lock.LockAsync().ConfigureAwait(false))
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
                    CurrentPackage = Packages[i];
                    
#if DRY_RUN
                    await Task.Delay(1000);
#else
                    try
                    {
                        await manager.RemovePackageAsync(CurrentPackage.Package.Id.FullName);
                    }
                    catch
                    {
                        // todo log error
                        Status = OperationState.Errored;
                        break;
                    }
#endif
                }

                Status = CancellationRequested ? OperationState.Canceled : OperationState.Completed;

                await Task.Delay(1000).ConfigureAwait(false);
                CloseRequested?.Invoke();
            }
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