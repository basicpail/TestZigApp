using DegaussingTestZigApp.Models;
using DegaussingTestZigApp.Services;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Net;

namespace DegaussingTestZigApp.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ModbusUDPService _modbusUDPService;
        private readonly ModbusRTUService _modbusRTUService;

        private const int MaxResponseCount = 5;

        public ObservableCollection<string> UDPResponseList { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> RTUResponseList { get; } = new ObservableCollection<string>();

        public DashboardViewModel(ModbusUDPService modbusUdpService, ModbusRTUService modbusRTUService)
        {
            _modbusUDPService = modbusUdpService;
            _modbusUDPService.UDPResponseSent += OnUDPResponseSent;

            _modbusRTUService = modbusRTUService;

            // 5칸 미리 초기화
            for (int i = 0; i < MaxResponseCount; i++)
            {
                UDPResponseList.Add(string.Empty);
                RTUResponseList.Add(string.Empty);
            }
        }

        [ObservableProperty]
        private string udpAddress = "127.0.0.1"; //ObservableProperty 설정이 우선순위, 아니면 Model의 기본값으로 settings 들어간다. 나중에 사용자 입력받아서 여기로 넣으면 된다.

        [ObservableProperty]
        private int udpPort = 502;

        [ObservableProperty]
        private string modbusUDPstatusMessage = "대기중";

        [ObservableProperty]
        private string modbusRTUstatusMessage = "대기중";

        [ObservableProperty]
        private string _lastResponseHex;

        [ObservableProperty]
        private string rtuAddress = "COM3";

        [ObservableProperty]
        private int baudRate = 9600;

        [ObservableProperty]
        private Parity parity = Parity.None;

        [ObservableProperty]
        private int dataBits = 8;

        [ObservableProperty]
        private StopBits stopBits = StopBits.One;


        [RelayCommand]
        private async Task ModbusUDPConnect()
        {
            var settings = new ModbusUDPSettings
            {
                Address = UdpAddress,
                Port = UdpPort
            };

            bool result = await _modbusUDPService.ConnectAsync(settings);
            ModbusUDPstatusMessage = result ? "연결 성공" : "연결 실패";
        }

        [RelayCommand]
        private async Task ModbusUDPDisconnect()
        {
            bool result = await _modbusUDPService.DisconnectAsync();
            ModbusUDPstatusMessage = result ? "연결 종료됨" : "종료 실패";
        }

        [RelayCommand]
        private async Task ModbusRTUConnect()
        {
            var settings = new ModbusRTUSettings
            {
                Address = RtuAddress,
                BaudRate = BaudRate,
                Parity = Parity,
                DataBits = DataBits,
                StopBits = StopBits,
            };

            bool result = await _modbusRTUService.ConnectAsync(settings);
            ModbusRTUstatusMessage = result ? "연결 성공" : "연결 실패";
        }

        [RelayCommand]
        private async Task ModbusRTUDisconnect()
        {
            bool result = await _modbusRTUService.DisconnectAsync();
            ModbusRTUstatusMessage = result ? "연결 종료됨" : "종료 실패";
        }

        private void OnUDPResponseSent(object? sender, byte[] response)
        {
            // 바이트 배열을 16진수 문자열로 변환하여 바인딩용 프로퍼티에 저장
            LastResponseHex = BitConverter.ToString(response).Replace("-", " ");
            //var decimalValues = string.Join(" ",LastResponseHex.Select(b => b.ToString()).ToList());
            //var decimalValues = string.Join(" ", LastResponseHex.Select(b => b.ToString()));

            var hexParts = LastResponseHex.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // 각 부분을 10진수로 파싱하여 문자열 변환
            var decimalValues = ConvertHexStringToDecimalString(LastResponseHex);
            var currentTime = GetKoreanFormattedTimestamp();

            App.Current.Dispatcher.Invoke(() =>
            {
                // 가장 마지막 값 삭제
                //ResponseList.RemoveAt(MaxResponseCount - 1);
                // 맨 앞에 새 값 삽입
                //ResponseList.Insert(0, LastResponseHex);

                //ResponseList.Insert(0, decimalValues);
                UDPResponseList.Insert(0, currentTime + "      " + decimalValues);
                if (UDPResponseList.Count > MaxResponseCount)
                    UDPResponseList.RemoveAt(UDPResponseList.Count - 1);
            });
        }


        private string ConvertHexStringToDecimalString(string hexString)
        {
            // HEX 문자열 → 공백 단위로 분할
            var hexParts = hexString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 각 부분을 10진수로 파싱하여 문자열 변환
            var decimalValues = hexParts.Select(h =>
            {
                if (byte.TryParse(h, System.Globalization.NumberStyles.HexNumber, null, out byte value))
                    return value.ToString();
                else
                    return "??"; // 변환 실패 시 표시
            });

            return string.Join(" ", decimalValues);
        }

        public string GetKoreanFormattedTimestamp()
        {
            var koreanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                                TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time"));

            return koreanTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }
}
