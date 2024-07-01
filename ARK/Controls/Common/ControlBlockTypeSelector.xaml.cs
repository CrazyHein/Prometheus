using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe.ControlBlock;
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
    /// ControlBlockTypeSelector.xaml 的交互逻辑
    /// </summary>
    public partial class ControlBlockTypeSelector : Window
    {
        public ControlBlockTypeSelector()
        {
            InitializeComponent();
        }

        public Type? ControlBlockType { get; private set; }

        private void SequentialButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            ControlBlockType = typeof(Sequential_S);
            DialogResult = true;
        }

        private void LoopButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            ControlBlockType = typeof(Loop_S);
            DialogResult = true;
        }

        private void SwitchButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            ControlBlockType = typeof(Switch_S);
            DialogResult = true;
        }
    }
}
