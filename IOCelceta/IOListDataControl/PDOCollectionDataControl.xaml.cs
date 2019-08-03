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

        enum __INTLK_LOGIC_DEFINITION_LIST_TASK
        {
            EDIT,
            APPEND,
            INSERT
        }

        private __INTLK_LOGIC_DEFINITION_LIST_TASK __task;
        private int __task_parameter;

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

            __available_objects_view = new ListCollectionView(dataModel.AvailableObjects as ObservableCollection<ObjectItemDataModel>);
            __object_item_filter = dataModel.AvailableObjectItemFilter;
            __available_objects_view.Filter = new Predicate<object>(__object_item_filter.FilterItem);
            __lsv_object_collection.ItemsSource = __available_objects_view;

            __available_objects_view.LiveFilteringProperties.Add(ObjectCollectionDataModel.DataTypePropertyName);
            __available_objects_view.LiveFilteringProperties.Add(ObjectCollectionDataModel.BindingModulePropertyName);
            __available_objects_view.LiveFilteringProperties.Add(ObjectCollectionDataModel.VariablePropertyName);
            __available_objects_view.IsLiveFiltering = true;

        }

        private void __on_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;   
            try
            {
                if (tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)(tag);
                    (DataContext as PDOCollectionDataModel).AppendPDOMapping(area, __lsv_object_collection.SelectedItem as ObjectItemDataModel);
                    __area_views[area].SelectedIndex = __area_views[area].Items.Count - 1;
                    __area_views[area].ScrollIntoView(__area_views[area].SelectedItem);
                }
                else
                {
                    __task = __INTLK_LOGIC_DEFINITION_LIST_TASK.APPEND;
                    __task_parameter = 0;
                    __lsb_interlock_logic_definitions.IsEnabled = false;
                    __grid_edit_intlk_logic_definition.IsEnabled = true;
                }
            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while adding controller pdo/intlk mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while adding controller pdo/intlk mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void __on_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                try
                {
                    if (tag != null)
                    {
                        IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)(tag);
                        int selectedIndex = __area_views[area].SelectedIndex;
                        (DataContext as PDOCollectionDataModel).RemovePDOMapping(selectedIndex, area);
                    }
                    else
                        (DataContext as PDOCollectionDataModel).RemoveIntlklogicDefinition(__lsb_interlock_logic_definitions.SelectedIndex);
                }
                catch (IOListParseExcepetion exception)
                {
                    string message;
                    if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while removing controller pdo/intlk mappings . \n{0}", exception.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while removing controller pdo/intlk mappings . \n{0}", exception.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true;
        }

        private void __on_replace_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
            ListView view = __area_views[area];
            int selectedIndex = view.SelectedIndex;
            try
            {
                (DataContext as PDOCollectionDataModel).ReplacePDOMapping(area, selectedIndex, __lsv_object_collection.SelectedItem as ObjectItemDataModel);
            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while replacing controller pdo mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while replacing controller pdo mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void __on_insert_element_before_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
            try
            {
                if(tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)(tag);
                    int selectedIndex = __area_views[area].SelectedIndex;
                    (DataContext as PDOCollectionDataModel).InsertPDOMapping(selectedIndex, area, __lsv_object_collection.SelectedItem as ObjectItemDataModel);
                    __area_views[area].SelectedIndex = selectedIndex;
                    __area_views[area].ScrollIntoView(__area_views[area].SelectedItem);
                }
                else
                {
                    __task = __INTLK_LOGIC_DEFINITION_LIST_TASK.INSERT;
                    __task_parameter = __lsb_interlock_logic_definitions.SelectedIndex;
                    __lsb_interlock_logic_definitions.IsEnabled = false;
                    __grid_edit_intlk_logic_definition.IsEnabled = true;
                }
            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while inserting controller pdo/intlk mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while inserting controller pdo/intlk mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void __on_move_up_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            int selectedIndex;
            object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
            try
            {
                if(tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)(tag);
                    ListView view = __area_views[area];
                    selectedIndex = view.SelectedIndex;
                    (DataContext as PDOCollectionDataModel).SwapPDOMapping(area, selectedIndex, selectedIndex - 1);
                    view.SelectedIndex = selectedIndex - 1;
                    view.ScrollIntoView(view.SelectedItem);
                }
                else
                {
                    selectedIndex = __lsb_interlock_logic_definitions.SelectedIndex;
                    (DataContext as PDOCollectionDataModel).SwapIntlklogicDefinition(selectedIndex, selectedIndex - 1);
                    __lsb_interlock_logic_definitions.SelectedIndex = selectedIndex - 1;
                    __lsb_interlock_logic_definitions.ScrollIntoView(__lsb_interlock_logic_definitions.SelectedItem);
                } 
            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo/intlk mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo/intlk mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void __on_move_down_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            int selectedIndex;
            object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
            try
            {
                if(tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                    ListView view = __area_views[area];
                    selectedIndex = view.SelectedIndex;
                    (DataContext as PDOCollectionDataModel).SwapPDOMapping(area, selectedIndex, selectedIndex + 1);
                    view.SelectedIndex = selectedIndex + 1;
                    view.ScrollIntoView(view.SelectedItem);
                }
                else
                {
                    selectedIndex = __lsb_interlock_logic_definitions.SelectedIndex;
                    (DataContext as PDOCollectionDataModel).SwapIntlklogicDefinition(selectedIndex, selectedIndex + 1);
                    __lsb_interlock_logic_definitions.SelectedIndex = selectedIndex + 1;
                    __lsb_interlock_logic_definitions.ScrollIntoView(__lsb_interlock_logic_definitions.SelectedItem);
                }

            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo/intlk mappings . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while swapping controller pdo/intlk mappings . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void __on_group_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
            if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as PDOCollectionDataModel).GroupPDOMappingByBindingModule(area);
                }
                catch (IOListParseExcepetion exception)
                {
                    string message;
                    if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while grouping controller pdo mappings . \n{0}", exception.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while grouping controller pdo mappings . \n{0}", exception.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true;
        }

        private void __on_edit_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            __task = __INTLK_LOGIC_DEFINITION_LIST_TASK.EDIT;
            __task_parameter = __lsb_interlock_logic_definitions.SelectedIndex;

            __txt_edit_intlk_logic_name.Text = (__lsb_interlock_logic_definitions.SelectedItem as IntlklogicDefinition).Name;
            __txt_edit_intlk_logic_target.Text = (__lsb_interlock_logic_definitions.SelectedItem as IntlklogicDefinition).TargetObjectIndexList;
            __txt_edit_intlk_logic_statement.Text = (__lsb_interlock_logic_definitions.SelectedItem as IntlklogicDefinition).Statement.ToString();

            __lsb_interlock_logic_definitions.IsEnabled = false;
            __grid_edit_intlk_logic_definition.IsEnabled = true;
        }

        private void __on_insert_element_before_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                if (tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                    ListView view = __area_views[area];
                    e.CanExecute = __lsv_object_collection.SelectedItem != null && view.SelectedItem != null;
                }
                else
                    e.CanExecute = __lsb_interlock_logic_definitions.SelectedItem != null &&
                        __lsb_interlock_logic_definitions.IsEnabled == true;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_replace_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                if (tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                    ListView view = __area_views[area];
                    e.CanExecute = __lsv_object_collection.SelectedItem != null && view.SelectedItem != null;
                }
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
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                if (tag != null)
                    e.CanExecute = __lsv_object_collection.SelectedItem != null;
                else
                    e.CanExecute = __lsb_interlock_logic_definitions.IsEnabled == true;
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
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                if (tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)tag;
                    ListView view = __area_views[area];
                    e.CanExecute = view.SelectedItem != null;
                }
                else
                    e.CanExecute = __lsb_interlock_logic_definitions.SelectedItem != null &&
                        __lsb_interlock_logic_definitions.IsEnabled == true;
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
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                if (tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                    ListView view = __area_views[area];
                    e.CanExecute = view.SelectedItem != null && view.SelectedIndex - 1 >= 0;
                }
                else
                    e.CanExecute = __lsb_interlock_logic_definitions.IsEnabled == true && 
                        __lsb_interlock_logic_definitions.SelectedItem != null && __lsb_interlock_logic_definitions.SelectedIndex - 1 >= 0;
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
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                if (tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                    ListView view = __area_views[area];
                    e.CanExecute = view.SelectedItem != null && view.SelectedIndex + 1 < view.Items.Count;
                }
                else
                    e.CanExecute = __lsb_interlock_logic_definitions.IsEnabled == true &&
                        __lsb_interlock_logic_definitions.SelectedItem != null && 
                        __lsb_interlock_logic_definitions.SelectedIndex + 1 < __lsb_interlock_logic_definitions.Items.Count;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_group_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                object tag = (__tab_pdo_container.SelectedItem as TabItem).Tag;
                if (tag != null)
                {
                    IO_LIST_PDO_AREA_T area = (IO_LIST_PDO_AREA_T)((__tab_pdo_container.SelectedItem as TabItem).Tag);
                    e.CanExecute = area == IO_LIST_PDO_AREA_T.RX_BIT || area == IO_LIST_PDO_AREA_T.TX_BIT;
                }
                else
                    e.CanExecute = false;
            }
            catch
            {
                e.CanExecute = false;
            }
        }
        private void __on_edit_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = (__tab_pdo_container.SelectedItem as TabItem).Tag == null &&
                    __lsb_interlock_logic_definitions.IsEnabled == true && __lsb_interlock_logic_definitions.SelectedItem != null;
            }
            catch
            {
                e.CanExecute = false;
            }
        }


        private void __on_enable_variable_name_filter_click(object sender, RoutedEventArgs e)
        {
            if (__chk_enable_variable_name_filter.IsChecked == true)
            {
                __object_item_filter.VariableName = (DataContext as PDOCollectionDataModel).FilterVariableName;
                __object_item_filter.EnableVariableNameFilter();
            }
            else
                __object_item_filter.DisableVariableNameFilter();

            __available_objects_view.Refresh();
        }

        private void __on_enable_data_type_name_filter_click(object sender, RoutedEventArgs e)
        {
            if (__chk_enable_data_type_name_filter.IsChecked == true)
            {
                var dataTypeCatalogue = (DataContext as PDOCollectionDataModel).DataHelper.DataTypeCatalogue.DataTypes;
                if (dataTypeCatalogue.Keys.Contains((DataContext as PDOCollectionDataModel).FilterDataTypeName))
                    __object_item_filter.DataType = dataTypeCatalogue[(DataContext as PDOCollectionDataModel).FilterDataTypeName];
                else
                    __object_item_filter.DataType = null;
                __object_item_filter.EnableDataTypeFilter();
            }
            else
                __object_item_filter.DisableDataTypeFilter();

            __available_objects_view.Refresh();
        }

        private void __on_enable_binding_module_name_filter_click(object sender, RoutedEventArgs e)
        {
            if (__chk_enable_binding_module_name_filter.IsChecked == true)
            {
                __object_item_filter.BindingModule = (DataContext as PDOCollectionDataModel).FilterModuleName;
                __object_item_filter.EnableBindingModuleFilter();
            }
            else
                __object_item_filter.DisableBindingModuleFilter();

            __available_objects_view.Refresh();
        }

        private void __on_new_filter_variable_name_enter(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                BindingExpression binging = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
                binging.UpdateSource();
                __object_item_filter.VariableName = (DataContext as PDOCollectionDataModel).FilterVariableName;
                __available_objects_view.Refresh();
            }
        }

        private void __on_new_filter_data_type_name_enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression binging = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
                binging.UpdateSource();

                var dataTypeCatalogue = (DataContext as PDOCollectionDataModel).DataHelper.DataTypeCatalogue.DataTypes;
                if (dataTypeCatalogue.Keys.Contains((DataContext as PDOCollectionDataModel).FilterDataTypeName))
                    __object_item_filter.DataType = dataTypeCatalogue[(DataContext as PDOCollectionDataModel).FilterDataTypeName];
                else
                    __object_item_filter.DataType = null;
                __available_objects_view.Refresh();
            }
        }

        private void __on_new_filter_binding_module_name_enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression binging = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
                binging.UpdateSource();
                __object_item_filter.BindingModule = (DataContext as PDOCollectionDataModel).FilterModuleName;
                __available_objects_view.Refresh();
            }
        }

        private void __on_data_binding_error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                (DataContext as PDOCollectionDataModel).FieldDataBindingErrors++;
            else if (e.Action == ValidationErrorEventAction.Removed)
                (DataContext as PDOCollectionDataModel).FieldDataBindingErrors--;
        }

        private void __on_commit_intlk_logic_edit(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (__task)
                {
                    case __INTLK_LOGIC_DEFINITION_LIST_TASK.APPEND:
                        (DataContext as PDOCollectionDataModel).AppendIntlklogicDefinition(__txt_edit_intlk_logic_name.Text,
                            __txt_edit_intlk_logic_target.Text, __txt_edit_intlk_logic_statement.Text);
                        __lsb_interlock_logic_definitions.SelectedIndex = __lsb_interlock_logic_definitions.Items.Count - 1;
                        __lsb_interlock_logic_definitions.ScrollIntoView(__lsb_interlock_logic_definitions.SelectedItem);
                        break;
                    case __INTLK_LOGIC_DEFINITION_LIST_TASK.INSERT:
                        (DataContext as PDOCollectionDataModel).InsertIntlklogicDefinition(__task_parameter, __txt_edit_intlk_logic_name.Text,
                            __txt_edit_intlk_logic_target.Text, __txt_edit_intlk_logic_statement.Text);
                        __lsb_interlock_logic_definitions.SelectedIndex = __task_parameter;
                        __lsb_interlock_logic_definitions.ScrollIntoView(__lsb_interlock_logic_definitions.SelectedItem);
                        break;
                    case __INTLK_LOGIC_DEFINITION_LIST_TASK.EDIT:
                        (DataContext as PDOCollectionDataModel).ModifyIntlklogicDefinition(__task_parameter, __txt_edit_intlk_logic_name.Text,
                            __txt_edit_intlk_logic_target.Text, __txt_edit_intlk_logic_statement.Text);
                        __lsb_interlock_logic_definitions.SelectedIndex = __task_parameter;
                        __lsb_interlock_logic_definitions.ScrollIntoView(__lsb_interlock_logic_definitions.SelectedItem);
                        break;
                }
                __lsb_interlock_logic_definitions.IsEnabled = true;
                __grid_edit_intlk_logic_definition.IsEnabled = false;
            }
            catch (IOListParseExcepetion exception)
            {
                string message;
                if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while committing the edit . \n{0}", exception.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while committing the edit . \n{0}", exception.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __on_cancel_intlk_logic_edit(object sender, RoutedEventArgs e)
        {
            __lsb_interlock_logic_definitions.IsEnabled = true;
            __grid_edit_intlk_logic_definition.IsEnabled = false;
        }
    }
}
