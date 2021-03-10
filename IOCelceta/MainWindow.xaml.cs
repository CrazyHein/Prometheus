using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.Catalogue;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private VariableCatalogue __variable_catalogue;
        private IOListDataHelper __io_list_data_helper;
        private Metadata __meta_data;
        private string __data_type_catalogue_exception = null;
        private string __controller_model_catalogue_exception = null;
        private string __variable_catalogue_exception = null;
        private string __io_list_file_name;
        private string __main_window_title;
        private byte[] __io_list_file_md5_hash = null;

        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            __main_window_title = Title;
            try
            {
                __data_type_catalogue = new DataTypeCatalogue();
                __data_type_catalogue.Load(Metadata.DataTypeCatalogue);
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
                __controller_model_catalogue.Load(Metadata.ControllerModelCatalogue);
            }
            catch(ModelCatalogueParseExcepetion e)
            {
                if (e.ErrorCode == MODEL_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION)
                    __controller_model_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Controller Model Catalogue] file . \n{0}", e.DataException.ToString());
                else
                    __controller_model_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Controller Model Catalogue] file . \n{0}", e.ErrorCode.ToString());
            }

            try
            {
                __variable_catalogue = new VariableCatalogue(__data_type_catalogue);
                __variable_catalogue.Load(Metadata.VariableCatalogue);
            }
            catch(VariableCatalogueParseExcepetion e)
            {
                if (e.ErrorCode ==  VARIABLE_CATALOGUE_FILE_ERROR_CODE_T.FILE_DATA_EXCEPTION)
                    __variable_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Variable Catalogue] file . \n{0}", e.DataException.ToString());
                else
                    __variable_catalogue_exception = string.Format("At least one unexpected error occurred while reading [Variable Catalogue] file . \n{0}", e.ErrorCode.ToString());
            }

            __catalogue_window_data_model = new CatalogueWindowDataModel(__controller_model_catalogue, __data_type_catalogue, __variable_catalogue);
            __io_list_data_helper = new IOListDataHelper(__controller_model_catalogue, __data_type_catalogue, __variable_catalogue);
            __meta_data = new Metadata(__io_list_data_helper);
        }

        private bool __io_list_file_dirty()
        {
            if (__tab_target_inforamtion.Content == null)
                return false;
            else
                return (__io_list_file_name == null) ||
                    ((__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel).Dirty == true ||
                    ((__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel).Dirty == true ||
                    ((__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel).Dirty == true ||
                    ((__tab_pdo_intlk_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel).Dirty == true;
        }

        private void __open_catalogue_dialog_executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new CatalogueWindow(__catalogue_window_data_model);
            window.ShowDialog();
        }

        private void __open_io_list_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (__io_list_file_dirty() == true)
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
                    __io_list_data_helper.SetDefault();
                    __io_list_file_md5_hash = __io_list_data_helper.Load(open.FileName);    
                }
                catch (IOListParseExcepetion exp)
                {
                    __io_list_file_md5_hash = null;

                    if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while reading [IO List] file . \n{0}", exp.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while reading [IO List] file . \n{0}", exp.ErrorCode.ToString());

                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    __io_list_file_name = open.FileName;
                    Title = $"{__main_window_title} --- {__io_list_file_name}";
                }

                TargetInformationDataModel targetInfoDataModel = new TargetInformationDataModel(__io_list_data_helper);
                __tab_target_inforamtion.Content = new TargetInformationDataControl(targetInfoDataModel);
                ControllerInformationDataModel controllerInfoDataModel = new ControllerInformationDataModel(__io_list_data_helper);
                __tab_controller_inforamtion.Content = new ControllerInformationDataControl(controllerInfoDataModel);
                ObjectCollectionDataModel objectCollectionDataModel = new ObjectCollectionDataModel(__io_list_data_helper);
                __tab_object_collection.Content = new ObjectCollectionDataControl(objectCollectionDataModel);
                PDOCollectionDataModel pdoCollectionDataModel = new PDOCollectionDataModel(__io_list_data_helper, objectCollectionDataModel);
                __tab_pdo_intlk_collection.Content = new PDOCollectionDataControl(pdoCollectionDataModel);
                //InterlockCollectionDataModel interlockCollectionDataModel = new InterlockCollectionDataModel(__io_list_data_helper, objectCollectionDataModel);
                //__tab_interlock_collection.Content = new InterlockCollectionDataControl(interlockCollectionDataModel);

                ((__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel).InitializeDataModel();
                ((__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel).InitializeDataModel();
                ((__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel).InitializeDataModel();
                ((__tab_pdo_intlk_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel).InitializeDataModel();
                //((__tab_interlock_collection.Content as InterlockCollectionDataControl).DataContext as InterlockCollectionDataModel).UpdateDataModel();
            }
        }

        private void __new_io_list_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (__io_list_file_dirty() == true)
            {
                if (MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }
            __io_list_file_name = null;
            Title = $"{__main_window_title} --- New";

            __io_list_data_helper.SetDefault();

            TargetInformationDataModel targetInfoDataModel = new TargetInformationDataModel(__io_list_data_helper);
            __tab_target_inforamtion.Content = new TargetInformationDataControl(targetInfoDataModel);
            ControllerInformationDataModel controllerInfoDataModel = new ControllerInformationDataModel(__io_list_data_helper);
            __tab_controller_inforamtion.Content = new ControllerInformationDataControl(controllerInfoDataModel);
            ObjectCollectionDataModel objectCollectionDataModel = new ObjectCollectionDataModel(__io_list_data_helper);
            __tab_object_collection.Content = new ObjectCollectionDataControl(objectCollectionDataModel);
            PDOCollectionDataModel pdoCollectionDataModel = new PDOCollectionDataModel(__io_list_data_helper, objectCollectionDataModel);
            __tab_pdo_intlk_collection.Content = new PDOCollectionDataControl(pdoCollectionDataModel);
            //InterlockCollectionDataModel interlockCollectionDataModel = new InterlockCollectionDataModel(__io_list_data_helper, objectCollectionDataModel);
            //__tab_interlock_collection.Content = new InterlockCollectionDataControl(interlockCollectionDataModel);

            ((__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel).InitializeDataModel();
            ((__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel).InitializeDataModel();
            ((__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel).InitializeDataModel();
            ((__tab_pdo_intlk_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel).InitializeDataModel();
            //((__tab_interlock_collection.Content as InterlockCollectionDataControl).DataContext as InterlockCollectionDataModel).UpdateDataModel();
        }

        private void __save_io_list_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            TargetInformationDataModel targetInfo = (__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel;

            if (targetInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Target Information)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ControllerInformationDataModel controllerInfo = (__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel;

            if (controllerInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Controller Information)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ObjectCollectionDataModel objectsInfo = (__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel;

            if (objectsInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Object Collection)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PDOCollectionDataModel pdoMappingsInfo = (__tab_pdo_intlk_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel;

            if (pdoMappingsInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (PDO Mapping Collection)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            targetInfo.FinalizeDataHelper();
            controllerInfo.FinalizeDataHelper();
            objectsInfo.FinalizeDataHelper();
            pdoMappingsInfo.FinalizeDataHelper();

            string message;
            try
            {
                __io_list_file_md5_hash = 
                __io_list_data_helper.Save(controllerInfo.ExtensionModules.Select(dataModel => dataModel.ReferenceName),
                    controllerInfo.EthernetModules.Select(dataModel => dataModel.ReferenceName),
                    objectsInfo.Objects.Select(dataModel => dataModel.Index),
                    __io_list_file_name);

                targetInfo.Dirty = false;
                controllerInfo.Dirty = false;
                objectsInfo.Dirty = false;
                pdoMappingsInfo.Dirty = false;
            }
            catch (IOListParseExcepetion exp)
            {
                __io_list_file_md5_hash = null;

                if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while saving [IO List] file . \n{0}", exp.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while saving [IO List] file . \n{0}", exp.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __save_io_list_file_as_executed(object sender, ExecutedRoutedEventArgs e)
        {
            TargetInformationDataModel targetInfo = (__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel;

            if (targetInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Target Information)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ControllerInformationDataModel controllerInfo = (__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel;

            if (controllerInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Controller Information)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ObjectCollectionDataModel objectsInfo = (__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel;

            if (objectsInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Object Collection)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PDOCollectionDataModel pdoMappingsInfo = (__tab_pdo_intlk_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel;

            if (pdoMappingsInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (PDO Mapping Collection)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "Extensible Markup Language(*.xml)|*.xml";
            save.AddExtension = true;
            save.DefaultExt = "xml";

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                targetInfo.FinalizeDataHelper();
                controllerInfo.FinalizeDataHelper();
                objectsInfo.FinalizeDataHelper();
                pdoMappingsInfo.FinalizeDataHelper();

                string message;
                try
                {
                    __io_list_file_md5_hash = 
                    __io_list_data_helper.Save(controllerInfo.ExtensionModules.Select(dataModel => dataModel.ReferenceName),
                        controllerInfo.EthernetModules.Select(dataModel => dataModel.ReferenceName),
                        objectsInfo.Objects.Select(dataModel => dataModel.Index), 
                        save.FileName);
                    __io_list_file_name = save.FileName;
                    Title = $"{__main_window_title} --- {__io_list_file_name}";
                    targetInfo.Dirty = false;
                    controllerInfo.Dirty = false;
                    objectsInfo.Dirty = false;
                    pdoMappingsInfo.Dirty = false;
                }
                catch (IOListParseExcepetion exp)
                {
                    __io_list_file_md5_hash = null;

                    if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while saving [IO List] file . \n{0}", exp.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while saving [IO List] file . \n{0}", exp.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }              
            }
        }

        private void __open_about_dialog_executed(object sender, ExecutedRoutedEventArgs e)
        {
            var about = new About(__meta_data);
            about.ShowDialog();
        }

        private void __on_main_window_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (__io_list_file_dirty() == true)
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
            if(__variable_catalogue_exception != null)
                MessageBox.Show(__variable_catalogue_exception, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void __save_io_list_file_as_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __tab_target_inforamtion.Content != null;
        }

        private void __save_io_list_file_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __tab_target_inforamtion.Content != null && __io_list_file_name != null;
        }

        private void __show_io_list_hash_code_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !__io_list_file_dirty() && __io_list_file_md5_hash != null ; 
        }

        private void __show_io_list_hash_code_executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show($"MD5 : {__io_list_file_md5_hash[0]:X2}{__io_list_file_md5_hash[1]:X2} - {__io_list_file_md5_hash[2]:X2}{__io_list_file_md5_hash[3]:X2} - " +
                $"{__io_list_file_md5_hash[4]:X2}{__io_list_file_md5_hash[5]:X2} - {__io_list_file_md5_hash[6]:X2}{__io_list_file_md5_hash[7]:X2} - " +
                $"{__io_list_file_md5_hash[8]:X2}{__io_list_file_md5_hash[9]:X2} - {__io_list_file_md5_hash[10]:X2}{__io_list_file_md5_hash[11]:X2} - " +
                $"{__io_list_file_md5_hash[12]:X2}{__io_list_file_md5_hash[13]:X2} - {__io_list_file_md5_hash[14]:X2}{__io_list_file_md5_hash[15]:X2}",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void __export_to_excel_file_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __tab_target_inforamtion.Content != null;
        }

        private void __export_to_excel_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            TargetInformationDataModel targetInfo = (__tab_target_inforamtion.Content as TargetInformationDataControl).DataContext as TargetInformationDataModel;

            if (targetInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Target Information)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ControllerInformationDataModel controllerInfo = (__tab_controller_inforamtion.Content as ControllerInformationDataControl).DataContext as ControllerInformationDataModel;

            if (controllerInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Controller Information)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ObjectCollectionDataModel objectsInfo = (__tab_object_collection.Content as ObjectCollectionDataControl).DataContext as ObjectCollectionDataModel;

            if (objectsInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (Object Collection)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PDOCollectionDataModel pdoMappingsInfo = (__tab_pdo_intlk_collection.Content as PDOCollectionDataControl).DataContext as PDOCollectionDataModel;

            if (pdoMappingsInfo.FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... (PDO Mapping Collection)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "Microsoft Office Excel 2007+ (*.xlsx)|*.xlsx";
            save.AddExtension = true;
            save.DefaultExt = "xlsx";

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                targetInfo.FinalizeDataHelper();
                controllerInfo.FinalizeDataHelper();
                objectsInfo.FinalizeDataHelper();
                pdoMappingsInfo.FinalizeDataHelper();

                string message;
                try
                {
                    var pass = System.Security.Cryptography.SHA256.Create();
                    var bytes = pass.ComputeHash(BitConverter.GetBytes((new object()).GetHashCode()));
                    __io_list_data_helper.Export(controllerInfo.ExtensionModules.Select(dataModel => dataModel.ReferenceName),
                        controllerInfo.EthernetModules.Select(dataModel => dataModel.ReferenceName),
                        objectsInfo.Objects.Select(dataModel => dataModel.Index),
                        save.FileName, IO_LIST_EXPORT_FORMAT.XLS2007, null, null,
                        bytes[7].ToString("X2") + bytes[15].ToString("X2") + bytes[23].ToString("X2") + bytes[31].ToString("X2"));
                }
                catch (IOListParseExcepetion exp)
                {
                    if (exp.ErrorCode == IO_LIST_FILE_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while exporting [IO List] file . \n{0}", exp.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while exporting [IO List] file . \n{0}", exp.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal class IsDirtyDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return " *";
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ConsoleControl
    {
        public static RoutedUICommand OpenIOListFile { get; private set; }
        public static RoutedUICommand NewIOListFile { get; private set; }
        public static RoutedUICommand SaveIOListFile { get; private set; }
        public static RoutedUICommand OpenAboutDialog { get; private set; }

        public static RoutedUICommand OpenCatalogueDialog { get; private set; }

        public static RoutedUICommand SaveIOListFileAs { get; private set; }

        public static RoutedUICommand ShowIOListFileHashCode { get; private set; }

        public static RoutedUICommand ExportToExcelFile { get; private set; }

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
            InputGestureCollection gestureSaveIOListFile = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S")
            };
            InputGestureCollection gestureSaveIOListFileAs = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+S")
            };
            InputGestureCollection gestureOpenAboutDialog = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+A")
            };
            InputGestureCollection gestureOpenCatalogueDialog = new InputGestureCollection
            {
                new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")
            };
            InputGestureCollection gestureShowIOListFileHashCode = new InputGestureCollection
            {
                new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H")
            };
            InputGestureCollection gestureExportToExcelFile = new InputGestureCollection
            {
                new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+E")
            };
            OpenIOListFile = new RoutedUICommand("Open", "OpenIOListFile", typeof(ConsoleControl), gestureOpenIOListFile);
            NewIOListFile = new RoutedUICommand("New", "NewIOListFile", typeof(ConsoleControl), gestureNewIOListFile);
            SaveIOListFileAs = new RoutedUICommand("Save As", "SaveIOListFileAs", typeof(ConsoleControl), gestureSaveIOListFileAs);
            SaveIOListFile = new RoutedUICommand("Save", "SaveIOListFile", typeof(ConsoleControl), gestureSaveIOListFile);
            OpenAboutDialog = new RoutedUICommand("About", "OpenAboutDialog", typeof(ConsoleControl), gestureOpenAboutDialog);
            OpenCatalogueDialog = new RoutedUICommand("Catalogue", "OpenCatalogueDialog", typeof(ConsoleControl), gestureOpenCatalogueDialog);
            ShowIOListFileHashCode = new RoutedUICommand("Hash", "ShowIOListFileHashCode", typeof(ConsoleControl), gestureShowIOListFileHashCode);
            ExportToExcelFile = new RoutedUICommand("Export", "ExportToExcelFile", typeof(ConsoleControl), gestureExportToExcelFile);
        }

    }
}
