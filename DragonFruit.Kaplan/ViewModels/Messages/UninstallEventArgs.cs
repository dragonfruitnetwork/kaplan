using System.Collections.Generic;
using Windows.ApplicationModel;

namespace DragonFruit.Kaplan.ViewModels.Messages
{
    public class UninstallEventArgs
    {
        public UninstallEventArgs(IEnumerable<Package> packages)
        {
            Packages = packages;
        }

        public IEnumerable<Package> Packages { get; }
    }
}