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
            __cmb_filter_binding_module.ItemsSource = dataModel.DataHelper.ControllerModuleCollection;
            __cmb_filter_binding_module.SelectedIndex = 0;
            ListCollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource) as ListCollectionView;
            view.Filter = new Predicate<object>(dataModel.ItemFilter.FilterItem);
            view.IsLiveGrouping = true;
            view.IsLiveFiltering = true;
            view.LiveGroupingProperties.Add("DataType");
            view.LiveGroupingProperties.Add("Binding");
            view.LiveFilteringProperties.Add("DataType");
            view.LiveFilteringProperties.Add("Binding");
        }

        private void __cmb_filter_binding_module_drop_down_opened(object sender, EventArgs e)
        {
            ObjectCollectionDataModel dataModel = DataContext as ObjectCollectionDataModel;
            if(dataModel.DataHelper.ControllerModulesUpdated == true)
            {
                CollectionViewSource.GetDefaultView(__cmb_filter_binding_module.ItemsSource).Refresh();
                dataModel.DataHelper.ControllerModulesUpdated = false;
            }
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
            if (__chk_filtered_by_binding_module.IsChecked == true && __cmb_filter_binding_module.SelectedItem != null)
            {
                filter.BindingModule = __cmb_filter_binding_module.SelectedItem.ToString();
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
            view.GroupDescriptions.Add((DataContext as ObjectCollectionDataModel).DataTypeGroupDescription);
        }

        private void __on_group_by_binding_click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource);
            view.GroupDescriptions.Clear();
            view.GroupDescriptions.Add((DataContext as ObjectCollectionDataModel).BindingModuleGroupDescription);
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

        private void __cmb_filter_binding_module_selection_changed(object sender, SelectionChangedEventArgs e)
        {
            if (__chk_filtered_by_binding_module.IsChecked == true && __cmb_filter_binding_module.SelectedItem != null)
            {
                ListCollectionView view = CollectionViewSource.GetDefaultView(__lsv_io_objects.ItemsSource) as ListCollectionView;
                (DataContext as ObjectCollectionDataModel).ItemFilter.BindingModule = __cmb_filter_binding_module.SelectedItem.ToString();
                view.Refresh();
            }
        }

        private void __on_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectCollectionDataModel host = DataContext as ObjectCollectionDataModel;
            ObjectItemDataModel newObjectItem = new ObjectItemDataModel(host.DataHelper.DataTypeCatalogue.DataTypes.Values.First());
            ObjectDataModel objectDataModel = new ObjectDataModel(host, newObjectItem, false);
            ObjectDataControl objectDataControl = new ObjectDataControl(objectDataModel);
            objectDataControl.ShowDialog();
            e.Handled = true;
        }

        private void __on_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ObjectItemDataModel selectedData = __lsv_io_objects.SelectedItem as ObjectItemDataModel;
            if (selectedData != null)
            {
                if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        ObjectCollectionDataModel dataModel = DataContext as ObjectCollectionDataModel;
                        dataModel.DataHelper.RemoveObjectData(selectedData.Index);
                        dataModel.Objects.Remove(selectedData);
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
                ObjectDataModel dataModel = new ObjectDataModel(hostData, selectedData);
                ObjectDataControl dataControl = new ObjectDataControl(dataModel);
                dataControl.ShowDialog();
            }
            e.Handled = true;
        }

        private void __on_remove_edit_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            if (__lsv_io_objects == null)
                e.CanExecute = false;
            else
                e.CanExecute = __lsv_io_objects.SelectedItem != null;
        }
    }
}
