using DegaussingTestZigApp.Helpers;
using DegaussingTestZigApp.Interfaces;
using DegaussingTestZigApp.Models;
using NModbus;
using NModbus.Data;
using NModbus.Serial;
using System.IO.Ports;

namespace DegaussingTestZigApp.Services
{
    public class ModbusRTUService : IModbusService<ModbusRTUSettings>
    {
        private SerialPort? _serialPort;
        private IModbusSlaveNetwork? _slaveNetwork;
        private CancellationTokenSource? _cts;
        private Task? _listenTask;
        private byte slaveID = 1;
        private IModbusSerialMaster _master;

        public bool IsConnected { get; private set; } = false;

        public event EventHandler<string>? LogReceived;
        public event EventHandler<byte[]>? RTUResponseSent;

        private readonly CustomSlaveDataStoreService _dataStore;

        public ModbusRTUService(CustomSlaveDataStoreService dataStore)
        {
            _dataStore = dataStore;
        }
        public async Task<bool> ConnectAsync(ModbusRTUSettings settings)
        {
            try
            {
                // 기존 포트가 열려 있다면 닫고 정리
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                        Log("[RTU-Slave] 기존 SerialPort를 닫았습니다.");
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                }

                _serialPort = new SerialPort(
                    settings.Address,
                    settings.BaudRate,
                    settings.Parity,
                    settings.DataBits,
                    settings.StopBits
                )
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                _serialPort.Open();

                var logger = new CustomModbusLogger();
                // 1. 데이터 저장소 생성 및 초기화
                var factory = new ModbusFactory();
                var adapter = new SerialPortAdapter(_serialPort);
                var randomSource = new RandomHoldingRegisterSource();
                //var dataStore = new DefaultSlaveDataStore();
                //var dataStore = new CustomSlaveDataStoreService(randomSource); //ReadHoldingRegister 할 때 마다 ReadPoints가 호출되어서 0~200 랜덤 값 들어간다.
                var dataStore = _dataStore; //ReadHoldingRegister 할 때 마다 ReadPoints가 호출되어서 0~200 랜덤 값 들어간다.


                // 125개의 값 생성 Holding Registers 0~124에 랜덤 값 채우기, 모든 레지스터에 값 넣어 놓기
                var rand = new Random();
                ushort[] values = new ushort[125];
                for (int i = 0; i < 125; i++)
                    values[i] = (ushort)rand.Next(0, 201);
                // 주소 0부터 시작하여 값 쓰기
                //dataStore.HoldingRegisters.ReadPoints(1, 1)
                dataStore.HoldingRegisters.WritePoints(0, values);

                //Slave Network 만들고, Slave를 Network에 Add
                var slave = factory.CreateSlave(slaveID, dataStore);
                _slaveNetwork = factory.CreateRtuSlaveNetwork(adapter);
                _slaveNetwork.AddSlave(slave);

                _cts = new CancellationTokenSource();
                _listenTask = Task.Run(() => ListenLoopAsync(_cts.Token));

                IsConnected = true;
                Log($"[RTU-Slave] Listening on {settings.Address} @ {settings.BaudRate}bps (SlaveId={slaveID})");

                return true;
            }
            catch (Exception ex)
            {
                Log($"[Error] Serial open failed: {ex.Message}");
                //return false;
                throw ex;
            }
        }

        public Task<bool> DisconnectAsync()
        {
            try
            {
                _cts?.Cancel();
                _listenTask?.Wait(1000);
                _serialPort?.Close();
                _serialPort?.Dispose();
                IsConnected = false;
                Log("[RTU-Slave] Disconnected.");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw ex;
                //return Task.FromResult(false);
            }
        }

        public Task<ModbusResponse> SendRequestAsync(ModbusRequest request)
        {
            // RTU Slave는 요청을 보낼 수 없음
            return Task.FromResult(new ModbusResponse
            {
                IsSuccess = false,
                ErrorMessage = "Slave cannot initiate requests"
            });
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            try
            {
                if (_slaveNetwork == null)
                    return;

                while (!token.IsCancellationRequested)
                {
                    await _slaveNetwork.ListenAsync(token);
                    //이벤트 삽입하여 viewmodel 로 데이터 전달
                    Log($"[RTU-Slave] Request Received!");
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 종료
            }
            catch (Exception ex)
            {
                Log($"[Error] Listen loop error: {ex.Message}");
            }
        }

        private void Log(string msg)
        {
            LogReceived?.Invoke(this, msg);
            Console.WriteLine(msg);
        }
    }
}