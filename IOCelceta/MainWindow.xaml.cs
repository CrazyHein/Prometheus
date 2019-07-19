using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CatalogueWindowDataModel __catalogue_window_data_model;
        private DataTypeCatalogue __data_type_catalogue;
        private ControllerModelCatalogue __controller_model_catalogue;
        private IOListDataHelper __io_list_data_helper;
        private string __data_type_catalogue_exception = null;
        private string __controller_model_catalogue_exception = null;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                __data_type_catalogue = new DataTypeCatalogue();
                __data_type_catalogue.Load("data_type_catalogue.xml");
            }
            catch(DataTypeCatalogueParseExcepetion e)
            {
                if (e.ErrorCode == DATA_TYPE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION)
                    __data_type_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Data Type Catalogue] file . \n{0}", e.DataException.ToString());
                else
                    __data_type_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Data Type Catalogue] file . \n{0}", e.ErrorCode.ToString());
            }
            try
            {
                __controller_model_catalogue = new ControllerModelCatalogue();
                __controller_model_catalogue.Load("controller_model_catalogue.xml");
            }
            catch(ModelCatalogueParseExcepetion e)
            {
                if (e.ErrorCode == MODEL_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION)
                    __controller_model_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Controller Model Catalogue] file . \n{0}", e.DataException.ToString());
                else
                    __controller_model_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Controller Model Catalogue] file . \n{0}", e.ErrorCode.ToString());
            }

            __catalogue_window_data_model = new CatalogueWindowDataModel(__controller_model_catalogue, __data_type_catalogue);
            __io_list_data_helper = new IOListDataHelper(__controller_model_catalogue, __data_type_catalogue);
        }

        private void __open_catalogue_dialog_executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new CatalogueWindow(__catalogue_window_data_model);
            window.ShowDialog();
        }

        private void __open_io_list_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (__tab_target_inforamtion.Content != null)
            {
                if (MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Extensible Markup Language(*.xml)|*.xml";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string message;
                try
                {
                    __io_list_data_helper.Load(open.FileName);
                }
                catch (IOListParseExcepetion exp)
                {
                    if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while reading [IO List] file . \n{0}", exp.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while reading [IO List] file . \n{0}", exp.ErrorCode.ToString());

                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                TargetInformationDataModel targetInfoDataModel = new TargetInformationDataModel(__io_list_data_helper);
                __tab_target_inforamtion.Content = new TargetInformationDataControl(targetInfoDataModel);
                ControllerInformationDataModel controllerInfoDataModel = new ControllerInformationDataModel(__io_list_data_helper);
                __tab_controller_inforamtion.Content = new ControllerInformationDataControl(controllerInfoDataModel);
                ObjectCollectionDataModel objectCollectionDataModel = new ObjectCollectionDataModel(__io_list_data_helper);
                __tab_object_collection.Content = new ObjectCollectionDataControl(objectCollectionDataModel);
                PDOCollectionDataModel pdoCollectionDataModel = new PDOCollectionDataModel(__io_list_data_helper, objectCollectionDataModel);
                __tab_pdo_collection.Content = new PDOCollectionDataControl(pdoCollectionDataModel);

                ((__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel).UpdateDataModel();
                ((__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel).UpdateDataModel();
                ((__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel).UpdateDataModel();
                ((__tab_pdo_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel).UpdateDataModel();
            }
        }

        private void __new_io_list_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (__tab_target_inforamtion.Content != null)
            {
                if (MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }
            __io_list_data_helper.SetDefault();

            TargetInformationDataModel targetInfoDataModel = new TargetInformationDataModel(__io_list_data_helper);
            __tab_target_inforamtion.Content = new TargetInformationDataControl(targetInfoDataModel);
            ControllerInformationDataModel controllerInfoDataModel = new ControllerInformationDataModel(__io_list_data_helper);
            __tab_controller_inforamtion.Content = new ControllerInformationDataControl(controllerInfoDataModel);
            ObjectCollectionDataModel objectCollectionDataModel = new ObjectCollectionDataModel(__io_list_data_helper);
            __tab_object_collection.Content = new ObjectCollectionDataControl(objectCollectionDataModel);
            PDOCollectionDataModel pdoCollectionDataModel = new PDOCollectionDataModel(__io_list_data_helper, objectCollectionDataModel);
            __tab_pdo_collection.Content = new PDOCollectionDataControl(pdoCollectionDataModel);

            ((__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel).UpdateDataModel();
            ((__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel).UpdateDataModel();
            ((__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel).UpdateDataModel();
            ((__tab_pdo_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel).UpdateDataModel();
        }

        private void __on_main_window_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (__tab_target_inforamtion.Content != null)
            {
                if (MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void __on_main_window_loaded(object sender, RoutedEventArgs e)
        {
            if(__data_type_catalogue_exception != null)
                MessageBox.Show(__data_type_catalogue_exception, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            if(__controller_model_catalogue_exception != null)
                MessageBox.Show(__controller_model_catalogue_exception, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal class ConsoleControl
    {
        public static RoutedUICommand OpenIOListFile { get; private set; }
        public static RoutedUICommand NewIOListFile { get; private set; }
        public static RoutedUICommand OpenAboutDialog { get; private set; }

        public static RoutedUICommand OpenCatalogueDialog { get; private set; }

        static ConsoleControl()
        {
            InputGestureCollection gestureOpenIOListFile = new InputGestureCollection
            {
                new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O")
            };
            InputGestureCollection gestureNewIOListFile = new InputGestureCollection
            {
                new KeyGesture(Key.N, ModifierKeys.Control, "Ctrl+N")
            };
            InputGestureCollection gestureOpenAboutDialog = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+A")
            };
            InputGestureCollection gestureOpenCatalogueDialog = new InputGestureCollection
            {
                new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")
            };
            OpenIOListFile = new RoutedUICommand("Open", "OpenIOListFile", typeof(ConsoleControl), gestureOpenIOListFile);
            NewIOListFile = new RoutedUICommand("New", "NewIOListFile", typeof(ConsoleControl), gestureNewIOListFile);
            OpenAboutDialog = new RoutedUICommand("About", "OpenAboutDialog", typeof(ConsoleControl), gestureOpenAboutDialog);
            OpenCatalogueDialog = new RoutedUICommand("Catalogue", "OpenCatalogueDialog", typeof(ConsoleControl), gestureOpenCatalogueDialog);
        }

    }
}
