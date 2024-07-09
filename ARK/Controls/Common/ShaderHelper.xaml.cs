using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.EventMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.Process;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    /// <summary>
    /// ShaderHelper.xaml 的交互逻辑
    /// </summary>
    public partial class ShaderHelper : Window
    {
        public ShaderModel? Shader { get; private set; }
        public ShaderHelper(ShaderModel shader)
        {
            InitializeComponent();
            ProcessShader ps = new UserProcessShaders([(shader.Name, shader.LeftValue, shader.RightValue)])[0];
            Shader = new ShaderModel(ps) { Owner = null };
            DataContext = Shader;
            
        }

        public ShaderHelper()
        {
            InitializeComponent();
            Shader = new ShaderModel() { Owner = null };
            DataContext = Shader;
        }

        private void CancelButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Shader.ApplyChanges();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred while applying changes:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
