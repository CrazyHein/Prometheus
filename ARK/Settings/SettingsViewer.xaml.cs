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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsViewer : Window
    {
        private bool __busy = false;
        private int __errors = 0;
        public bool HasError { get { return __errors != 0; } }
        public Settings Settings { get; private set; }

        public SettingsViewer(Settings settings)
        {
            InitializeComponent();
            Settings = settings;
            PreferenceSettings.DataContext = Settings.PreferenceProperty.Copy();
            ILinkSettings.DataContext = Settings.ILinkProperty.Copy();
        }

        private void Settings_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }



        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (HasError)
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Settings.PreferenceProperty = PreferenceSettings.DataContext as PreferenceProperty;
            Settings.ILinkProperty = ILinkSettings.DataContext as ILinkProperty;

            try
            {
                Settings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"At least one unexpected error occured while saving settings to configuration file : '{Settings.SettingsPath}'.\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void CommunicationTest_Click(object sender, RoutedEventArgs e)
        {
            var property = ILinkSettings.DataContext as ILinkProperty;
            Communicating.Visibility = Visibility.Visible;
            __busy = true;
            try
            {
                var ret = await RecipeDocument.InfoAsync(property.IPv4s, property.Port, property.SendTimeoutValue, property.ReceiveTimeoutValue);
                MessageBox.Show(this, $"Information Received:\nVersion: {ret.version}\nStep Capacity: {ret.steps}\nController Throughput: {ret.throughput}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, $"At least one unexpected error occured during the operation:\n" + ex.Message, "Error Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Communicating.Visibility = Visibility.Collapsed;
                __busy = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(__busy) e.Cancel = true;
        }
    }
}
