using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren.Utility
{
    /// <summary>
    /// ImportExport.xaml 的交互逻辑
    /// </summary>
    partial class ImportExport : Window
    {
        public ImportExport(ImportExportMode mode,
            VariableDictionary vd, IEnumerable<string> variableNames,
            ControllerConfiguration cc, IEnumerable<string> configurationNames,
            ObjectDictionary od, IEnumerable<uint> objectIndexes,
            ProcessDataImage txdiag, ProcessDataImage txbit, ProcessDataImage txblk,
            ProcessDataImage rxctl, ProcessDataImage rxbit, ProcessDataImage rxblk,
            InterlockCollection intlk, Miscellaneous misc,
            DataTypeCatalogue dataTypes, ControllerModelCatalogue models)
        {
            InitializeComponent();
            DataContext = new ImportExportModel(mode, 
                vd, variableNames, 
                cc, configurationNames,
                od, objectIndexes,
                txdiag, txbit, txblk, rxctl, rxbit, rxblk, intlk, misc,
                dataTypes, models);
            if (mode == ImportExportMode.Import)
            {
                CheckboxXLS.IsChecked = false;
                CheckboxXLS.IsEnabled = false;
                CheckboxXML.IsChecked = true;
                CheckboxXML.IsEnabled = false;
                CheckboxXMLVAR.IsChecked = true;
                CheckboxXMLVAR.IsEnabled = false;
                CheckboxXMLIO.IsChecked = true;
            }
        }

        public (VariableDictionary, ControllerConfiguration, ObjectDictionary,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage,
                    ProcessDataImage, ProcessDataImage, ProcessDataImage, InterlockCollection,
                    Miscellaneous) ImportResult
        {
            get; private set;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void OK_Click(object sender, RoutedEventArgs e)
        {
            ImportExportModel model = DataContext as ImportExportModel;
            switch (model.Mode)
            {
                case ImportExportMode.Export:
                    string msg = string.Empty;
                    if (model.XML)
                    {
                        if(model.XMLVAR)
                            msg += System.IO.File.Exists(model.VariableDictionaryPath) == true ? "Variable Dictionary :\n" + model.VariableDictionaryPath : String.Empty;
                        if (model.XMLIO && System.IO.File.Exists(model.IOListPath))
                        {
                            if (msg == String.Empty)
                                msg += "IO List :\n" + model.IOListPath;
                            else
                                msg += "\nIO List :\n" + model.IOListPath;
                        }
                    }
                    if (model.XLS)
                    {
                        if (System.IO.File.Exists(model.XlsArchivesPath))
                        {
                            if (msg == String.Empty)
                                msg += "Xls Archives :\n" + model.XlsArchivesPath;
                            else
                                msg += "\nXls Archives :\n" + model.XlsArchivesPath;
                        }
                    }

                    if (msg != String.Empty)
                    if (MessageBox.Show("The following file(s) already exist(s).\n\n" + msg + "\n\nDo you want to overwrite existing file(s)?",
                        "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;

                    model.IsBusy = true;
                    try
                    {
                        await Task.Run(() => model.Export());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    model.IsBusy = false;
                    break;
                case ImportExportMode.Import:
                    model.IsBusy = true;
                    try
                    {
                        await Task.Run(() => ImportResult = model.Import());
                        model.IsBusy = false;
                        DialogResult = true;
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        model.IsBusy = false;
                    }
                    break;
            } 
        }

        private void BrowseVariableDictionary_Click(object sender, RoutedEventArgs e)
        {
            ImportExportModel model = DataContext as ImportExportModel;
            
            switch (model.Mode)
            {
                case ImportExportMode.Export:
                    System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog() { DefaultExt = "xml", AddExtension = true};
                    save.InitialDirectory = System.IO.Path.GetDirectoryName(model.VariableDictionaryPath);
                    save.Filter = "Extensible Markup Language(*.xml)|*.xml";
                    if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        model.VariableDictionaryPath = save.FileName;
                    break;
                case ImportExportMode.Import:
                    System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog() { Multiselect = false};
                    open.InitialDirectory = System.IO.Path.GetDirectoryName(model.VariableDictionaryPath);
                    open.Filter = "Extensible Markup Language(*.xml)|*.xml";
                    if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        model.VariableDictionaryPath = open.FileName;
                    break;
            }
        }

        private void BrowseIOList_Click(object sender, RoutedEventArgs e)
        {
            ImportExportModel model = DataContext as ImportExportModel;

            switch (model.Mode)
            {
                case ImportExportMode.Export:
                    System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog() { DefaultExt = "xml", AddExtension = true };
                    save.InitialDirectory = System.IO.Path.GetDirectoryName(model.IOListPath);
                    save.Filter = "Extensible Markup Language(*.xml)|*.xml";
                    if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        model.IOListPath = save.FileName;
                    break;
                case ImportExportMode.Import:
                    System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog() { Multiselect = false };
                    open.InitialDirectory = System.IO.Path.GetDirectoryName(model.IOListPath);
                    open.Filter = "Extensible Markup Language(*.xml)|*.xml";
                    if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        model.IOListPath = open.FileName;
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if((DataContext as ImportExportModel).IsBusy == true)
                e.Cancel = true;
        }

        private void BrowseXlsArchives_Click(object sender, RoutedEventArgs e)
        {
            ImportExportModel model = DataContext as ImportExportModel;
            switch(model.Mode)
            {
                case ImportExportMode.Export:
                    System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog() { DefaultExt = "xlsx", AddExtension = true };
                    save.InitialDirectory = System.IO.Path.GetDirectoryName(model.IOListPath);
                    save.Filter = "Microsoft Office Excel 2007+ (*.xlsx)|*.xlsx";
                    if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        model.XlsArchivesPath = save.FileName;
                    break;
            }
        }
    }
}
