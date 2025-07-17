using DegaussingTestZigApp.Helpers;
using DegaussingTestZigApp.Interfaces;
using DegaussingTestZigApp.Models;
using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Services
{
    public class ModbusUDPService
    {
        private UdpClient? _udpClient;
        private IPEndPoint? _localEndPoint;
        private IPEndPoint? _udplEndPoint;
        private CancellationTokenSource? _cts;
        private CancellationTokenSource? _loopCts;
        private Task? _sendLoopTask;


        public bool IsConnected { get; set; } = false;
        
        
        public event EventHandler<string>? LogReceived;
        public event EventHandler<CommStatusEventArgs>? DispatchCommStatus;
        public event EventHandler<byte[]>? UDPRequestSent;

        public async Task<bool> ConnectAsync(ModbusUDPSettings settings)
        {
            try
            {
                _udplEndPoint = new IPEndPoint(IPAddress.Parse(settings.Address), settings.Port);//서버의 특정 주소 지정하고 싶을 때

                IsConnected = true;
                Log($"[UDP-Client] Ready to send to {_udplEndPoint}");

                DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Success, "UDP 바인딩 성공, 전송 시작"));
                return true;
            }
            catch (Exception ex)
            {
                Log($"[Error] Failed to bind UDP: {ex.Message}");
                IsConnected = false;
                return false;
                //throw ex;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                _cts?.Cancel();
                IsConnected = false;
                Log("[UDP-Client] Disconnected.");
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }

        public void StartSendingLoop(int interval, int timeout)
        {
            _loopCts?.Cancel(); // 기존 루프가 있으면 중지
            _loopCts = new CancellationTokenSource();

            _sendLoopTask = Task.Run(() => SendAndReceiveLoopAsync(_loopCts.Token, interval, timeout));
        }

        public async Task StopSendingLoopAsync()
        {
            if (_loopCts == null || _sendLoopTask == null)
            {
                Log("[UDP] 송수신 루프가 실행 중이 아닙니다.");
            }

            try
            {
                Log("[UDP] 송수신 루프 종료 요청");
                _loopCts?.Cancel();

                await _sendLoopTask;
                DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Info, "UDP 메시지 전송 종료, 연결 대기 중"));


            }
            catch (Exception ex)
            {
                Log($"[UDP] 송수신 루프 종료 중 예외 발생: {ex.Message}");
                DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Error, "송수신 루프 종료 중 에러 발생"));
            }
            finally
            {
                _loopCts?.Dispose();
                _loopCts = null;
                _sendLoopTask = null;
                Log("[UDP] 송수신 루프 종료 완료");
            }
        }




        private async Task SendAndReceiveLoopAsync(CancellationToken token,int interval, int timeout)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {

                    CommStatusEventArgs result = await SendAndReceiveAsync(timeout); // 실제 송수신 함수 호출

                    switch (result.StatusType)
                    {
                        case CommStatusType.Success:
                            Log("[Loop] 송수신 성공");
                            DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Success, result.Message));
                            break;

                        case CommStatusType.Timeout:
                            DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Warning, result.Message));
                            Log("[Loop] 응답 없음 (Timeout)");
                            break;

                        case CommStatusType.Error:
                            DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Error, result.Message));
                            Log($"[Loop] 통신 오류: {result.Message}");
                            break;
                    }
                    await Task.Delay(interval, token);
                }
                catch (OperationCanceledException)
                {

                    Log("[UDP] 송수신 루프 중단됨 (취소 요청)");
                    break;
                }
                catch (Exception ex)
                {
                    Log($"[UDP] 루프 예외: {ex.Message}");
                    break;
                }
            }
        }


        public async Task<CommStatusEventArgs> SendAndReceiveAsync(int timeout)
        {
            if (_udplEndPoint == null)
            {
                Log("[UDP-Client] Server endpoint not set.");
                return new CommStatusEventArgs(CommStatusType.Error, "Server endpoint not set");
            }

            try
            { 
                using var udpClient = new UdpClient(0); // 0은 OS가 임의 포트를 지정함. 이렇게 하면 작업이 끝난 뒤 udpClient.Close() 및 Dispose()가 자동으로 호출된다.
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                var dataToSend = GenerateRandomResponse();//0~200사이 값

                await udpClient.SendAsync(dataToSend, dataToSend.Length, _udplEndPoint);
                Log($"[UDP-Client] Sent: {BitConverter.ToString(dataToSend)}");

                using var cts = new CancellationTokenSource(timeout); //TimeoutMs시간이 지나면 자동으로 취소 요청 발생, 사실상 Task.Delay(TimeoutMs)와 동일
                var receiveTask = udpClient.ReceiveAsync(); //클라이언트이지만 서버의 응답을 받는다.
                var timeoutTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(receiveTask, timeoutTask); //ReceiveAsync의 응답과 Timeout 시간 중 먼저 끝나는 작업을 기다린다.
                if (completedTask == receiveTask) //timeout 전에 응답이 왔으면 정상
                {
                    try
                    {
                        var response = receiveTask.Result; //서버와의 연결이 끊기면 여기서 에러 발생.
                        //var response = await receiveTask;
                        // Log($"[UDP-Client] Received: {BitConverter.ToString(response.Buffer)}");
                        UDPRequestSent?.Invoke(this, dataToSend);
                    
                        IsConnected = true;
                        return new CommStatusEventArgs(CommStatusType.Success, "UDP 요청/응답 성공");
                    }
                    catch (SocketException ex) {
                        Log($"[UDP] 소켓 예외 발생. 소켓을 재생성합니다: {ex.Message}");


                        return new CommStatusEventArgs(CommStatusType.Error, "소켓 재연결 중");
                    }
                }
                else //timeout상황
                {
                    
                    Log("[UDP-Client] Timeout waiting for response.");
                    IsConnected = false;
                    return new CommStatusEventArgs(CommStatusType.Timeout, "Timeout. 서버로부터의 응답이 기준 시간을 초과 했습니다.");
                }
            }
            catch (SocketException ex)
            {
                Log($"[UDP-Client] Socket error: {ex.Message}");
                IsConnected = false;
                return new CommStatusEventArgs(CommStatusType.Error, ex.Message);
                //throw ex;
            }
            catch (Exception ex)
            {
                Log($"[UDP-Client] Communication error: {ex.Message}");
                IsConnected = false;
                //throw ex;
                return new CommStatusEventArgs(CommStatusType.Error, ex.Message);
            }
        }

        private byte[] GenerateRandomResponse()
        {
            Random rand = new Random();
            byte randomValue = (byte)rand.Next(0, 201); // 0 ~ 200
            return new byte[] { randomValue };
        }

        private void Log(string msg)
        {
            LogReceived?.Invoke(this, msg);
            Console.WriteLine(msg); // 디버깅용 콘솔 출력
        }
    }
}
