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
            ["AppTitle"] = ("ReIcon Pro", "ReIcon Pro"),
            ["AppSubtitle"] = ("Masaüstü Simge & Çözünürlük Yöneticisi", "Desktop Icon & Layout Manager"),
            
            // Navigation Tabs
            ["NavProfiles"] = ("📑 Düzenler", "📑 Profiles"),
            ["NavTools"] = ("⚡ Hızlı Araçlar", "⚡ Quick Tools"),
            ["NavSettings"] = ("⚙️ Ayarlar", "⚙️ Settings"),
            ["NavAbout"] = ("ℹ️ Hakkında", "ℹ️ About"),

            // Profiles View
            ["NewLayoutTitle"] = ("Yeni Düzen Oluştur", "Create New Layout"),
            ["LayoutNamePlaceholder"] = ("Örn: İş Düzeni / Oyun Modu", "e.g. Work Layout / Gaming Mode"),
            ["SaveButton"] = ("📸 Yeni Kaydet", "📸 Save New"),
            ["UpdateSelectedButton"] = ("🔄 Seçiliyi Güncelle", "🔄 Update Selected"),
            ["SearchPlaceholder"] = ("Düzen adında ara...", "Search layout names..."),
            ["SavedProfilesTitle"] = ("Kayıtlı Masaüstü Düzenleri", "Saved Desktop Layouts"),
            ["BtnRestore"] = ("⚡ Yükle", "⚡ Restore"),
            ["BtnUpdate"] = ("🔄 Güncelle", "🔄 Update"),
            ["BtnDelete"] = ("🗑️ Sil", "🗑️ Delete"),
            ["EmptyProfiles"] = ("Henüz kayıtlı bir masaüstü düzeni yok. Yukarıdan 'Yeni Kaydet' butonuna basarak ilk profilinizi oluşturun.", "No saved layouts yet. Click 'Save New' above to capture your first layout."),

            // Context Menu Items
            ["CtxRestore"] = ("⚡ Geri Yükle", "⚡ Restore"),
            ["CtxUpdate"] = ("🔄 Mevcut Masaüstü ile Güncelle (Üzerine Yaz)", "🔄 Overwrite with Current Desktop"),
            ["CtxRename"] = ("✏️ Yeniden Adlandır", "✏️ Rename"),
            ["CtxDuplicate"] = ("📋 Çoğalt / Klonla", "📋 Duplicate"),
            ["CtxCreateShortcut"] = ("🖥️ Masaüstü Kısayolu Oluştur", "🖥️ Create Desktop Shortcut"),
            ["CtxDelete"] = ("🗑️ Sil", "🗑️ Delete"),

            // Prompts & Dialogs
            ["RenamePromptTitle"] = ("Yeniden Adlandır", "Rename Profile"),
            ["RenamePromptMsg"] = ("Lütfen düzen için yeni bir ad girin:", "Please enter a new name for the layout:"),
            ["UpdateConfirmTitle"] = ("Profili Güncelle", "Update Profile"),
            ["UpdateConfirmMsg"] = ("'{0}' profili şu anki masaüstü simge konumları ile güncellensin mi?", "Do you want to overwrite '{0}' with current desktop icon positions?"),
            ["UpdateSuccess"] = ("✅ '{0}' profili başarıyla güncellendi ({1} simge).", "✅ Profile '{0}' successfully updated ({1} icons)."),
            ["ShortcutCreated"] = ("🖥️ '{0}' için masaüstüne kısayol oluşturuldu.", "🖥️ Desktop shortcut created for '{0}'."),

            // Quick Tools View
            ["ToolsTitle"] = ("Masaüstü & Ekran Araçları", "Desktop & Display Tools"),
            ["ToolsSubtitle"] = ("Windows masaüstünü hızla yönetmek için yerleşik Win32 yardımcıları.", "Native Win32 utilities for managing your Windows desktop on the fly."),
            ["BtnDesktopRefresh"] = ("🔄 Masaüstünü Yenile", "🔄 Refresh Desktop"),
            ["DescDesktopRefresh"] = ("Explorer masaüstü penceresine redraw mesajı göndererek simge önbelleğini yeniler.", "Sends redraw messages to Explorer desktop window to refresh icon cache."),
            ["BtnToggleAutoArrange"] = ("🎯 Otomatik Düzenlemeyi Değiştir", "🎯 Toggle Auto-Arrange"),
            ["DescAutoArrange"] = ("Windows'un simgeleri zorunlu sıralama modunu açar veya kapatır.", "Toggles Windows automatic icon repositioning mode on or off."),
            ["DisplayInfoTitle"] = ("Ekran & Monitör Durumu", "Display & Monitor Info"),
            ["PrimaryRes"] = ("Ana Monitör Çözünürlüğü:", "Primary Screen Resolution:"),
            ["VirtualRes"] = ("Sanal Ekran Toplam Alanı:", "Virtual Screen Total Area:"),
            ["MonitorCount"] = ("Algılanan Monitör Sayısı:", "Detected Monitors:"),

            // Settings View
            ["SettingsTitle"] = ("Uygulama Ayarları", "Application Settings"),
            ["SettingsSubtitle"] = ("ReIcon Pro başlangıç ve davranış tercihlerini yapılandırın.", "Configure ReIcon Pro startup preferences and behavior."),
            ["SettingStartWithWindows"] = ("Windows Başlangıcında Çalıştır", "Start with Windows"),
            ["DescStartWithWindows"] = ("Bilgisayar her açıldığında ReIcon Pro arka planda otomatik başlasın.", "Automatically start ReIcon Pro in the background on system boot."),
            ["SettingMinimizeToTray"] = ("Kapatınca Sistem Tepsisine Küçült", "Minimize to System Tray on Close"),
            ["DescMinimizeToTray"] = ("Pencereyi kapattığınızda uygulama tamamen çıkmak yerine saat yanında çalışır.", "Keep running in notification tray when window is closed instead of quitting."),
            ["SettingShowNotifications"] = ("Masaüstü Bildirimlerini Göster", "Show Desktop Notifications"),
            ["DescShowNotifications"] = ("Kayıt ve geri yükleme işlemlerinde tepsi balon bildirimi göster.", "Show notification balloons when saving or restoring layouts."),
            ["SettingAutoBackup"] = ("Çözünürlük Değişince Otomatik Yedek Al", "Auto-Backup on Resolution Change"),
            ["DescAutoBackup"] = ("Ekran çözünürlüğü veya monitör sayısı değiştiğinde otomatik yedek kaydeder.", "Automatically save a snapshot when screen resolution or monitors change."),
            ["BtnOpenFolder"] = ("📁 Profil Klasörünü Aç", "📁 Open Profiles Folder"),
            ["BtnClearAll"] = ("⚠️ Tüm Kayıtları Sıfırla", "⚠️ Wipe All Layouts"),

            // About View
            ["AboutTitle"] = ("ReIcon Pro Hakkında", "About ReIcon Pro"),
            ["AboutDesc"] = ("Windows 10 ve 11 için geliştirilmiş, hafif, taşınabilir ve sıfır bağımlılıklı masaüstü simge yöneticisi.", "Lightweight, portable, and zero-dependency desktop icon layout manager for Windows 10 & 11."),
            ["AboutVersion"] = ("Sürüm: 2.1.0 (Power Edition)", "Version: 2.1.0 (Power Edition)"),
            ["AboutDev"] = ("Geliştirici: TheK3R1M", "Developer: TheK3R1M"),
            ["BtnGithub"] = ("🌐 GitHub Repository", "🌐 GitHub Repository"),

            // System Tray & Status
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
