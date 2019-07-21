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
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    /// <summary>
    /// ControllerModuleDetail.xaml 的交互逻辑
    /// </summary>
    public partial class ControllerModuleDataControl : Window
    {
        private string __original_reference_name;
        private int __insert_pos;
        public ControllerModuleDataControl(ControllerModuleItemDataModel dataModel, string originalReferenceName, int insertPos = -1)
        {
            InitializeComponent();
            DataContext = dataModel;
            __original_reference_name = originalReferenceName;
            __insert_pos = insertPos;
            if (dataModel is ControllerExtensionModuleItemDataModel)
                __cmb_available_controller_modules.ItemsSource = dataModel.Host.DataHelper.ControllerCatalogue.ExtensionModels.Values;
            else if (dataModel is ControllerEthernetModuleItemDataModel)
                __cmb_available_controller_modules.ItemsSource = dataModel.Host.DataHelper.ControllerCatalogue.EthernetModels.Values;
        }

        private void __on_controller_module_selection_changed(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ControllerExtensionModuleItemDataModel)
            {
                __lst_rx_content.ItemsSource = (__cmb_available_controller_modules.SelectedItem as ControllerExtensionModel).RxVariables;
                __lst_tx_content.ItemsSource = (__cmb_available_controller_modules.SelectedItem as ControllerExtensionModel).TxVariables;
            }
            else if(DataContext is ControllerEthernetModuleItemDataModel)
            {
                __lst_rx_content.ItemsSource = (__cmb_available_controller_modules.SelectedItem as ControllerEthernetModel).RxVariables;
                __lst_tx_content.ItemsSource = (__cmb_available_controller_modules.SelectedItem as ControllerEthernetModel).TxVariables;
            }
        }

        private int __data_binding_error_counter = 0;

        private void __on_data_binding_error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __data_binding_error_counter++;
            else
                __data_binding_error_counter--;
        }

        private void __on_cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void __on_ok(object sender, RoutedEventArgs e)
        {
            if (__data_binding_error_counter != 0)
                MessageBox.Show("Invalid User Input ...","Error",MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                try
                {
                    ControllerModuleItemDataModel data = DataContext as ControllerModuleItemDataModel;
                    if (__original_reference_name == null)
                        data.Host.AddDataModel(data, __insert_pos);
                    else
                        data.Host.ModifyDataModel(__original_reference_name, data);
                    DialogResult = true;
                }
                catch(IOListParseExcepetion exception)
                {
                    string message;
                    if (exception.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while updating controller modules . \n{0}", exception.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while updating controller modules . \n{0}", exception.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
