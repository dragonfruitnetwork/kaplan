using System;

namespace DragonFruit.Kaplan.ViewModels
{
    public interface ICanCloseWindow
    {
        event Action CloseRequested;
    }
}