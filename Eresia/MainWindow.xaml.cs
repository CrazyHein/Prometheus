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
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Eresia
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ControllerModelCatalogue __controller_model_catalogue;
        private string __controller_model_catalogue_exception = null;
        private string __task_user_parameters_file_name;
        private string __main_window_title;

        public MainWindow()
        {
            InitializeComponent();
            __main_window_title = Title;
            try
            {
                __controller_model_catalogue = new ControllerModelCatalogue();
                __controller_model_catalogue.Load(Metadata.ControllerModelCatalogue);
            }
            catch (ModelCatalogueParseExcepetion e)
            {
                if (e.ErrorCode == MODEL_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION)
                    __controller_model_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Controller Model Catalogue] file . \n{0}", e.DataException.ToString());
                else
                    __controller_model_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Controller Model Catalogue] file . \n{0}", e.ErrorCode.ToString());
            }

            TaskUserParametersHelper helper = new TaskUserParametersHelper(__controller_model_catalogue);
            helper.Load("r2h_task_user_parameters.xml");
        }
    }
}
