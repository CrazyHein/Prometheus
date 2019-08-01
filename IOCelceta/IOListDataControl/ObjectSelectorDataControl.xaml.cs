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
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    /// <summary>
    /// ObjectSearcher.xaml 的交互逻辑
    /// </summary>
    public partial class ObjectSelectorDataControl : Window
    {
        private CollectionView __available_object_view;
        private ObjectDefinitionFilter __object_view_filter;
        public ObjectSelectorDataControl(ObjectSelectorDataModel dataModel)
        {
            InitializeComponent();
            DataContext = dataModel;
            __lsb_object_collection.ItemsSource = dataModel.AvailableObjects;
            __available_object_view = (CollectionView)CollectionViewSource.GetDefaultView(__lsb_object_collection.ItemsSource);   
            __object_view_filter = dataModel.DataFilter;
            __available_object_view.Filter = dataModel.DataFilter.FilterItem;
        }

        private void __on_ok(object sender, RoutedEventArgs e)
        {
            if((DataContext as ObjectSelectorDataModel).SelectedObject != null)
                DialogResult = true;
            else
                MessageBox.Show("Please select an object ...", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void __on_cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void __lsb_object_collection_mouse_double_click(object sender, MouseButtonEventArgs e)
        {
            if ((DataContext as ObjectSelectorDataModel).SelectedObject != null)
                DialogResult = true;
        }

        private void __on_enable_data_type_name_filter(object sender, RoutedEventArgs e)
        {
            if (__chk_enable_data_type_name_filter.IsChecked == true)
            {
                var dataTypeCatalogue = (DataContext as ObjectSelectorDataModel).DataTypes;
                if (dataTypeCatalogue.Keys.Contains(__txt_filter_data_type_name.Text))
                    __object_view_filter.DataType = dataTypeCatalogue[__txt_filter_data_type_name.Text];
                else
                    __object_view_filter.DataType = null;
                __object_view_filter.EnableDataTypeFilter();
            }
            else
                __object_view_filter.DisableDataTypeFilter();

            __available_object_view.Refresh();
        }

        private void __on_enable_variable_name_filter(object sender, RoutedEventArgs e)
        {
            if (__chk_enable_variable_name_filter.IsChecked == true)
            {
                __object_view_filter.VariableName = __txt_filter_variable_name.Text;
                __object_view_filter.EnableVariableNameFilter();
            }
            else
                __object_view_filter.DisableVariableNameFilter();

            __available_object_view.Refresh();
        }

        private void __txt_filter_data_type_name_enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var dataTypeCatalogue = (DataContext as ObjectSelectorDataModel).DataTypes;
                if (dataTypeCatalogue.Keys.Contains(__txt_filter_data_type_name.Text))
                    __object_view_filter.DataType = dataTypeCatalogue[__txt_filter_data_type_name.Text];
                else
                    __object_view_filter.DataType = null;

                __available_object_view.Refresh();
            }
        }

        private void __txt_filter_variable_name_enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                __object_view_filter.VariableName = __txt_filter_variable_name.Text;
                __available_object_view.Refresh();
            }
        }
    }
}
