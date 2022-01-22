using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.IOUtility;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Master;
using AMEC.PCSoftware.CommunicationProtocol.CrazyHein.SLMP.Message;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Debugger;
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
            DebuggerSettings.DataContext = Settings.SlmpTargetProperty;
            PreferenceSettings.DataContext = Settings.PreferenceProperty;
        }

        private void DebuggerSettings_Error(object sender, ValidationErrorEventArgs e)
        {
            if(e.Action == ValidationErrorEventAction.Added)
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
                Settings.Save();
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
            DialogResult = false;
        }

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            if (HasError)
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var property = DebuggerSettings.DataContext as SlmpTargetProperty;
                SocketInterface com = null;
                
                BusyIndicator.IsBusy = true;
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
                    BusyIndicator.IsBusy = false;
                    IsEnabled = true;
                    if (com != null) com.Dispose();
                    com = null;
                }
            }
        }
    }
}
