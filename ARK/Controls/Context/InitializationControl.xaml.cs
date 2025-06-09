using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim.Initialization;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls
{
    /// <summary>
    /// InitializationControl.xaml 的交互逻辑
    /// </summary>
    public partial class InitializationControl : UserControl
    {
        public InitializationControl(InitializationModel model)
        {
            InitializeComponent();
            DataContext = model;

            sfUserVariableInitialValuesViewer.RowDragDropController.Dropped += OnSfUserVariableInitialValues_Dropped;
            sfUserVariableInitialValuesViewer.RowDragDropController.Drop += OnSfUserVariableInitialValues_Drop;
        }

        private void OnSfUserVariableInitialValues_Drop(object? sender, GridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
                e.Handled = true;
        }

        private void OnSfUserVariableInitialValues_Dropped(object? sender, GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                int dragIndex = (DataContext as InitializationModel).UserVariableInitialValues.IndexOf(draggingRecords[0] as UserVariableInitialValueModel);
                int targetIndex = (int)e.TargetRecord;

                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;

                sfUserVariableInitialValuesViewer.View.BeginInit();
                try
                {
                    if (dragIndex != insertionIndex)
                        (DataContext as InitializationModel).MoveUserVariableInitialValue(dragIndex, insertionIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An exception has occurred while adjusting the order of user variables initial values:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    sfUserVariableInitialValuesViewer.View.EndInit();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter as string == "UserVariableInitialValues")
                {
                    (DataContext as InitializationModel).AddUserVariableInitialValue();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void AddRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as string == "UserVariableInitialValues")
                e.CanExecute = true;
        }

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter as string == "UserVariableInitialValues")
                {
                    (DataContext as InitializationModel).InsertUserVariableInitialValue(sfUserVariableInitialValuesViewer.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void InsertRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as string == "UserVariableInitialValues")
                e.CanExecute = sfUserVariableInitialValuesViewer.SelectedItems.Count == 1;
        }

        private void RemoveRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter as string == "UserVariableInitialValues")
                {
                    if (MessageBox.Show($"Are you sure you want to remove the following branch:\n{(sfUserVariableInitialValuesViewer.SelectedItem as UserVariableInitialValueModel)}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    (DataContext as InitializationModel).RemoveUserVariableInitialValueAt(sfUserVariableInitialValuesViewer.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void RemoveRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as string == "UserVariableInitialValues")
                e.CanExecute = sfUserVariableInitialValuesViewer.SelectedItems.Count == 1;
        }
    }
}
