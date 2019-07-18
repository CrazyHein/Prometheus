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

            __area_views = new Dictionary<IO_LIST_PDO_AREA_T, ListView>(6);
            __area_views.Add(IO_LIST_PDO_AREA_T.TX_DIAGNOSTIC, __lsv_tx_diagnostic_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.TX_BIT, __lsv_tx_bit_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.TX_BLOCK, __lsv_tx_block_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.RX_CONTROL, __lsv_rx_control_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.RX_BIT, __lsv_rx_bit_area);
            __area_views.Add(IO_LIST_PDO_AREA_T.RX_BLOCK, __lsv_rx_block_area);      
        }

        private void __on_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void __on_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
            ListView view = __area_views[area];
            int selectedIndex = view.SelectedIndex;
            ObservableCollection<ObjectItemDataModel> itemSource = view.ItemsSource as ObservableCollection<ObjectItemDataModel>;
            if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as PDOCollectionDataModel).DataHelper.RemovePDOItem(selectedIndex, area);
                    itemSource.RemoveAt(selectedIndex);
                    (DataContext as PDOCollectionDataModel).UpdateAreaActualSize(area);
                }
                catch (IOListParseExcepetion exception)
                {
                    string message;
                    if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while removing controller pdo mappings . \n{0}", exception.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while removing controller pdo mappings . \n{0}", exception.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true;
        }

        private void __on_modify_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void __on_move_element_up_command_executed(object sender, ExecutedRoutedEventArgs e)
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
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void __on_move_element_down_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
            ListView view = __area_views[area];
            int selectedIndex = view.SelectedIndex;
            ObservableCollection<ObjectItemDataModel> itemSource = view.ItemsSource as ObservableCollection<ObjectItemDataModel>;

            try
            {
                (DataContext as PDOCollectionDataModel).DataHelper.SwapPDOItem(area, selectedIndex, selectedIndex + 1);
                ObjectItemDataModel temp = itemSource[selectedIndex];
                itemSource[selectedIndex] = itemSource[selectedIndex + 1];
                itemSource[selectedIndex + 1] = temp;
                view.SelectedIndex = selectedIndex + 1;
            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void __on_insert_element_before_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void __on_remove_modify_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                ListView view = __area_views[area];
                e.CanExecute = view.SelectedItem != null;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_move_element_up_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                ListView view = __area_views[area];
                ObservableCollection<ObjectItemDataModel> itemSource = view.ItemsSource as ObservableCollection<ObjectItemDataModel>;
                e.CanExecute = view.SelectedItem != null && view.SelectedIndex - 1 >= 0;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_move_element_down_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                ListView view = __area_views[area];
                ObservableCollection<ObjectItemDataModel> itemSource = view.ItemsSource as ObservableCollection<ObjectItemDataModel>;
                e.CanExecute = view.SelectedItem != null && view.SelectedIndex + 1 < itemSource.Count;
            }
            catch
            {
                e.CanExecute = false;
            }
        }
    }
}
