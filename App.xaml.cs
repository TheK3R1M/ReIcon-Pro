using System;
using System.Windows;
using System.Windows.Threading;

namespace DesktopIconLayoutManager
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show($"Uygulama Hatalı Durumla Karşılaştı:\n\n{e.Exception.Message}\n\nDetay:\n{e.Exception.StackTrace}", 
                "ReIcon Pro - Hata", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                System.Windows.MessageBox.Show($"Kritik Uygulama Hatası:\n\n{ex.Message}", 
                    "ReIcon Pro - Kritik Hata", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }
}
