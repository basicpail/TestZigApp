//using DegaussingTestZigApp.Interfaces;
//using DegaussingTestZigApp.Models;
//using System;
//using System.IO.Ports;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace DegaussingTestZigApp.Services
//{
//    public class ModbusRTUService : IModbusService<ModbusRTUSettings>
//    {
//        private SerialPort? _serialPort;
//        private CancellationTokenSource? _cts;

//        public bool IsConnected { get; private set; } = false;

//        public event EventHandler<string>? LogReceived;

//        public async Task<bool> ConnectAsync(ModbusRTUSettings settings)
//        {
//            try
//            {
//                _serialPort = new SerialPort(settings.Address, settings.BaudRate, settings.Parity, settings.DataBits, settings.StopBits)
//                {
//                    ReadTimeout = 1000,
//                    WriteTimeout = 1000
//                };

//                _serialPort.Open();
//                _cts = new CancellationTokenSource();
//                IsConnected = true;

//                _ = Task.Run(() => ListenLoop(_cts.Token));

//                Log($"[RTU-Slave] Listening on {settings.Address} @ {settings.BaudRate}bps");
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Log($"[Error] Serial open failed: {ex.Message}");
//                return false;
//            }
//        }

//        public Task<bool> DisconnectAsync()
//        {
//            try
//            {
//                _cts?.Cancel();
//                _serialPort?.Close();
//                IsConnected = false;
//                Log("[RTU-Slave] Disconnected.");
//                return Task.FromResult(true);
//            }
//            catch
//            {
//                return Task.FromResult(false);
//            }
//        }

//        public Task<ModbusResponse> SendRequestAsync(ModbusRequest request)
//        {
//            // RTU Slave는 요청을 보낼 수 없음
//            return Task.FromResult(new ModbusResponse
//            {
//                IsSuccess = false,
//                ErrorMessage = "Slave cannot initiate requests"
//            });
//        }

//        private void ListenLoop(CancellationToken token)
//        {
//            byte[] buffer = new byte[256];

//            while (!token.IsCancellationRequested && _serialPort != null && _serialPort.IsOpen)
//            {
//                try
//                {
//                    int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);
//                    if (bytesRead > 4) // 최소 길이: ID(1) + Func(1) + CRC(2)
//                    {
//                        byte[] request = new byte[bytesRead];
//                        Array.Copy(buffer, request, bytesRead);
//                        Log($"[RTU] Received {bytesRead} bytes");

//                        byte[] response = HandleModbusRequest(request);
//                        if (response.Length > 0)
//                        {
//                            _serialPort.Write(response, 0, response.Length);
//                            Log($"[RTU] Sent response ({response.Length} bytes)");
//                        }
//                    }
//                }
//                catch (TimeoutException) { }
//                catch (Exception ex)
//                {
//                    Log($"[Error] Serial read error: {ex.Message}");
//                }
//            }
//        }

//        private byte[] HandleModbusRequest(byte[] request)
//        {
//            if (request.Length < 8)
//            {
//                Log("[Error] Invalid RTU request.");
//                return Array.Empty<byte>();
//            }

//            byte slaveId = request[0];
//            byte functionCode = request[1];
//            ushort startAddress = (ushort)((request[2] << 8) | request[3]);
//            ushort quantity = (ushort)((request[4] << 8) | request[5]);

//            Log($"[RTU] ID={slaveId}, FC=0x{functionCode:X2}, Start={startAddress}, Count={quantity}");

//            if (quantity == 0 || quantity > 125)
//            {
//                Log("[Error] Invalid quantity.");
//                return BuildErrorResponse(slaveId, functionCode, 0x03); // Illegal Data Value
//            }

//            // 0~200 랜덤 값 생성
//            Random rand = new Random();
//            byte[] dataBytes = new byte[quantity * 2];
//            for (int i = 0; i < quantity; i++)
//            {
//                ushort value = (ushort)rand.Next(0, 201);
//                dataBytes[i * 2] = (byte)(value >> 8);
//                dataBytes[i * 2 + 1] = (byte)(value & 0xFF);
//            }

//            byte[] pdu = new byte[3 + dataBytes.Length];
//            pdu[0] = slaveId;
//            pdu[1] = functionCode;
//            pdu[2] = (byte)dataBytes.Length;
//            Buffer.BlockCopy(dataBytes, 0, pdu, 3, dataBytes.Length);

//            return AddCRC(pdu);
//        }

//        private byte[] BuildErrorResponse(byte slaveId, byte functionCode, byte exceptionCode)
//        {
//            byte[] frame = new byte[]
//            {
//                slaveId,
//                (byte)(functionCode | 0x80),
//                exceptionCode
//            };
//            return AddCRC(frame);
//        }

//        private byte[] AddCRC(byte[] data)
//        {
//            ushort crc = ModbusUtil.CalculateCRC(data);
//            byte[] result = new byte[data.Length + 2];
//            Buffer.BlockCopy(data, 0, result, 0, data.Length);
//            result[^2] = (byte)(crc & 0xFF);       // CRC Low
//            result[^1] = (byte)(crc >> 8);         // CRC High
//            return result;
//        }

//        private void Log(string msg)
//        {
//            LogReceived?.Invoke(this, msg);
//            Console.WriteLine(msg);
//        }

//        public static class ModbusUtil
//        {
//            public static ushort CalculateCRC(byte[] data)
//            {
//                ushort crc = 0xFFFF;

//                for (int pos = 0; pos < data.Length; pos++)
//                {
//                    crc ^= data[pos];
//                    for (int i = 0; i < 8; i++)
//                    {
//                        bool lsb = (crc & 0x0001) != 0;
//                        crc >>= 1;
//                        if (lsb)
//                            crc ^= 0xA001;
//                    }
//                }

//                return crc;
//            }
//        }

//    }
//}
