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