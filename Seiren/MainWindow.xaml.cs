using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private string currentVisualStyle;
        private string currentSizeMode;
        #endregion

        private Dictionary<object, object> __viewers = new Dictionary<object, object>();

        private DataTypeCatalogue __data_type_catalogue;
        private ControllerModelCatalogue __controller_model_catalogue;
        private VariableDictionary __variable_dictionary;
        private ControllerConfiguration __controller_configuration;
        private ObjectDictionary __object_dictionary;
        private ProcessDataImage __tx_diagnotic_area;
        private ProcessDataImage __tx_bit_area;
        private ProcessDataImage __tx_block_area;
        private ProcessDataImage __rx_control_area;
        private ProcessDataImage __rx_bit_area;
        private ProcessDataImage __rx_block_area;
        private InterlockCollection __interlock_area;
        private Miscellaneous __misc_info;

        private DataTypesViewer __data_types_viewer;
        private DeviceModelsViewer __device_models_viewer;
        private VariablesViewer __variables_viewer;
        private ControllerConfigurationViewer __controller_configuration_viewer;
        private ObjectsViewer __objects_viewer;
        private MiscellaneousViewer __miscellaneous_viewer;

        private object? __current_user_control = null;
        private MainModel __main_model = new MainModel();
        private Settings __settings = new Settings();

        private DataSynchronizer __data_synchronizer;
        private UserInterfaceSynchronizer __user_interface_synchronizer;
        private ushort[] __tx_diagnotic_area_data;
        private ushort[] __tx_bit_area_data;
        private ushort[] __tx_block_area_data;
        private ushort[] __rx_control_area_data;
        private ushort[] __rx_bit_area_data;
        private ushort[] __rx_block_area_data;
        private List<ushort[]> __area_data_array;
        private ushort __area_data_end;

        #region Properties
        /// <summary>
        /// Gets or sets the current visual style.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentVisualStyle
        {
            get
            {
                return currentVisualStyle;
            }
            set
            {
                currentVisualStyle = value;
                OnVisualStyleChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current Size mode.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentSizeMode
        {
            get
            {
                return currentSizeMode;
            }
            set
            {
                currentSizeMode = value;
                OnSizeModeChanged();
            }
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;

            DataContext = __main_model;
        }
        /// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "FluentLight";
            CurrentSizeMode = "Default";

            try
            {
                __data_type_catalogue = new DataTypeCatalogue(__settings.DataTypeCataloguePath);
                __controller_model_catalogue = new ControllerModelCatalogue(__settings.ControllerModelCataloguePath);
                __settings.DataTypeCatalogueHash = __data_type_catalogue.MD5Code;
                __settings.ControllerModelCatalogueHash = __controller_model_catalogue.MD5Code;

                __data_types_viewer = new DataTypesViewer(__data_type_catalogue);
                __device_models_viewer = new DeviceModelsViewer(__controller_model_catalogue);

                __viewers[NavDataTypes] = __data_types_viewer;
                __viewers[NavDeviceModels] = __device_models_viewer;

                __user_interface_synchronizer = new UserInterfaceSynchronizer(this, __ui_data_refresh_handler);
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during loading metadata :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
        /// <summary>
        /// On Visual Style Changed.
        /// </summary>
        /// <remarks></remarks>
        private void OnVisualStyleChanged()
        {
            VisualStyles visualStyle = VisualStyles.Default;
            Enum.TryParse(CurrentVisualStyle, out visualStyle);
            if (visualStyle != VisualStyles.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetVisualStyle(this, visualStyle);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        private bool __data_model_has_changes()
        {
            if (__variables_viewer == null)
                return false;
            return (__variables_viewer.DataContext as VariablesModel).Modified ||
                    (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).Modified ||
                    (__objects_viewer.DataContext as ObjectsModel).Modified ||
                    (__miscellaneous_viewer.DataContext as MiscellaneousModel).Modified;
        }

        private void __reset_layout()
        {
            __variables_viewer = new VariablesViewer(__variable_dictionary, __data_type_catalogue);
            __controller_configuration_viewer = new ControllerConfigurationViewer(__controller_configuration, __controller_model_catalogue);
            __objects_viewer = new ObjectsViewer(__object_dictionary, __variable_dictionary, __controller_configuration,
                __tx_diagnotic_area, __tx_bit_area, __tx_block_area, __rx_control_area, __rx_bit_area, __rx_block_area, __interlock_area);
            __miscellaneous_viewer = new MiscellaneousViewer(__misc_info);

            (__variables_viewer.DataContext as VariablesModel).SubscriberObjects = __objects_viewer.DataContext as ObjectsModel;
            (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).SubscriberObjects = __objects_viewer.DataContext as ObjectsModel;

            NavVariableDictionary.DataContext = __variables_viewer.DataContext;
            NavControllerConfiguration.DataContext = __controller_configuration_viewer.DataContext;
            NavProcessDataDictionary.DataContext = __objects_viewer.DataContext;
            NavMiscellaneous.DataContext = __miscellaneous_viewer.DataContext;

            __viewers[NavVariableDictionary] = __variables_viewer;
            __viewers[NavControllerConfiguration] = __controller_configuration_viewer;
            __viewers[NavProcessDataDictionary] = __objects_viewer;
            __viewers[NavMiscellaneous] = __miscellaneous_viewer;

            UserControl.Content = null;
            __current_user_control = null;
        }

        private void __close_layout()
        {
            __variables_viewer = null;
            __controller_configuration_viewer = null;
            __objects_viewer = null;
            __miscellaneous_viewer = null;

            __viewers[NavVariableDictionary] = __variables_viewer;
            __viewers[NavControllerConfiguration] = __controller_configuration_viewer;
            __viewers[NavProcessDataDictionary] = __objects_viewer;
            __viewers[NavMiscellaneous] = __miscellaneous_viewer;

            UserControl.Content = null;
            __current_user_control = null;
        }

        public string? __update_binding_source()
        {
            __miscellaneous_viewer.UpdateBindingSource();
            __objects_viewer.UpdateBindingSource();
            if (__miscellaneous_viewer.HasError)
                return "At least one user input is invalid.(Miscellaneous)";
            if (__objects_viewer.HasError)
                return "At least one user input is invalid.(ObjectDictionary)";
            bool res = IOCelcetaHelper.OVERLAP_DETECTOR(new List<(uint, uint)>()
            {
                (__tx_diagnotic_area.OffsetInWord, __tx_diagnotic_area.SizeInWord),
                (__tx_bit_area.OffsetInWord, __tx_bit_area.SizeInWord),
                (__tx_block_area.OffsetInWord, __tx_block_area.SizeInWord),
                (__rx_control_area.OffsetInWord, __rx_control_area.SizeInWord),
                (__rx_bit_area.OffsetInWord, __rx_bit_area.SizeInWord),
                (__rx_block_area.OffsetInWord, __rx_block_area.SizeInWord)
            });
            if (res)
                return "The address range of one process data image overlaps another address range.";
            return null;
        }

        public void __commit_changes()
        {
            (__variables_viewer.DataContext as VariablesModel).CommitChanges();
            (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).CommitChanges();
            (__objects_viewer.DataContext as ObjectsModel).CommitChanges();
            (__miscellaneous_viewer.DataContext as MiscellaneousModel).CommitChanges();
        }

        private void __startup_debugger()
        {
            (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).InterlockLogics.ResetProcessDataValue();

            __tx_diagnotic_area_data = new ushort[__tx_diagnotic_area.ActualSizeInWord];
            __tx_bit_area_data = new ushort[__tx_bit_area.ActualSizeInWord];
            __tx_block_area_data = new ushort[__tx_block_area.ActualSizeInWord];
            __rx_control_area_data = new ushort[__rx_control_area.ActualSizeInWord];
            __rx_bit_area_data = new ushort[__rx_bit_area.ActualSizeInWord];
            __rx_block_area_data = new ushort[__rx_block_area.ActualSizeInWord];

            __area_data_array = new List<ushort[]>()
            {
                __tx_diagnotic_area_data, __tx_bit_area_data, __tx_block_area_data,
                __rx_control_area_data, __rx_bit_area_data, __rx_block_area_data
            };
            __area_data_end = 0;

            var areas = new List<(uint, ushort, bool)>()
            {
                (__tx_diagnotic_area.OffsetInWord, (ushort)(__tx_diagnotic_area.ActualSizeInWord), false),
                (__tx_bit_area.OffsetInWord, (ushort)(__tx_bit_area.ActualSizeInWord), false),
                (__tx_block_area.OffsetInWord, (ushort)(__tx_block_area.ActualSizeInWord), false),
                (__rx_control_area.OffsetInWord, (ushort)(__rx_control_area.ActualSizeInWord), true),
                (__rx_bit_area.OffsetInWord, (ushort)(__rx_bit_area.ActualSizeInWord), true),
                (__rx_block_area.OffsetInWord, (ushort)(__rx_block_area.ActualSizeInWord), true)
            };

            __data_synchronizer = new DataSynchronizer(areas);
            __main_model.IsBusy = true;
            BusyDialog diag = new BusyDialog(__data_synchronizer.Startup(__settings.SlmpTargetProperty));
            diag.ShowDialog();
            //await __data_synchronizer.Startup(new SlmpTargetProperty());
            __main_model.IsBusy = false;

            (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.IsOffline = false;
            (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.IsOffline = false;
            (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.IsOffline = false;
            (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.IsOffline = false;
            (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.IsOffline = false;
            (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.IsOffline = false;
            (__objects_viewer.DataContext as ObjectsModel).InterlockLogics.IsOffline = false;

            __main_model.IsOffline = false;
            __main_model.DebuggerExceptionMessage = "N/A";
            __main_model.DebuggerState =  DataSynchronizerState.Ready;
            __main_model.DebuggerPollingInterval = 0;
            __main_model.DebuggerHeartbeat = 0;

            __user_interface_synchronizer.Startup(__settings.SlmpTargetProperty.MonitoringTimer);
            CommandManager.InvalidateRequerySuggested();
        }

        private void __stop_debugger() 
        {
            __user_interface_synchronizer.Stop();
            __main_model.IsBusy = true;
            BusyDialog diag = new BusyDialog(__data_synchronizer.Stop());
            diag.ShowDialog();
            //await __data_synchronizer.Stop();
            __main_model.IsBusy = false;
            __data_synchronizer = null;

            (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.IsOffline = true;
            (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.IsOffline = true;
            (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.IsOffline = true;
            (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.IsOffline = true;
            (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.IsOffline = true;
            (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.IsOffline = true;
            (__objects_viewer.DataContext as ObjectsModel).InterlockLogics.IsOffline = true;

            __tx_diagnotic_area_data = null;
            __tx_bit_area_data = null;
            __tx_block_area_data = null;
            __rx_control_area_data = null;
            __rx_bit_area_data = null;
            __rx_block_area_data = null;
            __area_data_array = null;
            __area_data_end = 0;

            __main_model.IsOffline = true;
            __main_model.DebuggerPollingInterval = 0;
            __main_model.DebuggerHeartbeat = 0;
            if (__main_model.DebuggerState != DataSynchronizerState.Exception)
                __main_model.DebuggerState = DataSynchronizerState.Ready;
            CommandManager.InvalidateRequerySuggested();
        }

        private void __ui_data_refresh_handler()
        {
            if (__main_model.IsOffline == true || __data_synchronizer == null || __main_model.IsBusy)
                return;
            __main_model.DebuggerExceptionMessage = __data_synchronizer.ExceptionMessage;
            __main_model.DebuggerState = __data_synchronizer.State;
            __main_model.DebuggerPollingInterval = __data_synchronizer.PollingInterval;
            __main_model.DebuggerHeartbeat = __data_synchronizer.Counter;

            if(__main_model.DebuggerState == DataSynchronizerState.Connected)
            {
                __data_synchronizer.Exchange(__area_data_array, out __area_data_end);
                (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.ProcessDataValueChanged(__tx_diagnotic_area_data);
                (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.ProcessDataValueChanged(__tx_bit_area_data);
                (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.ProcessDataValueChanged(__tx_block_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.ProcessDataValueChanged(__rx_control_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.ProcessDataValueChanged(__rx_bit_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.ProcessDataValueChanged(__rx_block_area_data);
                (__objects_viewer.DataContext as ObjectsModel).InterlockLogics.ProcessDataValueChanged(__tx_bit_area_data, __rx_bit_area_data);
            }

            if (__main_model.DebuggerState == DataSynchronizerState.Exception)
            {
                __stop_debugger();
            }
        }

        /// <summary>
        /// On Size Mode Changed event.
        /// </summary>
        /// <remarks></remarks>
        private void OnSizeModeChanged()
        {
            SizeMode sizeMode = SizeMode.Default;
            Enum.TryParse(CurrentSizeMode, out sizeMode);
            if (sizeMode != SizeMode.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetSizeMode(this, sizeMode);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        private void MainNavigator_ItemClicked(object sender, Syncfusion.UI.Xaml.NavigationDrawer.NavigationItemClickedEventArgs e)
        {
            object v = null;
            if(e.Item.Header as string == "Settings")
            {
                SettingsViewer st = new SettingsViewer(__settings);
                st.ShowDialog();
            }
            else if (__viewers.TryGetValue(e.Item, out v) && v != __current_user_control)
            {
                UserControl.Content = v;
                __current_user_control = v;
            }
        }

        private void SaveAsCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string? error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "Foliage Ocean List File(*.folst)|*.folst";
            save.AddExtension = true;
            save.DefaultExt = "folst";

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    IOCelcetaHelper.Save(save.FileName,
                        __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                        __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                        __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                        __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info);
                    __main_model.CurrentlyOpenFile = save.FileName;
                    __commit_changes();
                }
                catch(LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAsCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened;
        }

        private void SaveCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                string? error = __update_binding_source();
                if (error != null)
                {
                    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                IOCelcetaHelper.Save(__main_model.CurrentlyOpenFile,
                    __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                    __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                    __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                    __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info);
                __commit_changes();
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsNonTemporaryFile && __data_model_has_changes();
        }

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Foliage Ocean List File(*.folst)|*.folst";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    (__variable_dictionary, __controller_configuration, __object_dictionary,
                        __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area, 
                        __interlock_area, __misc_info) = IOCelcetaHelper.Load(open.FileName, __data_type_catalogue, __controller_model_catalogue, out _);
                    __main_model.CurrentlyOpenFile = open.FileName;
                    __reset_layout();
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy;
        }

        private void ExportCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var wnd = new ImportExport(ImportExportMode.Export,
                    __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                    __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                    __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                    __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue);
            wnd.ShowDialog();
        }

        private void ExportCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsNonTemporaryFile && !__data_model_has_changes();
        }

        private void ImportCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            var wnd = new ImportExport(ImportExportMode.Import,
                    __variable_dictionary, null,
                    __controller_configuration, null,
                    __object_dictionary, null,
                    __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area, 
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue);
            if(wnd.ShowDialog() == true)
            {
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                        __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info) = wnd.ImportResult;
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
        }

        private void ImportCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy;
        }

        private void NewCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            try
            {
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                    __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info) = IOCelcetaHelper.Default(__data_type_catalogue, __controller_model_catalogue);
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy;
        }

        private void QuitCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            //
            Close();
        }

        private void DownloadviaFTPCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var wnd = new FTPUtility(FTPMode.Download,
                   __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                   __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                   __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                   __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                   __rx_control_area, __rx_bit_area, __rx_block_area,
                   __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue);
            wnd.ShowDialog();
        }

        private void DownloadviaFTPCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsNonTemporaryFile && !__data_model_has_changes();
        }

        private void UploadviaFTPCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            var wnd = new FTPUtility(FTPMode.Upload,
                    __variable_dictionary, null,
                    __controller_configuration, null,
                    __object_dictionary, null,
                    __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue);
            if (wnd.ShowDialog() == true)
            {
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                        __tx_diagnotic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info) = wnd.UploadResult;
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
        }

        private void UploadviaFTPCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy;
        }

        private void AboutCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var wnd = new About(__settings);
            wnd.ShowDialog();
        }

        private void CloseCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            __variable_dictionary = null;
            __controller_configuration = null;
            __object_dictionary = null;
            __tx_diagnotic_area = null;
            __tx_bit_area = null;
            __tx_block_area = null;
            __rx_control_area = null;
            __rx_bit_area = null;
            __rx_block_area = null;
            __interlock_area = null;
            __misc_info = null;

            __close_layout();
            __main_model.CurrentlyOpenFile = null;
        }

        private void CloseCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened && __main_model.IsOffline && !__main_model.IsBusy;
        }

        private void StartDebuggingCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            __startup_debugger();
        }

        private void StartDebuggingCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened && !__data_model_has_changes() && __main_model.IsOffline && !__main_model.IsBusy;
        }

        private void StopDebuggingCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            __stop_debugger();
        }

        private void StopDebuggingCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !__main_model.IsOffline && !__main_model.IsBusy;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (__main_model.IsOffline == false)
                e.Cancel = true;
            else
            {
                if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
                {
                    var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.No)
                        e.Cancel = true;
                }
            }
        }

        
    }
}
