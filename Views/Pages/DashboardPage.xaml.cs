using DegaussingTestZigApp.Controls;
using DegaussingTestZigApp.Interfaces;
using DegaussingTestZigApp.ViewModels.Controls;
using DegaussingTestZigApp.ViewModels.Pages;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DegaussingTestZigApp.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        private readonly IContentDialogService _dialogService = new ContentDialogService();
        private readonly ModbusRTUSettingViewModel _modbusRTUSettingViewModel;
        private readonly ModbusRTUSettingControl _modbusRTUSettingControl;
        
        private readonly ModbusUDPSettingViewModel _modbusUPDSettingViewModel;
        private readonly ModbusUDPSettingControl _modbusUDPSettingControl;
        public DashboardViewModel ViewModel { get; }


        public DashboardPage(DashboardViewModel viewModel)
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            ViewModel = viewModel;
            DataContext = this;
            Loaded += DashboardPage_Loaded;

            _modbusUPDSettingViewModel = new ModbusUDPSettingViewModel();
            _modbusUDPSettingControl = new ModbusUDPSettingControl(_modbusUPDSettingViewModel);

            _modbusRTUSettingViewModel = new ModbusRTUSettingViewModel();
            _modbusRTUSettingControl = new ModbusRTUSettingControl(_modbusRTUSettingViewModel);

            InitializeComponent();

        }
        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _dialogService.SetDialogHost(RootContentDialogPresenter);
        }
        private async void OpenModbusUdpSettingDialog(object sender, RoutedEventArgs e)
        {
            // 오버레이 레이어 표시
            DialogOverlayHost.Visibility = Visibility.Visible;

            var dialog = new ContentDialog
            {
                Title = "ModbusUDP 설정",
                Content = _modbusUDPSettingControl,
                PrimaryButtonText = "Connect",
                SecondaryButtonText = "Disconnect",
                CloseButtonText = "Close",
                Effect = null,
            };

            var result = await _dialogService.ShowAsync(dialog, CancellationToken.None);
            DialogOverlayHost.Visibility = Visibility.Collapsed;

            if (result == ContentDialogResult.Primary)
            {
                var settingVm = _modbusUDPSettingControl;
                ViewModel.UdpAddress = settingVm.ViewModel.Address;
                ViewModel.UdpPort = settingVm.ViewModel.Port;
                await ViewModel.ModbusUDPConnect(); 
            }
            else if (result == ContentDialogResult.Secondary)
            {
                await ViewModel.ModbusUDPDisconnect();
            }
            else
            {
                // Handle "No" button click or dialog close
            }
        }

        private async void OpenModbusRtuSettingDialog(object sender, RoutedEventArgs e)
        {
            // 오버레이 레이어 표시
            DialogOverlayHost.Visibility = Visibility.Visible;
            //var modbusRtuSettingControl = new ModbusRTUSettingControl(modbusRTUSettingViewModel); 

            var dialog = new ContentDialog
            {
                Title = "ModbusRTU 설정",
                Content = _modbusRTUSettingControl,
                PrimaryButtonText = "Connect",
                SecondaryButtonText = "Disconnect",
                CloseButtonText = "Close",
                Effect = null,
            };
            var result = await _dialogService.ShowAsync(dialog, CancellationToken.None);
            DialogOverlayHost.Visibility = Visibility.Collapsed;

            if (result == ContentDialogResult.Primary)
            {
                // Handle "Connect" button click
                var settingVm = _modbusRTUSettingControl;
                ViewModel.RtuAddress = settingVm.ViewModel.ComPort;
                ViewModel.BaudRate = settingVm.ViewModel.Baudrate;
                ViewModel.Parity = settingVm.ViewModel.Parity;
                ViewModel.DataBits = settingVm.ViewModel.Databit;
                ViewModel.StopBits = settingVm.ViewModel.StopBits;
                await ViewModel.ModbusRTUConnect();

            }// Handle "Disconnect" button click
            else if (result == ContentDialogResult.Secondary)
            {
                await ViewModel.ModbusRTUDisconnect();
            }
            else
            {
                // Handle "No" button click or dialog close
            }
        }

    }
}
