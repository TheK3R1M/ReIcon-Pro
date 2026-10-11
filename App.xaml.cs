using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DesktopIconLayoutManager.Core.Engine;
using DesktopIconLayoutManager.Core.Models;

namespace DesktopIconLayoutManager
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Handle CLI commands if arguments are provided
            if (e.Args.Length > 0)
            {
                HandleCommandLine(e.Args);
            }
        }

        private void HandleCommandLine(string[] args)
        {
            try
            {
                var engine = new IconLayoutEngine();
                var storage = new StorageManager();

                string cmd = args[0].ToLowerInvariant();
                string target = args.Length > 1 ? args[1] : string.Empty;

                if (cmd == "/restore" || cmd == "/r" || cmd == "--restore")
                {
                    var profiles = storage.GetAllProfiles();
                    IconLayoutProfile? match = null;

                    if (!string.IsNullOrEmpty(target))
                    {
                        match = profiles.FirstOrDefault(p => p.Name.Equals(target, StringComparison.OrdinalIgnoreCase))
                             ?? profiles.FirstOrDefault(p => p.Id.Equals(target, StringComparison.OrdinalIgnoreCase));
                    }
                    else if (profiles.Count > 0)
                    {
                        match = profiles[0]; // Latest profile
                    }

                    if (match != null)
                    {
                        engine.RestoreLayout(match, out _);
                    }
                    Shutdown();
                    return;
                }
                else if (cmd == "/save" || cmd == "/s" || cmd == "--save")
                {
                    string name = string.IsNullOrEmpty(target) ? $"Düzen_{DateTime.Now:yyyyMMdd_HHmmss}" : target;
                    var profile = engine.CaptureCurrentLayout(name);
                    storage.SaveProfile(profile);
                    Shutdown();
                    return;
                }
            }
            catch { }

            Shutdown();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show($"Uygulama Hatası:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", 
                "ReIcon Pro - Hata", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                System.Windows.MessageBox.Show($"Kritik Hata:\n\n{ex.Message}", 
                    "ReIcon Pro - Kritik Hata", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }
}
