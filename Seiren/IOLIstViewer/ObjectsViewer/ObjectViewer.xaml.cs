using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.TextInputLayout;
using Syncfusion.Windows.Controls.Input;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// ObjectViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ObjectViewer : Window
    {
        private Regex __ecatvar_binding_tips_reg = new Regex(@"^\$ECATVAR\$([0-9]{4}\$){3}", RegexOptions.Compiled);
        private Regex __cipassemblyio_binding_tips_reg = new Regex(@"^\$CIPASSEMBLYIO\$([0-9]{8}\$){2}", RegexOptions.Compiled);
        private ObjectsModel __object_model_collection;
        private InputDialogDisplayMode __input_mode;
        private int __insert_index;
        private ObjectModel __original_object_model;
        private ObjectModel __result_object_model;
        public ObjectModel Result { get; private set; } = null;
        public ObjectViewer(ObjectsModel models, ObjectModel model, InputDialogDisplayMode mode, int insertIndex = 0)
        {
            InitializeComponent();
            __object_model_collection = models;
            __original_object_model = model;
            __result_object_model = model.Clone();
            __input_mode = mode;
            __insert_index = insertIndex;

            VariableInput.AutoCompleteSource = models.Variables.Variables.Keys;
            ControllerModules.ItemsSource = models.ControllerConfiguration.Configurations.Keys;


            DataContext = __result_object_model;

            switch (mode)
            {
                case InputDialogDisplayMode.Add:
                    this.Title = "Add a new process data object";
                    break;
                case InputDialogDisplayMode.Insert:
                    this.Title = "Insert a new process data object";
                    break;
                case InputDialogDisplayMode.Edit:
                    this.Title = "Edit the process data object";
                    break;
            }
        }

        private void BindingTips_Click(object sender, RoutedEventArgs e)
        {
            __apply_binding_tips();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            __update_variable_info();
            __result_object_model.VariableName = __result_object_model.VariableName.Trim();
            foreach (var u in InputsGrid.Children)
            {
                if ((u as SfTextInputLayout)?.HasError == true)
                {
                    MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            if(__result_object_model.EnableBinding == true && __result_object_model.BindingDeviceName == null)
            {
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                switch (__input_mode)
                {
                    case InputDialogDisplayMode.Add:
                        __object_model_collection.Add(__result_object_model);
                        break;
                    case InputDialogDisplayMode.Insert:
                        __object_model_collection.Insert(__insert_index, __result_object_model);
                        break;
                    case InputDialogDisplayMode.Edit:
                        if(__original_object_model.Equals(__result_object_model) == false)
                            __object_model_collection.Replace(__original_object_model, __result_object_model);
                        break;
                }
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Result = __result_object_model;
            DialogResult = true;
        }

        private void __update_variable_info()
        {
            var binding = VariableInput.GetBindingExpression(SfTextBoxExt.TextProperty);
            binding.UpdateSource();
            if (__object_model_collection.Variables.Variables.TryGetValue(__result_object_model.VariableName.Trim(), out var v) == true)
            {
                __result_object_model.VariableDataType = v.Type.Name;
                __result_object_model.VariableUnit = v.Unit;
                __result_object_model.VariableComment = v.Comment;
            }
            else
            {
                __result_object_model.VariableDataType = "unnamed";
                __result_object_model.VariableUnit = "N/A";
            }
        }

        private void __apply_binding_tips()
        {
            var binding = VariableInput.GetBindingExpression(SfTextBoxExt.TextProperty);
            binding.UpdateSource();
            if (__object_model_collection.Variables.Variables.TryGetValue(__result_object_model.VariableName.Trim(), out var v) == true)
            {
                if (__ecatvar_binding_tips_reg.IsMatch(v.Comment))
                {
                    if (v.Type.BitSize == 1)
                        __result_object_model.BindingChannelIndex =
                            (uint)(Convert.ToUInt16(v.Comment.Substring(9, 4)) * 10000 + Convert.ToUInt16(v.Comment.Substring(19, 4)));
                    else
                        __result_object_model.BindingChannelIndex =
                            (uint)(Convert.ToUInt16(v.Comment.Substring(9, 4)) * 10000 + Convert.ToUInt16(v.Comment.Substring(19, 4)) / 8);
                }
                else if (__cipassemblyio_binding_tips_reg.IsMatch(v.Comment))
                {
                    if (v.Type.BitSize == 1)
                        __result_object_model.BindingChannelIndex = Convert.ToUInt32(v.Comment.Substring(24, 8));
                    else
                        __result_object_model.BindingChannelIndex = Convert.ToUInt32(v.Comment.Substring(24, 8)) / 8;
                }
            }
        }

        private void VariableInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                __update_variable_info();
                e.Handled = true;
            }
        }

        private void VariableInput_LostFocus(object sender, RoutedEventArgs e)
        {
            __update_variable_info();
        }

        private void ControllerModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ControllerModules.SelectedItem != null && __object_model_collection.ControllerConfiguration.Configurations.TryGetValue(ControllerModules.SelectedItem as string, out var device))
            {
                ModuleChannels.ItemsSource = device.DeviceModel.TxVariables.Keys.Concat(device.DeviceModel.RxVariables.Keys).Distinct();
            }
        }
    }
}
