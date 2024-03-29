﻿using AMEC.PCSoftware.RemoteConsole.CrazyHein.Prometheus.Lombardia;
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
    /// ConsistencyCheckResult.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleDeviceConfigurationViewer : Window
    {
        public SimpleDeviceConfigurationViewer(string title, string message, IEnumerable<DeviceConfiguration> deviceConfigurations)
        {
            InitializeComponent();
            MainViewer.ItemsSource = deviceConfigurations;
            Message.Text = message;
            this.Title = title;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
