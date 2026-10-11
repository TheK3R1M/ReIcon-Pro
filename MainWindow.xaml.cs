using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DesktopIconLayoutManager.Core.Engine;
using DesktopIconLayoutManager.Core.Helpers;
using DesktopIconLayoutManager.Core.Localization;
using DesktopIconLayoutManager.Core.Models;
using DesktopIconLayoutManager.Core.Win32;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MessageBox = System.Windows.MessageBox;

namespace DesktopIconLayoutManager
{
    public partial class MainWindow : Window
    {
        private readonly IconLayoutEngine _engine;
        private readonly StorageManager _storage;
        private readonly SettingsManager _settingsManager;
        private readonly NotifyIcon _notifyIcon;
        private List<IconLayoutProfile> _allProfiles = new();
        private bool _isExitTriggered = false;

        public MainWindow()
        {
            InitializeComponent();
            _engine = new IconLayoutEngine();
            _storage = new StorageManager();
            _settingsManager = new SettingsManager();

            // Setup System Tray
            _notifyIcon = new NotifyIcon
            {
                Text = "ReIcon Pro v2.1",
                Visible = true
            };

            LoadAppIconToNotifyIcon();
            BuildTrayMenu();

            _notifyIcon.DoubleClick += (s, e) => ShowWindowFromTray();

            // Set initial language from settings
            if (_settingsManager.CurrentSettings.Language == "EN")
            {
                LanguageManager.SetLanguage(AppLanguage.EN);
            }
            else
            {
                LanguageManager.SetLanguage(AppLanguage.TR);
            }

            LanguageManager.OnLanguageChanged += RefreshLocalizedUI;

            LoadSettingsIntoUI();
            RefreshLocalizedUI();
            CheckAutoArrangeState();
            UpdateDisplayInfoUI();
            LoadProfiles();
        }

        private void LoadAppIconToNotifyIcon()
        {
            try
            {
                Uri iconUri = new Uri("pack://application:,,,/app.ico");
                var info = System.Windows.Application.GetResourceStream(iconUri);
                if (info != null && info.Stream != null)
                {
                    _notifyIcon.Icon = new Icon(info.Stream);
                    this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(iconUri);
                    return;
                }
            }
            catch { }

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }
        }

        private void ShowTrayNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            if (!_settingsManager.CurrentSettings.ShowNotifications) return;

