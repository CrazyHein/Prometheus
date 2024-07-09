using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    /// <summary>
    /// LocalEventCollectionControl.xaml 的交互逻辑
    /// </summary>
    public partial class LocalEventCollectionControl : UserControl
    {
        public LocalEventModelCollection? DataSource
        { 
            get
            {
                return DataContext as LocalEventModelCollection;
            }
            set 
            {
                DataContext = value;
                sfLocalEventsViewer.ItemsSource = value.Events; 
            } 
        }
        public LocalEventCollectionControl()
        {
            InitializeComponent();
        }

        private string selected_items()
        {
            string prompt = null;
            if (sfLocalEventsViewer.SelectedItems.Count() == 1)
                prompt = (sfLocalEventsViewer.SelectedItem as LocalEventModel).Summary;
            else if (sfLocalEventsViewer.SelectedItems.Count() <= 5)
                prompt = String.Join("\n", sfLocalEventsViewer.SelectedItems.OrderBy(x => DataSource.Events.IndexOf(x)).Select(x => (x as LocalEventModel).Name));
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var s in sfLocalEventsViewer.SelectedItems.OrderBy(x => DataSource.Events.IndexOf(x)).Take(3))
                    sb.AppendLine((s as LocalEventModel).Name);
                sb.AppendLine("...");
                sb.AppendLine((sfLocalEventsViewer.SelectedItems.OrderBy(x => DataSource.Events.IndexOf(x)).Last() as LocalEventModel).Name);
                prompt = sb.ToString();
            }

            return prompt;
        }
        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DataSource.Add(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void AddCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void InsertCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try 
            { 
                DataSource.Insert(sfLocalEventsViewer.SelectedIndex, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void InsertCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfLocalEventsViewer.SelectedItems.Count == 1;
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show($"Are you sure you want to remove the following item(s):\n{selected_items()}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                DataSource.Remove(sfLocalEventsViewer.SelectedItems.Cast<LocalEventModel>(), true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfLocalEventsViewer.SelectedItem != null;
        }

        private void ExpressionHelperCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var parameter = (e.Parameter as GridRecordContextMenuInfo).Record as EventParameter;
            ExpressionHelper helper = new ExpressionHelper(parameter.Value);
            var ret = helper.ShowDialog();
            if (ret == true)
            {
                parameter.Value = helper.Expression.ToString();
            }
        }

        private void ExpressionHelperCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SfDataGrid dataGrid = (e.Parameter as GridRecordContextMenuInfo).DataGrid;
            string header = dataGrid.CurrentColumn.HeaderText;
            e.CanExecute = dataGrid.SelectedItems?.Count() == 1 && (header == "Value");
        }

        private void PasteBeforeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
            if (components != null)
                DataSource.Insert(sfLocalEventsViewer.SelectedIndex, components, true);
        }

        private void PasteBeforeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var components = Component.COMPONENT_ARRAY_IN_CLIPBOARD();
            if(components != null && components.Value.type == typeof(LocalEventModel))
                e.CanExecute = sfLocalEventsViewer.SelectedItems?.Count() == 1;
        }

        private void CopyLocalEvent(object sender, GridCopyPasteEventArgs e)
        {
            try
            {
                SfDataGrid dataGrid = e.OriginalSender as SfDataGrid;
                IEnumerable<LocalEventModel> locals = (e.OriginalSender as SfDataGrid).ItemsSource as IEnumerable<LocalEventModel>;
                JsonArray o = new JsonArray();
                foreach (var s in dataGrid.SelectedItems.OrderBy(x => locals.IndexOf(x)).Cast<LocalEventModel>())
                    o.Add(s.ToJson());
                Clipboard.SetDataObject(o.ToJsonString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
                if (components != null)
                    (DataContext as LocalEventModelCollection).Add(components, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var components = Component.COMPONENT_ARRAY_IN_CLIPBOARD();
            if (components != null && components.Value.type == typeof(LocalEventModel))
                e.CanExecute = true;
        }
    }
}
