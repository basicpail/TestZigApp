using DegaussingTestZigApp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Models
{
    public class ModbusUDPSettings : IModbusSettings
    {
        public string Address { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 502;
        public int BaudRate { get; set; } = 0;
        public int DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;
        public ModbusType Type => ModbusType.UDP;
    }
}
