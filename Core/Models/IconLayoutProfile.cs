using System;
using System.Collections.Generic;

namespace DesktopIconLayoutManager.Core.Models
{
    public class IconLayoutProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int VirtualScreenLeft { get; set; }
        public int VirtualScreenTop { get; set; }
        public int VirtualScreenWidth { get; set; }
        public int VirtualScreenHeight { get; set; }
        public int MonitorCount { get; set; }
        public string ResolutionDisplay => $"{ScreenWidth}x{ScreenHeight}";
        public List<DesktopIconItem> Icons { get; set; } = new List<DesktopIconItem>();
    }
}
