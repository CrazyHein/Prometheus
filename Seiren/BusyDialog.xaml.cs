using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ;
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
        private Task<DataSynchronizerState> __data_synchronizer_task;
        private Task<AcquisitionUnitState> __acquisition_unit_task;
        public object Result { get; private set; }
        public BusyDialog(Task<DataSynchronizerState> task)
        {
            InitializeComponent();
            __data_synchronizer_task = task;
        }

        public BusyDialog(Task<AcquisitionUnitState> task)
        {
            InitializeComponent();
            __acquisition_unit_task = task;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(__data_synchronizer_task != null)
                Result = await __data_synchronizer_task;
            else if(__acquisition_unit_task != null)
                Result = await __acquisition_unit_task;
            Close();
        }
    }
}
