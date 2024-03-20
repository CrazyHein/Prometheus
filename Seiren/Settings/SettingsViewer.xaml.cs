using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.IOUtility;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Master;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Message;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.DAQ;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility;
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
using DAQInterface = AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.IOUtility.SocketInterface;
using DAQPort = AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.IOUtility.TCP;
using DAQMaster = AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol.Master;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ.Protocol;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.OrbmentDAQ;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// SettingsViewer.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsViewer : Window
    {
        private int __errors = 0;
        public Settings Settings { get; private set; }
        public bool HasError { get { return __errors != 0; } }
        public SettingsViewer(Settings settings)
        {
            InitializeComponent();
            Settings = settings;
            DebuggerSettings.DataContext = Settings.SlmpTargetProperty.Copy();
            DAQSettings.DataContext = Settings.DAQTargetProperty.Copy();
            PreferenceSettings.DataContext = Settings.PreferenceProperty.Copy();
            FTPSettings.DataContext = Settings.FTPTargetProperty.Copy();
            AppInstallerSettings.DataContext = Settings.AppInstallerProperty.Copy();
        }

        public SettingsViewer(Settings settings, Settings import)
        {
            InitializeComponent();
            Settings = settings;
            DebuggerSettings.DataContext = import.SlmpTargetProperty.Copy();
            DAQSettings.DataContext = import.DAQTargetProperty.Copy();
            PreferenceSettings.DataContext = import.PreferenceProperty.Copy();
            FTPSettings.DataContext = import.FTPTargetProperty.Copy();
            AppInstallerSettings.DataContext = import.AppInstallerProperty.Copy();
        }

        private void DebuggerSettings_Error(object sender, ValidationErrorEventArgs e)
        {
            if(e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }
        private void DAQSettings_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }
        private void FTPSettings_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }
        private void AppInstallerSettings_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (HasError)
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                Settings.SlmpTargetProperty = DebuggerSettings.DataContext as SlmpTargetProperty;
                Settings.DAQTargetProperty = DAQSettings.DataContext as DAQTargetProperty;
                Settings.PreferenceProperty = PreferenceSettings.DataContext as PreferenceProperty;
                Settings.FTPTargetProperty = FTPSettings.DataContext as FTPTargetProperty;
                Settings.AppInstallerProperty = AppInstallerSettings.DataContext as AppInstallerProperty;
                try
                {
                    Settings.Save();
                }
                catch (Exception ex)
                {                    
                    MessageBox.Show(this, $"At least one unexpected error occured while saving settings to configuration file : '{Settings.SettingsPath}'.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (!System.IO.Directory.Exists(Settings.DAQTargetProperty.DataFilePath))
                {
                    if (MessageBox.Show($"Create directory --> {System.IO.Path.GetFullPath(Settings.DAQTargetProperty.DataFilePath)}?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(Settings.DAQTargetProperty.DataFilePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, $"At least one unexpected error occured while creating directory : '{Settings.DAQTargetProperty.DataFilePath}'.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error); 
                        }
                    }
                }
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (HasError)
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog() { DefaultExt = "json", AddExtension = true };
                save.InitialDirectory = System.IO.Path.Combine(System.Environment.CurrentDirectory, Settings.UserSettingsPath);
                save.Filter = "Seiren Configuration File(*.json)|*.json";
                if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        Settings s = new Settings(DebuggerSettings.DataContext as SlmpTargetProperty,
                            DAQSettings.DataContext as DAQTargetProperty,
                            FTPSettings.DataContext as FTPTargetProperty,
                            AppInstallerSettings.DataContext as AppInstallerProperty,
                            PreferenceSettings.DataContext as PreferenceProperty);
                        s.Save(save.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"At least one unexpected error occured while saving settings to configuration file : '{save.FileName}'.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void SLMPTest_Click(object sender, RoutedEventArgs e)
        {
            if (HasError)
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var property = DebuggerSettings.DataContext as SlmpTargetProperty;
                SocketInterface com = null;

                SLMPBusyIndicator.IsBusy = true;
                IsEnabled = false;
                try
                {
                    DESTINATION_ADDRESS_T destination = new DESTINATION_ADDRESS_T(
                        property.NetworkNumber, property.StationNumber, property.ModuleIONumber,
                        property.MultidropNumber, property.ExtensionStationNumber);

                    if (property.UDPTransportLayer == true)
                    {
                        com = new UDP(new System.Net.IPEndPoint(property.SourceIPv4, property.SourcePort),
                                new System.Net.IPEndPoint(property.DestinationIPv4, property.DestinationPort),
                                property.ReceiveBufferSize, property.SendTimeoutValue, property.ReceiveTimeoutValue);
                    }
                    else
                    {
                        com = new TCP(new System.Net.IPEndPoint(property.SourceIPv4, 0),
                                new System.Net.IPEndPoint(property.DestinationIPv4, property.DestinationPort),
                                property.SendTimeoutValue, property.ReceiveTimeoutValue);
                        await Task.Run(() => (com as TCP).Connect());
                    }

                    var master = new RemoteOperationMaster(property.FrameType, property.DataCode, property.R_DedicatedMessageFormat, com, ref destination,
                        property.SendBufferSize, property.ReceiveBufferSize, null);


                    (ushort end, string name, ushort code) = await master.ReadTypeNameAsync(property.MonitoringTimer);

                    if (end == (ushort)RESPONSE_MESSAGE_ENDCODE_T.NO_ERROR)
                        MessageBox.Show(this, $"Communication success, the destination device is { name.Trim()} (0x{code:X4}).", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show(this, $"The destination device returns end code 0x{end:X4}.", "Warning Message", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                catch (SLMPException ex)
                {
                    MessageBox.Show(this, "At least one unexpected error occured while doing communication test.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(this, "At least one unexpected error occured while doing communication test.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    SLMPBusyIndicator.IsBusy = false;
                    IsEnabled = true;
                    if (com != null) com.Dispose();
                    com = null;
                }
            }
        }

        private async void DAQTest_Click(object sender, RoutedEventArgs e)
        {
            
            if (HasError)
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var property = DAQSettings.DataContext as DAQTargetProperty;
                DAQPort com = null;

                DAQBusyIndicator.IsBusy = true;
                IsEnabled = false;
                try
                { 
                    com = new DAQPort(new System.Net.IPEndPoint(property.SourceIPv4, 0),
                            new System.Net.IPEndPoint(property.DestinationIPv4, property.DestinationPort),
                            property.SendTimeoutValue, property.ReceiveTimeoutValue);
                    await Task.Run(() => com.Connect());

                    var master = new DAQMaster(com, property.InternalReservedBufferSize);

                    DAQ_SERVER_INFO_T info = await master.AcquisiteServerInfoAsync(0x5555); 

                    ReadOnlyMemory<byte> data = null;
                    int received = 0;
                    do
                        (received, _, _, data) = await master.AcquisiteDataAsync(true, 0xAAAA, 1);
                    while (received != 1);
                    uint t0 = MemoryMarshal.Read<uint>(data.Span);
                    do
                        (received, _, _, data) = await master.AcquisiteDataAsync(true, 0xAAAA, 1);
                    while (received != 1);
                    uint t1 = MemoryMarshal.Read<uint>(data.Span);

                    string msg = String.Format("Communication success, the DAQ server returns following information:\nCapacity : {0}\nTx : {1}/{2}/{3}/{4}\nRx : {5}/{6}/{7}/{8}\nSample Rate : {9}",
                        info.capacity, 
                        info.tx_timestamp_size_in_word, info.diag_size_in_word, info.tx_bit_size_in_word, info.tx_blk_size_in_word,
                        info.rx_timestamp_size_in_word, info.ctrl_size_in_word, info.rx_bit_size_in_word, info.rx_blk_size_in_word,
                        (t1 - t0 + 500u)/1000u);
                    MessageBox.Show(this, msg,
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                catch (DAQException ex)
                {
                    MessageBox.Show(this, "At least one unexpected error occured while doing communication test.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "At least one unexpected error occured while doing communication test.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DAQBusyIndicator.IsBusy = false;
                    IsEnabled = true;
                    if (com != null) com.Dispose();
                    com = null;
                }
            }
        }
    }
}
