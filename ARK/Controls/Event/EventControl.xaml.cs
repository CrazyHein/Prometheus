using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using Syncfusion.UI.Xaml.Grid;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls
{
    /// <summary>
    /// EventControl.xaml 的交互逻辑
    /// </summary>
    public partial class EventControl : UserControl
    {
        public EventControl(EventModel evt)
        {
            InitializeComponent();
            DataContext = evt;
        }

        public void ResetDataModel(EventModel evt)
        {
            DataContext = evt;
        }

        GridRowSizingOptions __row_sizing_options = new GridRowSizingOptions();
        private void SfDataGrid_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
        {
            if (sfEventParametersViewer.GridColumnSizer.GetAutoRowHeight(e.RowIndex, __row_sizing_options, out var autoHeight))
            {
                if (autoHeight > 25)
                {
                    e.Height = autoHeight + 12;
                    e.Handled = true;
                }
            }
        }

        private void ExpressionHelperCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExpressionHelper helper = new ExpressionHelper((sfEventParametersViewer.SelectedItem as EventParameter).Value);
            var ret= helper.ShowDialog();
            if (ret == true)
            {
                (sfEventParametersViewer.SelectedItem as EventParameter).Value = helper.Expression.ToString();
            }
        }

        private void ExpressionHelperCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfEventParametersViewer.SelectedItem != null;
        }
    }
}