            try
            {
                _notifyIcon.ShowBalloonTip(2000, title, text, icon);
            }
            catch { }
        }

        private void BuildTrayMenu()
        {
            var contextMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem(LanguageManager.Get("TrayOpen"), null, (s, e) => ShowWindowFromTray());
            var quickSaveItem = new ToolStripMenuItem(LanguageManager.Get("TrayQuickSave"), null, (s, e) => QuickSaveFromTray());
            var restoreLastItem = new ToolStripMenuItem(LanguageManager.Get("TrayRestoreLast"), null, (s, e) => QuickRestoreLastFromTray());
            var langItem = new ToolStripMenuItem("🌐 Dil / Language: " + LanguageManager.CurrentLanguage, null, (s, e) => ToggleLanguageFromTray());
            var exitItem = new ToolStripMenuItem(LanguageManager.Get("TrayExit"), null, (s, e) => ExitApplication());

            contextMenu.Items.Add(openItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(quickSaveItem);
            contextMenu.Items.Add(restoreLastItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(langItem);
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ToggleLanguageFromTray()
        {
            var nextLang = LanguageManager.CurrentLanguage == AppLanguage.TR ? AppLanguage.EN : AppLanguage.TR;
            LanguageManager.SetLanguage(nextLang);
            _settingsManager.CurrentSettings.Language = nextLang.ToString();
            _settingsManager.SaveSettings();
        }

        private void ShowWindowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            _isExitTriggered = true;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void RootWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _settingsManager.CurrentSettings.MinimizeToTray)
            {
                Hide();
                ShowTrayNotification("ReIcon Pro", LanguageManager.CurrentLanguage == AppLanguage.TR ? "Uygulama arka planda simge durumuna küçültüldü." : "Minimized to notification tray.");
            }
        }

        private void RootWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExitTriggered && _settingsManager.CurrentSettings.MinimizeToTray)
            {
                e.Cancel = true;
                Hide();
                ShowTrayNotification("ReIcon Pro", LanguageManager.CurrentLanguage == AppLanguage.TR ? "Uygulama saat yanında çalışmaya devam ediyor." : "Running in background tray.");
            }
        }

        // ================= NAVIGATION SWITCHING =================
        private void NavBtnProfiles_Click(object sender, RoutedEventArgs e) => SwitchTab(0);
        private void NavBtnTools_Click(object sender, RoutedEventArgs e) { SwitchTab(1); UpdateDisplayInfoUI(); }
        private void NavBtnSettings_Click(object sender, RoutedEventArgs e) => SwitchTab(2);
        private void NavBtnAbout_Click(object sender, RoutedEventArgs e) => SwitchTab(3);

        private void SwitchTab(int tabIndex)
        {
            ViewProfiles.Visibility = tabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            ViewTools.Visibility = tabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
            ViewSettings.Visibility = tabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
            ViewAbout.Visibility = tabIndex == 3 ? Visibility.Visible : Visibility.Collapsed;

            var activeBg = (System.Windows.Media.Brush)FindResource("CardBg");
            var transparentBg = System.Windows.Media.Brushes.Transparent;
            var activeFg = (System.Windows.Media.Brush)FindResource("TextPrimary");
            var inactiveFg = (System.Windows.Media.Brush)FindResource("TextSecondary");

            NavBtnProfiles.Background = tabIndex == 0 ? activeBg : transparentBg;
            NavBtnProfiles.Foreground = tabIndex == 0 ? activeFg : inactiveFg;

            NavBtnTools.Background = tabIndex == 1 ? activeBg : transparentBg;
            NavBtnTools.Foreground = tabIndex == 1 ? activeFg : inactiveFg;

            NavBtnSettings.Background = tabIndex == 2 ? activeBg : transparentBg;
            NavBtnSettings.Foreground = tabIndex == 2 ? activeFg : inactiveFg;

            NavBtnAbout.Background = tabIndex == 3 ? activeBg : transparentBg;
            NavBtnAbout.Foreground = tabIndex == 3 ? activeFg : inactiveFg;
        }

        // ================= LOCALIZATION =================
        private void RefreshLocalizedUI()
        {
            TxtNavProfiles.Text = LanguageManager.Get("NavProfiles");
            TxtNavTools.Text = LanguageManager.Get("NavTools");
            TxtNavSettings.Text = LanguageManager.Get("NavSettings");
            TxtNavAbout.Text = LanguageManager.Get("NavAbout");

            TxtProfilesHeader.Text = LanguageManager.Get("SavedProfilesTitle");
            TxtNewLayoutTitle.Text = LanguageManager.Get("NewLayoutTitle");
            SaveButton.Content = LanguageManager.Get("SaveButton");
            BtnUpdateSelected.Content = LanguageManager.Get("UpdateSelectedButton");
            TxtSavedLayoutsCount.Text = LanguageManager.Get("NavProfiles");
            TxtEmptyNotice.Text = LanguageManager.Get("EmptyProfiles");

            TxtToolsHeader.Text = LanguageManager.Get("ToolsTitle");
            TxtToolsSubheader.Text = LanguageManager.Get("ToolsSubtitle");
            TxtToolRefreshTitle.Text = LanguageManager.Get("BtnDesktopRefresh");
            TxtToolRefreshDesc.Text = LanguageManager.Get("DescDesktopRefresh");
            TxtToolAutoArrangeTitle.Text = LanguageManager.Get("BtnToggleAutoArrange");
            TxtToolAutoArrangeDesc.Text = LanguageManager.Get("DescAutoArrange");
            TxtDisplayInfoTitle.Text = LanguageManager.Get("DisplayInfoTitle");
            TxtLblPrimaryRes.Text = LanguageManager.Get("PrimaryRes");
            TxtLblVirtualRes.Text = LanguageManager.Get("VirtualRes");
            TxtLblMonitorCount.Text = LanguageManager.Get("MonitorCount");

            TxtSettingsHeader.Text = LanguageManager.Get("SettingsTitle");
            TxtSettingsSubheader.Text = LanguageManager.Get("SettingsSubtitle");
            TxtSettingStartTitle.Text = LanguageManager.Get("SettingStartWithWindows");
            TxtSettingStartDesc.Text = LanguageManager.Get("DescStartWithWindows");
            TxtSettingMinimizeTitle.Text = LanguageManager.Get("SettingMinimizeToTray");
            TxtSettingMinimizeDesc.Text = LanguageManager.Get("DescMinimizeToTray");
            TxtSettingNotifTitle.Text = LanguageManager.Get("SettingShowNotifications");
            TxtSettingNotifDesc.Text = LanguageManager.Get("DescShowNotifications");
            TxtSettingAutoBackupTitle.Text = LanguageManager.Get("SettingAutoBackup");
            TxtSettingAutoBackupDesc.Text = LanguageManager.Get("DescAutoBackup");
            BtnOpenFolder.Content = LanguageManager.Get("BtnOpenFolder");
            BtnClearAll.Content = LanguageManager.Get("BtnClearAll");

            TxtAboutVersion.Text = LanguageManager.Get("AboutVersion");
            TxtAboutDesc.Text = LanguageManager.Get("AboutDesc");
            TxtAboutDev.Text = LanguageManager.Get("AboutDev");

            StatusBarText.Text = LanguageManager.Get("StatusReady");

            if (LanguageManager.CurrentLanguage == AppLanguage.TR)
            {
                BtnLangTR.Background = (System.Windows.Media.Brush)FindResource("PrimaryAccent");
                BtnLangTR.Foreground = System.Windows.Media.Brushes.White;
                BtnLangEN.Background = System.Windows.Media.Brushes.Transparent;
                BtnLangEN.Foreground = (System.Windows.Media.Brush)FindResource("TextSecondary");
            }
            else
            {
                BtnLangEN.Background = (System.Windows.Media.Brush)FindResource("PrimaryAccent");
                BtnLangEN.Foreground = System.Windows.Media.Brushes.White;
                BtnLangTR.Background = System.Windows.Media.Brushes.Transparent;
                BtnLangTR.Foreground = (System.Windows.Media.Brush)FindResource("TextSecondary");
            }

            SetPlaceholder();
            CheckAutoArrangeState();
            BuildTrayMenu();
        }

        private void BtnLangTR_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage(AppLanguage.TR);
            _settingsManager.CurrentSettings.Language = "TR";
            _settingsManager.SaveSettings();
        }

        private void BtnLangEN_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage(AppLanguage.EN);
            _settingsManager.CurrentSettings.Language = "EN";
            _settingsManager.SaveSettings();
        }

        private void SetPlaceholder()
        {
            if (string.IsNullOrWhiteSpace(ProfileNameInput.Text) || ProfileNameInput.Text.StartsWith("Örn:") || ProfileNameInput.Text.StartsWith("e.g."))
            {
                ProfileNameInput.Text = LanguageManager.Get("LayoutNamePlaceholder");
                ProfileNameInput.Foreground = (System.Windows.Media.Brush)FindResource("TextSecondary");
            }
        }

        private void ProfileNameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            string ph = LanguageManager.Get("LayoutNamePlaceholder");
            if (ProfileNameInput.Text == ph || ProfileNameInput.Text.StartsWith("Örn:") || ProfileNameInput.Text.StartsWith("e.g."))
            {
                ProfileNameInput.Text = string.Empty;
                ProfileNameInput.Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary");
            }
        }

        private void ProfileNameInput_LostFocus(object sender, RoutedEventArgs e) => SetPlaceholder();

        // ================= STATUS & PROFILES =================
        private void CheckAutoArrangeState()
        {
            IntPtr listViewHandle = DesktopListView.GetHandle();
            bool isAutoArrange = DesktopListView.IsAutoArrangeEnabled(listViewHandle);

            if (isAutoArrange)
            {
                BadgeDot.Fill = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#F43F5E"));
                BadgeText.Text = "Auto-Arrange: Aktif";
            }
            else
            {
                BadgeDot.Fill = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                BadgeText.Text = "Auto-Arrange: Pasif";
            }
        }

        private void UpdateDisplayInfoUI()
        {
            var primary = Screen.PrimaryScreen?.Bounds;
            if (primary.HasValue)
            {
                TxtValPrimaryRes.Text = $"{primary.Value.Width} x {primary.Value.Height}";
            }

            var virt = SystemInformation.VirtualScreen;
            TxtValVirtualRes.Text = $"{virt.Width} x {virt.Height} ({virt.Left}, {virt.Top})";
            TxtValMonitorCount.Text = $"{Screen.AllScreens.Length} Ekran / Monitor";
        }

        private void LoadProfiles()
        {
            _allProfiles = _storage.GetAllProfiles();
            ApplyFilter();
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            string query = SearchInput.Text?.Trim() ?? string.Empty;
            var list = string.IsNullOrEmpty(query) 
                ? _allProfiles 
                : _allProfiles.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

            ProfilesListView.ItemsSource = list;
            TxtCountBadge.Text = $"{list.Count} kayıt";
            EmptyStatePanel.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProfilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfilesListView.SelectedItem is IconLayoutProfile p)
            {
                ProfileNameInput.Text = p.Name;
                ProfileNameInput.Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary");
            }
        }

        private void ProfileItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProfilesListView.SelectedItem is IconLayoutProfile profile)
            {
                PerformRestore(profile);
            }
        }

        // ================= PROFILE ACTIONS (SAVE / UPDATE / RESTORE / RENAME / DELETE) =================
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSaveNew(ProfileNameInput.Text);
        }

        private void BtnUpdateSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesListView.SelectedItem is IconLayoutProfile profile)
            {
                PerformOverwrite(profile);
            }
            else
            {
                MessageBox.Show("Lütfen önce güncellenecek profili listeden seçin.", "ReIcon Pro", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IconLayoutProfile profile)
            {
                PerformOverwrite(profile);
            }
        }

        private void PerformOverwrite(IconLayoutProfile profile)
        {
            string confirmMsg = string.Format(LanguageManager.Get("UpdateConfirmMsg"), profile.Name);
            var result = MessageBox.Show(confirmMsg, LanguageManager.Get("UpdateConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var freshProfile = _engine.CaptureCurrentLayout(profile.Name);
                freshProfile.Id = profile.Id; // Keep existing ID
                freshProfile.CreatedAt = DateTime.Now; // Update timestamp

                _storage.SaveProfile(freshProfile);

                StatusBarText.Text = string.Format(LanguageManager.Get("UpdateSuccess"), freshProfile.Name, freshProfile.Icons.Count);
                ShowTrayNotification("ReIcon Pro", StatusBarText.Text, ToolTipIcon.Info);
                LoadProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IconLayoutProfile profile)
            {
                PerformRename(profile);
            }
        }

        private void PerformRename(IconLayoutProfile profile)
        {
            string? newName = PromptDialog.Show(LanguageManager.Get("RenamePromptTitle"), 
                                                LanguageManager.Get("RenamePromptMsg"), 
                                                profile.Name, 
                                                this);

            if (!string.IsNullOrWhiteSpace(newName) && newName != profile.Name)
            {
                profile.Name = newName;
                _storage.SaveProfile(profile);
                StatusBarText.Text = $"✏️ Profil yeniden adlandırıldı: '{newName}'";
                LoadProfiles();
            }
        }

        private void QuickSaveFromTray() => PerformSaveNew(string.Empty);

        private void PerformSaveNew(string customName)
        {
            try
            {
                string ph = LanguageManager.Get("LayoutNamePlaceholder");
                if (customName == ph || string.IsNullOrWhiteSpace(customName))
                {
                    customName = $"Düzen_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                IconLayoutProfile profile = _engine.CaptureCurrentLayout(customName);
                _storage.SaveProfile(profile);

                StatusBarText.Text = string.Format(LanguageManager.Get("SaveSuccess"), profile.Name, profile.Icons.Count);
                ShowTrayNotification("ReIcon Pro", StatusBarText.Text, ToolTipIcon.Info);

                ProfileNameInput.Text = string.Empty;
                SetPlaceholder();
                LoadProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText.Text = $"❌ Kaydetme hatası: {ex.Message}";
            }
        }

        private void QuickRestoreLastFromTray()
        {
            if (_allProfiles.Count > 0)
            {
                PerformRestore(_allProfiles[0]);
            }
            else
            {
                ShowTrayNotification("ReIcon Pro", LanguageManager.CurrentLanguage == AppLanguage.TR ? "Kayıtlı düzen bulunamadı." : "No saved profiles found.", ToolTipIcon.Warning);
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IconLayoutProfile profile)
            {
                PerformRestore(profile);
            }
        }

        private void PerformRestore(IconLayoutProfile profile)
        {
            try
            {
                bool success = _engine.RestoreLayout(profile, out string msg);
                CheckAutoArrangeState();

                if (success)
                {
                    StatusBarText.Text = string.Format(LanguageManager.Get("RestoreSuccess"), profile.Name, msg);
                    ShowTrayNotification("ReIcon Pro", StatusBarText.Text, ToolTipIcon.Info);
                    MessageBox.Show(StatusBarText.Text, "ReIcon Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusBarText.Text = $"❌ Geri yükleme hatası: {msg}";
                    MessageBox.Show($"Geri yükleme tamamlanamadı: {msg}", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Restorasyon sırasında beklenmeyen hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IconLayoutProfile profile)
            {
                PerformDelete(profile);
            }
        }

        private void PerformDelete(IconLayoutProfile profile)
        {
            var result = MessageBox.Show(string.Format(LanguageManager.Get("DeleteConfirmMsg"), profile.Name), 
                                         LanguageManager.Get("DeleteConfirmTitle"), 
                                         MessageBoxButton.YesNo, 
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _storage.DeleteProfile(profile.Id);
                StatusBarText.Text = string.Format(LanguageManager.Get("DeleteSuccess"), profile.Name);
                LoadProfiles();
            }
        }

        // ================= CONTEXT MENU HANDLERS =================
        private IconLayoutProfile? GetContextMenuTarget(object sender)
        {
            if (sender is System.Windows.Controls.MenuItem mi && mi.Parent is System.Windows.Controls.ContextMenu cm && cm.PlacementTarget is FrameworkElement fe)
            {
                return fe.DataContext as IconLayoutProfile;
            }
            return ProfilesListView.SelectedItem as IconLayoutProfile;
        }

        private void CtxRestore_Click(object sender, RoutedEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target != null) PerformRestore(target);
        }

        private void CtxUpdate_Click(object sender, RoutedEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target != null) PerformOverwrite(target);
        }

        private void CtxRename_Click(object sender, RoutedEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target != null) PerformRename(target);
        }

        private void CtxDuplicate_Click(object sender, RoutedEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target != null)
            {
                var clone = new IconLayoutProfile
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"{target.Name} (Kopya)",
                    CreatedAt = DateTime.Now,
                    ScreenWidth = target.ScreenWidth,
                    ScreenHeight = target.ScreenHeight,
                    VirtualScreenLeft = target.VirtualScreenLeft,
                    VirtualScreenTop = target.VirtualScreenTop,
                    VirtualScreenWidth = target.VirtualScreenWidth,
                    VirtualScreenHeight = target.VirtualScreenHeight,
                    MonitorCount = target.MonitorCount,
                    Icons = target.Icons.Select(i => new DesktopIconItem { Name = i.Name, X = i.X, Y = i.Y, MonitorIndex = i.MonitorIndex }).ToList()
                };
                _storage.SaveProfile(clone);
                StatusBarText.Text = $"📋 '{clone.Name}' çoğaltıldı.";
                LoadProfiles();
            }
        }

        private void CtxCreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target != null)
            {
                if (ShortcutHelper.CreateDesktopShortcut(target.Name, out string scPath))
                {
                    string msg = string.Format(LanguageManager.Get("ShortcutCreated"), target.Name);
                    StatusBarText.Text = msg;
                    ShowTrayNotification("ReIcon Pro", msg, ToolTipIcon.Info);
                    MessageBox.Show(msg, "ReIcon Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Kısayol oluşturulamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CtxDelete_Click(object sender, RoutedEventArgs e)
        {
            var target = GetContextMenuTarget(sender);
            if (target != null) PerformDelete(target);
        }

        // ================= QUICK TOOLS ACTIONS =================
        private void ToolRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IntPtr listView = DesktopListView.GetHandle();
                if (listView != IntPtr.Zero)
                {
                    NativeMethods.SendMessage(listView, NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                    NativeMethods.SendMessage(listView, NativeMethods.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                    NativeMethods.SendMessage(listView, 0x000F /* WM_PAINT */, IntPtr.Zero, IntPtr.Zero);
                    StatusBarText.Text = "✅ Masaüstü simgeleri ve önbellek yenilendi.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yenileme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToolAutoArrange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IntPtr listView = DesktopListView.GetHandle();
                if (listView != IntPtr.Zero)
                {
                    bool current = DesktopListView.IsAutoArrangeEnabled(listView);
                    DesktopListView.SetAutoArrange(listView, !current);
                    CheckAutoArrangeState();
                    StatusBarText.Text = $"Otomatik düzenleme: {(!current ? "Açıldı" : "Kapatıldı")}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= SETTINGS ACTIONS =================
        private void LoadSettingsIntoUI()
        {
            var s = _settingsManager.CurrentSettings;
            ChkStartWithWindows.IsChecked = s.StartWithWindows;
            ChkMinimizeToTray.IsChecked = s.MinimizeToTray;
            ChkShowNotifications.IsChecked = s.ShowNotifications;
            ChkAutoBackup.IsChecked = s.AutoBackupOnResolutionChange;
        }

        private void SettingChanged(object sender, RoutedEventArgs e)
        {
            var s = _settingsManager.CurrentSettings;
            s.StartWithWindows = ChkStartWithWindows.IsChecked == true;
            s.MinimizeToTray = ChkMinimizeToTray.IsChecked == true;
            s.ShowNotifications = ChkShowNotifications.IsChecked == true;
            s.AutoBackupOnResolutionChange = ChkAutoBackup.IsChecked == true;

            _settingsManager.SaveSettings();
            StatusBarText.Text = "⚙️ Ayarlar güncellendi.";
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dir = _storage.GetStorageDirectory();
                if (Directory.Exists(dir))
                {
                    Process.Start("explorer.exe", dir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klasör açılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Tüm kayıtlı masaüstü düzenlerini kalıcı olarak silmek istediğinize emin misiniz?", 
                "Tümünü Sıfırla", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                _storage.ClearAllProfiles();
                LoadProfiles();
                StatusBarText.Text = "⚠️ Tüm profiller temizlendi.";
            }
        }

        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://github.com/TheK3R1M/ReIcon-Pro") { UseShellExecute = true });
            }
            catch { }
        }
    }
}
