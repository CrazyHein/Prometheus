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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue
{
    /// <summary>
    /// CatalogueWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CatalogueWindow : Window
    {

        public CatalogueWindow(CatalogueWindowDataModel dataModel)
        {
            InitializeComponent();
            if (dataModel.UsedForDataTypesOnly == true)
            {
                __tab_controller_module_catalogue.Visibility = Visibility.Collapsed;
                __tab_variable_catalogue.Visibility = Visibility.Collapsed;
                __tab_control_catalogue.SelectedIndex = 1;

                __lsv_data_type_catalogue.ItemsSource = dataModel.DataTypes;
            }
            else
            {
                __lsv_data_type_catalogue.ItemsSource = dataModel.DataTypes;
                __lsv_controller_extension_module_catalogue.ItemsSource = dataModel.ExtensionModules;
                __lsv_controller_ethernet_module_catalogue.ItemsSource = dataModel.EthernetModules;
                __lsv_variable_catalogue.ItemsSource = dataModel.Variables;
            }
        }

        private void SubItemDetailInformation_Click(object sender, RoutedEventArgs e)
        {
            CatalogueWindowDataModel subDataModel = new CatalogueWindowDataModel((IReadOnlyList<DataTypeDefinition>)(((Button)sender).Tag));
            CatalogueWindow subCatalogueWindow = new CatalogueWindow(subDataModel);
            subCatalogueWindow.ShowDialog();
        }
    }
}
