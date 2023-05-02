using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Principal;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using DragonFruit.Kaplan.ViewModels.Enums;
using DragonFruit.Kaplan.ViewModels.Messages;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace DragonFruit.Kaplan.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private readonly WindowsIdentity _currentUser;
        private readonly PackageManager _packageManager;
        private readonly IDisposable _packageRefreshListener;
        private readonly ObservableAsPropertyHelper<IEnumerable<PackageViewModel>> _displayedPackages;

        private string _searchQuery = string.Empty;
        private PackageInstallationMode _packageMode = PackageInstallationMode.User;
        private IReadOnlyCollection<PackageViewModel> _discoveredPackages = Array.Empty<PackageViewModel>();

        public MainWindowViewModel()
        {
            _packageManager = new PackageManager();

            _currentUser = WindowsIdentity.GetCurrent();
            AvailablePackageModes = _currentUser.User != null
                ? Enum.GetValues<PackageInstallationMode>()
                : new[] { PackageInstallationMode.Machine };

            // create observables
            var packagesSelected = SelectedPackages.ToObservableChangeSet(x => x)
                .AutoRefresh()
                .ToCollection()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => x.Any());

            var stubsPresentInCurrentList = this.WhenValueChanged(x => x.DisplayedPackages)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(x => x.Any(y => y.Package.IsStub));

            _displayedPackages = this.WhenAnyValue(x => x.DiscoveredPackages, x => x.SearchQuery)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(q => q.Item1.Where(x => x.IsSearchMatch(q.Item2)))
                .ToProperty(this, x => x.DisplayedPackages);

            _packageRefreshListener =
                MessageBus.Current.Listen<UninstallEventArgs>().Subscribe(x => RefreshPackagesImpl());

            // create commands
            SelectStubPackages = ReactiveCommand.Create(SelectStubPackagesImpl, stubsPresentInCurrentList);
            RefreshPackages = ReactiveCommand.Create(RefreshPackagesImpl, outputScheduler: TaskPoolScheduler.Default);
            ClearSelection = ReactiveCommand.Create(() => SelectedPackages.Clear(), packagesSelected);
            RemovePackages = ReactiveCommand.Create(RemovePackagesImpl, packagesSelected);

            // auto refresh the package list if the user package filter switch is changed 
            // this needs the additional select to allow command invoking to work (see https://stackoverflow.com/a/54936685)
            this.WhenValueChanged(x => x.PackageMode)
                .Subscribe(_ => RefreshPackagesImpl());
        }

        public IEnumerable<PackageInstallationMode> AvailablePackageModes { get; }

        public ObservableCollection<PackageViewModel> SelectedPackages { get; } = new();

        public IEnumerable<PackageViewModel> DisplayedPackages => _displayedPackages.Value;

        public IReadOnlyCollection<PackageViewModel> DiscoveredPackages
        {
            get => _discoveredPackages;
            set => this.RaiseAndSetIfChanged(ref _discoveredPackages, value);
        }

        /// <summary>
        /// Gets or sets whether the <see cref="DiscoveredPackages"/> collection should show packages installed for the selected user
        /// </summary>
        public PackageInstallationMode PackageMode
        {
            get => _packageMode;
            set => this.RaiseAndSetIfChanged(ref _packageMode, value);
        }

        /// <summary>
        /// Gets or sets the search query used to filter <see cref="DisplayedPackages"/>
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public ICommand ClearSelection { get; }
        public ICommand RefreshPackages { get; }
        public ICommand SelectStubPackages { get; }
        public ICommand RemovePackages { get; }

        private void RefreshPackagesImpl()
        {
            IEnumerable<Package> packages;

            switch (PackageMode)
            {
                case PackageInstallationMode.User when _currentUser.User != null:
                    packages = _packageManager.FindPackagesForUser(_currentUser.User.Value);
                    break;

                case PackageInstallationMode.Machine:
                    packages = _packageManager.FindPackages();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            SearchQuery = string.Empty;
            DiscoveredPackages = packages.Where(x => x.SignatureKind != PackageSignatureKind.System)
                .Select(x => new PackageViewModel(x))
                .ToList();

            // ensure the ui doesn't have non-existent packages nominated through an intersection
            SelectedPackages.Clear();
            SelectedPackages.AddRange(SelectedPackages.IntersectBy(DiscoveredPackages.Select(x => x.Id), x => x.Id));
        }

        private void SelectStubPackagesImpl()
        {
            SelectedPackages.Clear();
            SelectedPackages.AddRange(DisplayedPackages.Where(x => x.Package.IsStub).Union(SelectedPackages));
        }

        private void RemovePackagesImpl()
        {
            MessageBus.Current.SendMessage(new UninstallEventArgs(SelectedPackages.Select(x => x.Package)));
        }

        public void Dispose()
        {
            _displayedPackages?.Dispose();
            _packageRefreshListener?.Dispose();
        }
    }
}