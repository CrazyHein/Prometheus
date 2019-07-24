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

        private void __on_data_binding_error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                (DataContext as ControllerInformationDataModel).FieldDataBindingErrors++;
            else if (e.Action == ValidationErrorEventAction.Removed)
                (DataContext as ControllerInformationDataModel).FieldDataBindingErrors--;
        }

        private void __on_extension_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
            if (host.DataHelper.ControllerCatalogue.ExtensionModels.Count == 0)
                MessageBox.Show("There is no available [Extension Model] in [Controller Catalogue] .", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                ControllerExtensionModuleItemDataModel data = new ControllerExtensionModuleItemDataModel(host,
                    host.DataHelper.ControllerCatalogue.ExtensionModels.First().Value);
                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(data, null, -1);
                if (moduleDataControl.ShowDialog() == true)
                {

                }
            }
            e.Handled = true;
        }

        private void __on_extension_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
                    dataModel.RemoveExtensionDataModel(__lsv_extension_modules.SelectedIndex);
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

            e.Handled = true;
        }

        private void __on_extension_edit_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerExtensionModuleItemDataModel selectedData = __lsv_extension_modules.SelectedItem as ControllerExtensionModuleItemDataModel;
            if (selectedData != null)
            {
                ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
                ControllerExtensionModuleItemDataModel data = new ControllerExtensionModuleItemDataModel(host,
                    selectedData.Model, selectedData.ReferenceName, selectedData.LocalAddress);
                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(data, selectedData.ReferenceName);
                if (moduleDataControl.ShowDialog() == true)
                {

                }
            }
            e.Handled = true;
        }

        private void __on_extension_move_up_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
            int selectedIndex = __lsv_extension_modules.SelectedIndex;
            dataModel.SwapExtensionDataModel(selectedIndex, selectedIndex - 1);
            __lsv_extension_modules.SelectedIndex = selectedIndex - 1;
            __lsv_extension_modules.ScrollIntoView(__lsv_extension_modules.SelectedItem);
            e.Handled = true;
        }

        private void __on_extension_move_down_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
            int selectedIndex = __lsv_extension_modules.SelectedIndex;
            dataModel.SwapExtensionDataModel(selectedIndex, selectedIndex + 1);
            __lsv_extension_modules.SelectedIndex = selectedIndex + 1;
            __lsv_extension_modules.ScrollIntoView(__lsv_extension_modules.SelectedItem);
            e.Handled = true;
        }

        private void __on_extension_insert_element_before_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
            if (host.DataHelper.ControllerCatalogue.ExtensionModels.Count == 0)
                MessageBox.Show("There is no available [Extension Model] in [Controller Catalogue] .", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                ControllerExtensionModuleItemDataModel data = new ControllerExtensionModuleItemDataModel(host,
                    host.DataHelper.ControllerCatalogue.ExtensionModels.First().Value);
                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(data, null, __lsv_extension_modules.SelectedIndex);
                if (moduleDataControl.ShowDialog() == true)
                {

                }
            }
            e.Handled = true;
        }

        private void __on_ethernet_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
            if (host.DataHelper.ControllerCatalogue.EthernetModels.Count == 0)
                MessageBox.Show("There is no available [Ethernet Model] in [Controller Catalogue] .", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                ControllerEthernetModuleItemDataModel data = new ControllerEthernetModuleItemDataModel(host,
                    host.DataHelper.ControllerCatalogue.EthernetModels.First().Value);

                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(data, null);
                if (moduleDataControl.ShowDialog() == true)
                {

                }
            }
            e.Handled = true;
        }

        private void __on_ethernet_remove_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
                    dataModel.RemoveEthernetDataModel(__lsv_ethernet_modules.SelectedIndex);
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

            e.Handled = true;
        }

        private void __on_ethernet_edit_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerEthernetModuleItemDataModel selectedData = __lsv_ethernet_modules.SelectedItem as ControllerEthernetModuleItemDataModel;
            if (selectedData != null)
            {
                ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
                ControllerEthernetModuleItemDataModel data = new ControllerEthernetModuleItemDataModel(host,
                    selectedData.Model, selectedData.ReferenceName, selectedData.IPAddress, selectedData.Port);
                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(data, selectedData.ReferenceName);
                if (moduleDataControl.ShowDialog() == true)
                {

                }
            }
            e.Handled = true;
        }

        private void __on_ethernet_move_up_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
            int selectedIndex = __lsv_ethernet_modules.SelectedIndex;
            dataModel.SwapEthernetDataModel(selectedIndex, selectedIndex - 1);
            __lsv_ethernet_modules.SelectedIndex = selectedIndex - 1;
            __lsv_ethernet_modules.ScrollIntoView(__lsv_ethernet_modules.SelectedItem);
        }

        private void __on_ethernet_move_down_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel dataModel = DataContext as ControllerInformationDataModel;
            int selectedIndex = __lsv_ethernet_modules.SelectedIndex;
            dataModel.SwapEthernetDataModel(selectedIndex, selectedIndex + 1);
            __lsv_ethernet_modules.SelectedIndex = selectedIndex + 1;
            __lsv_ethernet_modules.ScrollIntoView(__lsv_ethernet_modules.SelectedItem);
        }

        private void __on_ethernet_insert_element_before_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            ControllerInformationDataModel host = DataContext as ControllerInformationDataModel;
            if (host.DataHelper.ControllerCatalogue.EthernetModels.Count == 0)
                MessageBox.Show("There is no available [Ethernt Model] in [Controller Catalogue] .", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                ControllerEthernetModuleItemDataModel data = new ControllerEthernetModuleItemDataModel(host,
                    host.DataHelper.ControllerCatalogue.EthernetModels.First().Value);
                ControllerModuleDataControl moduleDataControl = new ControllerModuleDataControl(data, null, __lsv_ethernet_modules.SelectedIndex);
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

        private void __on_extension_move_up_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = __lsv_extension_modules.SelectedItem != null && __lsv_extension_modules.SelectedIndex - 1 >= 0;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_extension_move_down_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = __lsv_extension_modules.SelectedItem != null && __lsv_extension_modules.SelectedIndex + 1 < __lsv_extension_modules.Items.Count;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_ethernet_move_up_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = __lsv_ethernet_modules.SelectedItem != null && __lsv_ethernet_modules.SelectedIndex - 1 >= 0;
            }
            catch
            {
                e.CanExecute = false;
            }
        }

        private void __on_ethernet_move_down_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = __lsv_ethernet_modules.SelectedItem != null && __lsv_ethernet_modules.SelectedIndex + 1 < __lsv_ethernet_modules.Items.Count;
            }
            catch
            {
                e.CanExecute = false;
            }
        }
    }
}
