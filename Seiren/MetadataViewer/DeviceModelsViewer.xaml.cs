using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// DeviceModelsViewer.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceModelsViewer : UserControl
    {
        private double[] __trick = new double[2] { -1, 1 };
        private int __trick_index = 0;
        public DeviceModelsViewer(ControllerModelCatalogue cmc)
        {
            InitializeComponent();
            LocalExtensionModelsViewer.ItemsSource = cmc.LocalExtensionModels.Values;
            RemoteEthernetModelsViewer.ItemsSource = cmc.RemoteEthernetModels.Values;
        }

        GridRowSizingOptions __row_sizing_options = new GridRowSizingOptions();
        private void LocalExtensionModelsViewer_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
        {
            if (LocalExtensionModelsViewer.GridColumnSizer.GetAutoRowHeight(e.RowIndex, __row_sizing_options, out var autoHeight))
            {
                if (autoHeight > 25)
                {
                    e.Height = autoHeight + 12;
                    e.Handled = true;
                }
            }
        }

        private void RemoteEthernetModelsViewer_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
        {
            if (RemoteEthernetModelsViewer.GridColumnSizer.GetAutoRowHeight(e.RowIndex, __row_sizing_options, out var autoHeight))
            {
                if (autoHeight > 25)
                {
                    e.Height = autoHeight + 12;
                    e.Handled = true;
                }
            }
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            int l = LocalExtensionModelsViewer.Columns.Count - 1;
            int r = RemoteEthernetModelsViewer.Columns.Count - 1;
            
            if ((bool)e.NewValue == true)
            {
                LocalExtensionModelsViewer.Columns[l].Width += __trick[__trick_index%2];
                RemoteEthernetModelsViewer.Columns[r].Width += __trick[__trick_index%2];
                __trick_index++;
            }
            /*
            else
            {
                if (LocalExtensionModelsViewer.Columns[l].Width == double.NaN)
                    LocalExtensionModelsViewer.Columns[l].Width = LocalExtensionModelsViewer.Columns[l].ActualWidth;
                LocalExtensionModelsViewer.Columns[l].Width--;
                RemoteEthernetModelsViewer.Columns[r].Width--;
            }
            */
        }
    }

    class RxTxVariablesText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Dictionary<string, uint> dict = value as Dictionary<string, uint>;
            if (dict != null && dict.Count != 0)
            {
                StringBuilder str = new StringBuilder();
                foreach (string key in dict.Keys)
                    str.AppendFormat("{0} : {1}\n", key, dict[key]);
                str.Remove(str.Length - 1, 1);
                return str;
            }
            else
                return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
