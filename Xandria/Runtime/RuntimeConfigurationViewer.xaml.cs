using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia.OrbmentParameters;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    /// <summary>
    /// RuntimeConfigurationViewer.xaml 的交互逻辑
    /// </summary>
    public partial class RuntimeConfigurationViewer : Window
    {
        public RuntimeConfigurationViewer(RuntimeConfiguration data)
        {
            InitializeComponent();
            DataContext = new RuntimeConfigurationModel(data);
        }

        private int __errors = 0;
        private void Input_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }

        public bool HasError { get { return __errors != 0; } }

        public void UpdateBindingSource()
        {
            foreach (var c in DeviceIOScanTaskSettings.Children)
            {
                var input = (c as CheckBox).Content as SfTextInputLayout; 
                (input?.InputView as TextBox)?.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            foreach (var c in DeviceControlTaskSettings.Children)
            {
                var input = (c as CheckBox).Content as SfTextInputLayout;
                (input?.InputView as TextBox)?.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            foreach (var c in DLinkServiceSettings.Children)
            {
                var input = (c as CheckBox).Content as SfTextInputLayout;
                (input?.InputView as TextBox)?.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            foreach (var c in ILinkServiceSettings.Children)
            {
                var input = (c as CheckBox).Content as SfTextInputLayout;
                (input?.InputView as TextBox)?.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            foreach (var c in RLinkServiceSettings.Children)
            {
                var input = (c as CheckBox).Content as SfTextInputLayout;
                (input?.InputView as TextBox)?.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UpdateBindingSource();
            if (HasError == false)
            {
                UserConfiguration = (DataContext as RuntimeConfigurationModel).Configuration;
                DialogResult = true;
            }
            else
                MessageBox.Show("At least one user input is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public RuntimeConfiguration UserConfiguration { get; private set; }
    }
}
