using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Step;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.ShaderMechansim;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Shared;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls
{
    /// <summary>
    /// StepControl.xaml 的交互逻辑
    /// </summary>
    public partial class StepControl : UserControl, IStepControl
    {
        //public StepModel OriginalStepModel { get; private set; }
        //private SequentialModel __container;
        public StepControl(StepModel step)
        {
            InitializeComponent();
            //OriginalStepModel = step;
            //__container = step.Container;
            DataContext = step;
        }

        public void ResetDataModel(StepModel step)
        {
            //OriginalStepModel = step;
            //__container = step.Container;
            DataContext = step;
        }

        public void UpdateBindingSource()
        {
            var binding = txtStepName.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();

            binding = txtTerminationCondition.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();

            binding = txtStepTimeout.GetBindingExpression(IntegerTextBox.ValueProperty);
            binding.UpdateSource();
        }

        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if(e.Parameter == (DataContext as SimpleStepModel).LocalEvents)
                {
                    (DataContext as SimpleStepModel).AddLocalEvent();
                }
                else if(e.Parameter == (DataContext as SimpleStepModel).Shaders)
                {
                    (DataContext as SimpleStepModel).AddStepAction();
                }
                else if (e.Parameter == (DataContext as SimpleStepModel).PostShaders)
                {
                    (DataContext as SimpleStepModel).AddPostStepAction();
                }
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
                if (e.Parameter == (DataContext as SimpleStepModel).LocalEvents)
                {
                    (DataContext as SimpleStepModel).InsertLocalEvent(sfLocalEventsViewer.SelectedIndex);
                }
                else if (e.Parameter == (DataContext as SimpleStepModel).Shaders)
                {
                    (DataContext as SimpleStepModel).InsertStepAction(sfStepActionsViewer.SelectedIndex);
                }
                else if (e.Parameter == (DataContext as SimpleStepModel).PostShaders)
                {
                    (DataContext as SimpleStepModel).InsertPostStepAction(sfPostStepActionsViewer.SelectedIndex);
                }
                else if (e.Parameter is GridRecordContextMenuInfo)
                {
                    object dataContext = (e.Parameter as GridRecordContextMenuInfo).DataGrid.ItemsSource;
                    if (dataContext == (DataContext as SimpleStepModel).LocalEvents)
                        (DataContext as SimpleStepModel).InsertLocalEvent(sfLocalEventsViewer.SelectedIndex);
                    else if(dataContext == (DataContext as SimpleStepModel).Shaders)
                        (DataContext as SimpleStepModel).InsertStepAction(sfStepActionsViewer.SelectedIndex);
                    else if (dataContext == (DataContext as SimpleStepModel).PostShaders)
                        (DataContext as SimpleStepModel).InsertPostStepAction(sfPostStepActionsViewer.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void InsertCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DataContext == null)
                e.CanExecute = false;
            else if (e.Parameter == (DataContext as SimpleStepModel).LocalEvents)
                e.CanExecute = sfLocalEventsViewer.SelectedItems?.Count() == 1;
            else if (e.Parameter == (DataContext as SimpleStepModel).Shaders)
                e.CanExecute = sfStepActionsViewer.SelectedItems?.Count() == 1;
            else if (e.Parameter == (DataContext as SimpleStepModel).PostShaders)
                e.CanExecute = sfPostStepActionsViewer.SelectedItems?.Count() == 1;
            else if (e.Parameter is GridRecordContextMenuInfo)
                e.CanExecute = (e.Parameter as GridRecordContextMenuInfo).DataGrid.SelectedItems?.Count() == 1;
        }

        private string __selected_itmes(object e)
        {
            string prompt = null;
            if (e == (DataContext as SimpleStepModel).LocalEvents)
            {
                if (sfLocalEventsViewer.SelectedItems.Count() == 1)
                    prompt = (sfLocalEventsViewer.SelectedItem as LocalEventModel).Summary;
                else if (sfLocalEventsViewer.SelectedItems.Count() <= 5)
                    prompt = String.Join("\n", sfLocalEventsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).LocalEvents.IndexOf(x)).Select(x => (x as LocalEventModel).Name));
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var s in sfLocalEventsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).LocalEvents.IndexOf(x)).Take(3))
                        sb.AppendLine((s as LocalEventModel).Name);
                    sb.AppendLine("...");
                    sb.AppendLine((sfLocalEventsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).LocalEvents.IndexOf(x)).Last() as LocalEventModel).Name);
                    prompt = sb.ToString();
                }
            }
            else if (e == (DataContext as SimpleStepModel).Shaders)
            {
                if (sfStepActionsViewer.SelectedItems.Count() == 1)
                    prompt = (sfStepActionsViewer.SelectedItem as ShaderModel).Summary;
                else if (sfStepActionsViewer.SelectedItems.Count() <= 5)
                    prompt = String.Join("\n", sfStepActionsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).Shaders.IndexOf(x)).Select(x => (x as ShaderModel).Name));
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var s in sfStepActionsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).Shaders.IndexOf(x)).Take(3))
                        sb.AppendLine((s as ShaderModel).Name);
                    sb.AppendLine("...");
                    sb.AppendLine((sfStepActionsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).Shaders.IndexOf(x)).Last() as ShaderModel).Name);
                    prompt = sb.ToString();
                }
            }
            else if (e == (DataContext as SimpleStepModel).PostShaders)
            {
                if (sfPostStepActionsViewer.SelectedItems.Count() == 1)
                    prompt = (sfPostStepActionsViewer.SelectedItem as ShaderModel).Summary;
                else if (sfPostStepActionsViewer.SelectedItems.Count() <= 5)
                    prompt = String.Join("\n", sfPostStepActionsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).Shaders.IndexOf(x)).Select(x => (x as ShaderModel).Name));
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var s in sfPostStepActionsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).Shaders.IndexOf(x)).Take(3))
                        sb.AppendLine((s as ShaderModel).Name);
                    sb.AppendLine("...");
                    sb.AppendLine((sfPostStepActionsViewer.SelectedItems.OrderBy(x => (DataContext as SimpleStepModel).Shaders.IndexOf(x)).Last() as ShaderModel).Name);
                    prompt = sb.ToString();
                }
            }
            else
                prompt = string.Empty;
            return prompt;
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                object itemsource = e.Parameter is GridRecordContextMenuInfo ? (e.Parameter as GridRecordContextMenuInfo).DataGrid.ItemsSource : e.Parameter;
                if (MessageBox.Show($"Are you sure you want to remove the following item(s):\n{__selected_itmes(itemsource)}", 
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question)== MessageBoxResult.No)
                    return;

                if (itemsource == (DataContext as SimpleStepModel).LocalEvents)
                {
                    (DataContext as SimpleStepModel).RemoveLocalEvents(sfLocalEventsViewer.SelectedItems.Cast<LocalEventModel>());
                }
                else if (itemsource == (DataContext as SimpleStepModel).Shaders)
                {
                    (DataContext as SimpleStepModel).RemoveStepActions(sfStepActionsViewer.SelectedItems.Cast<ShaderModel>());
                }
                else if (itemsource == (DataContext as SimpleStepModel).PostShaders)
                {
                    (DataContext as SimpleStepModel).RemovePostStepActions(sfPostStepActionsViewer.SelectedItems.Cast<ShaderModel>());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DataContext == null)
                e.CanExecute = false;
            else if (e.Parameter == (DataContext as SimpleStepModel).LocalEvents)
                e.CanExecute = sfLocalEventsViewer.SelectedItem != null;
            else if (e.Parameter == (DataContext as SimpleStepModel).Shaders)
                e.CanExecute = sfStepActionsViewer.SelectedItem != null;
            else if (e.Parameter == (DataContext as SimpleStepModel).PostShaders)
                e.CanExecute = sfPostStepActionsViewer.SelectedItem != null;
            else if (e.Parameter is GridRecordContextMenuInfo)
                e.CanExecute = (e.Parameter as GridRecordContextMenuInfo).DataGrid.SelectedItem != null;
        }

        private void ExpressionHelperCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((e.Parameter as GridRecordContextMenuInfo).Record is EventParameter)
            {
                var parameter = (e.Parameter as GridRecordContextMenuInfo).Record as EventParameter;
                ExpressionHelper helper = new ExpressionHelper(parameter.Value);
                var ret = helper.ShowDialog();
                if (ret == true)
                {
                    parameter.Value = helper.Expression.ToString();
                }
            }
            else if((e.Parameter as GridRecordContextMenuInfo).Record is ShaderModel)
            {
                var shader = (e.Parameter as GridRecordContextMenuInfo).Record as ShaderModel;
                string header = (e.Parameter as GridRecordContextMenuInfo).DataGrid.CurrentColumn.HeaderText;
                string expr;
                if (header == "Operand")
                    expr = shader.LeftValue;
                else
                    expr = shader.RightValue;
                ExpressionHelper helper = new ExpressionHelper(expr);
                if (helper.ShowDialog() == true)
                {
                    if (header == "Operand")
                        shader.LeftValue = helper.Expression.ToString();
                    else
                        shader.RightValue = helper.Expression.ToString();
                }
            }
            e.Handled = true;
        }

        private void ExpressionHelperCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SfDataGrid dataGrid = (e.Parameter as GridRecordContextMenuInfo).DataGrid;
            string header = dataGrid.CurrentColumn.HeaderText;

            e.CanExecute = dataGrid.SelectedItems?.Count() == 1 && (header == "Value" || header == "Operand" || header == "Expression");
        }

        private void PasteBeforeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if ((e.Parameter as GridRecordContextMenuInfo).DataGrid.ItemsSource == (DataContext as SimpleStepModel).Shaders)
                {
                    var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
                    if (components != null)
                        (DataContext as SimpleStepModel).InsertStepActions((e.Parameter as GridRecordContextMenuInfo).DataGrid.SelectedIndex, components);
                }
                else if ((e.Parameter as GridRecordContextMenuInfo).DataGrid.ItemsSource == (DataContext as SimpleStepModel).PostShaders)
                {
                    var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
                    if (components != null)
                        (DataContext as SimpleStepModel).InsertPostStepActions((e.Parameter as GridRecordContextMenuInfo).DataGrid.SelectedIndex, components);
                }
                else if ((e.Parameter as GridRecordContextMenuInfo).DataGrid.ItemsSource == (DataContext as SimpleStepModel).LocalEvents)
                {
                    var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
                    if (components != null)
                        (DataContext as SimpleStepModel).InsertLocalEvents((e.Parameter as GridRecordContextMenuInfo).DataGrid.SelectedIndex, components);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteBeforeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var components = Component.COMPONENT_ARRAY_IN_CLIPBOARD();
            if (components != null)
            { 
                if((e.Parameter as GridRecordContextMenuInfo).DataGrid.ItemsSource is IEnumerable<ShaderModel> && components.Value.type == typeof(ShaderModel))
                    e.CanExecute = (e.Parameter as GridRecordContextMenuInfo).DataGrid.SelectedItems?.Count() == 1;
                else if ((e.Parameter as GridRecordContextMenuInfo).DataGrid.ItemsSource is IEnumerable<LocalEventModel> && components.Value.type == typeof(LocalEventModel))
                    e.CanExecute = (e.Parameter as GridRecordContextMenuInfo).DataGrid.SelectedItems?.Count() == 1;
            }
            else
                e.CanExecute = false;
        }

        private void PropagateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to clear the step actions of all subsequent steps and overwrite the step actions of subsequent steps with the step actions of this step ?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            var step = DataContext as SimpleStepModel;
            int pos = (step.Owner as SequentialModel).SubSteps.IndexOf(step);
            JsonArray o = new JsonArray();
            foreach (var s in (e.Parameter as IEnumerable<ShaderModel>))
                o.Add(s.ToJson());
            foreach (var s in (step.Owner as SequentialModel).SubSteps.Cast<SimpleStepModel>().TakeLast((step.Owner as SequentialModel).SubSteps.Count() - pos - 1))
            {
                if (e.Parameter == (DataContext as SimpleStepModel).Shaders)
                {
                    s.ClearStepAction();
                    s.AddStepActions(o);
                }
                else
                {
                    s.ClearPostStepAction();
                    s.AddPostStepActions(o);
                }
                s.ApplyChanges();
            }
        }

        private void PropagateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext as SimpleStepModel)?.Modified == false;
        }

        private void CopyShader(object sender, GridCopyPasteEventArgs e)
        {
            try
            {
                SfDataGrid dataGrid = e.OriginalSender as SfDataGrid;
                IEnumerable<ShaderModel> shaders = (e.OriginalSender as SfDataGrid).ItemsSource as IEnumerable<ShaderModel>;
                JsonArray o = new JsonArray();
                foreach (var s in dataGrid.SelectedItems.OrderBy(x => shaders.IndexOf(x)).Cast<ShaderModel>())
                    o.Add(s.ToJson());
                Clipboard.SetDataObject(o.ToJsonString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void CopyLocalEvent(object sender, GridCopyPasteEventArgs e)
        {
            try
            {
                SfDataGrid dataGrid = e.OriginalSender as SfDataGrid;
                IEnumerable<LocalEventModel> shaders = (e.OriginalSender as SfDataGrid).ItemsSource as IEnumerable<LocalEventModel>;
                JsonArray o = new JsonArray();
                foreach (var s in dataGrid.SelectedItems.OrderBy(x => shaders.IndexOf(x)).Cast<LocalEventModel>())
                    o.Add(s.ToJson());
                Clipboard.SetDataObject(o.ToJsonString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteLocalEventCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
                if (components != null)
                    (DataContext as SimpleStepModel).AddLocalEvents(components);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteLocalEventCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var components = Component.COMPONENT_ARRAY_IN_CLIPBOARD();
            if (components != null && components.Value.type == typeof(LocalEventModel))
                e.CanExecute = true;
        }

        private void PasteShaderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
                if (components != null)
                    (DataContext as SimpleStepModel).AddStepActions(components);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PasteShaderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var components = Component.COMPONENT_ARRAY_IN_CLIPBOARD();
            if (components != null && components.Value.type == typeof(ShaderModel))
                e.CanExecute = true;
        }

        private void PastePostShaderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var components = JsonNode.Parse(Clipboard.GetText()).AsArray();
                if (components != null)
                    (DataContext as SimpleStepModel).AddPostStepActions(components);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void PastePostShaderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var components = Component.COMPONENT_ARRAY_IN_CLIPBOARD();
            if (components != null && components.Value.type == typeof(ShaderModel))
                e.CanExecute = true;
        }
    }
}
