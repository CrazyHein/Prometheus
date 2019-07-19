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
        private ListCollectionView __available_objects_view;
        private ObjectItemFilter __object_item_filter;

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

            __available_objects_view = new ListCollectionView(dataModel.AvailableObjects);
            __object_item_filter = dataModel.AvailableObjectItemFilter;
            __available_objects_view.Filter = new Predicate<object>(__object_item_filter.FilterItem);
            __lsv_object_collection.ItemsSource = __available_objects_view;

            __available_objects_view.LiveFilteringProperties.Add("DataType");
            __available_objects_view.LiveFilteringProperties.Add("Binding");
            __available_objects_view.LiveFilteringProperties.Add("FriendlyName");
            __available_objects_view.IsLiveFiltering = true;

        }

        private void __on_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
            ListView view = __area_views[area];
            ObservableCollection<ObjectItemDataModel> itemSource = view.ItemsSource as ObservableCollection<ObjectItemDataModel>;
            uint objectIndex = (__lsv_object_collection.SelectedItem as ObjectItemDataModel).Index;

            try
            {
                IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T selectedObject = (DataContext as PDOCollectionDataModel).DataHelper.IOObjectDictionary[objectIndex];
                (DataContext as PDOCollectionDataModel).DataHelper.AppendPDOItem(area, selectedObject);
                (DataContext as PDOCollectionDataModel).UpdateAreaActualSize(area);
                itemSource.Add(__lsv_object_collection.SelectedItem as ObjectItemDataModel);
                //view.ScrollIntoView(view.Items[view.Items.Count - 1]);
                bool res = __lsv_object_collection.SelectedItem == view.SelectedItem;
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
                    (DataContext as PDOCollectionDataModel).UpdateAreaActualSize(area);
                    itemSource.RemoveAt(selectedIndex);    
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

        private void __on_replace_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void __on_insert_element_before_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
            ListView view = __area_views[area];
            int selectedIndex = view.SelectedIndex;
            ObservableCollection<ObjectItemDataModel> itemSource = view.ItemsSource as ObservableCollection<ObjectItemDataModel>;
            uint objectIndex = (__lsv_object_collection.SelectedItem as ObjectItemDataModel).Index;
            
            try
            {
                IO_LIST_OBJECT_COLLECTION_T.OBJECT_DEFINITION_T selectedObject = (DataContext as PDOCollectionDataModel).DataHelper.IOObjectDictionary[objectIndex];
                (DataContext as PDOCollectionDataModel).DataHelper.InsertPDOItem(selectedIndex, area, selectedObject);
                (DataContext as PDOCollectionDataModel).UpdateAreaActualSize(area);
                itemSource.Insert(selectedIndex, __lsv_object_collection.SelectedItem as ObjectItemDataModel);
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

        private void __on_move_up_element_command_executed(object sender, ExecutedRoutedEventArgs e)
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

        private void __on_move_down_element_command_executed(object sender, ExecutedRoutedEventArgs e)
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

        private void __on_replace_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                ListView view = __area_views[area];
                e.CanExecute = __lsv_object_collection.SelectedItem != null && view.SelectedItem != null;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_add_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = __lsv_object_collection.SelectedItem != null;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_remove_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
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

        private void __on_move_up_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
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

        private void __on_move_down_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
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

        private void __on_enable_friendly_name_filter_click(object sender, RoutedEventArgs e)
        {
            if (__chk_enable_friendly_name_filter.IsChecked == true)
            {
                __object_item_filter.FriendlyName = (DataContext as PDOCollectionDataModel).FilterFriendlyName;
                __object_item_filter.EnableFriendlyNameFilter();
            }
            else
                __object_item_filter.DisableFriendlyNameFilter();

            __available_objects_view.Refresh();
        }

        private void __on_new_filter_friendly_name_enter(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                BindingExpression binging = __txt_filter_friendly_name.GetBindingExpression(TextBox.TextProperty);
                binging.UpdateSource();
                __object_item_filter.FriendlyName = (DataContext as PDOCollectionDataModel).FilterFriendlyName;
                __available_objects_view.Refresh();
            }
        }
    }
}
