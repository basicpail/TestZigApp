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
        private const int TimeoutMs = 500;

        public event EventHandler<string>? LogReceived;
        public event EventHandler<CommStatusEventArgs>? DispatchCommStatus;
        public event EventHandler<byte[]>? UDPRequestSent;

        public async Task<bool> ConnectAsync(ModbusUDPSettings settings)
        {
            try
            {
                // 기존 UdpClient가 있으면 닫고 정리
                if (_udpClient != null)
                {
                    _udpClient.Close(); // 내부적으로 Dispose도 수행
                    _udpClient = null;
                    Log("[UDP-Client] 기존 UdpClient를 닫았습니다.");
                }

                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }

                _udplEndPoint = new IPEndPoint(IPAddress.Parse(settings.Address), settings.Port);//서버의 특정 주소 지정하고 싶을 때
                _udpClient = new UdpClient();
                _cts = new CancellationTokenSource();

                IsConnected = true;
                Log($"[UDP-Client] Ready to send to {_udplEndPoint}");

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
                StopSendingLoop();
                _cts?.Cancel();
                _udpClient?.Close();
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

        public void StartSendingLoop(int timeout)
        {
            _loopCts?.Cancel(); // 기존 루프가 있으면 중지
            _loopCts = new CancellationTokenSource();

            _sendLoopTask = Task.Run(() => SendAndReceiveLoopAsync(_loopCts.Token, timeout));
        }

        private async Task SendAndReceiveLoopAsync(CancellationToken token, int timeout)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {

                    bool success = await SendAndReceiveAsync(); // 실제 송수신 함수 호출

                    if (success)
                        Log("[Loop] 송수신 성공");
                    else
                        Log("[Loop] 연결 실패 또는 응답 없음");

                    await Task.Delay(timeout, token); // 1초 대기
                }
                catch (OperationCanceledException)
                {
                    Log("[Loop] 송수신 루프 취소됨");
                    break;
                }
                catch (Exception ex)
                {
                    Log($"[Loop] 예외 발생: {ex.Message}");
                    DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Error, ex.Message));
                    //throw ex;
                    IsConnected = false;
                }
            }
        }


        public async Task<bool> SendAndReceiveAsync()
        {
            if (_udpClient == null || _udplEndPoint == null)
            {
                Log("[UDP-Client] Not connected.");
                return false;
            }

            try
            {
                var dataToSend = GenerateRandomResponse();//0~200사이 값

                await _udpClient.SendAsync(dataToSend, dataToSend.Length, _udplEndPoint);
                Log($"[UDP-Client] Sent: {BitConverter.ToString(dataToSend)}");

                using var cts = new CancellationTokenSource(TimeoutMs); //TimeoutMs시간이 지나면 자동으로 취소 요청 발생, 사실상 Task.Delay(TimeoutMs)와 동일
                var receiveTask = _udpClient.ReceiveAsync(); //클라이언트이지만 서버의 응답을 받는다.
                var completedTask = await Task.WhenAny(receiveTask, Task.Delay(TimeoutMs, cts.Token)); //ReceiveAsync의 응답과 Timeout 시간 중 먼저 끝나는 작업을 기다린다.

                if (completedTask == receiveTask) //timeout 전에 응답이 왔으면 정상
                {
                    var response = receiveTask.Result;
                    Log($"[UDP-Client] Received: {BitConverter.ToString(response.Buffer)}");
                    UDPRequestSent?.Invoke(this, dataToSend);
                    IsConnected = true;
                    return true;
                }
                else //timeout상황
                {
                    UDPRequestSent?.Invoke(this, dataToSend);
                    DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Warning, "UDP Server Response Timeout"));
                    Log("[UDP-Client] Timeout waiting for response.");
                    IsConnected = false;
                    return false;
                }
            }
            catch (SocketException ex)
            {
                Log($"[UDP-Client] Socket error: {ex.Message}");
                IsConnected = false;
                DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Error, ex.Message));
                return false;
                //throw ex;
            }
            catch (Exception ex)
            {
                Log($"[UDP-Client] Communication error: {ex.Message}");
                DispatchCommStatus?.Invoke(this, new CommStatusEventArgs(CommStatusType.Error, ex.Message));
                IsConnected = false;
                //throw ex;
                return false;
            }
        }
        public void StopSendingLoop()
        {
            _loopCts?.Cancel();
            _loopCts = null;
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
