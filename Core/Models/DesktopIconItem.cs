namespace DesktopIconLayoutManager.Core.Models
{
    public class DesktopIconItem
    {
        public string Name { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int MonitorIndex { get; set; } = 0;
    }
}
