using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// BusyDialog.xaml 的交互逻辑
    /// </summary>
    public partial class BusyDialog : Window
    {
        private Task<DataSynchronizerState> __task ;
        public object Result { get; private set; }
        public BusyDialog(Task<DataSynchronizerState> task)
        {
            InitializeComponent();
            __task = task;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Result = await __task;
            Close();
        }
    }
}
