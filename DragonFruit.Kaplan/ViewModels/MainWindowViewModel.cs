// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Avalonia.Threading;
using DragonFruit.Kaplan.ViewModels.Enums;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace DragonFruit.Kaplan.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IDisposable
    {
        private readonly ILogger _logger;
        private readonly WindowsIdentity _currentUser;
        private readonly PackageManager _packageManager;

        private readonly ObservableAsPropertyHelper<IEnumerable<PackageViewModel>> _displayedPackages;

        private string _searchQuery = string.Empty;
        private PackageInstallationMode _packageMode = PackageInstallationMode.User;
        private IReadOnlyCollection<PackageViewModel> _discoveredPackages = Array.Empty<PackageViewModel>();

        public MainWindowViewModel()
        {
            _packageManager = new PackageManager();
            _currentUser = WindowsIdentity.GetCurrent();
            _logger = App.GetLogger<MainWindowViewModel>();

            AvailablePackageModes = _currentUser.User != null
                ? Enum.GetValues<PackageInstallationMode>()
                : [PackageInstallationMode.Machine];

            // create observables
            var packagesSelected = SelectedPackages.ToObservableChangeSet()
                .ToCollection()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => x.Count != 0);

            _displayedPackages = this.WhenAnyValue(x => x.DiscoveredPackages, x => x.SearchQuery, x => x.SelectedPackages)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(q =>
                {
                    // because filters remove selected entries, the search will split the listing into two groups, with the matches showing above
                    var matches = q.Item1.ToLookup(x => x.IsSearchMatch(q.Item2));
                    return matches[true].Concat(matches[false]);
                })
                .ToProperty(this, x => x.DisplayedPackages);

            // create commands
            RefreshPackages = ReactiveCommand.CreateFromTask(RefreshPackagesImpl);
            RemovePackages = ReactiveCommand.Create(RemovePackagesImpl, packagesSelected);
            ClearSelection = ReactiveCommand.Create(() => SelectedPackages.Clear(), packagesSelected);
            ShowAbout = ReactiveCommand.CreateFromTask(() => AboutPageInteraction.Handle(Unit.Default).ToTask());

            AboutPageInteraction = new Interaction<Unit, Unit>();
            BeginRemovalInteraction = new Interaction<RemovalProgressViewModel, PackageRemover.OperationState>();

            // auto refresh the package list if the user package filter switch is changed
            this.WhenValueChanged(x => x.PackageMode).ObserveOn(RxApp.TaskpoolScheduler).Subscribe(_ => RefreshPackages.Execute(null));
        }

        public IEnumerable<PackageInstallationMode> AvailablePackageModes { get; }

        public ObservableCollection<PackageViewModel> SelectedPackages { get; } = new();

        public IEnumerable<PackageViewModel> DisplayedPackages => _displayedPackages.Value;

        private IReadOnlyCollection<PackageViewModel> DiscoveredPackages
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

        public ICommand ShowAbout { get; }
        public ICommand ClearSelection { get; }
        public ICommand RemovePackages { get; }
        public ICommand RefreshPackages { get; }

        public Interaction<Unit, Unit> AboutPageInteraction { get; }
        public Interaction<RemovalProgressViewModel, PackageRemover.OperationState> BeginRemovalInteraction { get; }

        private async Task RefreshPackagesImpl()
        {
            IEnumerable<Package> packages;

            switch (PackageMode)
            {
                case PackageInstallationMode.User when _currentUser.User != null:
                    _logger.LogInformation("Loading Packages for user {userId}", _currentUser.User.Value);
                    packages = _packageManager.FindPackagesForUser(_currentUser.User.Value);
                    break;

                case PackageInstallationMode.Machine:
                    _logger.LogInformation("Loading machine-wide packages");
                    packages = _packageManager.FindPackages();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var filteredPackageModels = packages.Where(x => x.SignatureKind != PackageSignatureKind.System)
                .Select(x => new PackageViewModel(x))
                .ToList();

            _logger.LogDebug("Discovered {x} packages", filteredPackageModels.Count);

            // ensure the ui doesn't have non-existent packages nominated through an intersection
            // ToList needed due to deferred nature of iterators used.
            var reselectedPackages = filteredPackageModels.IntersectBy(SelectedPackages.Select(x => x.Package.Id.FullName), x => x.Package.Id.FullName).ToList();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SelectedPackages.Clear();

                SearchQuery = string.Empty;
                DiscoveredPackages = filteredPackageModels;
                SelectedPackages.AddRange(reselectedPackages);
            });
        }

        private async Task RemovePackagesImpl()
        {
            var remover = new PackageRemover(PackageMode, _packageManager, SelectedPackages.Select(x => x.Package).ToList());
            var cts = new CancellationTokenSource();

            using (var model = new RemovalProgressViewModel(remover, cts))
            {
                _logger.LogInformation("Starting removal of {x} packages", remover.TotalPackages);
                _ = remover.RemovePackagesAsync(cts.Token);

                await BeginRemovalInteraction.Handle(model);
            }

            // reload packages after interaction ends
            RefreshPackages.Execute(null);
        }

        public void Dispose()
        {
            _displayedPackages?.Dispose();
        }
    }
}