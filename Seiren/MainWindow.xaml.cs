using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
        private ProcessDataImage __tx_diagnostic_area;
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
        private ushort[] __tx_diagnostic_area_data;
        private ushort[] __tx_bit_area_data;
        private ushort[] __tx_block_area_data;
        private ushort[] __rx_control_area_data;
        private ushort[] __rx_bit_area_data;
        private ushort[] __rx_block_area_data;
        private List<ushort[]> __area_data_array;
        private ushort __area_data_end;

        private DataAcquisitionUnit __data_acquisition_unit;
        private UserInterfaceSynchronizer __user_interface_acquisition_unit;

        private OperatingHistory __operating_history;
        private RecentlyOpened __recently_opened;

        private EtherCATPDOViewer __ecat_pdo_viewer;
        private EtherCATVaribleDataTypeConverter __ecat_variable_datatype_converter;

        private CIPAssemblyIOAllocationViewer __cip_assembly_io_viewer;
        private CIPAssemblyIODataTypeConverter __cip_assembly_io_datatype_converter;

        private SmartECATUtility __smart_ecat_utility;

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

            __operating_history = new OperatingHistory(__settings.PreferenceProperty.RecordOperatingUndoQueueDepth);
            __main_model.UndoOperatingRecords = __operating_history.UndoOperatingRecords;
            __main_model.RedoOperatingRecords = __operating_history.RedoOperatingRecords;
            __recently_opened = new RecentlyOpened(__settings.PreferenceProperty.RecentlyOpenedFileCollectionCapacity);
            __main_model.RecentlyOpened = __recently_opened.PathCollection;
            DataContext = __main_model;
        }

        static MainWindow()
        {
            DebugConsole.CreateConsole();
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
                __data_type_catalogue = new DataTypeCatalogue(Settings.DataTypeCataloguePath);
                __controller_model_catalogue = new ControllerModelCatalogue(Settings.ControllerModelCataloguePath);
                Settings.DataTypeCatalogueHash = __data_type_catalogue.MD5Code;
                Settings.ControllerModelCatalogueHash = __controller_model_catalogue.MD5Code;

                DebugConsole.WriteInfo($"Data Type Catalogue Path: {Settings.DataTypeCataloguePath}");
                DebugConsole.WriteInfo($"Data Type Catalogue Hash: {string.Join("-", Settings.DataTypeCatalogueHash.Select(b => b.ToString("X2")))}");
                DebugConsole.WriteInfo($"Controller Model Catalogue Path: {Settings.ControllerModelCataloguePath}");
                DebugConsole.WriteInfo($"Controller Model Catalogue Hash: {string.Join("-", Settings.ControllerModelCatalogueHash.Select(b => b.ToString("X2")))}");
                DebugConsole.WriteInfo($"Seiren Version: {Settings.SeirenVersion}");
                DebugConsole.WriteInfo($"Lombardia Version: {Settings.LombardiaVersion}");
                DebugConsole.WriteInfo($"Gagharv Version: {Settings.GagharvVersion}");
                DebugConsole.WriteInfo($"Tirasweel Version: {Settings.TirasweelVersion}");

                __data_types_viewer = new DataTypesViewer(__data_type_catalogue);
                __device_models_viewer = new DeviceModelsViewer(__controller_model_catalogue);

                __viewers[NavDataTypes] = DataTypes;
                __viewers[NavDeviceModels] = DeviceModels;
                __viewers[NavVariableDictionary] = Variables;
                __viewers[NavControllerConfiguration] = ControllerModules;
                __viewers[NavProcessDataDictionary] = Objects;
                __viewers[NavMiscellaneous] = Miscellaneous;

                DataTypes.Content = __data_types_viewer;
                DeviceModels.Content = __device_models_viewer;

                __user_interface_synchronizer = new UserInterfaceSynchronizer(this, __ui_data_refresh_handler);
                __user_interface_acquisition_unit = new UserInterfaceSynchronizer(this, __daq_ui_data_refresh_handler);

                __ecat_variable_datatype_converter = new EtherCATVaribleDataTypeConverter(__data_type_catalogue);
                __cip_assembly_io_datatype_converter = new CIPAssemblyIODataTypeConverter(__data_type_catalogue);

                if (!System.IO.Directory.Exists(Settings.UserSettingsPath))
                {
                    System.IO.Directory.CreateDirectory(Settings.UserSettingsPath);
                }
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
            __update_binding_source();
            return (__variables_viewer.DataContext as VariablesModel).Modified ||
                    (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).Modified ||
                    (__objects_viewer.DataContext as ObjectsModel).ContentModified ||
                    (__miscellaneous_viewer.DataContext as MiscellaneousModel).Modified;
        }

        private void __collapse_all_viewers()
        {
            foreach(var v in UserControl.Children)
                (v as System.Windows.Controls.ContentControl).Visibility = Visibility.Collapsed;
            __current_user_control = null;
            MainNavigator.SelectedItem = null;
        }

        private void __reset_layout()
        {
            __close_layout();
            __operating_history.Clear();

            __variables_viewer = new VariablesViewer(__variable_dictionary, __data_type_catalogue, __operating_history);
            __controller_configuration_viewer = new ControllerConfigurationViewer(__controller_configuration, __controller_model_catalogue, __operating_history);
            __objects_viewer = new ObjectsViewer(__object_dictionary, __variable_dictionary, __controller_configuration, __variables_viewer.DataContext as VariablesModel, __controller_configuration_viewer.DataContext as ControllerConfigurationModel,
                __tx_diagnostic_area, __tx_bit_area, __tx_block_area, __rx_control_area, __rx_bit_area, __rx_block_area, __interlock_area, __operating_history);
            __miscellaneous_viewer = new MiscellaneousViewer(__misc_info);

            (__variables_viewer.DataContext as VariablesModel).SubscriberObjects = __objects_viewer.DataContext as ObjectsModel;
            (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).SubscriberObjects = __objects_viewer.DataContext as ObjectsModel;

            NavVariableDictionary.DataContext = __variables_viewer.DataContext;
            NavControllerConfiguration.DataContext = __controller_configuration_viewer.DataContext;
            NavProcessDataDictionary.DataContext = __objects_viewer.DataContext;
            NavMiscellaneous.DataContext = __miscellaneous_viewer.DataContext;

            Variables.Content = __variables_viewer;
            ControllerModules.Content = __controller_configuration_viewer;
            Objects.Content = __objects_viewer;
            Miscellaneous.Content = __miscellaneous_viewer;

            __collapse_all_viewers();
        }

        private void __close_layout()
        {
            __variables_viewer = null;
            __controller_configuration_viewer = null;
            __objects_viewer = null;
            __miscellaneous_viewer = null;

            NavVariableDictionary.DataContext = null;
            NavControllerConfiguration.DataContext = null;
            NavProcessDataDictionary.DataContext = null;
            NavMiscellaneous.DataContext = null;

            Variables.Content = __variables_viewer;
            ControllerModules.Content = __controller_configuration_viewer;
            Objects.Content = __objects_viewer;
            Miscellaneous.Content = __miscellaneous_viewer;

            __collapse_all_viewers();
            __operating_history.Clear();
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
                (__tx_diagnostic_area.OffsetInWord, __tx_diagnostic_area.SizeInWord),
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

        private void __startup_debugger(DataSyncMode rxbit, DataSyncMode rxblock, DataSyncMode rxcotrol)
        {
            DebugConsole.WriteInfo("Startup Debugger");
            (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.ResetProcessDataValue();
            (__objects_viewer.DataContext as ObjectsModel).InterlockLogics.ResetProcessDataValue();

            __tx_diagnostic_area_data = new ushort[__tx_diagnostic_area.ActualSizeInWord];
            __tx_bit_area_data = new ushort[__tx_bit_area.ActualSizeInWord];
            __tx_block_area_data = new ushort[__tx_block_area.ActualSizeInWord];
            __rx_control_area_data = new ushort[__rx_control_area.ActualSizeInWord];
            __rx_bit_area_data = new ushort[__rx_bit_area.ActualSizeInWord];
            __rx_block_area_data = new ushort[__rx_block_area.ActualSizeInWord];

            __area_data_array = new List<ushort[]>()
            {
                __tx_diagnostic_area_data, __tx_bit_area_data, __tx_block_area_data,
                __rx_control_area_data, __rx_bit_area_data, __rx_block_area_data
            };
            __area_data_end = 0;

            var areas = new List<(uint, uint, IEnumerable<(uint bitpos, uint bitsize)>, DataSyncMode)>()
            {
                (__tx_diagnostic_area.OffsetInWord, (__tx_diagnostic_area.ActualSizeInWord), __tx_diagnostic_area.ProcessDatas.Select(p => new ValueTuple<uint, uint>(p.BitPos, p.ProcessObject.Variable.Type.BitSize)), DataSyncMode.Read),
                (__tx_bit_area.OffsetInWord, (__tx_bit_area.ActualSizeInWord),  __tx_bit_area.ProcessDatas.Select(p => new ValueTuple<uint, uint>(p.BitPos, p.ProcessObject.Variable.Type.BitSize)), DataSyncMode.Read),
                (__tx_block_area.OffsetInWord, (__tx_block_area.ActualSizeInWord),  __tx_block_area.ProcessDatas.Select(p => new ValueTuple<uint, uint>(p.BitPos, p.ProcessObject.Variable.Type.BitSize)), DataSyncMode.Read),
                (__rx_control_area.OffsetInWord, (__rx_control_area.ActualSizeInWord),  __rx_control_area.ProcessDatas.Select(p => new ValueTuple<uint, uint>(p.BitPos, p.ProcessObject.Variable.Type.BitSize)), rxcotrol),
                (__rx_bit_area.OffsetInWord, (__rx_bit_area.ActualSizeInWord),  __rx_bit_area.ProcessDatas.Select(p => new ValueTuple<uint, uint>(p.BitPos, p.ProcessObject.Variable.Type.BitSize)), rxbit),
                (__rx_block_area.OffsetInWord, (__rx_block_area.ActualSizeInWord),  __rx_block_area.ProcessDatas.Select(p => new ValueTuple<uint, uint>(p.BitPos, p.ProcessObject.Variable.Type.BitSize)), rxblock)
            };

            __data_synchronizer = new DataSynchronizer(areas);
            __main_model.IsBusy = true;
            BusyDialog diag = new BusyDialog(__data_synchronizer.Startup(__settings.SlmpTargetProperty, __area_data_array));
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

            (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.IsMonitoring = true;
            (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.IsMonitoring = true;
            (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.IsMonitoring = true;
            (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.IsMonitoring = rxcotrol == DataSyncMode.Write ? false : true;
            (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.IsMonitoring = rxbit == DataSyncMode.Write ? false : true;
            (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.IsMonitoring = rxblock == DataSyncMode.Write ? false : true;

            //if (monitoring == false)
            {
                (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.InitProcessDataValue(__tx_diagnostic_area_data);
                (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.InitProcessDataValue(__tx_bit_area_data);
                (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.InitProcessDataValue(__tx_block_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.InitProcessDataValue(__rx_control_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.InitProcessDataValue(__rx_bit_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.InitProcessDataValue(__rx_block_area_data);
            }

            __main_model.IsOffline = false;
            __main_model.IsMonitoring = rxcotrol != DataSyncMode.Write && rxbit != DataSyncMode.Write && rxblock != DataSyncMode.Write;
            __main_model.DebuggerExceptionMessage = "N/A";
            __main_model.DebuggerState =  DataSynchronizerState.Ready;
            __main_model.DebuggerPollingInterval = 0;
            __main_model.DebuggerHeartbeat = 0;

            __user_interface_synchronizer.Startup(__settings.SlmpTargetProperty.PollingInterval);
            CommandManager.InvalidateRequerySuggested();
        }

        private void __stop_debugger() 
        {
            DebugConsole.WriteInfo("Stopping Debugger");
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

            __tx_diagnostic_area_data = null;
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
                (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.ProcessDataValueChanged(__tx_diagnostic_area_data);
                (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.ProcessDataValueChanged(__tx_bit_area_data);
                (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.ProcessDataValueChanged(__tx_block_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.ProcessDataValueChanged(__rx_control_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.ProcessDataValueChanged(__rx_bit_area_data);
                (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.ProcessDataValueChanged(__rx_block_area_data);
                (__objects_viewer.DataContext as ObjectsModel).InterlockLogics.ProcessDataValueChanged(__tx_bit_area_data, __rx_bit_area_data);
            }

            if (__main_model.DebuggerState == DataSynchronizerState.Exception)
            {
                DebugConsole.WriteException($"Debugger Exception({__main_model.DebuggerExceptionMessage})");
                __stop_debugger();
            }
        }

        private void __startup_daq_unit()
        {
            DebugConsole.WriteInfo("Startup Data Acquisition");
            __data_acquisition_unit = new DataAcquisitionUnit();
            __main_model.IsBusy = true;
            BusyDialog diag = new BusyDialog(__data_acquisition_unit.Startup(__settings.DAQTargetProperty, 
                __tx_diagnostic_area.ProcessDatas, __tx_bit_area.ProcessDatas, __tx_block_area.ProcessDatas,
                __rx_control_area.ProcessDatas, __rx_bit_area.ProcessDatas, __rx_block_area.ProcessDatas));
            diag.ShowDialog();
            __main_model.IsBusy = false;

            (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.IsDataAcquisiting = true;
            (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.IsDataAcquisiting = true;
            (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.IsDataAcquisiting = true;
            (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.IsDataAcquisiting = true;
            (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.IsDataAcquisiting = true;
            (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.IsDataAcquisiting = true;

            __main_model.IsDataAcquisiting = true;
            __main_model.DAQUnitExceptionMessage = "N/A";
            __main_model.DAQUnitState = AcquisitionUnitState.Idle;
            __main_model.DAQUnitDiskWriteInterval = 0;
            __main_model.DAQUnitHeartbeat = 0;
            __main_model.DAQUnitStatus = new AcquisitionUnitStatus();

            __user_interface_acquisition_unit.Startup(__settings.DAQTargetProperty.ExpectedDiskWriteInterval);
            CommandManager.InvalidateRequerySuggested();
        }

        private void __stop_daq_unit()
        {
            DebugConsole.WriteInfo("Stopping Data Acquisition");
            __user_interface_acquisition_unit.Stop();
            __main_model.IsBusy = true;
            BusyDialog diag = new BusyDialog(__data_acquisition_unit.Stop());
            diag.ShowDialog();
            __main_model.IsBusy = false;
            __data_acquisition_unit = null;

            (__objects_viewer.DataContext as ObjectsModel).TxDiagnosticObjects.IsDataAcquisiting = false;
            (__objects_viewer.DataContext as ObjectsModel).TxBitObjects.IsDataAcquisiting = false;
            (__objects_viewer.DataContext as ObjectsModel).TxBlockObjects.IsDataAcquisiting = false;
            (__objects_viewer.DataContext as ObjectsModel).RxControlObjects.IsDataAcquisiting = false;
            (__objects_viewer.DataContext as ObjectsModel).RxBitObjects.IsDataAcquisiting = false;
            (__objects_viewer.DataContext as ObjectsModel).RxBlockObjects.IsDataAcquisiting = false;

            __main_model.IsDataAcquisiting = false;
            __main_model.DAQUnitDiskWriteInterval = 0;
            __main_model.DAQUnitHeartbeat = 0;
            if (__main_model.DAQUnitState != AcquisitionUnitState.Exception)
                __main_model.DAQUnitState = AcquisitionUnitState.Idle;
            CommandManager.InvalidateRequerySuggested();
        }

        private void __daq_ui_data_refresh_handler()
        {
            if (__main_model.IsDataAcquisiting == false || __data_acquisition_unit == null || __main_model.IsBusy)
                return;
            __main_model.DAQUnitExceptionMessage = __data_acquisition_unit.ExceptionMessage;
            __main_model.DAQUnitState = __data_acquisition_unit.State;
            __main_model.DAQUnitDiskWriteInterval = __data_acquisition_unit.DiskWriteInterval;
            __main_model.DAQUnitHeartbeat = __data_acquisition_unit.Counter;
            __main_model.DAQUnitStatus = __data_acquisition_unit.Status;

            if (__main_model.DAQUnitState == AcquisitionUnitState.Exception)
            {
                DebugConsole.WriteException($"Data Acquisition Exception({ __main_model.DAQUnitExceptionMessage})");
                __stop_daq_unit();
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
            if(e.Item.Header as string == "Open Settings")
            {
                SettingsViewer st = new SettingsViewer(__settings);
                st.ShowDialog();
            }
            else if (e.Item.Header as string == "Import Settings")
            {
                System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
                open.Filter = "Seiren Configuration File(*.json)|*.json";
                open.Multiselect = false;
                open.InitialDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, Settings.UserSettingsPath);
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Settings s = new Settings(open.FileName);
                    SettingsViewer st = new SettingsViewer(__settings, s);
                    st.ShowDialog();
                }
            }
            else if (e.Item.Header as string == "Export Settings")
            {
                System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog() { DefaultExt = "json", AddExtension = true };
                save.InitialDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, Settings.UserSettingsPath);
                save.Filter = "Seiren Configuration File(*.json)|*.json";
                if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        __settings.Save(save.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"At least one unexpected error occured while saving settings to configuration file : '{save.FileName}'.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
            else if (__viewers.TryGetValue(e.Item, out v) && v != __current_user_control)
            {
                foreach (var content in UserControl.Children)
                    (content as System.Windows.Controls.ContentControl).Visibility = Visibility.Collapsed;
                (v as System.Windows.Controls.ContentControl).Visibility = Visibility.Visible;

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
            if (__main_model.IsNonTemporaryFile)
            {
                save.FileName = System.IO.Path.GetFileName(__main_model.CurrentlyOpenFile);
                save.InitialDirectory = System.IO.Path.GetDirectoryName(__main_model.CurrentlyOpenFile);
            }
            else
            {
                save.FileName = "io_list.folst";
                save.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    DebugConsole.WriteInfo($"Save flie '{save.FileName}'.");
                    IOCelcetaHelper.Save(save.FileName,
                        __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                        __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                        __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                        __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info);
                    __main_model.CurrentlyOpenFile = save.FileName;
                    __commit_changes();
                    __recently_opened.Add(save.FileName);
                }
                catch(LombardiaException ex)
                {
                    DebugConsole.WriteException(ex);
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
                DebugConsole.WriteInfo($"Save flie '{__main_model.CurrentlyOpenFile}'.");
                IOCelcetaHelper.Save(__main_model.CurrentlyOpenFile,
                    __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                    __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                    __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info);
                __commit_changes();
            }
            catch (LombardiaException ex)
            {
                DebugConsole.WriteException(ex);
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
                    DebugConsole.WriteInfo($"Open flie '{open.FileName}'.");
                    (__variable_dictionary, __controller_configuration, __object_dictionary,
                        __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area, 
                        __interlock_area, __misc_info) = IOCelcetaHelper.Load(open.FileName, __data_type_catalogue, __controller_model_catalogue, out _);
                    __main_model.CurrentlyOpenFile = open.FileName;
                    __recently_opened.Add(open.FileName);
                    __reset_layout();
                }
                catch (LombardiaException ex)
                {
                    DebugConsole.WriteException(ex);
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy && !__main_model.IsDataAcquisiting;
        }

        private void ExportCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string? error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var wnd = new ImportExport(ImportExportMode.Export,
                    __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                    __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                    __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue,
                    new ImportExportPreference()
                    {
                        SeparateOutHardwareIntlk = __settings.PreferenceProperty.SeparateHardwareInterlocks,
                        SeparateOutExclusiveIntlk = __settings.PreferenceProperty.SeparateExclusiveInterlocks,
                        XlsSheetProtectionPassword = __settings.PreferenceProperty.XlsSheetProtectionPassword,
                        HardwareIntlkSheetName = __settings.PreferenceProperty.HardwareInterlocksAlias,
                        NonHardwareIntlkSheetName = __settings.PreferenceProperty.NonHardwareInterlocksAlias,
                        ExclusiveIntlkSheetName = __settings.PreferenceProperty.ExclusiveInterlocksAlias,
                        NonExclusiveIntlkSheetName = __settings.PreferenceProperty.NonExclusiveInterlocksAlias
                    },
                    __main_model.IsNonTemporaryFile ? __main_model.CurrentlyOpenFile : null);
            wnd.ShowDialog();
        }

        private void ExportCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened;//__main_model.IsNonTemporaryFile && !__data_model_has_changes();
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
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area, 
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue,
                    null,
                    __main_model.IsNonTemporaryFile ? __main_model.CurrentlyOpenFile : null);
            if(wnd.ShowDialog() == true)
            {
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                        __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info) = wnd.ImportResult;
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
        }

        private void ImportCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy && !__main_model.IsDataAcquisiting;
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
                DebugConsole.WriteInfo($"Create new file with default settings.");
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info) = IOCelcetaHelper.Default(__data_type_catalogue, __controller_model_catalogue);
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
            catch (LombardiaException ex)
            {
                DebugConsole.WriteException(ex);
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy && !__main_model.IsDataAcquisiting;
        }

        private void QuitCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            //
            Close();
        }

        private void DownloadviaFTPCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string? error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var wnd = new FTPUtility(FTPMode.Download,
                   __variable_dictionary, (__variables_viewer.DataContext as VariablesModel).VariableNames,
                   __controller_configuration, (__controller_configuration_viewer.DataContext as ControllerConfigurationModel).ReferenceNames,
                   __object_dictionary, (__objects_viewer.DataContext as ObjectsModel).ObjectIndexes,
                   __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                   __rx_control_area, __rx_bit_area, __rx_block_area,
                   __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue,
                   __settings.FTPTargetProperty, __settings.AppInstallerProperty);
            wnd.ShowDialog();
        }

        private void DownloadviaFTPCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened;//__main_model.IsNonTemporaryFile && !__data_model_has_changes();
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
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue,
                    __settings.FTPTargetProperty, __settings.AppInstallerProperty);
            if (wnd.ShowDialog() == true)
            {
                DebugConsole.WriteInfo($"Create new file with settings reading from connected controller.");
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                        __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                        __rx_control_area, __rx_bit_area, __rx_block_area,
                        __interlock_area, __misc_info) = wnd.UploadResult;
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
        }

        private void UploadviaFTPCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsBusy && !__main_model.IsDataAcquisiting;
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

            DebugConsole.WriteInfo($"Close flie '{__main_model.CurrentlyOpenFile}'.");

            __variable_dictionary = null;
            __controller_configuration = null;
            __object_dictionary = null;
            __tx_diagnostic_area = null;
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
            e.CanExecute = __main_model.IsOpened && __main_model.IsOffline && !__main_model.IsBusy && !__main_model.IsDataAcquisiting;
        }

        private void StartMonitoringCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            __startup_debugger(DataSyncMode.Read, DataSyncMode.Read, DataSyncMode.Read);
        }

        private void StartDebuggingCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if(__settings.PreferenceProperty.RxBitAreaSyncMode == DataSyncMode.Write || 
                __settings.PreferenceProperty.RxBlockAreaSyncMode == DataSyncMode.Write)
            {
                var res = MessageBox.Show("Debugging RxBitArea/TxBlockArea in WRITING_MODE (May conflict with AMEC GUI) ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            __startup_debugger(__settings.PreferenceProperty.RxBitAreaSyncMode, 
                __settings.PreferenceProperty.RxBlockAreaSyncMode, 
                __settings.PreferenceProperty.RxControlAreaSyncMode);
        }

        private void StartDebuggingCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened && !__data_model_has_changes() && __main_model.IsOffline && !__main_model.IsBusy;
        }

        private void StartBackgroundDAQCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (__tx_diagnostic_area.ProcessDatas.Any(d => d.DAQ == true) == false &&
                __tx_block_area.ProcessDatas.Any(d => d.DAQ == true) == false &&
                __tx_bit_area.ProcessDatas.Any(d => d.DAQ == true) == false &&
                __rx_control_area.ProcessDatas.Any(d => d.DAQ == true) == false &&
                __rx_block_area.ProcessDatas.Any(d => d.DAQ == true) == false &&
                __rx_bit_area.ProcessDatas.Any(d => d.DAQ == true) == false)
            {
                MessageBox.Show("No process data objects marked as DAQ were found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            __startup_daq_unit();
        }

        private void StartBackgroundDAQCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened && !__data_model_has_changes() && !__main_model.IsDataAcquisiting && !__main_model.IsBusy;
        }

        private void StopDebuggingCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            __stop_debugger();
        }

        private void StopDebuggingCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !__main_model.IsOffline && !__main_model.IsBusy;
        }

        private void StopBackgroundDAQCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            __stop_daq_unit();
        }

        private void StopBackgroundDAQCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsDataAcquisiting && !__main_model.IsBusy;
        }

        private void RecordUndoCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            OperatingRecord rd;
            if (e.Parameter == null)
            {
                rd = __operating_history.Undo();
                rd.Host.Undo(rd);
            }
            else
            {
                do
                {
                    rd = __operating_history.Undo();
                    rd.Host.Undo(rd);
                }
                while (rd != e.Parameter);
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void RecordUndoCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && __main_model.IsOpened && __operating_history.CanUndo && !__main_model.IsDataAcquisiting;
        }

        private void RecordRedoCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            OperatingRecord rd;
            if (e.Parameter == null)
            {
                rd = __operating_history.Redo();
                rd.Host.Redo(rd);
            }
            else
            {
                //__main_model.IsBusy = true;
                //BusyDialog diag = new BusyDialog(__data_synchronizer.Stop());
                //diag.ShowDialog();
                //__main_model.IsBusy = false;
                do
                {
                    rd = __operating_history.Redo();
                    rd.Host.Redo(rd);
                }
                while (rd != e.Parameter);
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void RecordRedoCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && __main_model.IsOpened && __operating_history.CanRedo && !__main_model.IsDataAcquisiting;
        }

        private void OpenCompareCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string? error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Foliage Ocean List File(*.folst)|*.folst";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    MessageBox.Show(__compare_result(IOCelcetaHelper.Load(open.FileName, __data_type_catalogue, __controller_model_catalogue, out _), true), "Comparison Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportCompareCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string? error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var wnd = new ImportExport(ImportExportMode.Compare,
                    __variable_dictionary, null,
                    __controller_configuration, null,
                    __object_dictionary, null,
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue,
                    null,
                    __main_model.IsNonTemporaryFile ? __main_model.CurrentlyOpenFile : null);
            if (wnd.ShowDialog() == true)
                MessageBox.Show(__compare_result(wnd.ImportResult, (wnd.DataContext as ImportExportModel).XMLIO), "Comparison Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UploadCompareCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string? error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var wnd = new FTPUtility(FTPMode.Compare,
                    __variable_dictionary, null,
                    __controller_configuration, null,
                    __object_dictionary, null,
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info, __data_type_catalogue, __controller_model_catalogue, __settings.FTPTargetProperty, __settings.AppInstallerProperty);
            if (wnd.ShowDialog() == true)
                MessageBox.Show(__compare_result(wnd.UploadResult, (wnd.DataContext as FTPUtilityModel).IO), "Comparison Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CompareCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened;
        }

        private void SaveLayoutStateCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            __objects_viewer.SaveLayoutState(null);
        }

        private void SaveLayoutStateCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened && __objects_viewer.LayoutFinished && false;
        }
        private void RemoteOperationCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            RemoteOperation remote = new RemoteOperation(__settings.SlmpTargetProperty);
            remote.ShowDialog();
        }

        private void RemoteOperationCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOffline && !__main_model.IsDataAcquisiting;
        }

        private void BrowseEtherCATPDOs_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "EtherCAT-Network-Information Files(*.xml)|*.xml";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    __ecat_pdo_viewer = new EtherCATPDOViewer(new ENIUtilityModel(open.FileName), this);
                    __ecat_pdo_viewer.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BrowseEtherCATPDOs_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __ecat_pdo_viewer == null || __ecat_pdo_viewer.IsClosed;
        }

        private void BrowseCIPAssemblyIOs_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "CIP Assembly IO Allocation Files(*.xml)|*.xml";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    __cip_assembly_io_viewer = new CIPAssemblyIOAllocationViewer(new CIPAssemblyIOAllocationModel(open.FileName), this);
                    __cip_assembly_io_viewer.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BrowseCIPAssemblyIOs_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __cip_assembly_io_viewer == null || __cip_assembly_io_viewer.IsClosed;
        }

        private void SmartECATUtiltiy_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if(MessageBox.Show("Are you sure to launch SMART-ECAT Utility ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                __smart_ecat_utility = new SmartECATUtility(new SmartECATUtilityModel());
                __smart_ecat_utility.Show();
            }           
        }

        private void SmartECATUtiltiy_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __smart_ecat_utility == null || __smart_ecat_utility.IsClosed;
        }

        private string __compare_result((VariableDictionary vd, ControllerConfiguration cc, ObjectDictionary od,
                    ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
                    ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk, InterlockCollection intlk,
                    Miscellaneous misc) target, bool io)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Variable Dictionary");
            if (__variable_dictionary.IsEquivalent(target.vd))
                sb.Append("\t\t✔\n");
            else
                sb.Append("\t\t✖\n");
            if (io)
            {
                sb.Append("Controller Configuration");
                if (__controller_configuration.IsEquivalent(target.cc))
                    sb.Append("\t✔\n");
                else
                    sb.Append("\t✖\n");

                sb.Append("Object Dictionary");
                if (__object_dictionary.IsEquivalent(target.od))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Tx Diagnostic Area");
                if (__tx_diagnostic_area.IsEquivalent(target.txdiag))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Tx Bit Area");
                if (__tx_bit_area.IsEquivalent(target.txbit))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Tx Block Area");
                if (__tx_block_area.IsEquivalent(target.txblk))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Rx Control Area");
                if (__rx_control_area.IsEquivalent(target.rxctl))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Rx Bit Area");
                if (__rx_bit_area.IsEquivalent(target.rxbit))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Rx Block Area");
                if (__rx_block_area.IsEquivalent(target.rxblk))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Interlock Area");
                if (__interlock_area.IsEquivalent(target.intlk))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");

                sb.Append("Miscellaneous");
                if (__misc_info.IsEquivalent(target.misc))
                    sb.Append("\t\t✔\n");
                else
                    sb.Append("\t\t✖\n");
            }
            else
                sb.Append("IO List File \t\tNot Implemented\n");
            return sb.ToString();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (__main_model.IsOffline == false || __main_model.IsDataAcquisiting)
            {
                MessageBox.Show("Please stop debugging/monitoring/DAQ first .", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Cancel = true;
            }
            else
            {
                if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
                {
                    var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.No)
                        e.Cancel = true;
                }
            }
            if (e.Cancel == false && __ecat_pdo_viewer != null)
                __ecat_pdo_viewer.Close();
            if (e.Cancel == false && __cip_assembly_io_viewer != null)
                __cip_assembly_io_viewer.Close();

            if (e.Cancel == false && __smart_ecat_utility != null)
                __smart_ecat_utility.Close();
        }

        private void UndoMenuItemAdv_Click(object sender, RoutedEventArgs e)
        {
            UndoButton.IsDropDownOpen = false;
        }

        private void RedoMenuItemAdv_Click(object sender, RoutedEventArgs e)
        {
            RedoButton.IsDropDownOpen = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            __recently_opened.Flush(); 
        }

        private void RecentlyOpened_Click(object sender, RoutedEventArgs e)
        {
            Syncfusion.Windows.Shared.MenuItemAdv menu = e.OriginalSource as Syncfusion.Windows.Shared.MenuItemAdv;
            if (RecentlyOpenedMenu == e.OriginalSource || __recently_opened.PathCollection.Count == 0 || __main_model.IsOffline == false || __main_model.IsBusy == true || __main_model.IsDataAcquisiting)
                return;
           
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }
            string path = menu.Header.ToString();
            try
            {
                DebugConsole.WriteInfo($"Open flie '{path}'.");
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info) = IOCelcetaHelper.Load(path, __data_type_catalogue, __controller_model_catalogue, out _);
                __main_model.CurrentlyOpenFile = path;
                __recently_opened.Add(path);
                __reset_layout();
            }
            catch (LombardiaException ex)
            {
                DebugConsole.WriteException(ex);
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Source != CurrentlyOpenedFile)
                return;

            string path = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];

            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }
     
            try
            {
                DebugConsole.WriteInfo($"Open flie '{path}'.");
                (__variable_dictionary, __controller_configuration, __object_dictionary,
                    __tx_diagnostic_area, __tx_bit_area, __tx_block_area,
                    __rx_control_area, __rx_bit_area, __rx_block_area,
                    __interlock_area, __misc_info) = IOCelcetaHelper.Load(path, __data_type_catalogue, __controller_model_catalogue, out _);
                __main_model.CurrentlyOpenFile = path;
                __recently_opened.Add(path);
                __reset_layout();
            }
            catch (LombardiaException ex)
            {
                DebugConsole.WriteException(ex);
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Source == CurrentlyOpenedFile)
            {
                var p = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (__main_model.IsDataAcquisiting || __main_model.IsOffline == false || __main_model.IsBusy == true || p == null || p.Length != 1 || File.Exists(p[0]) == false || p[0].EndsWith(".folst") == false)
                {
                    e.Handled = true;
                    e.Effects = DragDropEffects.None;
                }
                else
                    e.Effects = DragDropEffects.Copy;
            }
        }

        public void AddEtherCATVariable(EtherCATVariableInfo info)
        {
            if (__variables_viewer != null)
                __variables_viewer.AddEtherCATVariable(info, __ecat_variable_datatype_converter);
            else
                MessageBox.Show("Perhaps you should first create a new file or open an existing one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void AddEtherCATVariables(IEnumerable<EtherCATVariableInfo> infos)
        {
            if (__variables_viewer != null)
                __variables_viewer.AddEtherCATVariables(infos, __ecat_variable_datatype_converter, true);
            else
                MessageBox.Show("Perhaps you should first create a new file or open an existing one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void EventHistory_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

            EventLogViewer viewer = new EventLogViewer(__settings.FTPTargetProperty);
            viewer.ShowDialog();
        }

        private void EventHistory_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void AddCIPAssemblyIO(CIPAssemblyIOInfo info)
        {
            if (__variables_viewer != null)
                __variables_viewer.AddCIPAssemblyIO(info, __cip_assembly_io_datatype_converter);
            else
                MessageBox.Show("Perhaps you should first create a new file or open an existing one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void AddCIPAssemblyIOs(IEnumerable<CIPAssemblyIOInfo> infos)
        {
            if (__variables_viewer != null)
                __variables_viewer.AddCIPAssemblyIOs(infos, __cip_assembly_io_datatype_converter, true);
            else
                MessageBox.Show("Perhaps you should first create a new file or open an existing one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
