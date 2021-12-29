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
            DataTypeCatalogue dataTypes, ControllerModelCatalogue models)
        {
            InitializeComponent();
            DataContext = new FTPUtilityModel(mode,
                vd, variableNames,
                cc, configurationNames,
                od, objectIndexes,
                txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc,
                dataTypes, models);
            if(mode == FTPMode.Upload)
            {
                CheckboxVAR.IsChecked = true;
                CheckboxVAR.IsEnabled = false;
            }
        }

        public (VariableDictionary, ControllerConfiguration, ObjectDictionary,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
                    Miscellaneous) UploadResult
        {
            get; private set;
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
