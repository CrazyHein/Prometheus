﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using Syncfusion.UI.Xaml.Grid;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// InterlockViewer.xaml 的交互逻辑
    /// </summary>
    public partial class InterlockCollectionViewer : UserControl
    {
        public InterlockCollectionViewer(InterlockCollection ic, ObjectDictionary od, ProcessDataImage txbit, ProcessDataImage rxbit, SfDataGrid source)
        {
            InitializeComponent();
            DataContext = new InterlockCollectionModel(ic, od, txbit, rxbit, (source.DataContext as ObjectsModel));
        }

        private InputDialogDisplayMode __display_mode;

        private void EditRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList?.SelectedItem != null && (DataContext as InterlockCollectionModel).IsOffline == true;
        }

        private void EditRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InterlockLogicContainer.IsEnabled = false;
            InterlockLogic logic = (InterlockLogicList.SelectedItem as InterlockLogicModel).Logic;
            InputInterlockLogicName.Text = logic.Name;
            StringBuilder sb = new StringBuilder();
            foreach(var t in logic.Targets)
            {
                sb.Append("0x").Append(t.ProcessObject.Index.ToString("X08"));
                sb.Append("\r\n");
            }
            InputInterlockLogicTargets.Text = sb.ToString().TrimEnd();
            InputInterlockLogicStatement.Text = logic.Statement.Serialize();
            InputArea.IsEnabled = true;
            __display_mode = InputDialogDisplayMode.Edit;
        }

        private void AddRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList != null && (DataContext as InterlockCollectionModel).IsOffline == true;
        }

        private void AddRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InterlockLogicContainer.IsEnabled = false;
            InputInterlockLogicName.Text = String.Empty;
            InputInterlockLogicTargets.Text = String.Empty;
            InputInterlockLogicStatement.Text = String.Empty;
            InputArea.IsEnabled = true;
            __display_mode = InputDialogDisplayMode.Add;
        }

        private void InsertRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList?.SelectedItem != null && (DataContext as InterlockCollectionModel).IsOffline == true;
        }

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InterlockLogicContainer.IsEnabled = false;
            InputInterlockLogicName.Text = String.Empty;
            InputInterlockLogicTargets.Text = String.Empty;
            InputInterlockLogicStatement.Text = String.Empty;
            InputArea.IsEnabled = true;
            __display_mode = InputDialogDisplayMode.Insert;
        }

        private void RemoveRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList?.SelectedItem != null && (DataContext as InterlockCollectionModel).IsOffline == true;
        }

        private void RemoveRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string record = (InterlockLogicList.SelectedItem as InterlockLogicModel).Logic.Name;
            if (MessageBox.Show("Are you sure you want to remove the record :\n" + record, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    (DataContext as InterlockCollectionModel).Remove(InterlockLogicList.SelectedIndex);
                }
                catch (LombardiaException ex)
                {
                    MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InterlockLogicContainer.IsEnabled = true;
            InputArea.IsEnabled = false;
        }

        private void ConfirmCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                switch (__display_mode)
                {
                    case InputDialogDisplayMode.Edit:
                        (DataContext as InterlockCollectionModel).Replace(InterlockLogicList.SelectedIndex,
                            InputInterlockLogicName.Text, InputInterlockLogicTargets.Text, InputInterlockLogicStatement.Text);
                        break;
                    case InputDialogDisplayMode.Add:
                        (DataContext as InterlockCollectionModel).Add(InputInterlockLogicName.Text, InputInterlockLogicTargets.Text, InputInterlockLogicStatement.Text);
                        break;
                    case InputDialogDisplayMode.Insert:
                        (DataContext as InterlockCollectionModel).Insert(InterlockLogicList.SelectedIndex,
                            InputInterlockLogicName.Text, InputInterlockLogicTargets.Text, InputInterlockLogicStatement.Text);
                        break;
                }
                InterlockLogicContainer.IsEnabled = true;
                InputArea.IsEnabled = false;
            }
            catch (LombardiaException ex)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}