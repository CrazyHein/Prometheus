using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Context;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.ControlBlock;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Step;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Recipe;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private string currentVisualStyle = "FluentDark";
        private string currentSizeMode = "Default";
        #endregion
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "FluentDark";
            CurrentSizeMode = "Default";
        }

        private DocumentModel __document_model;

        private Settings __settings;
        private RecipeDocument? __rcp_document;


        private SortedDictionary<string, UserControl?> __content_controls;
        public MainWindow()
        {
            InitializeComponent();

            __settings = new Settings();

            __document_model = new DocumentModel(__settings);

            DataContext = __document_model;

            
            __content_controls = new SortedDictionary<string, UserControl?>();
            __content_controls[ContextManager.Tag.ToString()] = null;
            __content_controls[GlobalEventManager.Tag.ToString()] = null;
            __content_controls[ControlBlockManager.Tag.ToString()] = null;

            this.Loaded += OnLoaded;

            /*
            __document = new RecipeDocument("123.rcp");
            Component.Context = __document.Context;

            (var _, var _, var _, var _, var txbit, var txblk, var _, var rxbit, var rxblk, var _, var _) = 
            IOCelcetaHelper.Load("io.folst", __data_type_catalogue, __controller_model_catalogue, out _);

            __processdata_collection = new Dictionary<uint, ProcessData>();
            foreach (var p in txbit.ProcessDatas.Concat(txblk.ProcessDatas).Concat(rxbit.ProcessDatas).Concat(rxblk.ProcessDatas))
                __processdata_collection[p.ProcessObject.Index] = p;

            Component.Tags = __processdata_collection;
            __globals = new GlobalEventModelCollection(__document);
            __blocks = new ControlBlockModelCollection(__document);

            GlobalEventManager.Content = new GlobalEventManager(__globals, __blocks, ContentControl);
            ControlBlockManager.Content = new ControlBlockManager(__globals, __blocks, ContentControl);
            */
        }

        private void __reset_layout()
        {
            __content_controls[ContextManager.Tag.ToString()] = null;
            __content_controls[GlobalEventManager.Tag.ToString()] = null;
            __content_controls[ControlBlockManager.Tag.ToString()] = null;
            ContentControl.Content = null;
        }

        private void __close_layout()
        {

        }

        private bool __unapplied_changes(bool updateBinding)
        {
            //(__content_controls[GlobalEventManager.Tag.ToString()]?.Content as GlobalEventControl)?.UpdateBindingSource();
            //UserControl? blk = __content_controls[ControlBlockManager.Tag.ToString()]?.Content as UserControl;
            //if (blk is IControlBlockControl)
            //(blk as IControlBlockControl).UpdateBindingSource();
            //else if (blk is IStepControl)
            //(blk as IStepControl).UpdateBindingSource();
            bool currentmodified = false, globalsmodified = false, controlsmodified = false;
            if (ContentControl != null)
            {
                if (updateBinding)
                {
                    if (ContentControl.Content is GlobalEventControl)
                        (ContentControl.Content as GlobalEventControl).UpdateBindingSource();
                    else if (ContentControl.Content is IControlBlockControl)
                        (ContentControl.Content as IControlBlockControl).UpdateBindingSource();
                    else if (ContentControl.Content is IStepControl)
                        (ContentControl.Content as IStepControl).UpdateBindingSource();
                }
                currentmodified = ((ContentControl.Content as UserControl)?.DataContext as Component)?.Modified == true;

                globalsmodified = (__content_controls[GlobalEventManager.Tag.ToString()]?.DataContext as Component)?.Modified == true;
                controlsmodified = (__content_controls[ControlBlockManager.Tag.ToString()]?.DataContext as Component)?.Modified == true;
                return (currentmodified || globalsmodified || controlsmodified);
            }
            return false;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            { 
                __content_controls[(e.RemovedItems[0] as TabItem).Tag.ToString()] = ContentControl.Content as UserControl;
                ContentControl.Content = __content_controls[(e.AddedItems[0] as TabItem).Tag.ToString()];
            }
        }

        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            About about = new About(__settings);
            about.ShowDialog();
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (__unapplied_changes(true) || __document_model.UnsavedChanges)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            __rcp_document = new RecipeDocument();
            var tags = new Dictionary<uint, ProcessData>();

            if (MessageBox.Show("Do you want to create labels from an IO List file ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                System.Windows.Forms.OpenFileDialog labels = new System.Windows.Forms.OpenFileDialog();
                labels.Filter = "Foliage Ocean List File(*.folst)|*.folst";
                labels.Multiselect = false;

                if (labels.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        (var _, var _, var _, var _, var txbit, var txblk, var _, var rxbit, var rxblk, var _, var _) = IOCelcetaHelper.Load(labels.FileName, Settings.DataTypeCatalogue, Settings.ControllerModelCatalogue, out _);
                        foreach (var p in txbit.ProcessDatas.Concat(txblk.ProcessDatas).Concat(rxbit.ProcessDatas).Concat(rxblk.ProcessDatas))
                            tags[p.ProcessObject.Index] = p;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            ContextModel.Tags = tags;

            __document_model.GlobalEventManager = new GlobalEventModelCollection(__rcp_document);
            __document_model.ControlBlockManager = new ControlBlockModelCollection(__rcp_document);

            ContextManager.Content = new ContextManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);
            GlobalEventManager.Content = new GlobalEventManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);
            ControlBlockManager.Content = new ControlBlockManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);

            __document_model.FileOpened = string.Empty;
            __reset_layout();
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(__unapplied_changes(true) || __document_model.UnsavedChanges)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Napishtim Source File(*.npstsrc)|*.npstsrc";
            open.Multiselect = false;
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    __rcp_document = new RecipeDocument(open.FileName);  
                }
                catch(Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var tags = new Dictionary<uint, ProcessData>();

                if (MessageBox.Show("Do you want to create labels from an IO List file ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    System.Windows.Forms.OpenFileDialog labels = new System.Windows.Forms.OpenFileDialog();
                    labels.Filter = "Foliage Ocean List File(*.folst)|*.folst";
                    labels.Multiselect = false;

                    if (labels.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            (var _, var _, var _, var _, var txbit, var txblk, var _, var rxbit, var rxblk, var _, var _) = IOCelcetaHelper.Load(labels.FileName, Settings.DataTypeCatalogue, Settings.ControllerModelCatalogue, out _);
                            foreach (var p in txbit.ProcessDatas.Concat(txblk.ProcessDatas).Concat(rxbit.ProcessDatas).Concat(rxblk.ProcessDatas))
                                tags[p.ProcessObject.Index] = p;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

                ContextModel.Tags = tags;

                __document_model.GlobalEventManager = new GlobalEventModelCollection(__rcp_document);
                __document_model.ControlBlockManager = new ControlBlockModelCollection(__rcp_document);

                ContextManager.Content = new ContextManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);
                GlobalEventManager.Content = new GlobalEventManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);
                ControlBlockManager.Content = new ControlBlockManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);

                __document_model.FileOpened = open.FileName;

                __reset_layout();
            }
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                __rcp_document.SaveAs(__document_model.FileOpened, __document_model.GlobalEventManager.Events.Select(e => e.Index));
                __document_model.GlobalEventManager.IsDirty = false;
                __document_model.ControlBlockManager.IsDirty = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __unapplied_changes(false) == false && __document_model != null && __document_model.UnsavedChanges == true && __document_model.IsTemporaryFile == false;
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog();
            save.Filter = "Napishtim Source File(*.npstsrc)|*.npstsrc";
            save.AddExtension = true;
            save.DefaultExt = "npstsrc";
            if (__document_model.IsTemporaryFile == false)
            {
                save.FileName = System.IO.Path.GetFileName(__document_model.FileOpened);
                save.InitialDirectory = System.IO.Path.GetDirectoryName(__document_model.FileOpened);
            }
            else
            {
                save.FileName = "recipe.npstsrc";
                save.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    __rcp_document.SaveAs(save.FileName, __document_model.GlobalEventManager.Events.Select(e => e.Index));
                    __document_model.FileOpened = save.FileName;
                    __document_model.GlobalEventManager.IsDirty = false;
                    __document_model.ControlBlockManager.IsDirty = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __unapplied_changes(false) == false && __document_model != null && __document_model.IsOpened;
        }

        private void ApplyChangesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var content = (ContentControl.Content as UserControl).DataContext;
                if (content is Component)
                    (content as Component).ApplyChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyChangesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((ContentControl?.Content as UserControl)?.DataContext as Component)?.Modified == true;
        }

        private void DiscardChangesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var content = (ContentControl.Content as UserControl).DataContext;
            if (content is Component)
                (content as Component).DiscardChanges();
        }

        private void DiscardChangesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((ContentControl?.Content as UserControl)?.DataContext as Component)?.Modified == true;
        }

        private void SummaryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(GlobalEventManager.IsSelected)
            {
                if (__document_model.GlobalEventManager != null)
                {
                    SummaryViewer summaryViewer = new SummaryViewer(__document_model.GlobalEventManager.Summary) { Owner = this };
                    summaryViewer.ShowDialog();
                }
            }
            else if (ControlBlockManager.IsSelected)
            {
                if (__document_model.ControlBlockManager != null)
                {
                    SummaryViewer summaryViewer = new SummaryViewer(__document_model.ControlBlockManager.Summary) { Owner = this };
                    summaryViewer.ShowDialog();
                }
            }
        }

        private void SummaryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (GlobalEventManager.IsSelected)
            {
                e.CanExecute = __document_model.GlobalEventManager?.Events.All(x => x.Modified == false) == true;
            }
            else if(ControlBlockManager.IsSelected)
            {
                e.CanExecute = (ContentControl.Content as UserControl) == null || ((ContentControl.Content as UserControl).DataContext as Component)?.Modified == false;
            }
            else
                e.CanExecute = false;
        }

        private void BuildCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                __rcp_document.BuildSteps();
                ScriptViewer scriptViewer = new ScriptViewer(__settings.ILinkProperty, __rcp_document.Globals, __rcp_document.CompiledControlSteps) { Owner = this };
                scriptViewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                __document_model.AddOutput(ex.ToString());
            }
        }

        private void BuildCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = __unapplied_changes(false) == false && __document_model?.FileSaved == true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (__unapplied_changes(true) || __document_model.UnsavedChanges)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        private void RecentlyOpened_Click(object sender, RoutedEventArgs e)
        {
            Syncfusion.Windows.Shared.MenuItemAdv menu = e.OriginalSource as Syncfusion.Windows.Shared.MenuItemAdv;
            if (RecentlyOpenedMenu == e.OriginalSource || __document_model.RecentlyOpenedFiles.Count() == 0)
                return;

            if (__unapplied_changes(true) || __document_model.UnsavedChanges)
            {
                var res = MessageBox.Show("Discard the changes you have made ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.No)
                    return;
            }

            string path = menu.Header.ToString();
            try
            {
                __rcp_document = new RecipeDocument(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var tags = new Dictionary<uint, ProcessData>();

            if (MessageBox.Show("Do you want to create labels from an IO List file ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                System.Windows.Forms.OpenFileDialog labels = new System.Windows.Forms.OpenFileDialog();
                labels.Filter = "Foliage Ocean List File(*.folst)|*.folst";
                labels.Multiselect = false;

                if (labels.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        (var _, var _, var _, var _, var txbit, var txblk, var _, var rxbit, var rxblk, var _, var _) = IOCelcetaHelper.Load(labels.FileName, Settings.DataTypeCatalogue, Settings.ControllerModelCatalogue, out _);
                        foreach (var p in txbit.ProcessDatas.Concat(txblk.ProcessDatas).Concat(rxbit.ProcessDatas).Concat(rxblk.ProcessDatas))
                            tags[p.ProcessObject.Index] = p;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            ContextModel.Tags = tags;

            __document_model.GlobalEventManager = new GlobalEventModelCollection(__rcp_document);
            __document_model.ControlBlockManager = new ControlBlockModelCollection(__rcp_document);

            ContextManager.Content = new ContextManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);
            GlobalEventManager.Content = new GlobalEventManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);
            ControlBlockManager.Content = new ControlBlockManager(__document_model.GlobalEventManager, __document_model.ControlBlockManager, ContentControl);

            __document_model.FileOpened = path;

            __reset_layout();
        }

        private void MenuOpenScriptViewer_Click(object sender, RoutedEventArgs e)
        {
            ScriptViewer viewer = new ScriptViewer(__settings.ILinkProperty);
            viewer.ShowDialog();
        }
    }
}