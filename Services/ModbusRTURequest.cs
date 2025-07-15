using DegaussingTestZigApp.Helpers;
using NModbus;
using NModbus.Serial;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Services
{
    public class ModbusRTURequest
    {
        private readonly string _comPortMaster = "COM5";
        private SerialPort _serialPortMaster;
        private IModbusSerialMaster _master;
        public event EventHandler<string>? LogReceived;

        public async Task StartAsync()
        {
            try
            {
                _serialPortMaster = new SerialPort(_comPortMaster, 9600, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                _serialPortMaster.Open();

                var factory = new ModbusFactory();
                var adapterMaster = new SerialPortAdapter(_serialPortMaster);


                //Master 세팅
                _master = factory.CreateRtuMaster(adapterMaster);

                //// 3. Master → Slave 쓰기
                //Console.WriteLine("Master: WriteSingleRegister to address 100"); //여기서 펜딩
                //await _master.WriteSingleRegisterAsync(1, 100, 12345); // slaveId, address, value

                //// 4. Master → Slave 읽기
                //Console.WriteLine("Master: ReadHoldingRegisters from address 100");
                ushort[] result = await _master.ReadHoldingRegistersAsync(1, 100, 1);


                // Cleanup
                await Task.Delay(100);
                _serialPortMaster.Close();
            }
            catch (Exception ex)
            {
                _serialPortMaster.Close();
                Log($"[Error] Failed to Request Modbus : {ex.Message}");
            }
        }
        private void Log(string msg)
        {
            LogReceived?.Invoke(this, msg);
            Console.WriteLine(msg); // 디버깅용 콘솔 출력
        }
    }
}
