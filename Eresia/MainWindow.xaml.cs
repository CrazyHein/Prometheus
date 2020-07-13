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
        private TaskUserParametersHelper __task_user_configuration_parameters_helper;
        private string __controller_model_catalogue_exception = null;
        private string __task_user_parameters_file_name;
        private string __main_window_title;
        private byte[] __task_user_parameters_file_md5_hash = null;

        public MainWindow()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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

            __task_user_configuration_parameters_helper = new TaskUserParametersHelper(__controller_model_catalogue);
        }

        private void __on_data_binding_error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                (DataContext as TaskUserParametersDataModel).FieldDataBindingErrors++;
            else if (e.Action == ValidationErrorEventAction.Removed)
                (DataContext as TaskUserParametersDataModel).FieldDataBindingErrors--;
        }

        private void __open_task_user_configuration_parameters_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (__task_user_configuration_parameters_file_dirty() == true)
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
                    __task_user_configuration_parameters_helper.SetDefault();
                    __task_user_parameters_file_md5_hash = __task_user_configuration_parameters_helper.Load(open.FileName);        
                }
                catch (TaskUserParametersExcepetion exp)
                {
                    __task_user_parameters_file_md5_hash = null;

                    if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while reading [Task User Configuration Parameters] file . \n{0}", exp.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while reading [Task User Configuration Parameters] file . \n{0}", exp.ErrorCode.ToString());

                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    __task_user_parameters_file_name = open.FileName;
                    Title = $"{__main_window_title} --- {__task_user_parameters_file_name}";
                }

                DataContext = new TaskUserParametersDataModel(__task_user_configuration_parameters_helper);
                (DataContext as TaskUserParametersDataModel).UpdateDataModel();
            }
        }

        private void __new_task_user_configuration_parameters_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (__task_user_configuration_parameters_file_dirty() == true)
            {
                if (MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }
            __task_user_parameters_file_name = null;
            Title = $"{__main_window_title} --- New";

            __task_user_configuration_parameters_helper.SetDefault();
            DataContext = new TaskUserParametersDataModel(__task_user_configuration_parameters_helper);
            (DataContext as TaskUserParametersDataModel).UpdateDataModel();
        }

        private void __save_task_user_configuration_parameters_file_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((DataContext as TaskUserParametersDataModel).FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string message;
            try
            {
                (DataContext as TaskUserParametersDataModel).UpdateDataHelper();
                __task_user_parameters_file_md5_hash = 
                __task_user_configuration_parameters_helper.Save(__task_user_parameters_file_name);
                (DataContext as TaskUserParametersDataModel).Dirty = false;
            }
            catch (TaskUserParametersExcepetion exp)
            {
                __task_user_parameters_file_md5_hash = null;
                if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while saving [Task User Configuration Parameters] file . \n{0}", exp.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while saving [Task User Configuration Parameters] file . \n{0}", exp.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __save_task_user_configuration_parameters_file_as_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((DataContext as TaskUserParametersDataModel).FieldDataBindingErrors != 0)
            {
                MessageBox.Show("Invalid User Input ... ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "Extensible Markup Language(*.xml)|*.xml";
            save.AddExtension = true;
            save.DefaultExt = "xml";

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string message;
                try
                {
                    (DataContext as TaskUserParametersDataModel).UpdateDataHelper();
                    __task_user_parameters_file_md5_hash = 
                    __task_user_configuration_parameters_helper.Save(save.FileName);
                    (DataContext as TaskUserParametersDataModel).Dirty = false;
                    __task_user_parameters_file_name = save.FileName;
                    Title = $"{__main_window_title} --- {__task_user_parameters_file_name}";
                }
                catch (TaskUserParametersExcepetion exp)
                {
                    __task_user_parameters_file_md5_hash = null;
                    if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                        message = string.Format("At least one unexpected error occurred while saving [Task User Configuration Parameters] file . \n{0}", exp.DataException.ToString());
                    else
                        message = string.Format("At least one unexpected error occurred while saving [Task User Configuration Parameters] file . \n{0}", exp.ErrorCode.ToString());

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void __show_task_user_configuration_parameters_file_hash_code_executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show($"MD5 : {__task_user_parameters_file_md5_hash[0]:X2}{__task_user_parameters_file_md5_hash[1]:X2} - {__task_user_parameters_file_md5_hash[2]:X2}{__task_user_parameters_file_md5_hash[3]:X2} - " +
                $"{__task_user_parameters_file_md5_hash[4]:X2}{__task_user_parameters_file_md5_hash[5]:X2} - {__task_user_parameters_file_md5_hash[6]:X2}{__task_user_parameters_file_md5_hash[7]:X2} - " +
                $"{__task_user_parameters_file_md5_hash[8]:X2}{__task_user_parameters_file_md5_hash[9]:X2} - {__task_user_parameters_file_md5_hash[10]:X2}{__task_user_parameters_file_md5_hash[11]:X2} - " +
                $"{__task_user_parameters_file_md5_hash[12]:X2}{__task_user_parameters_file_md5_hash[13]:X2} - {__task_user_parameters_file_md5_hash[14]:X2}{__task_user_parameters_file_md5_hash[15]:X2}",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void __save_task_user_configuration_parameters_file_as_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null;
        }

        private void __save_task_user_configuration_parameters_file_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __task_user_parameters_file_name != null && DataContext != null;
        }

        private void __show_task_user_configuration_parameters_file_hash_code_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !__task_user_configuration_parameters_file_dirty() && __task_user_parameters_file_md5_hash != null;
        }

        private void __open_about_dialog_executed(object sender, ExecutedRoutedEventArgs e)
        {
            var about = new About();
            about.ShowDialog();
        }

        private void __add_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as TaskUserParametersDataModel).AddExtensionModuleDataModel();
            __lsb_extension_modules.SelectedIndex = __lsb_extension_modules.Items.Count - 1;
            __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);
        }

        private void __add_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null;
        }

        private void __insert_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as TaskUserParametersDataModel).InsertExtensionModuleDataModel(__lsb_extension_modules.SelectedIndex);
            __lsb_extension_modules.SelectedIndex = __lsb_extension_modules.SelectedIndex - 1;
            __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);
        }

        private void __insert_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedItem != null;
        }

        private void __remove_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
                (DataContext as TaskUserParametersDataModel).RemoveExtensionModuleDataModel(__lsb_extension_modules.SelectedIndex);

        }

        private void __remove_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedItem != null;
        }

        private void __move_up_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            int selectedIndex = __lsb_extension_modules.SelectedIndex;
            (DataContext as TaskUserParametersDataModel).SwapExtensionModuleDataModel(selectedIndex, selectedIndex - 1);
            __lsb_extension_modules.SelectedIndex = selectedIndex - 1;
            __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);
        }

        private void __move_up_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedItem != null && __lsb_extension_modules.SelectedIndex >= 1;
        }

        private void __move_down_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            int selectedIndex = __lsb_extension_modules.SelectedIndex;
            (DataContext as TaskUserParametersDataModel).SwapExtensionModuleDataModel(selectedIndex, selectedIndex + 1);
            __lsb_extension_modules.SelectedIndex = selectedIndex + 1;
            __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);
        }

        private void __move_down_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedItem != null && __lsb_extension_modules.SelectedIndex + 1 < __lsb_extension_modules.Items.Count;
        }

        private void __add_ethernet_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as TaskUserParametersDataModel).AddEthernetModuleDataModel();
            __lsb_ethernet_modules.SelectedIndex = __lsb_ethernet_modules.Items.Count - 1;
            __lsb_ethernet_modules.ScrollIntoView(__lsb_ethernet_modules.SelectedItem);
        }

        private void __add_ethernet_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null;
        }

        private void __insert_ethernet_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as TaskUserParametersDataModel).InsertEthernetModuleDataModel(__lsb_ethernet_modules.SelectedIndex);
            __lsb_ethernet_modules.SelectedIndex = __lsb_ethernet_modules.SelectedIndex - 1;
            __lsb_ethernet_modules.ScrollIntoView(__lsb_ethernet_modules.SelectedItem);
        }

        private void __insert_ethernet_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_ethernet_modules.SelectedItem != null;
        }

        private void __remove_ethernet_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                (DataContext as TaskUserParametersDataModel).RemoveEthernetModuleDataModel(__lsb_ethernet_modules.SelectedIndex);
        }

        private void __remove_ethernet_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_ethernet_modules.SelectedItem != null;
        }

        private void __move_up_ethernet_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            int selectedIndex = __lsb_ethernet_modules.SelectedIndex;
            (DataContext as TaskUserParametersDataModel).SwapEthernetModuleDataModel(selectedIndex, selectedIndex - 1);
            __lsb_ethernet_modules.SelectedIndex = selectedIndex - 1;
            __lsb_ethernet_modules.ScrollIntoView(__lsb_ethernet_modules.SelectedItem);
        }

        private void __move_up_ethernet_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_ethernet_modules.SelectedItem != null && __lsb_ethernet_modules.SelectedIndex >= 1;
        }

        private void __move_down_ethernet_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            int selectedIndex = __lsb_ethernet_modules.SelectedIndex;
            (DataContext as TaskUserParametersDataModel).SwapEthernetModuleDataModel(selectedIndex, selectedIndex + 1);
            __lsb_ethernet_modules.SelectedIndex = selectedIndex + 1;
            __lsb_ethernet_modules.ScrollIntoView(__lsb_ethernet_modules.SelectedItem);    
        }

        private void __move_down_ethernet_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_ethernet_modules.SelectedItem != null && __lsb_ethernet_modules.SelectedIndex + 1 < __lsb_ethernet_modules.Items.Count;
        }

        private void __on_main_window_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (__task_user_configuration_parameters_file_dirty() == true)
            {
                if (MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void __on_main_window_loaded(object sender, RoutedEventArgs e)
        {
            if (__task_user_parameters_file_name != null)
                MessageBox.Show(__task_user_parameters_file_name, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool __task_user_configuration_parameters_file_dirty()
        {
            if (DataContext == null)
                return false;
            else
                return (__task_user_parameters_file_name == null) || (DataContext as TaskUserParametersDataModel).Dirty;
        }
    }

    class DataContextVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ConsoleControl
    {
        public static RoutedUICommand OpenTaskUserParametersFile { get; private set; }
        public static RoutedUICommand NewTaskUserParametersFile { get; private set; }
        public static RoutedUICommand SaveTaskUserParametersFile { get; private set; }
        public static RoutedUICommand OpenAboutDialog { get; private set; }

        public static RoutedUICommand SaveTaskUserParametersFileAs { get; private set; }

        public static RoutedUICommand ShowTaskUserParametersFileHashCode { get; private set; }

        public static RoutedUICommand AddExtensionModule { get; private set; }
        public static RoutedUICommand InsertExtensionModule { get; private set; }
        public static RoutedUICommand RemoveExtensionModule { get; private set; }
        public static RoutedUICommand MoveUpExtensionModule { get; private set; }
        public static RoutedUICommand MoveDownExtensionModule { get; private set; }

        public static RoutedUICommand AddEthernetModule { get; private set; }
        public static RoutedUICommand InsertEthernetModule { get; private set; }
        public static RoutedUICommand RemoveEthernetModule { get; private set; }
        public static RoutedUICommand MoveUpEthernetModule { get; private set; }
        public static RoutedUICommand MoveDownEthernetModule { get; private set; }

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
            InputGestureCollection gestureShowTaskUserParametersFileHashCode = new InputGestureCollection
            {
                new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H")
            };
            InputGestureCollection gestureOpenAboutDialog = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+A")
            };

            InputGestureCollection gestureAddExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P")
            };
            InputGestureCollection gestureInsertExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.I, ModifierKeys.Control, "Ctrl+I")
            };
            InputGestureCollection gestureRemoveExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.R, ModifierKeys.Control, "Ctrl+R")
            };
            InputGestureCollection gestureMoveUpExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.U, ModifierKeys.Control, "Ctrl+U")
            };
            InputGestureCollection gestureMoveDownExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")
            };

            InputGestureCollection gestureAddEthernetModule = new InputGestureCollection
            {
                new KeyGesture(Key.P, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+P")
            };
            InputGestureCollection gestureInsertEthernetModule = new InputGestureCollection
            {
                new KeyGesture(Key.I, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+I")
            };
            InputGestureCollection gestureRemoveEthernetModule = new InputGestureCollection
            {
                new KeyGesture(Key.R, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+R")
            };
            InputGestureCollection gestureMoveUpEthernetModule = new InputGestureCollection
            {
                new KeyGesture(Key.U, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+U")
            };
            InputGestureCollection gestureMoveDownEthernetModule = new InputGestureCollection
            {
                new KeyGesture(Key.D, ModifierKeys.Control|ModifierKeys.Shift, "Ctrl+Shift+D")
            };


            OpenTaskUserParametersFile = new RoutedUICommand("Open", "OpenTaskUserParametersFile", typeof(ConsoleControl), gestureOpenTaskUserParametersFile);
            NewTaskUserParametersFile = new RoutedUICommand("New", "NewTaskUserParametersFile", typeof(ConsoleControl), gestureNewTaskUserParametersFile);
            SaveTaskUserParametersFileAs = new RoutedUICommand("Save As", "SaveTaskUserParametersFileAs", typeof(ConsoleControl), gestureSaveTaskUserParametersFileAs);
            SaveTaskUserParametersFile = new RoutedUICommand("Save", "SaveTaskUserParametersFile", typeof(ConsoleControl), gestureSaveTaskUserParametersFile);
            ShowTaskUserParametersFileHashCode = new RoutedUICommand("Hash", "ShowTaskUserParametersFileHashCode", typeof(ConsoleControl), gestureShowTaskUserParametersFileHashCode);
            OpenAboutDialog = new RoutedUICommand("About", "OpenAboutDialog", typeof(ConsoleControl), gestureOpenAboutDialog);

            AddExtensionModule = new RoutedUICommand("Add", "AddExtensionModule", typeof(ConsoleControl), gestureAddExtensionModule);
            InsertExtensionModule = new RoutedUICommand("Insert", "InsertExtensionModule", typeof(ConsoleControl), gestureInsertExtensionModule);
            RemoveExtensionModule = new RoutedUICommand("Remove", "RemoveExtensionModule", typeof(ConsoleControl), gestureRemoveExtensionModule);
            MoveUpExtensionModule = new RoutedUICommand("Move Up", "MoveUpExtensionModule", typeof(ConsoleControl), gestureMoveUpExtensionModule);
            MoveDownExtensionModule = new RoutedUICommand("Move Down", "MoveDownExtensionModule", typeof(ConsoleControl), gestureMoveDownExtensionModule);

            AddEthernetModule = new RoutedUICommand("Add", "AddEthernetModule", typeof(ConsoleControl), gestureAddEthernetModule);
            InsertEthernetModule = new RoutedUICommand("Insert", "InsertEthernetModule", typeof(ConsoleControl), gestureInsertEthernetModule);
            RemoveEthernetModule = new RoutedUICommand("Remove", "RemoveEthernetModule", typeof(ConsoleControl), gestureRemoveEthernetModule);
            MoveUpEthernetModule = new RoutedUICommand("Move Up", "MoveUpEthernetModule", typeof(ConsoleControl), gestureMoveUpEthernetModule);
            MoveDownEthernetModule = new RoutedUICommand("Move Down", "MoveDownEthernetModule", typeof(ConsoleControl), gestureMoveDownEthernetModule);
        }
    }
}
