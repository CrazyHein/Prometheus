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

            __task_user_configuration_parameters_helper = new TaskUserParametersHelper(__controller_model_catalogue);
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
                    __task_user_configuration_parameters_helper.Load(open.FileName);
                }
                catch (TaskUserParametersExcepetion exp)
                {
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

        }

        private void __save_task_user_configuration_parameters_file_as_executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void __save_task_user_configuration_parameters_file_as_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void __save_task_user_configuration_parameters_file_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void __open_about_dialog_executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void __add_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                (DataContext as TaskUserParametersDataModel).AddExtensionModuleDataModel();
                __lsb_extension_modules.SelectedIndex = __lsb_extension_modules.Items.Count - 1;
                __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);

            }
            catch(TaskUserParametersExcepetion exp)
            {
                string message;
                if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while adding ExtensionModule . \n{0}", exp.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while adding ExtensionModule . \n{0}", exp.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __add_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null;
        }

        private void __insert_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                (DataContext as TaskUserParametersDataModel).InsertExtensionModuleDataModel(__lsb_extension_modules.SelectedIndex);
                __lsb_extension_modules.SelectedIndex = __lsb_extension_modules.SelectedIndex - 1;
                __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);

            }
            catch (TaskUserParametersExcepetion exp)
            {
                string message;
                if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while inserting ExtensionModule . \n{0}", exp.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while inserting ExtensionModule . \n{0}", exp.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __insert_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedItem != null;
        }

        private void __remove_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
                    (DataContext as TaskUserParametersDataModel).RemoveExtensionModuleDataModel(__lsb_extension_modules.SelectedIndex);

            }
            catch (TaskUserParametersExcepetion exp)
            {
                string message;
                if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while removing ExtensionModule . \n{0}", exp.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while removing ExtensionModule . \n{0}", exp.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __remove_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedItem != null;
        }

        private void __move_up_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                int selectedIndex = __lsb_extension_modules.SelectedIndex;
                (DataContext as TaskUserParametersDataModel).SwapExtensionModuleDataModel(selectedIndex, selectedIndex - 1);
                __lsb_extension_modules.SelectedIndex = selectedIndex - 1;
                __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);

            }
            catch (TaskUserParametersExcepetion exp)
            {
                string message;
                if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while moving up ExtensionModule . \n{0}", exp.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while moving up ExtensionModule . \n{0}", exp.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __move_up_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedIndex >= 1;
        }

        private void __move_down_extension_module_executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                int selectedIndex = __lsb_extension_modules.SelectedIndex;
                (DataContext as TaskUserParametersDataModel).SwapExtensionModuleDataModel(selectedIndex, selectedIndex + 1);
                __lsb_extension_modules.SelectedIndex = selectedIndex + 1;
                __lsb_extension_modules.ScrollIntoView(__lsb_extension_modules.SelectedItem);
            }
            catch (TaskUserParametersExcepetion exp)
            {
                string message;
                if (exp.ErrorCode == TASK_USER_PARAMETERS_ERROR_T.FILE_DATA_EXCEPTION)
                    message = string.Format("At least one unexpected error occurred while moving down ExtensionModule . \n{0}", exp.DataException.ToString());
                else
                    message = string.Format("At least one unexpected error occurred while moving down ExtensionModule . \n{0}", exp.ErrorCode.ToString());

                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void __move_down_extension_module_can_executed(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataContext != null && __lsb_extension_modules.SelectedIndex + 1 < __lsb_extension_modules.Items.Count;
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

        public static RoutedUICommand AddExtensionModule { get; private set; }
        public static RoutedUICommand InsertExtensionModule { get; private set; }
        public static RoutedUICommand RemoveExtensionModule { get; private set; }
        public static RoutedUICommand MoveUpExtensionModule { get; private set; }
        public static RoutedUICommand MoveDownExtensionModule { get; private set; }

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

            InputGestureCollection gestureAddExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+P")
            };
            InputGestureCollection gestureInsertExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+I")
            };
            InputGestureCollection gestureRemoveExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+R")
            };
            InputGestureCollection gestureMoveUpExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+U")
            };
            InputGestureCollection gestureMoveDownExtensionModule = new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control, "Ctrl+D")
            };


            OpenTaskUserParametersFile = new RoutedUICommand("Open", "OpenTaskUserParametersFile", typeof(ConsoleControl), gestureOpenTaskUserParametersFile);
            NewTaskUserParametersFile = new RoutedUICommand("New", "NewTaskUserParametersFile", typeof(ConsoleControl), gestureNewTaskUserParametersFile);
            SaveTaskUserParametersFileAs = new RoutedUICommand("Save As", "SaveTaskUserParametersFileAs", typeof(ConsoleControl), gestureSaveTaskUserParametersFileAs);
            SaveTaskUserParametersFile = new RoutedUICommand("Save", "SaveTaskUserParametersFile", typeof(ConsoleControl), gestureSaveTaskUserParametersFile);
            OpenAboutDialog = new RoutedUICommand("About", "OpenAboutDialog", typeof(ConsoleControl), gestureOpenAboutDialog);

            AddExtensionModule = new RoutedUICommand("Add", "AddModule", typeof(ConsoleControl), gestureAddExtensionModule);
            InsertExtensionModule = new RoutedUICommand("Insert", "InsertModule", typeof(ConsoleControl), gestureInsertExtensionModule);
            RemoveExtensionModule = new RoutedUICommand("Remove", "RemoveModule", typeof(ConsoleControl), gestureRemoveExtensionModule);
            MoveUpExtensionModule = new RoutedUICommand("Move Up", "MoveUpModule", typeof(ConsoleControl), gestureMoveUpExtensionModule);
            MoveDownExtensionModule = new RoutedUICommand("Move Down", "MoveDownModule", typeof(ConsoleControl), gestureMoveDownExtensionModule);
        }
    }
}
