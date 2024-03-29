﻿using System;
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

namespace AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.IOCelceta.IOListDataControl
{
    /// <summary>
    /// BasicInformationDataControl.xaml 的交互逻辑
    /// </summary>
    public partial class TargetInformationDataControl : UserControl
    {
        public TargetInformationDataControl(TargetInformationDataModel dataModel)
        {
            InitializeComponent();
            DataContext = dataModel;
        }

        private void __on_data_binding_error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                (DataContext as TargetInformationDataModel).FieldDataBindingErrors++;
            else if (e.Action == ValidationErrorEventAction.Removed)
                (DataContext as TargetInformationDataModel).FieldDataBindingErrors--;
        }
    }
}
