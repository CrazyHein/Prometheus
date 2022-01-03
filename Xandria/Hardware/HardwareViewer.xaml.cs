using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    /// <summary>
    /// HardwareViewer.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareViewer : Window
    {
        private HardwareModels __hardware_collection_model;
        private InputDialogDisplayMode __input_mode;
        private int __insert_index;
        private HardwareModel __original_hardware_model;
        private HardwareModel __result_hardware_model;
        public HardwareModel Result { get; private set; } = null;
        public HardwareViewer(HardwareModels models, HardwareModel model, InputDialogDisplayMode mode, int insertIndex = 0)
        {
            InitializeComponent();
            __hardware_collection_model = models;
            __original_hardware_model = model;
            __result_hardware_model = new HardwareModel()
            {
                DeviceModel = model.DeviceModel,
                Switch = model.Switch,
                LocalAddress = model.LocalAddress,
                IPv4 = model.IPv4,
                Port = model.Port,
                CustomFields = new List<(string key, string value)>(model.CustomFields)
            };
            __input_mode = mode;
            __insert_index = insertIndex;
            DataContext = __result_hardware_model;
            switch (mode)
            {
                case InputDialogDisplayMode.AddLocal:
                    this.Title = "Add a new local hardware module";
                    AvailableDeviceModels.ItemsSource = models.ControllerModelCatalogue.LocalExtensionModels.Values;
                    InputRemoteIPv4.IsEnabled = false;
                    InputRemotePort.IsEnabled = false;
                    CustomInput.AutoCompleteSource = LocalHardwareModule.CUSTOMS;
                    break;
                case InputDialogDisplayMode.InsertLocal:
                    this.Title = "Insert a new local hardware module";
                    AvailableDeviceModels.ItemsSource = models.ControllerModelCatalogue.LocalExtensionModels.Values;
                    InputRemoteIPv4.IsEnabled = false;
                    InputRemotePort.IsEnabled = false;
                    CustomInput.AutoCompleteSource = LocalHardwareModule.CUSTOMS;
                    break;
                case InputDialogDisplayMode.EditLocal:
                    this.Title = "Edit the local hardware module";
                    AvailableDeviceModels.ItemsSource = models.ControllerModelCatalogue.LocalExtensionModels.Values;
                    InputRemoteIPv4.IsEnabled = false;
                    InputRemotePort.IsEnabled = false;
                    CustomInput.AutoCompleteSource = LocalHardwareModule.CUSTOMS;
                    break;
                case InputDialogDisplayMode.AddRemote:
                    this.Title = "Add a new remote hardware module";
                    AvailableDeviceModels.ItemsSource = models.ControllerModelCatalogue.RemoteEthernetModels.Values;
                    InputLocalAddress.IsEnabled = false;
                    CustomInput.AutoCompleteSource = RemoteHardwareModule.CUSTOMS;
                    break;
                case InputDialogDisplayMode.InsertRemote:
                    this.Title = "Insert a new remote hardware module";
                    AvailableDeviceModels.ItemsSource = models.ControllerModelCatalogue.RemoteEthernetModels.Values;
                    InputLocalAddress.IsEnabled = false;
                    CustomInput.AutoCompleteSource = RemoteHardwareModule.CUSTOMS;
                    break;
                case InputDialogDisplayMode.EditRemote:
                    this.Title = "Edit the remote hardware module";
                    AvailableDeviceModels.ItemsSource = models.ControllerModelCatalogue.RemoteEthernetModels.Values;
                    InputLocalAddress.IsEnabled = false;
                    CustomInput.AutoCompleteSource = RemoteHardwareModule.CUSTOMS;
                    break;
            }
            AvailableDeviceModels.SelectedItem = model.DeviceModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if(__errors != 0)
            {
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                switch (__input_mode)
                {
                    case InputDialogDisplayMode.AddLocal:
                        __hardware_collection_model.AddLocal(__result_hardware_model);
                        break;
                    case InputDialogDisplayMode.InsertLocal:
                        __hardware_collection_model.InsertLocal(__insert_index, __result_hardware_model);
                        break;
                    case InputDialogDisplayMode.EditLocal:
                        __hardware_collection_model.ReplaceLocal(__insert_index, __result_hardware_model);
                        break;
                    case InputDialogDisplayMode.AddRemote:
                        __hardware_collection_model.AddRemote(__result_hardware_model);
                        break;
                    case InputDialogDisplayMode.InsertRemote:
                        __hardware_collection_model.InsertRemote(__insert_index, __result_hardware_model);
                        break;
                    case InputDialogDisplayMode.EditRemote:
                        __hardware_collection_model.ReplaceRemote(__insert_index, __result_hardware_model);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Result = __result_hardware_model;
            DialogResult = true;
        }

        private int __errors = 0;
        private void InputsStack_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }
    }
}
