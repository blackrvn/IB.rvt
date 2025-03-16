using System.Windows.Media;

namespace Library.Interfaces
{
    public interface IPlaceholderTextHost
    {
        string Text { get; set; }
        Brush Foreground { get; set; }
    }
}
