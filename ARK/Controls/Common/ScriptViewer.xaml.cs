using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ExceptionMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    /// <summary>
    /// ScriptViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptViewer : Window
    {
        List<(uint idx, string name, Event evt)> __globals;
        List<(string name, Prometheus.Napishtim.Engine.StepMechansim.Step stp)> __steps;
        ExceptionResponse? __exception_response;

        public ScriptViewer(ILinkProperty network)
        {
            InitializeComponent();

            txtIPAddress.Text = network.IPv4.ToString();
            txtPortNumber.Value = network.Port;
            txtRecvTimeout.Value = network.ReceiveTimeoutValue;
            txtSendTimeout.Value = network.SendTimeoutValue;

            __steps = new List<(string name, Prometheus.Napishtim.Engine.StepMechansim.Step stp)>();
            __globals = new List<(uint idx, string name, Event evt)>();
            listSteps.ItemsSource = __steps.Select(x => $"{x.Item2.ID}: {x.Item1}");
            listGlobalEvents.ItemsSource = __globals.Select(x => $"{x.idx}: {x.name}");
        }

        public ScriptViewer(ILinkProperty network, IEnumerable<(uint, string, Event)> globalEvents, IEnumerable<(string, Prometheus.Napishtim.Engine.StepMechansim.Step)> steps, ExceptionResponse? exception)
        {
            InitializeComponent();

            txtIPAddress.Text = network.IPv4.ToString();
            txtPortNumber.Value = network.Port;
            txtRecvTimeout.Value = network.ReceiveTimeoutValue;
            txtSendTimeout.Value = network.SendTimeoutValue;

            __steps = new List<(string name, Prometheus.Napishtim.Engine.StepMechansim.Step stp)>(steps);
            __globals = new List<(uint idx, string name, Event evt)>(globalEvents);
            listSteps.ItemsSource = __steps.Select(x => $"{x.Item2.ID}: {x.Item1}");
            listGlobalEvents.ItemsSource = __globals.Select(x => $"{x.idx}: {x.name}");

            txtExceptionResponseContent.Text = exception?.ToString();
            __exception_response = exception;
        }

        private void listSteps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtStepContent.Text = __steps[listSteps.SelectedIndex].stp.ToString();
        }

        private void listGlobalEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtGlobalEventContent.Text = __globals[listGlobalEvents.SelectedIndex].evt.ToString();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "Napishtim Script File(*.npstscp)|*.npstscp";
            save.AddExtension = true;
            save.DefaultExt = "npstscp";

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    RecipeDocument.SaveScript(save.FileName, __globals, __steps, __exception_response);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Napishtim Script File(*.npstscp)|*.npstscp";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var ret = RecipeDocument.ParseScript(open.FileName);
                    __steps = new List<(string name, Prometheus.Napishtim.Engine.StepMechansim.Step stp)>(ret.steps);
                    __globals = new List<(uint idx, string name, Event evt)>(ret.globalEvents);
                    __exception_response = ret.exceptionResponse;
                    listSteps.ItemsSource = __steps.Select(x => $"{x.Item2.ID}: {x.Item1}");
                    listGlobalEvents.ItemsSource = __globals.Select(x => $"{x.idx}: {x.name}");

                    txtStepContent.Text = string.Empty;
                    txtGlobalEventContent.Text = string.Empty;
                    txtExceptionResponseContent.Text = ret.exceptionResponse?.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            BusyDialog busy = new BusyDialog(RecipeDocument.DownloadAsync(__globals, __steps.Select(x => x.stp), __exception_response,
                txtIPAddress.Text,
                (ushort)(txtPortNumber.Value.HasValue? txtPortNumber.Value: 8367),
                txtSendTimeout.Value.HasValue ? (int)(txtSendTimeout.Value) : 5000,
                txtRecvTimeout.Value.HasValue ? (int)(txtRecvTimeout.Value) : 5000));
            busy.ShowDialog();
        }
    }
}
