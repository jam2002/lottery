using System;
using System.Windows;

namespace Lottery.App
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Error encountered! Please contact support." + Environment.NewLine + e.Exception.Message);
            e.Handled = true;
        }
    }
}
