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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    /// <summary>
    /// InterlockDataControl.xaml 的交互逻辑
    /// </summary>
    public partial class InterlockCollectionDataControl : UserControl
    {
        public InterlockCollectionDataControl(InterlockCollectionDataModel dataModel)
        {
            InitializeComponent();
            DataContext = dataModel;

            __lsb_interlock_logic_definitions.ItemsSource = dataModel.InterlockLogicDefinitions;
        }



        private void __on_add_element_command_executed(object sender, ExecutedRoutedEventArgs e)
        {  
            if(sender is ListBox)
            {
                var lsb = sender as ListBox;
            }        
        }

        private void __on_insert_element_before_command_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ListBox)
            {
                var lsb = sender as ListBox;
                string info = string.Format("Insert Before {0}", lsb.SelectedIndex);
                MessageBox.Show(info);
            }
        }

        private void __on_add_element_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void __on_insert_element_before_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is ListBox)
            {
                var lsb = sender as ListBox;
                if (lsb.SelectedIndex != -1)
                    e.CanExecute = true;
            }
            else
                e.CanExecute = false;
        }
    }
}
