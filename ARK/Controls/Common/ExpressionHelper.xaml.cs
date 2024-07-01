using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression;
using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.AU;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.ARK.Controls.Common
{
    /// <summary>
    /// ExpressionHelper.xaml 的交互逻辑
    /// </summary>
    public partial class ExpressionHelper : Window
    {
        public AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Napishtim.Engine.Expression.Expression Expression { get; private set; }
        public ExpressionHelper(string expression)
        {
            InitializeComponent();
            sfIOVariablesViewer.ItemsSource = ContextModel.Tags?.Select(x => x.Value);
            sfBuiltInFunctionsViewer.ItemsSource = ArithmeticUnit.CompatibleArithmeticUnitNames.Select(x => KeyValuePair.Create(x.Key, x.Value));
            sfSystemVariablesViewer.ItemsSource = EnvVariableReference.ENV_VARIABLE_INFO.Select(x => KeyValuePair.Create(x.Key, x.Value));
            txtExpression.Text = expression;

            try
            {
                Expression = new Prometheus.Napishtim.Engine.Expression.Expression(expression, null);
            }
            catch 
            { 
                Expression = Prometheus.Napishtim.Engine.Expression.Expression.ZERO; 
            }
        }

        private void sfIOVariablesViewer_CellDoubleTapped(object sender, Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs e)
        {
            txtExpression.Text += $"@0x{(sfIOVariablesViewer.SelectedItem as ProcessData).ProcessObject.Index.ToString("X8")}";
        }

        private void sfIOVariablesViewer_CopyGridCellContent(object sender, Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs e)
        {
            if(e.Column.HeaderText == "ID")
                e.ClipBoardValue = $"@0x{(e.RowData as ProcessData).ProcessObject.Index.ToString("X8")}";
            else
                e.Handled = true;
        }

        private void sfSystemVariablesViewer_CopyGridCellContent(object sender, Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs e)
        {
            if (e.Column.HeaderText == "Comment")
                e.Handled = true;
            else
                e.ClipBoardValue = $"&{((KeyValuePair<ENV_VARIABLE_TYPE_T, string>)e.RowData).Key.ToString()}";
        }

        private void sfSystemVariablesViewer_CellDoubleTapped(object sender, Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs e)
        {
            txtExpression.Text += $"&{((KeyValuePair<ENV_VARIABLE_TYPE_T, string>)sfSystemVariablesViewer.SelectedItem).Key.ToString()}";
        }

        private void sfBuiltInFunctionsViewer_CellDoubleTapped(object sender, Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs e)
        {
            txtExpression.Text += ((KeyValuePair<string, string>)sfBuiltInFunctionsViewer.SelectedItem).Key;
        }

        private void sfBuiltInFunctionsViewer_CopyGridCellContent(object sender, Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs e)
        {
            if (e.Column.HeaderText == "Comment")
                e.Handled = true;
            else
                e.ClipBoardValue = ((KeyValuePair<string, string>)e.RowData).Key;
        }

        private void CancelButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButtonAdv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Expression = new Prometheus.Napishtim.Engine.Expression.Expression(txtExpression.Text.Trim(), null);
                DialogResult = true;
            }
            catch(NaposhtimException ex)
            {
                MessageBox.Show($"The input expression is invalid:\n{ex.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        GridRowSizingOptions __row_sizing_options = new GridRowSizingOptions();
        private void sfIOVariablesViewer_QueryRowHeight(object sender, Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs e)
        {
            if (sfIOVariablesViewer.GridColumnSizer.GetAutoRowHeight(e.RowIndex, __row_sizing_options, out var autoHeight))
            {
                if (autoHeight > 25)
                {
                    e.Height = autoHeight + 12;
                    e.Handled = true;
                }
            }
        }
    }
}
