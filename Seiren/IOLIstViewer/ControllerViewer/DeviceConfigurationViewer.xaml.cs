using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.TextInputLayout;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// DeviceConfigurationViewer.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceConfigurationViewer : Window
    {
        private ControllerConfigurationModel __controller_configuration_collection;
        private InputDialogDisplayMode __input_mode;
        private int __insert_index;
        private DeviceConfigurationModel __original_device_configuration;
        private DeviceConfigurationModel __result_device_configuration;
        public DeviceConfigurationModel Result { get; private set; } = null;
        public DeviceConfigurationViewer(ControllerConfigurationModel models, DeviceConfigurationModel model, InputDialogDisplayMode mode, int insertIndex = 0)
        {
            InitializeComponent();
            __controller_configuration_collection = models;
            __original_device_configuration = model;
            __result_device_configuration = new DeviceConfigurationModel()
            {
                DeviceModel = model.DeviceModel,
                Switch = model.Switch,
                LocalAddress = model.LocalAddress,
                IPv4 = model.IPv4,
                Port = model.Port,
                ReferenceName = model.ReferenceName
            };
            __input_mode = mode;
            __insert_index = insertIndex;
            IEnumerable<DeviceModel> deviceModels = models.ControllerModelCatalogue.LocalExtensionModels.Values;
            AvailableDeviceModels.ItemsSource = deviceModels.Concat(models.ControllerModelCatalogue.RemoteEthernetModels.Values);
            AvailableDeviceModels.SelectedItem = model.DeviceModel;
            DataContext = __result_device_configuration;
            switch (mode)
            {
                case InputDialogDisplayMode.Add:
                    this.Title = "Add a new device configuraion";
                    break;
                case InputDialogDisplayMode.Insert:
                    this.Title = "Insert a new device configuraion";
                    break;
                case InputDialogDisplayMode.Edit:
                    this.Title = "Edit the device configuraion";
                    break;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            __result_device_configuration.ReferenceName = __result_device_configuration.ReferenceName.Trim();
            if (__result_device_configuration.ReferenceName.Length == 0)
            {
                MessageBox.Show("The device reference name is invalid(Length == 0).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var u in InputsStack.Children)
            {
                if ((u as SfTextInputLayout)?.HasError == true)
                {
                    MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            try
            {
                switch (__input_mode)
                {
                    case InputDialogDisplayMode.Add:
                        __controller_configuration_collection.Add(__result_device_configuration);
                        break;
                    case InputDialogDisplayMode.Insert:
                        __controller_configuration_collection.Insert(__insert_index, __result_device_configuration);
                        break;
                    case InputDialogDisplayMode.Edit:
                        if(__original_device_configuration.Equals(__result_device_configuration) == false)
                            __controller_configuration_collection.Replace(__original_device_configuration, __result_device_configuration);
                        break;
                }
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Result = __result_device_configuration;
            DialogResult = true;
        }
    }
}
