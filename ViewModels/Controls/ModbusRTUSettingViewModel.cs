using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace DegaussingTestZigApp.ViewModels.Controls
{
    public partial class ModbusRTUSettingViewModel : ObservableObject
    {
        public ObservableCollection<string> AvailableComPorts { get; } =
        new ObservableCollection<string>(SerialPort.GetPortNames());

        public ObservableCollection<int> AvailableBaudrates { get; } =
            new ObservableCollection<int> { 9600, 19200, 38400, 57600, 115200 };

        public ObservableCollection<int> AvailableDatabits { get; } =
            new ObservableCollection<int> { 7, 8 };

        public ObservableCollection<Parity> AvailableParities { get; } =
            new ObservableCollection<Parity>((Parity[])Enum.GetValues(typeof(Parity)));

        public ObservableCollection<StopBits> AvailableStopBits { get; } =
            new ObservableCollection<StopBits>(
                ((StopBits[])Enum.GetValues(typeof(StopBits)))
                .Where(sb => sb != StopBits.None)
            );

        [ObservableProperty] private string _comPort;
        [ObservableProperty] private int _baudrate;
        [ObservableProperty] private int _databit;
        [ObservableProperty] private Parity _parity = Parity.None;
        [ObservableProperty] private StopBits _stopBits = StopBits.One;


        [RelayCommand]
        private void RefreshComPorts()
        {
            var ports = SerialPort.GetPortNames();

            AvailableComPorts.Clear();
            foreach (var port in ports)
            {
                AvailableComPorts.Add(port);
            }

            // 선택된 포트가 사라졌다면 null 처리
            if (!AvailableComPorts.Contains(ComPort))
            {
                ComPort = null;
            }
        }

        
    }
}
