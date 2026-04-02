using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// EventLogViewer.xaml 的交互逻辑
    /// </summary>
    public partial class EventLogViewer : Window
    {
        private static string __TITLE = "EventLog Viewer";

        private Platform __platform;
        public EventLogViewer(FTPTargetProperty property, Platform platform)
        {
            InitializeComponent();
            __platform = platform;
            DataContext = new EventLogModel() 
            {
                HostIPv4 = property.HostIPv4String,
                HostPort = property.HostPort,
                User = property.User,
                Password = property.Password,
                Timeout = property.TimeoutValue,
                ReadWriteTimeout = property.ReadWriteTimeoutValue,
                Platform = platform
            };

            if(platform != Platform.R12CCPU)
            {
                ViewOrbmentLogOnlyChk.IsEnabled = false;
                BrowseOpenLocalBt.IsEnabled = false;
                ChooseEventsStorageCombo.IsEnabled = false;
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            EventLogModel model = DataContext as EventLogModel;
            try
            {
                model.IsBusy = true;
                ControlPanelGrid.IsEnabled = false;
                await Task.Run(() => model.Upload());
                model.IsBusy = false;
                ControlPanelGrid.IsEnabled = true;

                switch(__platform)
                {
                    case Platform.R12CCPU:
                        this.Title = __TITLE + " - via FTP - " + model.HistoryDestination.ToString();
                        break;
                    case Platform.MXRL:
                        this.Title = __TITLE + " - via SFTP - LinuxRT FS";
                        break;
                    default:
                        this.Title = __TITLE + " - via FTP - " + model.HistoryDestination.ToString();
                        break;
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                model.IsBusy = false;
                ControlPanelGrid.IsEnabled = true;
            }
        }

        private async void BrowseOpenLocal_Click(object sender, RoutedEventArgs e)
        {
            EventLogModel model = DataContext as EventLogModel;
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Event log history file(*.LOG)|*.LOG";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    model.IsBusy = true;
                    ControlPanelGrid.IsEnabled = false;
                    model.LocalEventLogPath = open.FileName;
                    await Task.Run(() => model.ReadLocal());
                    LocalEventLogFilePathTxt.Text = model.LocalEventLogPath;
                    model.IsBusy = false;
                    ControlPanelGrid.IsEnabled = true;

                    this.Title = __TITLE + " - via Local - " + model.LocalEventLogPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    LocalEventLogFilePathTxt.Text = "";
                    model.IsBusy = false;
                    ControlPanelGrid.IsEnabled = true;
                }
            }
        }

        GridRowSizingOptions __row_sizing_options = new GridRowSizingOptions();
        private void MainViewer_QueryRowHeight(object sender, QueryRowHeightEventArgs e)
        {
            if (MainViewer.GridColumnSizer.GetAutoRowHeight(e.RowIndex, __row_sizing_options, out var autoHeight))
            {
                if (autoHeight > 25)
                {
                    e.Height = autoHeight + 12;
                    e.Handled = true;
                }
            }
        }
    }
}
