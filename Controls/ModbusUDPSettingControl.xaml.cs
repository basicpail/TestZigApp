﻿using DegaussingTestZigApp.ViewModels.Controls;
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

namespace DegaussingTestZigApp.Controls
{
    /// <summary>
    /// ModbusUDPSettingControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModbusUDPSettingControl : UserControl
    {
        public ModbusUDPSettingViewModel ViewModel { get; }
        public ModbusUDPSettingControl(ModbusUDPSettingViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
