using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    /// <summary>
    /// PDOCollectionDataControl.xaml 的交互逻辑
    /// </summary>
    public partial class PDOCollectionDataControl : UserControl
    {
        private Dictionary<IO_LIST_PDO_AREA_T, ListView> __area_views;

        public PDOCollectionDataControl(PDOCollectionDataModel dataModel)
        {
            InitializeComponent();
            DataContext = dataModel;

            __area_views = new Dictionary<IO_LIST_PDO_AREA_T, ListView>();
            __area_views.Add(IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC, __lsv_tx_diagnostic_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.TX_BIT, __lsv_tx_bit_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.TX_BLOCK, __lsv_tx_block_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.RX_CONTROL, __lsv_rx_control_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.RX_BIT, __lsv_rx_bit_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.RX_BLOCK, __lsv_rx_block_area);
        }

        private void __on_pdo_item_move_up_click(object sender, RoutedEventArgs e)
        {
            IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
            ListView view = __area_views[area];
            int selectedIndex = view.SelectedIndex;
            ObservableCollection<ObjectItemDataModel> itemSource = view.ItemsSource as ObservableCollection<ObjectItemDataModel>;
            
            try
            {
                (DataContext as PDOCollectionDataModel).DataHelper.SwapPDOItem(area, selectedIndex, selectedIndex - 1);
                ObjectItemDataModel temp = itemSource[selectedIndex];
                itemSource[selectedIndex] = itemSource[selectedIndex - 1];
                itemSource[selectedIndex - 1] = temp;
                view.SelectedIndex = selectedIndex - 1;
            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while swap controller pdo mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while swap controller pdo mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
