using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// OrbmentAppInstaller.xaml 的交互逻辑
    /// </summary>
    public partial class OrbmentAppInstaller : Window
    {
        public OrbmentAppInstaller(FTPUtilityModel ftp, AppInstallerProperty app)
        {
            InitializeComponent();
            DataContext = new AppInstallerModel(ftp, app);
        }

        private void ApplicationFileGrid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var content = (e.Data as DataObject).GetFileDropList();
                AppInstallerModel model = DataContext as AppInstallerModel;
                foreach (var s in content)
                {
                    if (System.IO.File.Exists(s))
                    {
                        model.ApplicationFileCollection.Add(new System.IO.FileInfo(s));
                    }
                }
                e.Handled = true;
            }
            catch
            {
                e.Handled = false;
            }
        }

        private void ApplicationFileGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Source == SfDataGrid_ApplicationFile && e.Key == Key.Delete && SfDataGrid_ApplicationFile.SelectedItem != null)
            {
                FileInfo info = (FileInfo)SfDataGrid_ApplicationFile.SelectedItem;
                var ret = MessageBox.Show($"Are you sure you want to delete the following item:\n{info.Name} -- {info.Length} -- {info.LastWriteTime}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ret == MessageBoxResult.Yes)
                    (DataContext as AppInstallerModel).ApplicationFileCollection.Remove(info);
            }
        }

        private async void BtnStartTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (__errors != 0)
            {
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            AppInstallerModel model = DataContext as AppInstallerModel;
            if (model.ApplicationFileCollection.Count == 0)
            {
                if (MessageBox.Show("You did not attach any application file, do you want to continue?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
            }
            try
            {
                model.IsBusy = true;
                model.InstallationState = "Starting";
                model.InstallationProgress = 0;
                model.InstallationExceptionInfo = "N/A";

                ConsistencyResult ret = ConsistencyResult.Unknown;
                IEnumerable<DeviceConfiguration> notfound = null;
                bool consistency = false;
                await Task.Run(() => (ret, notfound) = model.ConfigurationConsistency());
                if (ret == ConsistencyResult.Exception)
                {
                    var rsp = MessageBox.Show("Can not read hardware configuration file, so the consistency check is not performed.\nAre you sure you want to download IO List anyway?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (rsp == MessageBoxResult.Yes)
                        consistency = true;
                }
                else if (ret == ConsistencyResult.Inconsistent)
                {
                    SimpleDeviceConfigurationViewer result = new SimpleDeviceConfigurationViewer("Consistency Check Result",
                        "The following hardware configuration(s) in IO List file is(are) not found in the system hardware configuration(s).\nAre you sure you want to download IO List anyway?",
                        notfound);

                    if (result.ShowDialog() == true)
                        consistency = true;
                }
                else
                    consistency = true;

                if (consistency)
                {
                    await Task.Run(() => model.InstallApplication(true));
                    await Task.Delay(1000);
                    model.InstallationState = "Done";
                }
                else
                {
                    model.InstallationState = $"About: Hardware configuration consistency check failure.";
                }
  
                model.InstallationProgress = 0;
                model.IsBusy = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                model.IsBusy = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppInstallerModel model = DataContext as AppInstallerModel;
            if(model.IsBusy)
                e.Cancel = true;
        }

        private int __errors = 0;
        private void Grid_Error(object sender, ValidationErrorEventArgs e)
        {
            if(e.Action == ValidationErrorEventAction.Added)
                __errors ++;
            else
                __errors --;
        }
    }
}
