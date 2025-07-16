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
    public class ModbusUDPServiceOld : IModbusService<ModbusUDPSettings>
    {
        private UdpClient? _udpClient;
        private IPEndPoint? _localEndPoint;
        private IPEndPoint? _udplEndPoint;
        private CancellationTokenSource? _cts;

        public bool IsConnected { get; private set; } = false;

        public event EventHandler<string>? LogReceived;
        public event EventHandler<byte[]>? UDPResponseSent;

        public async Task<bool> ConnectAsync(ModbusUDPSettings settings)
        {
            try
            {
                // 기존 UdpClient가 있으면 닫고 정리
                if (_udpClient != null)
                {
                    _udpClient.Close(); // 내부적으로 Dispose도 수행
                    _udpClient = null;
                    Log("[UDP-Slave] 기존 UdpClient를 닫았습니다.");
                }

                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }

                IPAddress bindAddress = IPAddress.Parse(settings.Address); //특정 주소 지정하고 싶을 때
                _udplEndPoint = new IPEndPoint(bindAddress, settings.Port);//특정 주소 지정하고 싶을 때
                //_localEndPoint = new IPEndPoint(IPAddress.Any, settings.Port); // 0.0.0.0으로 지정해서 모든 네트워크 인터페이스에서 수신 대기
                //_udpClient = new UdpClient(_localEndPoint);
                _udpClient = new UdpClient(_udplEndPoint);
                _cts = new CancellationTokenSource();
                IsConnected = true;

                _ = Task.Run(() => ListenLoopAsync(_cts.Token)); // 비동기 수신 루프 시작

                Log($"[Slave] Listening on UDP port {settings.Port}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"[Error] Failed to bind UDP port: {ex.Message}");
                IsConnected = false;
                //return false;
                throw ex;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                _cts?.Cancel();
                _udpClient?.Close();
                IsConnected = false;
                Log("[Slave] UDP Listener stopped.");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<ModbusResponse> SendRequestAsync(ModbusRequest request)
        {
            // Slave에서는 요청 전송 기능은 필요 없음
            return Task.FromResult(new ModbusResponse
            {
                IsSuccess = false,
                ErrorMessage = "Slave cannot initiate requests"
            });
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient!.ReceiveAsync();
                    var requestBytes = result.Buffer;
                    var remoteEP = result.RemoteEndPoint;

                    Log($"[Slave] Received request from {remoteEP}");

                    //ModbusUDP 형식으로 요청이 왔을 때 응답 생성
                    //byte[] response = HandleModbusRequest(requestBytes);

                    //일반 UDP 데이터가 요청으로 왔을 때 응답 생성
                    byte[] response = GenerateRandomResponse();

                    //데이터 바인딩 해야한다.
                    await _udpClient.SendAsync(response, response.Length, remoteEP);
                    UDPResponseSent?.Invoke(this, response);

                    Log($"[Slave] Sent response to {remoteEP}");
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                        Log($"[Error] Receive loop error: {ex.Message}");
                }
            }
        }

        private byte[] GenerateRandomResponse()
        {
            Random rand = new Random();
            byte randomValue = (byte)rand.Next(0, 201); // 0 ~ 200
            return new byte[] { randomValue };
        }


        private byte[] HandleModbusRequest(byte[] request)
        {
            if (request.Length < 12)
            {
                Log("[Error] Invalid request length.");
                return new byte[0];
            }

            // MBAP Header
            ushort transactionId = (ushort)((request[0] << 8) | request[1]);
            byte unitId = request[6];
            byte functionCode = request[7];

            // 요청 파싱 (기능코드와 무관하게 주소 및 개수만 사용)
            ushort startAddress = (ushort)((request[8] << 8) | request[9]);
            ushort quantity = (ushort)((request[10] << 8) | request[11]);

            Log($"[Request] TxID={transactionId}, Function=0x{functionCode:X2}, Unit={unitId}, Start={startAddress}, Count={quantity}");

            if (quantity == 0 || quantity > 125)
            {
                Log("[Error] Invalid quantity requested.");
                return BuildErrorResponse(transactionId, unitId, functionCode, 0x03); // Illegal Data Value
            }

            // 0~200 범위 랜덤 값 채우기
            Random rand = new Random();
            byte[] dataBytes = new byte[quantity * 2];
            for (int i = 0; i < quantity; i++)
            {
                ushort value = (ushort)rand.Next(0, 201);
                dataBytes[i * 2] = (byte)(value >> 8);       // High byte
                dataBytes[i * 2 + 1] = (byte)(value & 0xFF); // Low byte
            }

            // 응답 PDU: Function Code + Byte Count + Data[]
            byte[] pdu = new byte[2 + dataBytes.Length];
            pdu[0] = functionCode;
            pdu[1] = (byte)dataBytes.Length;
            Buffer.BlockCopy(dataBytes, 0, pdu, 2, dataBytes.Length);

            // 응답 MBAP 헤더 구성 7바이트 이후가 PDU
            ushort responseLength = (ushort)(pdu.Length + 1); // +1 for Unit ID
            byte[] response = new byte[7 + pdu.Length];
            response[0] = (byte)(transactionId >> 8);
            response[1] = (byte)(transactionId & 0xFF);
            response[2] = 0;
            response[3] = 0;
            response[4] = (byte)(responseLength >> 8);
            response[5] = (byte)(responseLength & 0xFF);
            response[6] = unitId;
            Buffer.BlockCopy(pdu, 0, response, 7, pdu.Length); //response(7bytes) + pdu(2bytes + data bytes)

            return response;
        }


        private void Log(string msg)
        {
            LogReceived?.Invoke(this, msg);
            Console.WriteLine(msg); // 디버깅용 콘솔 출력
        }

        /*
         * 
         * 
        0x01	Illegal Function
        0x02	Illegal Data Address
        0x03	Illegal Data Value
        0x04	Slave Device Failure
         */
        private byte[] BuildErrorResponse(ushort txId, byte unitId, byte functionCode, byte exceptionCode)
        {
            // Modbus 예외 응답 PDU:
            // Function Code | 0x80 비트 OR (오류 표시)
            // Exception Code

            byte[] pdu = new byte[]
            {
        (byte)(functionCode | 0x80), // 오류 표시를 위해 상위 비트 설정
        exceptionCode
            };

            ushort responseLength = (ushort)(pdu.Length + 1); // Unit ID 포함
            byte[] response = new byte[7 + pdu.Length];

            // MBAP Header
            response[0] = (byte)(txId >> 8);       // Transaction ID High
            response[1] = (byte)(txId & 0xFF);     // Transaction ID Low
            response[2] = 0x00;                    // Protocol ID High
            response[3] = 0x00;                    // Protocol ID Low
            response[4] = (byte)(responseLength >> 8);  // Length High
            response[5] = (byte)(responseLength & 0xFF);// Length Low
            response[6] = unitId;                       // Unit ID

            // PDU 복사
            Buffer.BlockCopy(pdu, 0, response, 7, pdu.Length);

            Log($"[ErrorResponse] Function=0x{functionCode:X2}, Exception=0x{exceptionCode:X2}");

            return response;
        }

    }
}
