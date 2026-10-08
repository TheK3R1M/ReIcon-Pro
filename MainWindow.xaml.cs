using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DesktopIconLayoutManager.Core.Engine;
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
        private const string PlaceholderText = "Örn: Çalışma Düzeni / Oyun Düzeni";

        public MainWindow()
        {
            InitializeComponent();
            _engine = new IconLayoutEngine();
            _storage = new StorageManager();

            SetPlaceholder();
            CheckAutoArrangeState();
            LoadProfiles();
        }

        private void SetPlaceholder()
        {
            if (string.IsNullOrWhiteSpace(ProfileNameInput.Text))
            {
                ProfileNameInput.Text = PlaceholderText;
                ProfileNameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            }
        }

        private void ProfileNameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProfileNameInput.Text == PlaceholderText)
            {
                ProfileNameInput.Text = string.Empty;
                ProfileNameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
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
                BadgeDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                BadgeText.Text = "Auto-Arrange: Aktif (Geri yüklemede otomatik kapatılacaktır)";
            }
            else
            {
                BadgeDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                BadgeText.Text = "Auto-Arrange: Pasif (Düzen kaydetmeye hazır)";
            }
        }

        private void LoadProfiles()
        {
            List<IconLayoutProfile> profiles = _storage.GetAllProfiles();
            ProfilesListView.ItemsSource = profiles;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string inputName = ProfileNameInput.Text;
                if (inputName == PlaceholderText || string.IsNullOrWhiteSpace(inputName))
                {
                    inputName = $"Düzen_{DateTime.Now:yyyyMMdd_HHmmss}";
                }

                IconLayoutProfile profile = _engine.CaptureCurrentLayout(inputName);
                _storage.SaveProfile(profile);

                StatusBarText.Text = $"✅ '{profile.Name}' başarıyla kaydedildi ({profile.Icons.Count} simge).";
                ProfileNameInput.Text = string.Empty;
                SetPlaceholder();
                LoadProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Layout kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusBarText.Text = $"❌ Kaydetme hatası: {ex.Message}";
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IconLayoutProfile profile)
            {
                try
                {
                    bool success = _engine.RestoreLayout(profile, out string msg);
                    CheckAutoArrangeState();

                    if (success)
                    {
                        StatusBarText.Text = $"⚡ '{profile.Name}' geri yüklendi. {msg}";
                        MessageBox.Show($"Düzen başarıyla geri yüklendi!\n{msg}", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
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
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is IconLayoutProfile profile)
            {
                var result = MessageBox.Show($"'{profile.Name}' profilini silmek istediğinize emin misiniz?", "Profil Sil", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _storage.DeleteProfile(profile.Id);
                    StatusBarText.Text = $"🗑️ '{profile.Name}' düzeni silindi.";
                    LoadProfiles();
                }
            }
        }
    }
}
