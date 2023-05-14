// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Threading.Tasks;

namespace DragonFruit.Kaplan.ViewModels
{
    /// <summary>
    /// Exposes a method to run content after the window has been rendered.
    /// </summary>
    public interface IExecutesTaskPostLoad
    {
        Task Perform();
    }
}