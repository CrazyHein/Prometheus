using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    /// <summary>
    /// ControllerInformationDataControl.xaml 的交互逻辑
    /// </summary>
    public partial class ControllerInformationDataControl : UserControl
    {
        public ControllerInformationDataControl(ControllerInformationDataModel dataModel)
        {
            InitializeComponent();
            DataContext = dataModel;
            __lsv_extension_modules.ItemsSource = dataModel.ExtensionModules;
            __lsv_ethernet_modules.ItemsSource = dataModel.EthernetModules;
        }

        private int __data_binding_error_counter = 0;
        private void __on_data_binding_error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __data_binding_error_counter++;
            else
                __data_binding_error_counter--;
        }

        private void __on_extension_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;

            ControllerExtensionModuleDataModel data = new ControllerExtensionModuleDataModel(
                host.DataHelper.ControllerCatalogue.ExtensionModels.Keys.First(),
                host.DataHelper.ControllerCatalogue.ExtensionModels[host.DataHelper.ControllerCatalogue.ExtensionModels.Keys.First()].Name);
            ControllerModuleDataModel moduleDataModel = new ControllerModuleDataModel(host, data, null, false);
            ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(moduleDataModel);
            if (moduleDataControl.ShowDialog() == true)
            {

            }
            e.Handled = true;
        }

        private void __on_extension_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerExtensionModuleDataModel selectedData = __lsv_extension_modules.SelectedItem as ControllerExtensionModuleDataModel;
            if (selectedData != null)
            {
                if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
                        dataModel.DataHelper.RemoveControllerModule(selectedData.ReferenceName);
                        dataModel.ExtensionModules.Remove(selectedData);
                    }
                    catch (IOListParseExcepetion exp)
                    {
                        string message;
                        if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                            message = string.Format("At least one unexpected error occurred while removing controller module . \n{0}", exp.DataException.ToString());
                        else
                            message = string.Format("At least one unexpected error occurred while removing controller module . \n{0}", exp.ErrorCode.ToString());

                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true;
        }

        private void __on_extension_edit_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerExtensionModuleDataModel selectedData = __lsv_extension_modules.SelectedItem as ControllerExtensionModuleDataModel;
            if (selectedData != null)
            {
                ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
                ControllerModuleDataModel moduleDataModel = new ControllerModuleDataModel(host, selectedData, null);
                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(moduleDataModel);
                if (moduleDataControl.ShowDialog() == true)
                {

                }
            }
            e.Handled = true;
        }

        private void __on_ethernet_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;

            ControllerEthernetModuleDataModel data = new ControllerEthernetModuleDataModel(
                host.DataHelper.ControllerCatalogue.EthernetModels.Keys.First(),
                host.DataHelper.ControllerCatalogue.EthernetModels[host.DataHelper.ControllerCatalogue.EthernetModels.Keys.First()].Name);
            ControllerModuleDataModel moduleDataModel = new ControllerModuleDataModel(host, null, data, false);
            ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(moduleDataModel);
            if (moduleDataControl.ShowDialog() == true)
            {

            }
            e.Handled = true;
        }

        private void __on_ethernet_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerEthernetModuleDataModel selectedData = __lsv_ethernet_modules.SelectedItem as ControllerEthernetModuleDataModel;
            if (selectedData != null)
            {
                if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
                        dataModel.DataHelper.RemoveControllerModule(selectedData.ReferenceName);
                        dataModel.EthernetModules.Remove(selectedData);
                    }
                    catch (IOListParseExcepetion exp)
                    {
                        string message;
                        if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                            message = string.Format("At least one unexpected error occurred while removing controller module . \n{0}", exp.DataException.ToString());
                        else
                            message = string.Format("At least one unexpected error occurred while removing controller module . \n{0}", exp.ErrorCode.ToString());

                        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true;
        }

        private void __on_ethernet_edit_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerEthernetModuleDataModel selectedData = __lsv_ethernet_modules.SelectedItem as ControllerEthernetModuleDataModel;
            if (selectedData != null)
            {
                ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
                ControllerModuleDataModel moduleDataModel = new ControllerModuleDataModel(host, null, selectedData);
                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(moduleDataModel);
                if (moduleDataControl.ShowDialog() == true)
                {

                }
            }
            e.Handled = true;
        }

        private void __on_extension_remove_edit_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            if (__lsv_extension_modules == null)
                e.CanExecute = false;
            else
                e.CanExecute = __lsv_extension_modules.SelectedItem != null;
        }

        private void __on_ethernet_remove_edit_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            if (__lsv_ethernet_modules == null)
                e.CanExecute = false;
            else
                e.CanExecute = __lsv_ethernet_modules.SelectedItem != null;
        }
    }
}
