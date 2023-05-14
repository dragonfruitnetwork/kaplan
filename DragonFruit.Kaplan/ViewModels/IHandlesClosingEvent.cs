// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.ComponentModel;

namespace DragonFruit.Kaplan.ViewModels
{
    /// <summary>
    /// Exposes a method for handling window close requests
    /// </summary>
    public interface IHandlesClosingEvent
    {
        void OnClose(CancelEventArgs args);
    }
}