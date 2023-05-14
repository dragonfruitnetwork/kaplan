// Kaplan Copyright (c) DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;

namespace DragonFruit.Kaplan.ViewModels
{
    public interface ICanCloseWindow
    {
        event Action CloseRequested;
    }
}