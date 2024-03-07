using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
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
using System.Xml;

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Seiren
{
    /// <summary>
    /// InterlockViewer.xaml 的交互逻辑
    /// </summary>
    public partial class InterlockCollectionViewer : UserControl
    {
        public InterlockCollectionViewer(InterlockCollection ic, ObjectDictionary od, ProcessDataImage txbit, ProcessDataImage rxbit, SfDataGrid source, OperatingHistory history)
        {
            InitializeComponent();
            DataContext = new InterlockCollectionModel(ic, od, txbit, rxbit, (source.DataContext as ObjectsModel), history);
        }

        private InputDialogDisplayMode __display_mode;
        private uint __attribute;

        private void EditRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList?.SelectedItem != null && (DataContext as InterlockCollectionModel).IsOffline == true;
        }

        private void EditRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InterlockLogicContainer.IsEnabled = false;
            InterlockLogic logic = (InterlockLogicList.SelectedItem as InterlockLogicModel).Logic;
            InputInterlockLogicIsHardware.IsChecked = (InterlockLogicList.SelectedItem as InterlockLogicModel).IsHardware;
            InputInterlockLogicIsExclusive.IsChecked = (InterlockLogicList.SelectedItem as InterlockLogicModel).IsExclusive;
            __attribute = logic.Attr;
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
            var defaultValue = __DefaultInterlockLogicModel;
            InterlockLogicContainer.IsEnabled = false;
            InputInterlockLogicIsHardware.IsChecked = (defaultValue.attr & (uint)InterlockAttribute.Hardware) != 0;
            InputInterlockLogicIsExclusive.IsChecked = (defaultValue.attr & (uint)InterlockAttribute.Exclusive) != 0; ;
            __attribute = defaultValue.attr;
            InputInterlockLogicName.Text = defaultValue.name;
            InputInterlockLogicTargets.Text = defaultValue.target;
            InputInterlockLogicStatement.Text = defaultValue.statement;
            InputArea.IsEnabled = true;
            __display_mode = InputDialogDisplayMode.Add;
        }

        private void InsertRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList?.SelectedItem != null && (DataContext as InterlockCollectionModel).IsOffline == true;
        }

        private void InsertRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var defaultValue = __DefaultInterlockLogicModel;
            InterlockLogicContainer.IsEnabled = false;
            InputInterlockLogicIsHardware.IsChecked = (defaultValue.attr & (uint)InterlockAttribute.Hardware) != 0;
            InputInterlockLogicIsExclusive.IsChecked = (defaultValue.attr & (uint)InterlockAttribute.Exclusive) != 0;
            __attribute = defaultValue.attr; ;
            InputInterlockLogicName.Text = defaultValue.name;
            InputInterlockLogicTargets.Text = defaultValue.target;
            InputInterlockLogicStatement.Text = defaultValue.statement;
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

        private void MoveUpRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList?.SelectedItem != null && (DataContext as InterlockCollectionModel).IsOffline == true && InterlockLogicList.SelectedIndex != 0;
        }

        private void MoveDownRecordCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InterlockLogicList?.SelectedItem != null && (DataContext as InterlockCollectionModel).IsOffline == true && InterlockLogicList.SelectedIndex != InterlockLogicList.Items.Count - 1;
        }

        private void MoveUpRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int src = InterlockLogicList.SelectedIndex;
            (DataContext as InterlockCollectionModel).Move(src, src - 1);
            InterlockLogicList.SelectedIndex = src - 1;
            InterlockLogicList.ScrollIntoView(InterlockLogicList.SelectedItem);
        }
        private void MoveDownRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int src = InterlockLogicList.SelectedIndex;
            (DataContext as InterlockCollectionModel).Move(src, src + 1);
            InterlockLogicList.SelectedIndex = src + 1;
            InterlockLogicList.ScrollIntoView(InterlockLogicList.SelectedItem);
        }

        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InterlockLogicContainer.IsEnabled = true;
            InputArea.IsEnabled = false;
        }

        private void ConfirmCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int index = InterlockLogicList.SelectedIndex;
            if (InputInterlockLogicIsHardware.IsChecked == true)
                __attribute |= (uint)InterlockAttribute.Hardware;
            else
                __attribute &= ~(uint)InterlockAttribute.Hardware;
            if (InputInterlockLogicIsExclusive.IsChecked == true)
                __attribute |= (uint)InterlockAttribute.Exclusive;
            else
                __attribute &= ~(uint)InterlockAttribute.Exclusive;
            try
            {
                switch (__display_mode)
                {
                    case InputDialogDisplayMode.Edit:
                        (DataContext as InterlockCollectionModel).Replace(InterlockLogicList.SelectedIndex, __attribute,
                            InputInterlockLogicName.Text, InputInterlockLogicTargets.Text, InputInterlockLogicStatement.Text);
                        break;
                    case InputDialogDisplayMode.Add:
                        (DataContext as InterlockCollectionModel).Add(__attribute, InputInterlockLogicName.Text, InputInterlockLogicTargets.Text, InputInterlockLogicStatement.Text);
                        index = (DataContext as InterlockCollectionModel).InterlockLogicModels.Count - 1;
                        break;
                    case InputDialogDisplayMode.Insert:
                        (DataContext as InterlockCollectionModel).Insert(InterlockLogicList.SelectedIndex, __attribute,
                            InputInterlockLogicName.Text, InputInterlockLogicTargets.Text, InputInterlockLogicStatement.Text);
                        break;
                }
                InterlockLogicList.SelectedIndex = index;
                InterlockLogicList.ScrollIntoView((DataContext as InterlockCollectionModel).InterlockLogicModels[index]);
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

        private void DefaultRecordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ex = RecordUtility.CopyRecords(new List<InterlockLogicModel>() { InterlockLogicList.SelectedItem as InterlockLogicModel });
            if (ex != null)
            {
                MessageBox.Show("At least one exception has occurred during the operation :\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (bool vailid, uint attr, string name, string target, string statement) __DefaultInterlockLogicModel
        {
            get
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(Clipboard.GetText().Trim());

                        XmlNode rootNode = xmlDoc.SelectSingleNode("/" + typeof(InterlockLogicModel).FullName);
                        if (rootNode.NodeType != XmlNodeType.Element ||
                            rootNode.Attributes["Version"].Value != Settings.SeirenVersion ||
                            rootNode.Attributes["DataType"].Value != Settings.DataTypeCatalogueHashString ||
                            rootNode.Attributes["Controller"].Value != Settings.ControllerModelCatalogueHashString)
                            return (false, 0, string.Empty, string.Empty, string.Empty);
                        XmlNode node  = rootNode.SelectSingleNode(typeof(InterlockLogicModel).Name);
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            uint attr = Convert.ToUInt32(node.SelectSingleNode("Attr").FirstChild.Value, 16);
                            string name = node.SelectSingleNode("Name").FirstChild.Value;
                            string target = node.SelectSingleNode("Target").FirstChild.Value;
                            string statement = node.SelectSingleNode("Statement").FirstChild.Value;
                            return (true, attr, name, target, statement);
                        }
                        return (false, 0, string.Empty, string.Empty, string.Empty);
                    }
                    else
                        return (false, 0, string.Empty, string.Empty, string.Empty);
                }
                catch
                {
                    return (false, 0, string.Empty, string.Empty, string.Empty);
                }
            }
        }

        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            var defaultValue = __DefaultInterlockLogicModel;
            if (defaultValue.vailid)
            {
                InputInterlockLogicIsHardware.IsChecked = (defaultValue.attr & (uint)InterlockAttribute.Hardware) != 0;
                InputInterlockLogicIsExclusive.IsChecked = (defaultValue.attr & (uint)InterlockAttribute.Exclusive) != 0;
                __attribute = defaultValue.attr; ;
                InputInterlockLogicName.Text = defaultValue.name;
                InputInterlockLogicTargets.Text = defaultValue.target;
                InputInterlockLogicStatement.Text = defaultValue.statement;
            }
        }
    }
}
