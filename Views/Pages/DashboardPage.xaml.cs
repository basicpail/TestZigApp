using DegaussingTestZigApp.Controls;
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
        public DashboardViewModel ViewModel { get; }
        public ModbusUDPSettingViewModel modbusUDPSettingViewModel { get; }
        public ModbusRTUSettingViewModel modbusRTUSettingViewModel { get; }


        public DashboardPage(DashboardViewModel viewModel)
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            ViewModel = viewModel;
            DataContext = this;
            Loaded += DashboardPage_Loaded;
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

            var modbusUdpSettingControl = new ModbusUDPSettingControl(modbusUDPSettingViewModel);
            var dialog = new ContentDialog
            {
                Title = "ModbusUDP 설정",
                Content = modbusUdpSettingControl,
                PrimaryButtonText = "Connect",
                CloseButtonText = "Close",
                Effect = null,
            };

            var result = await _dialogService.ShowAsync(dialog, CancellationToken.None);
            DialogOverlayHost.Visibility = Visibility.Collapsed;

            if (result == ContentDialogResult.Primary)
            {
               
                // Handle "Yes" button click
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
            var modbusRtuSettingControl = new ModbusRTUSettingControl(modbusRTUSettingViewModel);

            var dialog = new ContentDialog
            {
                Title = "ModbusRTU 설정",
                Content = modbusRtuSettingControl,
                PrimaryButtonText = "Connect",
                CloseButtonText = "Close",
                Effect = null,
            };
            var result = await _dialogService.ShowAsync(dialog, CancellationToken.None);
            DialogOverlayHost.Visibility = Visibility.Collapsed;

            if (result == ContentDialogResult.Primary)
            {
                // Handle "Yes" button click
            }
            else
            {
                // Handle "No" button click or dialog close
            }
        }

    }
}
