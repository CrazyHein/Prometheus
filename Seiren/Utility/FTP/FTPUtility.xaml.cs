using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.TextInputLayout;
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
    /// FTPUtility.xaml 的交互逻辑
    /// </summary>
    public partial class FTPUtility : Window
    {
        public FTPUtility(FTPMode mode,
            VariableDictionary vd, IEnumerable<string> variableNames,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk,
            InterlockCollection intlk, Miscellaneous misc,
            DataTypeCatalogue dataTypes, ControllerModelCatalogue models, FTPTargetProperty property, AppInstallerProperty app)
        {
            InitializeComponent();
            DataContext = new FTPUtilityModel(mode,
                vd, variableNames,
                cc, configurationNames,
                od, objectIndexes,
                txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc,
                dataTypes, models)
            {
                HostIPv4 = property.HostIPv4String,
                HostPort = property.HostPort,
                User = property.User,
                Password = property.Password,
                Timeout = property.TimeoutValue,
                ReadWriteTimeout = property.ReadWriteTimeoutValue
            };
            if (mode == FTPMode.Upload || mode == FTPMode.Compare)
            {
                CheckboxVAR.IsChecked = true;
                CheckboxVAR.IsEnabled = false;
                BtnOpenAppInstaller.Visibility = Visibility.Hidden;
            }
            else
            {
                CheckboxVAR.IsChecked = true;
                CheckboxVAR.IsEnabled = false;
                CheckboxIO.IsChecked = true;
                CheckboxIO.IsEnabled = false;
            }
            __app_installer_property = app;
        }

        private AppInstallerProperty __app_installer_property;

        public (VariableDictionary vd, ControllerConfiguration cc, ObjectDictionary od,
                    ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
                    ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk, InterlockCollection intlk,
                    Miscellaneous misc) UploadResult
        {
            get; private set;
        }
        private void OpenAppInstaller_Click(object sender, RoutedEventArgs e)
        {
            foreach (var u in InputsGrid.Children)
            {
                if ((u as SfTextInputLayout)?.HasError == true)
                {
                    MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            OrbmentAppInstaller installer = new OrbmentAppInstaller(DataContext as FTPUtilityModel, __app_installer_property);
            installer.ShowDialog();
        }

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
                case FTPMode.Compare:
                    model.IsBusy = true;
                    try
                    {
                        await Task.Run(() => UploadResult = model.Upload()); ;
                        model.IsBusy = false;
                        DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        model.IsBusy = false;
                    }
                    break;
                case FTPMode.Download:
                    model.IsBusy = true;
                    try
                    {
                        ConsistencyResult ret = ConsistencyResult.Unknown;
                        IEnumerable<DeviceConfiguration> notfound = null;
                        bool download = false;
                        await Task.Run(() => (ret, notfound) = model.ConfigurationConsistency());
                        if(ret == ConsistencyResult.Exception)
                        {
                            var rsp = MessageBox.Show("Can not read hardware configuration file, so the consistency check is not performed.\nAre you sure you want to download IO List anyway?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (rsp == MessageBoxResult.Yes)
                                download = true;
                        }
                        else if(ret == ConsistencyResult.Inconsistent)
                        {
                            SimpleDeviceConfigurationViewer result = new SimpleDeviceConfigurationViewer("Consistency Check Result",
                                "The following hardware configuration(s) in IO List file is(are) not found in the system hardware configuration(s).\nAre you sure you want to download IO List anyway?",
                                notfound);
                            
                            if (result.ShowDialog() == true)
                                download = true;
                        }
                        else
                            download = true;

                        if (download)
                            await Task.Run(() => model.Download());
                    }
                    catch (Exception ex)
                    {
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
