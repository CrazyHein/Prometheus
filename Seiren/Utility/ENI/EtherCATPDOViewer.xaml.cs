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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    /// <summary>
    /// EtherCATPDOs.xaml 的交互逻辑
    /// </summary>
    public partial class EtherCATPDOViewer : Window
    {
        private MainWindow __host;
        public EtherCATPDOViewer(ENIUtilityModel model, MainWindow host)
        {
            InitializeComponent();
            DataContext = model;
            __host = host;
        }

        public bool IsClosed { get; private set; } = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach(var variable in (DataContext as ENIUtilityModel).EtherCATVariables)
                variable.IsChecked = true;
        }

        private void ButtonUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var variable in (DataContext as ENIUtilityModel).EtherCATVariables)
                variable.IsChecked = false;
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            __host.AddEtherCATVariable(VariableViewer.SelectedItem as EtherCATVariableInfo);
        }

        private void AddAllSelectedRecordsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            __host.AddEtherCATVariables((DataContext as ENIUtilityModel).EtherCATVariables.Where(v => v.IsChecked));
        }

        private void AddAllSelectedRecordsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && (DataContext as ENIUtilityModel).EtherCATVariables.Any(v => v.IsChecked);
        }
    }
}
