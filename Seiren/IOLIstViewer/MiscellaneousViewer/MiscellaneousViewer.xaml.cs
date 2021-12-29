using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// MiscellaneousViewer.xaml 的交互逻辑
    /// </summary>
    public partial class MiscellaneousViewer : UserControl
    {
        public MiscellaneousViewer(Miscellaneous misc)
        {
            InitializeComponent();
            DataContext = new MiscellaneousModel(misc);
        }

        public void UpdateBindingSource()
        {
            var binding = InputIOListName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();

            binding = InputDescription.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();

            binding = InputVariableDictionary.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();

            binding = InputMCServerIPv4.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();

            binding = InputMCServerPort.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }

        private int __errors;
        private void Input_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                __errors++;
            else
                __errors--;
        }

        public bool HasError { get { return __errors != 0; } }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.OriginalSource.GetType() == typeof(TextBox))
            {
                var binding = (e.OriginalSource as TextBox).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }
        }
    }
}
