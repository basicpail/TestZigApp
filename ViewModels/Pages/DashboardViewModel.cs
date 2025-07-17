using DegaussingTestZigApp.Helpers;
using DegaussingTestZigApp.Models;
using DegaussingTestZigApp.Services;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Net;
using Wpf.Ui.Controls;

namespace DegaussingTestZigApp.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ModbusUDPService _modbusUDPService;
        private readonly ModbusRTUService _modbusRTUService;
        private readonly ModbusLoopbackTest _modbusLoopbackTest;
        private readonly ModbusRTURequest _modbusRequest;
        private readonly RandomHoldingRegisterSource _randomHoldingRegisterSource;

        private const int MaxResponseCount = 1;

        //public ObservableCollection<string> UDPResponseList { get; } = new ObservableCollection<string>();
        public ObservableCollection<ResponseItem> UDPRequestList { get; } = new();
        [ObservableProperty]
        private int _udpRequestNum = 0;
        public ObservableCollection<ResponseItem> RTUResponseList { get; } = new();
        [ObservableProperty]
        private int _rtuResponseNum = 0;

        public DashboardViewModel(ModbusUDPService modbusUdpService, ModbusRTUService modbusRTUService, ModbusLoopbackTest modbusLoopbackTest, ModbusRTURequest modbusRTURequest, RandomHoldingRegisterSource randomHoldingRegisterSource)
        {
            _modbusUDPService = modbusUdpService;
            _modbusUDPService.UDPRequestSent += OnUDPRequestSent;
            _modbusUDPService.DispatchCommStatus += OnDispatchCommStatus;

            _modbusRTUService = modbusRTUService;

            _modbusLoopbackTest = modbusLoopbackTest;

            _modbusRequest = modbusRTURequest;

            _randomHoldingRegisterSource = randomHoldingRegisterSource;
            _randomHoldingRegisterSource.RandomDataList += OnRandomHoldingRegisterSource;
            _randomHoldingRegisterSource.DispatchCommStatus += OnDispatchCommStatus;

        }

        

        private string rtuMaterAddress = "COM5"; //Request 버튼 눌려서 Master가 요청 보낼때 사용하는 Port

        [ObservableProperty]
        private string _udpAddress; //ObservableProperty 설정이 우선순위, 아니면 Model의 기본값으로 settings 들어간다. 나중에 사용자 입력받아서 여기로 넣으면 된다.

        [ObservableProperty]
        private int _udpPort;

        [ObservableProperty]
        private int _udpInterval = 1000;

        [ObservableProperty]
        private int _udpTimeOut = 30000;

        [ObservableProperty]
        private int _udpTimeOutCount = 0;

        [ObservableProperty]
        private int _udpErrorCount = 0;

        [ObservableProperty]
        private int _rtuTimeOutCount = 0;

        [ObservableProperty]
        private int _rtuErrorCount = 0;

        [ObservableProperty]
        private string _lastRequestHex;

        [ObservableProperty]
        private string rtuAddress;

        [ObservableProperty]
        private int baudRate;

        [ObservableProperty]
        private Parity parity;

        [ObservableProperty]
        private int dataBits;

        [ObservableProperty]
        private StopBits stopBits;

        [ObservableProperty]
        private bool _isInfoBarOpen = true;

        [ObservableProperty]
        private string _rtuInfoBarMessage = "ModbusRTU 연결 대기 중";

        [ObservableProperty]
        private InfoBarSeverity _rtuInfoBarSeverity = InfoBarSeverity.Informational;

        [ObservableProperty]
        private string _udpInfoBarMessage = "UDP 연결 대기 중";

        [ObservableProperty]
        private InfoBarSeverity _udpInfoBarSeverity = InfoBarSeverity.Informational;


        [RelayCommand]
        private async Task ModbusLoopbackTest()
        {
            await _modbusLoopbackTest.StartAsync();
        }

        [RelayCommand]
        private async Task ModbusRTURequest()
        {
            var settings = new ModbusRTUSettings
            {
                Address = rtuMaterAddress,
                BaudRate = BaudRate,
                Parity = Parity,
                DataBits = DataBits,
                StopBits = StopBits,
            };
            await _modbusRequest.StartAsync(settings);
            SetRTUReponseResult();
        }
        public async Task ModbusUDPConnect()
        {
            try
            {
                var settings = new ModbusUDPSettings
                {
                    Address = UdpAddress,
                    Port = UdpPort
                };
                bool result = await _modbusUDPService.ConnectAsync(settings);
                
                if (result)
                {
                    try
                    {
                        _modbusUDPService.StartSendingLoop(UdpInterval, UdpTimeOut); // 연결 성공 시 주기적 전송 시작
                    }
                    catch (Exception ex)
                    {
                        SetUDPConnectionResult(false, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                SetUDPConnectionResult(false, ex.Message);
            }
        }

        public async Task ModbusUDPDisconnect()
        {
            try
            {
                //bool result = await _modbusUDPService.DisconnectAsync();
                await _modbusUDPService.StopSendingLoopAsync();
                UDPRequestList.Clear();
            }
            catch (Exception ex)
            {
                SetUDPConnectionResult(false, ex.Message);
            }
        }

        private void OnDispatchCommStatus(object? sender, CommStatusEventArgs e)
        {
            switch (e.StatusType)
            {
                case CommStatusType.Success:
                    UdpInfoBarMessage = e.Message;
                    UdpInfoBarSeverity = InfoBarSeverity.Success;
                    IsInfoBarOpen = true;
                    break;
                case CommStatusType.Warning:
                    UdpTimeOutCount++;
                    UdpInfoBarMessage = e.Message;
                    UdpInfoBarSeverity = InfoBarSeverity.Warning;
                    IsInfoBarOpen = true;
                    break;
                case CommStatusType.Error:
                    UdpErrorCount++;
                    UdpInfoBarMessage = e.Message;
                    UdpInfoBarSeverity = InfoBarSeverity.Error;
                    IsInfoBarOpen = true;
                    break;
                case CommStatusType.Info:
                    UdpRequestNum = 0;
                    UdpTimeOutCount = 0;
                    UdpErrorCount = 0;
                    UdpInfoBarMessage = e.Message;
                    UdpInfoBarSeverity = InfoBarSeverity.Informational;
                    IsInfoBarOpen = true;
                    break;
                case CommStatusType.RtuSuccess:
                    RtuInfoBarMessage = e.Message;
                    RtuInfoBarSeverity = InfoBarSeverity.Success;
                    IsInfoBarOpen = true;
                    break;
            }
        }

        public async Task ModbusRTUConnect()
        {
            var settings = new ModbusRTUSettings
            {
                Address = RtuAddress,
                BaudRate = BaudRate,
                Parity = Parity,
                DataBits = DataBits,
                StopBits = StopBits,
            };
            try
            {
                bool result = await _modbusRTUService.ConnectAsync(settings);
                SetRTUConnectionResult(true);
            }
            catch (Exception ex)
            {
                SetRTUConnectionResult(false, ex.Message);
            }
        }
        public async Task ModbusRTUDisconnect()
        {
            try
            {
                bool result = await _modbusRTUService.DisconnectAsync();
                RtuInfoBarMessage = result ? "ModbusRTU 연결 종료, 대기 중" : "ModbusRTU 연결 종료 실패";
                RtuInfoBarSeverity = result ? InfoBarSeverity.Informational : InfoBarSeverity.Error;
                IsInfoBarOpen = true;
                RtuResponseNum = 0;
                RTUResponseList.Clear();


            }
            catch (Exception ex)
            {
                SetRTUConnectionResult(false, ex.Message);
            }
        }

        public void SetRTUConnectionResult(bool success, string? message = null)
        {
            RtuInfoBarMessage = success ? "ModbusRTU 연결 성공, 메시지 수신 대기 중" : (message ?? "ModbusRTU 연결 실패");
            RtuInfoBarSeverity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            IsInfoBarOpen = true;
        }

        public void SetRTUReponseResult()
        {
            RtuInfoBarMessage = "ModbusRTU 응답 성공";
            RtuInfoBarSeverity = InfoBarSeverity.Success;
            IsInfoBarOpen = true;
        }


        public void SetUDPConnectionResult(bool success, string? message = null)
        {
            UdpInfoBarMessage = success ? "ModbusUDP Connected" : (message ?? "ModbusUDP Connection failed");
            UdpInfoBarSeverity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            IsInfoBarOpen = true;
        }

        private void OnUDPRequestSent(object? sender, byte[] response)
        {
            // 바이트 배열을 16진수 문자열로 변환하여 바인딩용 프로퍼티에 저장
            LastRequestHex = BitConverter.ToString(response).Replace("-", " ");
            //var decimalValues = string.Join(" ",LastResponseHex.Select(b => b.ToString()).ToList());
            //var decimalValues = string.Join(" ", LastResponseHex.Select(b => b.ToString()));

            var hexParts = LastRequestHex.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // 각 부분을 10진수로 파싱하여 문자열 변환
            var decimalValues = ConvertHexStringToDecimalString(LastRequestHex);
            var currentTime = GetKoreanFormattedTimestamp();

            App.Current.Dispatcher.Invoke(() =>
            {
                // 가장 마지막 값 삭제
                //ResponseList.RemoveAt(MaxResponseCount - 1);
                // 맨 앞에 새 값 삽입
                //ResponseList.Insert(0, LastResponseHex);

                //ResponseList.Insert(0, decimalValues);
                //UDPResponseList.Insert(0, currentTime + "      " + decimalValues);
                
                UDPRequestList.Insert(0, new ResponseItem
                {
                    ResponseNum = ++UdpRequestNum,
                    Timestamp = currentTime,
                    Value = decimalValues
                });
                if (UDPRequestList.Count > MaxResponseCount)
                    UDPRequestList.RemoveAt(UDPRequestList.Count - 1);
            });
        }

        private void OnRandomHoldingRegisterSource(object? sender, ushort[] response)
        {
            var currentTime = GetKoreanFormattedTimestamp();
            App.Current.Dispatcher.Invoke(() =>
            {
                //RTUResponseList.Insert(0, currentTime + "      " + response[0]); //값을 하나만 읽어서 첫번째 배열 요소만 가져온다.
                RTUResponseList.Insert(0, new ResponseItem
                {
                    ResponseNum = ++RtuResponseNum,
                    Timestamp = currentTime,
                    Value = response[0].ToString()
                }); //값을 하나만 읽어서 첫번째 배열 요소만 가져온다.
                if (RTUResponseList.Count > MaxResponseCount)
                    RTUResponseList.RemoveAt(RTUResponseList.Count - 1);
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
