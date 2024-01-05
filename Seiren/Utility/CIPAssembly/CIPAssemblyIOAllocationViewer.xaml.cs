using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    /// <summary>
    /// CIPAssemblyIOAllocationViewer.xaml 的交互逻辑
    /// </summary>
    public partial class CIPAssemblyIOAllocationViewer : Window
    {
        private MainWindow __host;
        public CIPAssemblyIOAllocationViewer(CIPAssemblyIOAllocationModel model, MainWindow host)
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
            foreach (var variable in (DataContext as CIPAssemblyIOAllocationModel).CIPAssemblyIOs)
                variable.IsChecked = true;
        }

        private void ButtonUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var variable in (DataContext as CIPAssemblyIOAllocationModel).CIPAssemblyIOs)
                variable.IsChecked = false;
        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (CIPAssemblyIOInfo variable in VariableViewer.SelectedItems)
                variable.IsChecked = true;
        }

        private void ButtonUnselect_Click(object sender, RoutedEventArgs e)
        {
            foreach (CIPAssemblyIOInfo variable in VariableViewer.SelectedItems)
                variable.IsChecked = false;
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            __host.AddCIPAssemblyIO(VariableViewer.SelectedItem as CIPAssemblyIOInfo);
        }

        private void AddAllSelectedRecordsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            __host.AddCIPAssemblyIOs((DataContext as CIPAssemblyIOAllocationModel).CIPAssemblyIOs.Where(v => v.IsChecked));
        }

        private void AddAllSelectedRecordsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && (DataContext as CIPAssemblyIOAllocationModel).CIPAssemblyIOs.Any(v => v.IsChecked);
        }
    }
}
