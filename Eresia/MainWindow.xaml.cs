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

    internal class ConsoleControl
    {
        public static RoutedUICommand OpenTaskUserParametersFile { get; private set; }
        public static RoutedUICommand NewTaskUserParametersFile { get; private set; }
        public static RoutedUICommand SaveTaskUserParametersFile { get; private set; }
        public static RoutedUICommand OpenAboutDialog { get; private set; }

        public static RoutedUICommand SaveTaskUserParametersFileAs { get; private set; }

        static ConsoleControl()
        {
            InputGestureCollection gestureOpenTaskUserParametersFile = new InputGestureCollection
            {
                new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O")
            };
            InputGestureCollection gestureNewTaskUserParametersFile = new InputGestureCollection
            {
                new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N")
            };
            InputGestureCollection gestureSaveTaskUserParametersFile = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S")
            };
            InputGestureCollection gestureSaveTaskUserParametersFileAs = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+S")
            };
            InputGestureCollection gestureOpenAboutDialog = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+A")
            };
            OpenTaskUserParametersFile = new RoutedUICommand("Open", "OpenTaskUserParametersFile", typeof(ConsoleControl), gestureOpenTaskUserParametersFile);
            NewTaskUserParametersFile = new RoutedUICommand("New", "NewTaskUserParametersFile", typeof(ConsoleControl), gestureNewTaskUserParametersFile);
            SaveTaskUserParametersFileAs = new RoutedUICommand("Save As", "SaveTaskUserParametersFileAs", typeof(ConsoleControl), gestureSaveTaskUserParametersFileAs);
            SaveTaskUserParametersFile = new RoutedUICommand("Save", "SaveTaskUserParametersFile", typeof(ConsoleControl), gestureSaveTaskUserParametersFile);
            OpenAboutDialog = new RoutedUICommand("About", "OpenAboutDialog", typeof(ConsoleControl), gestureOpenAboutDialog);
        }
    }
}
