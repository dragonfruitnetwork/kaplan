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