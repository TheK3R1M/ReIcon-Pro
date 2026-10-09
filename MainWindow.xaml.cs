using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DesktopIconLayoutManager.Core.Engine;
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
        private readonly NotifyIcon _notifyIcon;
        private List<IconLayoutProfile> _allProfiles = new();
        private bool _isExitTriggered = false;

        public MainWindow()
        {
            InitializeComponent();
            _engine = new IconLayoutEngine();
            _storage = new StorageManager();

            // Setup System Tray NotifyIcon
            _notifyIcon = new NotifyIcon
            {
                Text = "ReIcon Pro",
                Visible = true
            };

            LoadAppIconToNotifyIcon();
            BuildTrayMenu();

            _notifyIcon.DoubleClick += (s, e) => ShowWindowFromTray();

            LanguageManager.OnLanguageChanged += RefreshLocalizedUI;

            RefreshLocalizedUI();
            CheckAutoArrangeState();
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
            try
            {
                _notifyIcon.ShowBalloonTip(2000, title, text, icon);
            }
            catch
            {
                // Safe fallback for Windows 11 notifications
            }
        }

        private void BuildTrayMenu()
        {
            var contextMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem(LanguageManager.Get("TrayOpen"), null, (s, e) => ShowWindowFromTray());
            var quickSaveItem = new ToolStripMenuItem(LanguageManager.Get("TrayQuickSave"), null, (s, e) => QuickSaveFromTray());
            var restoreLastItem = new ToolStripMenuItem(LanguageManager.Get("TrayRestoreLast"), null, (s, e) => QuickRestoreLastFromTray());
            var langItem = new ToolStripMenuItem("🌐 Language (Dil): " + LanguageManager.CurrentLanguage, null, (s, e) => ToggleLanguageFromTray());
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
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                ShowTrayNotification("ReIcon Pro", LanguageManager.CurrentLanguage == AppLanguage.TR ? "Uygulama tepsisinde çalışmaya devam ediyor." : "Running in system tray.");
            }
        }

        private void RootWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExitTriggered)
            {
                e.Cancel = true;
                Hide();
                ShowTrayNotification("ReIcon Pro", LanguageManager.CurrentLanguage == AppLanguage.TR ? "Uygulama arka planda simge durumuna küçültüldü." : "Minimized to system tray.");
            }
        }

        private void RefreshLocalizedUI()
        {
            TxtAppTitle.Text = LanguageManager.Get("AppTitle");
            TxtAppSubtitle.Text = LanguageManager.Get("AppSubtitle");
            TxtNewLayoutTitle.Text = LanguageManager.Get("NewLayoutTitle");
            SaveButton.Content = LanguageManager.Get("SaveButton");
            TxtSavedProfilesTitle.Text = LanguageManager.Get("SavedProfilesTitle");
            HeaderName.Content = LanguageManager.Get("ColName");
            HeaderDate.Content = LanguageManager.Get("ColDate");
            HeaderResolution.Content = LanguageManager.Get("ColResolution");
            HeaderIcons.Content = LanguageManager.Get("ColIcons");
            HeaderActions.Content = LanguageManager.Get("ColActions");
            StatusBarText.Text = LanguageManager.Get("StatusReady");

            if (LanguageManager.CurrentLanguage == AppLanguage.TR)
            {
                BtnLangTR.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1"));
                BtnLangTR.Foreground = System.Windows.Media.Brushes.White;
                BtnLangEN.Background = System.Windows.Media.Brushes.Transparent;
                BtnLangEN.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            }
            else
            {
                BtnLangEN.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1"));
                BtnLangEN.Foreground = System.Windows.Media.Brushes.White;
                BtnLangTR.Background = System.Windows.Media.Brushes.Transparent;
                BtnLangTR.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            }

            SetPlaceholder();
            CheckAutoArrangeState();
            BuildTrayMenu();
        }

        private void BtnLangTR_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage(AppLanguage.TR);
        }

        private void BtnLangEN_Click(object sender, RoutedEventArgs e)
        {
            LanguageManager.SetLanguage(AppLanguage.EN);
        }

        private void SetPlaceholder()
        {
            if (string.IsNullOrWhiteSpace(ProfileNameInput.Text) || ProfileNameInput.Text == "Örn: Çalışma Düzeni / Oyun Düzeni" || ProfileNameInput.Text == "e.g. Work Layout / Gaming Layout")
            {
                ProfileNameInput.Text = LanguageManager.Get("LayoutNamePlaceholder");
                ProfileNameInput.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            }
        }

        private void ProfileNameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            string ph = LanguageManager.Get("LayoutNamePlaceholder");
            if (ProfileNameInput.Text == ph || ProfileNameInput.Text == "Örn: Çalışma Düzeni / Oyun Düzeni" || ProfileNameInput.Text == "e.g. Work Layout / Gaming Layout")
            {
                ProfileNameInput.Text = string.Empty;
                ProfileNameInput.Foreground = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
            }
        }

        private void ProfileNameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            SetPlaceholder();
        }

        private void CheckAutoArrangeState()
        {
            IntPtr listViewHandle = DesktopListView.GetHandle();
            bool isAutoArrange = DesktopListView.IsAutoArrangeEnabled(listViewHandle);

            if (isAutoArrange)
            {
                BadgeDot.Fill = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                BadgeText.Text = LanguageManager.Get("AutoArrangeActive");
            }
            else
            {
                BadgeDot.Fill = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                BadgeText.Text = LanguageManager.Get("AutoArrangeDisabled");
            }
        }

        private void LoadProfiles()
        {
            _allProfiles = _storage.GetAllProfiles();
            ApplyFilter();
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(query))
            {
                ProfilesListView.ItemsSource = _allProfiles;
            }
            else
            {
                ProfilesListView.ItemsSource = _allProfiles.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSave(ProfileNameInput.Text);
        }

        private void QuickSaveFromTray()
        {
            PerformSave(string.Empty);
        }

        private void PerformSave(string customName)
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
                var result = MessageBox.Show(string.Format(LanguageManager.Get("DeleteConfirmMsg"), profile.Name), LanguageManager.Get("DeleteConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _storage.DeleteProfile(profile.Id);
                    StatusBarText.Text = string.Format(LanguageManager.Get("DeleteSuccess"), profile.Name);
                    LoadProfiles();
                }
            }
        }
    }
}
