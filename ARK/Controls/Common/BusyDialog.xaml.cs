using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    /// <summary>
    /// BusyDialog.xaml 的交互逻辑
    /// </summary>
    public partial class BusyDialog : Window
    {
        private Task __async_task;
        public Exception? Exception { get; private set; } = null;
        public BusyDialog(Task task)
        {
            InitializeComponent();
            __async_task = task;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await __async_task;
            }
            catch (Exception ex)
            {
                Exception = ex;
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Close();
        }
    }
}
