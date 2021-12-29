using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using System.Windows.Controls;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// DataTypesViewer.xaml 的交互逻辑
    /// </summary>
    public partial class DataTypesViewer : UserControl
    {
        public DataTypesViewer(DataTypeCatalogue dtc)
        {
            InitializeComponent();
            DataContext = dtc.DataTypes.Values;
        }
    }
}
