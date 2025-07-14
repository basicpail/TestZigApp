using DegaussingTestZigApp.Helpers;
using NModbus;
using NModbus.Data;
using NModbus.Serial;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Services
{
    public class ModbusLoopbackTest
    {
        private readonly string _comPort = "COM4";
        private readonly string _comPortMaster = "COM5";
        private SerialPort _serialPort;
        private SerialPort _serialPortMaster;
        private IModbusSerialMaster _master;
        private IModbusSlaveNetwork _slaveNetwork;
        private CancellationTokenSource _cts;

        public async Task StartAsync()
        {
            _serialPort = new SerialPort(_comPort, 9600, Parity.None, 8, StopBits.One);
            _serialPortMaster = new SerialPort(_comPortMaster, 9600, Parity.None, 8, StopBits.One);
            _serialPort.Open();
            _serialPortMaster.Open();

            var factory = new ModbusFactory();
            var adapter = new SerialPortAdapter(_serialPort);
            var adapterMaster = new SerialPortAdapter(_serialPortMaster);

            //Slave 세팅
            //var dataStore = new DefaultSlaveDataStore();
            var dataStore = new CustomSlaveDataStore();
            var slave = factory.CreateSlave(1, dataStore);
            _slaveNetwork = factory.CreateRtuSlaveNetwork(adapter);
            _slaveNetwork.AddSlave(slave);

            _cts = new CancellationTokenSource();

            //Slave 응답 루프 실행
            Task slaveTask = Task.Run(() => _slaveNetwork.ListenAsync(_cts.Token), _cts.Token);

            //Master 세팅
            _master = factory.CreateRtuMaster(adapterMaster);

            // 약간 대기 후 요청
            await Task.Delay(500); // Slave 네트워크 시작 시간 확보

            //// 3. Master → Slave 쓰기
            //Console.WriteLine("Master: WriteSingleRegister to address 100"); //여기서 펜딩
            //await _master.WriteSingleRegisterAsync(1, 100, 12345); // slaveId, address, value

            //// 4. Master → Slave 읽기
            //Console.WriteLine("Master: ReadHoldingRegisters from address 100");
            ushort[] result = await _master.ReadHoldingRegistersAsync(1, 100, 1);
            //Console.WriteLine($"Read value: {result[0]}");

            ushort[] result2 = await _master.ReadHoldingRegistersAsync(1, 100, 1);


            ushort[] result3 = await _master.ReadHoldingRegistersAsync(1, 100, 1);

            // Cleanup
            await Task.Delay(1000);
            _cts.Cancel();
            _serialPort.Close();
        }
    }
}
