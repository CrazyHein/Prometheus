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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Context
{
    /// <summary>
    /// Labels.xaml 的交互逻辑
    /// </summary>
    public partial class LabelsControl : UserControl
    {
        public LabelsControl()
        {
            InitializeComponent();
            sfLabelsViewer.ItemsSource = ContextModel.Tags?.Values;
        }

        GridRowSizingOptions __row_sizing_options = new GridRowSizingOptions();
        private void sfLabelsViewer_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
        {
            if (sfLabelsViewer.GridColumnSizer.GetAutoRowHeight(e.RowIndex, __row_sizing_options, out var autoHeight))
            {
                if (autoHeight > 25)
                {
                    e.Height = autoHeight + 12;
                    e.Handled = true;
                }
            }
        }
    }
}
