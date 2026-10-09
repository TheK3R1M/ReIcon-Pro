namespace DesktopIconLayoutManager.Core.Models
{
    public class AppSettings
    {
        public bool StartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        public bool ShowNotifications { get; set; } = true;
        public bool AutoBackupOnResolutionChange { get; set; } = false;
        public string Language { get; set; } = "TR";
    }
}
