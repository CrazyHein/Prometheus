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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    /// <summary>
    /// RemoteOperation.xaml 的交互逻辑
    /// </summary>
    public partial class RemoteOperation : Window
    {
        private SlmpTargetProperty __target;
        public RemoteOperation(SlmpTargetProperty target)
        {
            InitializeComponent();
            __target = target;
            DataContext = new RemoteOperationModel();
        }

        private async void RemoteOperation_Click(object sender, RoutedEventArgs e)
        {
            SocketInterface com = null;
            var ro = DataContext as RemoteOperationModel;
            ro.IsBusy = true;
            try
            {
                DESTINATION_ADDRESS_T destination = new DESTINATION_ADDRESS_T(
                        __target.NetworkNumber, __target.StationNumber, __target.ModuleIONumber,
                        __target.MultidropNumber, __target.ExtensionStationNumber);

                if (__target.UDPTransportLayer == true)
                {
                    com = new UDP(new System.Net.IPEndPoint(__target.SourceIPv4, __target.SourcePort),
                            new System.Net.IPEndPoint(__target.DestinationIPv4, __target.DestinationPort),
                            __target.ReceiveBufferSize, __target.SendTimeoutValue, __target.ReceiveTimeoutValue);
                }
                else
                {
                    com = new TCP(new System.Net.IPEndPoint(__target.SourceIPv4, 0),
                            new System.Net.IPEndPoint(__target.DestinationIPv4, __target.DestinationPort),
                            __target.SendTimeoutValue, __target.ReceiveTimeoutValue);
                    await Task.Run(() => (com as TCP).Connect());
                }

                var master = new RemoteOperationMaster(__target.FrameType, __target.DataCode, __target.R_DedicatedMessageFormat, com, ref destination,
                        __target.SendBufferSize, __target.ReceiveBufferSize, null);

                ushort end = 0;
                switch (ro.RemoteOperation)
                {
                    case REMOTE_OPERATION_T.RUN:
                        end = await master.RunAsync(__target.MonitoringTimer, ro.RemoteControlMode, ro.RemoteClearMode);
                        MessageBox.Show(this, $"The <RUN> operation returns code : {end}(0x{end:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case REMOTE_OPERATION_T.STOP:
                        end = await master.StopAsync(__target.MonitoringTimer);
                        MessageBox.Show(this, $"The <STOP> operation returns code : {end}(0x{end:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case REMOTE_OPERATION_T.PAUSE:
                        end = await master.PauseAsync(__target.MonitoringTimer, ro.RemoteControlMode);
                        MessageBox.Show(this, $"The <PAUSE> operation returns code : {end}(0x{end:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case REMOTE_OPERATION_T.LATCH_CLEAR:
                        end = await master.LatchClearAsync(__target.MonitoringTimer);
                        MessageBox.Show(this, $"The <LATCH_CLEAR> operation returns code : {end}(0x{end:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case REMOTE_OPERATION_T.RESET:
                        end = await master.ResetAsync(__target.MonitoringTimer);
                        MessageBox.Show(this, $"The <RESET> operation returns code : {end}(0x{end:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case REMOTE_OPERATION_T.READ_TYPE:
                        var res = await master.ReadTypeNameAsync(__target.MonitoringTimer);
                        if(res.Item1 != (ushort)RESPONSE_MESSAGE_ENDCODE_T.NO_ERROR)
                            MessageBox.Show(this, $"The <READ_TYPE> operation returns code : {res.Item1}(0x{res.Item1:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        else
                            MessageBox.Show(this, $"The remote controller is { res.Item2.Trim()} (0x{res.Item3:X4}).", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    default:
                        break;
                }
            }
            catch (SLMPException ex)
            {
                MessageBox.Show(this, "At least one unexpected error occured while doing remote operation.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "At least one unexpected error occured while doing remote operation.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ro.IsBusy = false;
                if (com != null) com.Dispose();
                com = null;
            }
        }

        private async void ECATCommand_Click(object sender, RoutedEventArgs e)
        {
            if(TxtInputECATModuleAddress.HasError || TxtInputECATCommandWaiting.HasError)
            {
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SocketInterface com = null;
            var ro = DataContext as RemoteOperationModel;
            ro.IsBusy = true;
            try
            {
                DESTINATION_ADDRESS_T destination = new DESTINATION_ADDRESS_T(
                        __target.NetworkNumber, __target.StationNumber, __target.ModuleIONumber,
                        __target.MultidropNumber, __target.ExtensionStationNumber);

                if (__target.UDPTransportLayer == true)
                {
                    com = new UDP(new System.Net.IPEndPoint(__target.SourceIPv4, __target.SourcePort),
                            new System.Net.IPEndPoint(__target.DestinationIPv4, __target.DestinationPort),
                            __target.ReceiveBufferSize, __target.SendTimeoutValue, __target.ReceiveTimeoutValue);
                }
                else
                {
                    com = new TCP(new System.Net.IPEndPoint(__target.SourceIPv4, 0),
                            new System.Net.IPEndPoint(__target.DestinationIPv4, __target.DestinationPort),
                            __target.SendTimeoutValue, __target.ReceiveTimeoutValue);
                    await Task.Run(() => (com as TCP).Connect());
                }

                var master = new DeviceAccessMaster(__target.FrameType, __target.DataCode, __target.R_DedicatedMessageFormat, com, ref destination,
                        __target.SendBufferSize, __target.ReceiveBufferSize, null);

                ushort end = 0;
                end = await master.WriteModuleAccessDeviceInWordAsync(__target.MonitoringTimer, $"U{ro.ECATModuleAddress / 16:X3}", RemoteOperationModel.ECATCommandRequestRegister, 1, new ushort[1] { (ushort)ro.ECATCommand });
                if (end != 0)
                {
                    MessageBox.Show(this, $"Post <{ro.ECATCommand}> operation command returns code : {end}(0x{end:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                await Task.Delay(ro.ECATCommandWaiting);

                var rsp = new ushort[1];
                end = await master.ReadModuleAccessDeviceInWordAsync(__target.MonitoringTimer, $"U{ro.ECATModuleAddress / 16:X3}", RemoteOperationModel.ECATCommandResponseRegister, 1, rsp);

                if (end != 0)
                {
                    MessageBox.Show(this, $"Read <{ro.ECATCommand}> operation response returns code : {end}(0x{end:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBox.Show(this, $"<{ro.ECATCommand}> operation response is : {(SMART_ECAT_COMMAND_T)rsp[0]}({rsp[0]:X4})", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SLMPException ex)
            {
                MessageBox.Show(this, "At least one unexpected error occured while doing remote operation.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "At least one unexpected error occured while doing remote operation.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ro.IsBusy = false;
                if (com != null) com.Dispose();
                com = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((DataContext as RemoteOperationModel).IsBusy == true)
                e.Cancel = true;
        }
    }
}
