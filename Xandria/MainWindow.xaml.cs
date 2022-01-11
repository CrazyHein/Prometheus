using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria.Utility;
using Syncfusion.SfSkinManager;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Xandria
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		#region Fields
        private string currentVisualStyle;
		private string currentSizeMode;
        #endregion
        private ControllerModelCatalogue __controller_model_catalogue;
        private TaskUserParameterHelper __task_user_parameter_helper;
        private MainModel __main_model = new MainModel();
        private HardwareCollectionViewer __hardware_collection_viewer;
        private Info __info = new Info();
        #region Properties
        /// <summary>
        /// Gets or sets the current visual style.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentVisualStyle
        {
            get
            {
                return currentVisualStyle;
            }
            set
            {
                currentVisualStyle = value;
                OnVisualStyleChanged();
            }
        }
		
		/// <summary>
        /// Gets or sets the current Size mode.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentSizeMode
        {
            get
            {
                return currentSizeMode;
            }
            set
            {
                currentSizeMode = value;
                OnSizeModeChanged();
            }
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
			this.Loaded += OnLoaded;

            DataContext = __main_model;
        }
		/// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "FluentLight";
	        CurrentSizeMode = "Default";

            try
            {
                __controller_model_catalogue = new ControllerModelCatalogue(__info.ControllerModelCataloguePath);
                __info.ControllerModelCatalogueHash = __controller_model_catalogue.MD5Code;
                //__task_user_parameters_helper = new TaskUserParameterHelper(__controller_model_catalogue, "r2h_task_user_parameters.xml", out _);
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during loading metadata :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
		/// <summary>
        /// On Visual Style Changed.
        /// </summary>
        /// <remarks></remarks>
        private void OnVisualStyleChanged()
        {
            VisualStyles visualStyle = VisualStyles.Default;
            Enum.TryParse(CurrentVisualStyle, out visualStyle);            
            if (visualStyle != VisualStyles.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetVisualStyle(this, visualStyle);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }
		
		/// <summary>
        /// On Size Mode Changed event.
        /// </summary>
        /// <remarks></remarks>
        private void OnSizeModeChanged()
        {
            SizeMode sizeMode = SizeMode.Default;
            Enum.TryParse(CurrentSizeMode, out sizeMode);
            if (sizeMode != SizeMode.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetSizeMode(this, sizeMode);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        private bool __data_model_has_changes()
        {
            if (__hardware_collection_viewer == null)
                return false;
            return (__hardware_collection_viewer.DataContext as HardwareModels).Modified ;
        }

        private void __reset_layout()
        {
            __hardware_collection_viewer = new HardwareCollectionViewer(__task_user_parameter_helper, __controller_model_catalogue);
            HardwareModuleConfiguraton.Content = __hardware_collection_viewer;
            HardwareModuleConfiguraton.DataContext = __hardware_collection_viewer.DataContext;
        }

        private void __close_layout()
        {
            __hardware_collection_viewer = null;
            HardwareModuleConfiguraton.Content = null;
        }

        public string __update_binding_source()
        {
            __hardware_collection_viewer.UpdateBindingSource();
            if(__hardware_collection_viewer.HasError)
                return "At least one user input is invalid.(Hardware Module Configuration)";
            return null;
        }

        public void __commit_changes()
        {
            (__hardware_collection_viewer.DataContext as HardwareModels).CommitChanges();
        }

        private void NewCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            try
            {
                __task_user_parameter_helper = new TaskUserParameterHelper(__controller_model_catalogue);
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Task User Parameters File(*.xml)|*.xml";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    __task_user_parameter_helper = new TaskUserParameterHelper( __controller_model_catalogue, open.FileName, out _);
                    __main_model.CurrentlyOpenFile = open.FileName;
                    __reset_layout();
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened;
        }

        private void CloseCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            __task_user_parameter_helper = null;
            __close_layout();
            __main_model.CurrentlyOpenFile = null;
        }

        private void SaveAsCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened;
        }

        private void SaveAsCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "Task User Parameters File(*.xml)|*.xml";
            save.AddExtension = true;
            save.DefaultExt = "xml";
            save.FileName = "r2h_task_user_parameters.xml";

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    __task_user_parameter_helper.Save(save.FileName);
                    __main_model.CurrentlyOpenFile = save.FileName;
                    __commit_changes();
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsNonTemporaryFile && __data_model_has_changes();
        }

        private void SaveCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                string error = __update_binding_source();
                if (error != null)
                {
                    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                __task_user_parameter_helper.Save(__main_model.CurrentlyOpenFile);
                __commit_changes();
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuitCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            //
            Close();
        }

        private void AboutCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var wnd = new About(__info);
            wnd.ShowDialog();
        }

        private void DownloadviaFTPCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __main_model.IsOpened;//__main_model.IsNonTemporaryFile && !__data_model_has_changes();
        }

        private void DownloadviaFTPCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string error = __update_binding_source();
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var wnd = new FTPUtility(FTPMode.Download, __controller_model_catalogue, __task_user_parameter_helper);
            wnd.ShowDialog();
        }

        private void UploadviaFTPCommand_CanExecuted(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void UploadviaFTPCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            var wnd = new FTPUtility(FTPMode.Upload, __controller_model_catalogue, __task_user_parameter_helper);
            if (wnd.ShowDialog() == true)
            {
                __task_user_parameter_helper = wnd.UploadResult;
                __main_model.CurrentlyOpenFile = String.Empty;
                __reset_layout();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((__main_model.IsNonTemporaryFile && __data_model_has_changes()) || __main_model.IsTemporaryFile)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }
    }
}
