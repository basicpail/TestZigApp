using DegaussingTestZigApp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Models
{
    public class ModbusRTUSettings : IModbusSettings
    {
        public string Address { get; set; } = "COM3";
        public int Port { get; set; } = 0; //RTU에서 사용안함
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;
        public ModbusType Type => ModbusType.RTU;
    }
}
