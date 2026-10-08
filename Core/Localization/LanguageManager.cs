using System;
using System.Collections.Generic;

namespace DesktopIconLayoutManager.Core.Localization
{
    public enum AppLanguage
    {
        TR,
        EN
    }

    public static class LanguageManager
    {
        public static AppLanguage CurrentLanguage { get; set; } = AppLanguage.TR;
        public static event Action? OnLanguageChanged;

        private static readonly Dictionary<string, (string TR, string EN)> Translations = new()
        {
            ["AppTitle"] = ("ReIcon Pro - Masaüstü Simge Yöneticisi", "ReIcon Pro - Desktop Icon Manager"),
            ["AppSubtitle"] = ("Masaüstü simge düzenlerinizi kaydedin, çözünürlük değişikliklerinde anında geri yükleyin.", "Save desktop icon layouts and instantly restore them across display changes."),
            ["AutoArrangeActive"] = ("Auto-Arrange: Aktif (Geri yüklemede kapatılır)", "Auto-Arrange: Active (Disabled on restore)"),
            ["AutoArrangeDisabled"] = ("Auto-Arrange: Pasif (Hazır)", "Auto-Arrange: Disabled (Ready)"),
            ["NewLayoutTitle"] = ("Yeni Düzen Kaydet", "Save New Layout"),
            ["LayoutNamePlaceholder"] = ("Örn: Çalışma Düzeni / Oyun Düzeni", "e.g. Work Layout / Gaming Layout"),
            ["SaveButton"] = ("📸 Düzeni Kaydet", "📸 Save Layout"),
            ["SearchPlaceholder"] = ("Düzenlerde ara...", "Search profiles..."),
            ["SavedProfilesTitle"] = ("Kaydedilmiş Düzenler", "Saved Layouts"),
            ["ColName"] = ("Düzen Adı", "Layout Name"),
            ["ColDate"] = ("Tarih", "Date"),
            ["ColResolution"] = ("Çözünürlük", "Resolution"),
            ["ColIcons"] = ("Simge", "Icons"),
            ["ColActions"] = ("İşlemler", "Actions"),
            ["BtnRestore"] = ("⚡ Geri Yükle", "⚡ Restore"),
            ["BtnDelete"] = ("🗑️ Sil", "🗑️ Delete"),
            ["TrayOpen"] = ("📱 Göster", "📱 Open ReIcon Pro"),
            ["TrayQuickSave"] = ("📸 Hızlı Kaydet", "📸 Quick Save"),
            ["TrayRestoreLast"] = ("⚡ Son Düzeni Yükle", "⚡ Restore Last Layout"),
            ["TrayExit"] = ("❌ Çıkış", "❌ Exit"),
            ["StatusReady"] = ("Hazır.", "Ready."),
            ["SaveSuccess"] = ("✅ '{0}' kaydedildi ({1} simge).", "✅ Saved '{0}' ({1} icons)."),
            ["RestoreSuccess"] = ("⚡ '{0}' geri yüklendi. {1}", "⚡ Restored '{0}'. {1}"),
            ["DeleteConfirmTitle"] = ("Profil Sil", "Delete Profile"),
            ["DeleteConfirmMsg"] = ("'{0}' düzenini silmek istediğinize emin misiniz?", "Are you sure you want to delete '{0}'?"),
            ["DeleteSuccess"] = ("🗑️ '{0}' düzeni silindi.", "🗑️ Deleted profile '{0}'.")
        };

        public static string Get(string key)
        {
            if (Translations.TryGetValue(key, out var tuple))
            {
                return CurrentLanguage == AppLanguage.TR ? tuple.TR : tuple.EN;
            }
            return key;
        }

        public static void SetLanguage(AppLanguage language)
        {
            CurrentLanguage = language;
            OnLanguageChanged?.Invoke();
        }
    }
}
