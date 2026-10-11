using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace DesktopIconLayoutManager.Core.Helpers
{
    public class PromptDialog : Window
    {
        private readonly TextBox _inputBox;
        public string ResponseText => _inputBox.Text.Trim();

        public PromptDialog(string title, string promptText, string defaultText = "")
        {
            Title = title;
            Width = 440;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");

            var mainGrid = new Grid { Margin = new Thickness(20) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var promptBlock = new TextBlock
            {
                Text = promptText,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(promptBlock, 0);
            mainGrid.Children.Add(promptBlock);

            _inputBox = new TextBox
            {
                Text = defaultText,
                Height = 38,
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(_inputBox, 1);
            mainGrid.Children.Add(_inputBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            var okButton = new Button
            {
                Content = "Tamam",
                Width = 90,
                Height = 34,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 8, 0)
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };

            var cancelButton = new Button
            {
                Content = "İptal",
                Width = 90,
                Height = 34,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC")),
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
            Loaded += (s, e) =>
            {
                _inputBox.Focus();
                _inputBox.SelectAll();
            };
        }

        public static string? Show(string title, string prompt, string defaultVal = "", Window? owner = null)
        {
            var dlg = new PromptDialog(title, prompt, defaultVal);
            if (owner != null) dlg.Owner = owner;
            return dlg.ShowDialog() == true ? dlg.ResponseText : null;
        }
    }
}
