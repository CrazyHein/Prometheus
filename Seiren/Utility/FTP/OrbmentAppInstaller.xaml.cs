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
            AppInstallerModel model = DataContext as AppInstallerModel;
            try
            {
                model.IsBusy = true;
                model.InstallationState = "Starting";
                model.InstallationExceptionInfo = "N/A";

                await Task.Run(() => model.InstallApplication());
                await Task.Delay(1000);

                model.InstallationState = "Done";
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
    }
}
