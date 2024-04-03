using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.TextInputLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria.Utility
{
    /// <summary>
    /// FTPUtility.xaml 的交互逻辑
    /// </summary>
    public partial class FTPUtility : Window
    {
        public FTPUtility(FTPMode mode, ControllerModelCatalogue cc, TaskUserParameterHelper helper)
        {
            InitializeComponent();
            __original_helper = helper;
            DataContext = new FTPUtilityModel(mode, cc, helper);
            if(mode == FTPMode.Upload)
            {
                ChkboxDownloadOrbment.Visibility = Visibility.Collapsed;
                TxtInputOrbmentVersion.Visibility = Visibility.Collapsed;
            }
        }

        public TaskUserParameterHelper UploadResult { get; private set; }
        private TaskUserParameterHelper __original_helper = null;
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private async void OK_Click(object sender, RoutedEventArgs e)
        {
            FTPUtilityModel model = DataContext as FTPUtilityModel;
            foreach (var u in InputsGrid.Children)
            {
                if ((u as SfTextInputLayout)?.HasError == true)
                {
                    MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            switch (model.Mode)
            {
                case FTPMode.Upload:
                    model.IsBusy = true;
                    try
                    {
                        model.TransferState = "Upload r2h_task_user_parameters file";
                        await Task.Run(() => UploadResult = model.Upload()); ;

                        foreach (var device in UploadResult.LocalHardwareCollection)
                        {
                            foreach (var field in device.CustomFields)
                            {             
                                if (Regex.IsMatch(field.Value, @"^(/[^/]+)+$"))
                                {
                                    var ret = MessageBox.Show($"Do you want to upload the file:\n{field.Key}: {field.Value}\nWhich is referenced by the following module definition:\n{device.LocalAddress:X4}: {device.DeviceModel}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (ret == MessageBoxResult.Yes)
                                    {
                                        string ext = System.IO.Path.GetExtension(field.Value);
                                        System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog() { AddExtension = true, CheckFileExists = true };
                                        save.Filter = $"{field.Key} Files(*{ext})|*{ext}";
                                        save.FileName = $"{device.DeviceModel}_{System.IO.Path.GetFileName(field.Value)}";
                                        try
                                        {
                                            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                            {
                                                model.TransferState = $"Upload the referenced file: {field.Value}";
                                                await Task.Run(() => model.Upload(field.Value, save.FileName));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Exception occurred while transfering the file: ftp://{model.HostIPv4}:{model.HostPort}{field.Value} -> {save.FileName}\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }
                        }
                        
                        foreach (var device in UploadResult.RemoteHardwareCollection)
                        {
                            foreach (var field in device.CustomFields)
                            {
                                if (Regex.IsMatch(field.Value, @"^(/[^/]+)+$"))
                                {
                                    var ret = MessageBox.Show($"Do you want to upload file:\n{field.Key}: {field.Value}\nWhich is referenced by the following module definition:\n{device.IPv4}@{device.Port}: {device.DeviceModel}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (ret == MessageBoxResult.Yes)
                                    {
                                        string ext = System.IO.Path.GetExtension(field.Value);
                                        System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog() { AddExtension = true, CheckFileExists = true};
                                        save.Filter = $"{field.Key} Files(*{ext})|*{ext}";
                                        save.FileName = $"{device.DeviceModel}_{System.IO.Path.GetFileName(field.Value)}";
                                        try
                                        {
                                            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                            {
                                                model.TransferState = $"Upload the referenced file: {field.Value}";
                                                await Task.Run(() => model.Upload(field.Value, save.FileName));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Exception occurred while transfering file: ftp://{model.HostIPv4}:{model.HostPort}{field.Value} -> {save.FileName}\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }
                        }
                        model.TransferState = "Done";
                        model.IsBusy = false;
                        DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        model.TransferState = $"Exception: {model.TransferState}";
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        model.IsBusy = false;
                    }
                    break;
                case FTPMode.Download:
                    if(ChkboxDownloadOrbment.IsChecked == true && model.SelectedOrbmentVersion == null)
                    {
                        MessageBox.Show("You must select a valid Orbment Version before downloading Orbment Runtime Binary files.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    model.IsBusy = true;
                    try
                    {
                        model.TransferState = "Download r2h_task_user_parameters file";
                        await Task.Run(() => model.Download());

                        model.TransferState = "Download the referenced file(s)";
                        foreach (var device in __original_helper.LocalHardwareCollection)
                        {
                            foreach (var field in device.CustomFields)
                            {
                                if (Regex.IsMatch(field.Value, @"^(/[^/]+)+$"))
                                {
                                    var ret = MessageBox.Show($"Do you want to download file:\n{field.Key}: {field.Value}\nWhich is referenced by the following module definition:\n{device.LocalAddress:X4}: {device.DeviceModel}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (ret == MessageBoxResult.Yes)
                                    {
                                        string ext = System.IO.Path.GetExtension(field.Value);
                                        System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
                                        open.Filter = $"{field.Key} Files(*{ext})|*{ext}";
                                        open.FileName = $"{device.DeviceModel}_{System.IO.Path.GetFileName(field.Value)}";
                                        try
                                        {
                                            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                            {
                                                model.TransferState = $"Download the referenced file: {field.Value}";
                                                await Task.Run(() => model.Download(open.FileName, field.Value));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Exception occurred while transfering the file: {open.FileName} -> ftp://{model.HostIPv4}:{model.HostPort}{field.Value}\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }
                        }
                        foreach (var device in __original_helper.RemoteHardwareCollection)
                        {
                            foreach (var field in device.CustomFields)
                            {
                                if (Regex.IsMatch(field.Value, @"^(/[^/]+)+$"))
                                {
                                    var ret = MessageBox.Show($"Do you want to download file:\n{field.Key}: {field.Value}\nWhich is referenced by the following module definition:\n{device.IPv4}@{device.Port}: {device.DeviceModel}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (ret == MessageBoxResult.Yes)
                                    {
                                        string ext = System.IO.Path.GetExtension(field.Value);
                                        System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
                                        open.Filter = $"{field.Key} Files(*{ext})|*{ext}";
                                        open.FileName = $"{device.DeviceModel}_{System.IO.Path.GetFileName(field.Value)}";
                                        try
                                        {
                                            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                            {
                                                model.TransferState = $"Download the referenced file: {field.Value}";
                                                await Task.Run(() => model.Download(open.FileName, field.Value));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Exception occurred while transfering the file: {open.FileName} -> ftp://{model.HostIPv4}:{model.HostPort}{field.Value}\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }
                        }
                        if (ChkboxDownloadOrbment.IsChecked == true)
                        {
                            model.TransferState = "Download Orbment Runtime Binary files";
                            await Task.Run(() => model.DownloadOrbmentBinary());
                        }

                        model.TransferState = "Done";
                    }
                    catch (Exception ex)
                    {
                        model.TransferState = $"Exception: {model.TransferState}";
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    model.IsBusy = false;
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((DataContext as FTPUtilityModel).IsBusy == true)
                e.Cancel = true;
        }
    }
}
