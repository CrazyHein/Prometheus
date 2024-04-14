using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Console;
using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TextInputLayout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// SmartECATUtility.xaml 的交互逻辑
    /// </summary>
    public partial class SmartECATUtility : Window
    {
        public SmartECATUtility(SmartECATUtilityModel model)
        {
            InitializeComponent();
            DataContext = model;
            ENIPathTxt.Text = model.SmartECATProperty.InstallerProperty.LocalNetworkInformationFilePath;
            LICPathTxt.Text = model.SmartECATProperty.InstallerProperty.LocalLicenseFilePath;
        }

        public bool IsClosed { get; private set; } = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void SaveCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __ftp_settings_errors == 0 && __installer_settings_error == 0;
            e.Handled = true;
        }

        private void SaveButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            if (__ftp_settings_errors != 0 || __installer_settings_error != 0)
            {
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SmartECATUtilityModel model = DataContext as SmartECATUtilityModel;
            try
            {
                model.SmartECATProperty.Save();
                MessageBox.Show($"Save SMART-ECAT settings to configuration file : '{model.SmartECATProperty.SettingsPath}'.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"At least one unexpected error occured while saving SMART_ECAT settings to configuration file : '{model.SmartECATProperty.SettingsPath}'.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void BrowseENIBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "EtherCAT-Network-Information Files(*.xml)|*.xml";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                (DataContext as SmartECATUtilityModel).SmartECATProperty.InstallerProperty.LocalNetworkInformationFilePath = open.FileName;
                ENIPathTxt.Text = open.FileName;
            }
        }

        private void BrowseLICBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "SMART-ECAT License Files(*.lic)|*.lic";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                (DataContext as SmartECATUtilityModel).SmartECATProperty.InstallerProperty.LocalLicenseFilePath = open.FileName;
                LICPathTxt.Text = open.FileName;
            }
        }

        private async void StartTransferBtn_Click(object sender, RoutedEventArgs e)
        {
            SmartECATUtilityModel model = DataContext as SmartECATUtilityModel;
            if (__ftp_settings_errors != 0 || __installer_settings_error != 0)
            {
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (model.SmartECATProperty.InstallerProperty.TransferLicense && System.IO.File.Exists(model.SmartECATProperty.InstallerProperty.LocalLicenseFilePath) == false)
            {
                MessageBox.Show($"The LIC file: \"{model.SmartECATProperty.InstallerProperty.LocalLicenseFilePath}\" does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (model.SmartECATProperty.InstallerProperty.TransferNetworkInformation && System.IO.File.Exists(model.SmartECATProperty.InstallerProperty.LocalNetworkInformationFilePath) == false)
            {
                MessageBox.Show($"The ENI file: \"{model.SmartECATProperty.InstallerProperty.LocalNetworkInformationFilePath}\" does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (model.SmartECATProperty.InstallerProperty.TransferCFG)
            {
                string ignore = string.Empty;
                if (model.SmartECATProperty.InstallerProperty.BootFromSD == false)
                    ignore += "\nLog File Size(kByte)";
                if (model.SmartECATProperty.InstallerProperty.PlatformModel == SmartECATPlatform.RD55UP06_V)
                    ignore += "\nMain Port";
                if (model.SmartECATProperty.InstallerProperty.PlatformModel == SmartECATPlatform.RD55UP06_V && model.SmartECATProperty.InstallerProperty.EnableCableRedundancy)
                    ignore += "\nEnable Cable Redundancy";
                if (ignore != string.Empty)
                {
                    if(MessageBox.Show("The following field(s) will be ignored. Are you sure you want to continue?"+ignore, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                }
            }

            try
            {
                model.IsBusy = true;
                model.InstallationState = "Starting";
                model.InstallationProgress = 0;
                model.InstallationExceptionInfo = "N/A";

                await Task.Run(() => model.TransferLicenseFile());
                await Task.Delay(1000);

                await Task.Run(() => model.TransferFirmwareFile());
                await Task.Delay(1000);

                await Task.Run(() => model.TransferConfiguration());
                await Task.Delay(1000);

                await Task.Run(() => model.TransferNetworkInformationFile());
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

        private async void RefreshRemoteLogListBtn_Click(object sender, RoutedEventArgs e)
        {
            SmartECATUtilityModel model = DataContext as SmartECATUtilityModel;
            if (__ftp_settings_errors != 0)
            {
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                model.IsBusy = true;
                await Task.Run(() => model.RefreshRemoteLogFileList());
                model.IsBusy = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                model.IsBusy = false;
            }
        }

        private void OpenLocalLogFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            SmartECATUtilityModel model = DataContext as SmartECATUtilityModel;
            System.Windows.Forms.FolderBrowserDialog open = new System.Windows.Forms.FolderBrowserDialog();

            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    model.RefreshLocalLogFileList(open.SelectedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void LogListGrid_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            SmartECATUtilityModel model = DataContext as SmartECATUtilityModel;
            if (e.Record as SmartECATLogFileInfo != null)
            {
                if ((e.Record as SmartECATLogFileInfo).Local == false)
                {
                    if (__ftp_settings_errors != 0)
                    {
                        MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                try
                {
                    model.IsBusy = true;
                    await Task.Run(() => model.ReadLogContent(e.Record as SmartECATLogFileInfo)); ;
                    model.IsBusy = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    model.IsBusy = false;
                }
            }
        }

        int __ftp_settings_errors = 0;
        private void FTPSettingsGrid_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __ftp_settings_errors++;
            else
                __ftp_settings_errors--;
        }

        int __installer_settings_error = 0;
        private void InstallerSettingsGrid_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __installer_settings_error++;
            else
                __installer_settings_error--;
        }

        private void LogEntrySaveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            SmartECATUtilityModel model = DataContext as SmartECATUtilityModel;
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "SMART-ECAT Log Files(*.log)|*.log";
            save.FileName = System.IO.Path.GetFileName(model.CurrentOpenedLogFileName);
            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(save.FileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, Encoding.ASCII))
                {
                    foreach(var entry in model.LogEntryList)
                        sw.WriteLine(entry.ToString());
                }
            }
        }
    }
}
