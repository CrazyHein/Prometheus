using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// ObjectCollectionDataControl.xaml 的交互逻辑
    /// </summary>
    public partial class ObjectCollectionDataControl : UserControl
    {
        public ObjectCollectionDataControl(ObjectCollectionDataModel dataModel)
        {
            InitializeComponent();
            DataContext = dataModel;
            __lsv_io_objects.ItemsSource = dataModel.Objects;
            __cmb_filter_data_type.ItemsSource = dataModel.DataHelper.DataTypeCatalogue.DataTypes.Values;
            __cmb_filter_data_type.SelectedIndex = 0;
            ListCollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource) as ListCollectionView;
            view.Filter = new Predicate<object>(dataModel.ItemFilter.FilterItem);
            view.IsLiveGrouping = true;
            view.IsLiveFiltering = true;
            view.LiveGroupingProperties.Add(ObjectCollectionDataModel.DataTypePropertyName);
            view.LiveGroupingProperties.Add(ObjectCollectionDataModel.BindingModulePropertyName);
            view.LiveFilteringProperties.Add(ObjectCollectionDataModel.DataTypePropertyName);
            view.LiveFilteringProperties.Add(ObjectCollectionDataModel.BindingModulePropertyName);
        }
        private void __on_enable_data_type_filter(object sender, RoutedEventArgs e)
        {
            ListCollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource) as ListCollectionView;
            ObjectItemFilter filter = (DataContext as ObjectCollectionDataModel).ItemFilter;
            if (__chk_filtered_by_data_type.IsChecked == true && __cmb_filter_data_type.SelectedItem != null)
            {
                filter.DataType = __cmb_filter_data_type.SelectedItem as DataTypeDefinition;
                filter.EnableDataTypeFilter();
            }
            else if(__chk_filtered_by_data_type.IsChecked == false)
            {
                filter.DisableDataTypeFilter();
            }
            view.Refresh();
        }

        private void __on_enable_binding_module_filter(object sender, RoutedEventArgs e)
        {
            ListCollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource) as ListCollectionView;
            ObjectItemFilter filter = (DataContext as ObjectCollectionDataModel).ItemFilter;
            if (__chk_filtered_by_binding_module.IsChecked == true)
            {
                filter.BindingModule = __txt_filter_binding_module.Text;
                filter.EnableBindingModuleFilter();

            }
            else if (__chk_filtered_by_binding_module.IsChecked == false)
            {
                filter.DisableBindingModuleFilter();
            }
            view.Refresh();
        }

        private void __on_group_by_none_click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource);
            view.GroupDescriptions.Clear();
        }

        private void __on_group_by_data_type_click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(ObjectCollectionDataModel.DataTypeGroupDescription);
        }

        private void __on_group_by_binding_click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add(ObjectCollectionDataModel.BindingModuleGroupDescription);
        }

        private void __cmb_filter_data_type_selection_changed(object sender, SelectionChangedEventArgs e)
        {
            if (__chk_filtered_by_data_type.IsChecked == true && __cmb_filter_data_type.SelectedItem != null)
            {
                ListCollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource) as ListCollectionView;
                (DataContext as ObjectCollectionDataModel).ItemFilter.DataType = __cmb_filter_data_type.SelectedItem as DataTypeDefinition;
                view.Refresh();
            }
        }

        private void __on_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectCollectionDataModel host = DataContext as ObjectCollectionDataModel;

            if (host.DataHelper.DataTypeCatalogue.DataTypes.Count == 0)
                MessageBox.Show("There is no available [Data Type] in [DataType Catalogue] .", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ObjectItemDataModel newObjectItem = new ObjectItemDataModel(host, host.DataHelper.DataTypeCatalogue.DataTypes.Values.First());
            ObjectDataControl objectDataControl = new ObjectDataControl(newObjectItem, true);
            objectDataControl.ShowDialog();
            e.Handled = true;
        }

        private void __on_insert_element_before_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectCollectionDataModel host = DataContext as ObjectCollectionDataModel;

            if (host.DataHelper.DataTypeCatalogue.DataTypes.Count == 0)
                MessageBox.Show("There is no available [Data Type] in [DataType Catalogue] .", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ObjectItemDataModel newObjectItem = new ObjectItemDataModel(host, host.DataHelper.DataTypeCatalogue.DataTypes.Values.First());
            ObjectDataControl objectDataControl = new ObjectDataControl(newObjectItem, true, __lsv_io_objects.SelectedIndex);
            objectDataControl.ShowDialog();
            e.Handled = true;
        }

        private void __on_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectItemDataModel selectedData = __lsv_io_objects.SelectedItem as ObjectItemDataModel;
            int selectedPos = __lsv_io_objects.SelectedIndex;
            if (selectedData != null)
            {
                if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        ObjectCollectionDataModel dataModel = DataContext as ObjectCollectionDataModel;
                        dataModel.RemoveDataModel(selectedData.Index, selectedPos);
                    }
                    catch (IOListParseExcepetion exp)
                    {
                        string message;
                        if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                            message = string.Format("At least one unexpected error occurred while removing controller object . \n{0}", exp.DataException.ToString());
                        else
                            message = string.Format("At least one unexpected error occurred while removing controller object . \n{0}", exp.ErrorCode.ToString());

                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true;
        }

        private void __on_edit_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectItemDataModel selectedData = __lsv_io_objects.SelectedItem as ObjectItemDataModel;
            ObjectCollectionDataModel hostData = DataContext as ObjectCollectionDataModel;
            if (selectedData != null)
            {
                ObjectItemDataModel newObjectItem = selectedData.Clone();
                ObjectDataControl objectDataControl = new ObjectDataControl(newObjectItem, false);
                objectDataControl.ShowDialog();
            }
            e.Handled = true;
        }

        private void __on_move_up_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectCollectionDataModel dataModel = DataContext as ObjectCollectionDataModel;
            int selectedIndex = __lsv_io_objects.SelectedIndex;
            dataModel.SwapDataModel(selectedIndex, selectedIndex - 1);
            __lsv_io_objects.SelectedIndex = selectedIndex - 1;
            __lsv_io_objects.ScrollIntoView(__lsv_io_objects.SelectedItem);
            e.Handled = true;
        }

        private void __on_move_down_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectCollectionDataModel dataModel = DataContext as ObjectCollectionDataModel;
            int selectedIndex = __lsv_io_objects.SelectedIndex;
            //dataModel.SwapDataModel(selectedIndex + 1, selectedIndex);
            dataModel.SwapDataModel(selectedIndex, selectedIndex + 1);
            __lsv_io_objects.SelectedIndex = selectedIndex + 1;
            __lsv_io_objects.ScrollIntoView(__lsv_io_objects.SelectedItem);
            e.Handled = true;
        }

        private void __on_remove_edit_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            if (__lsv_io_objects == null)
                e.CanExecute = false;
            else
                e.CanExecute = __lsv_io_objects.SelectedItem != null;
        }

        private void __on_move_up_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = __lsv_io_objects.SelectedItem != null && __lsv_io_objects.SelectedIndex - 1 >= 0;
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
                e.CanExecute = __lsv_io_objects.SelectedItem != null && __lsv_io_objects.SelectedIndex + 1 < __lsv_io_objects.Items.Count;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __txt_filter_binding_module_key_down(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ListCollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource) as ListCollectionView;
                (DataContext as ObjectCollectionDataModel).ItemFilter.BindingModule = __txt_filter_binding_module.Text;
                view.Refresh();

                e.Handled = true;
            }
        }
    }
}
