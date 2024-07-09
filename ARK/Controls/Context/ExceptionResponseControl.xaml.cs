using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// ExceptionResponseControl.xaml 的交互逻辑
    /// </summary>
    public partial class ExceptionResponseControl : UserControl
    {
        public ExceptionResponseControl(ExceptionResponseModel exception)
        {
            InitializeComponent();
            DataContext = exception;

            sfGridBranches.RowDragDropController.Dropped += OnSfGridBranches_Dropped;
            sfGridBranches.RowDragDropController.Drop += OnSfGridBranches_Drop;

            LocalEventCollection.DataSource = (exception as SimpleExceptionResponseModel).LocalEvents;
        }

        private void OnSfGridBranches_Drop(object? sender, GridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
                e.Handled = true;
        }

        private void OnSfGridBranches_Dropped(object? sender, GridRowDroppedEventArgs e)
        {
            if (e.DropPosition != DropPosition.None)
            {
                ObservableCollection<object> draggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;
                int dragIndex = (DataContext as SimpleExceptionResponseModel).Branches.IndexOf(draggingRecords[0] as SimpleExceptionResponseBranchModel);
                int targetIndex = (int)e.TargetRecord;

                int insertionIndex = 0;
                if (dragIndex > targetIndex)
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex : targetIndex + 1;
                else
                    insertionIndex = e.DropPosition == DropPosition.DropAbove ? targetIndex - 1 : targetIndex;

                sfGridBranches.View.BeginInit();
                try
                {
                    if (dragIndex != insertionIndex)
                        (DataContext as SimpleExceptionResponseModel).MoveBranch(dragIndex, insertionIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An exception has occurred while adjusting the priorities of branches:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    sfGridBranches.View.EndInit();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }


        private void AddBranchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter == (DataContext as SimpleExceptionResponseModel).Branches)
                {
                    (DataContext as SimpleExceptionResponseModel).AddBranch();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void AddBranchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void InsertBranchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter == (DataContext as SimpleExceptionResponseModel).Branches)
                {
                    (DataContext as SimpleExceptionResponseModel).InsertBranch(sfGridBranches.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void InsertBranchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridBranches.SelectedItems.Count == 1;
        }

        private void RemoveBranchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter == (DataContext as SimpleExceptionResponseModel).Branches)
                {
                    if (MessageBox.Show($"Are you sure you want to remove the following branch:\n{(sfGridBranches.SelectedItem as SimpleExceptionResponseBranchModel).Name}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    (DataContext as SimpleExceptionResponseModel).RemoveBranchAt(sfGridBranches.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception has occurred during operation:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void RemoveBranchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sfGridBranches.SelectedItems.Count == 1;
        }
    }
}
