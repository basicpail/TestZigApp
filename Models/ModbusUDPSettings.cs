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
        public string Address { get; set; }
        public int Port { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public ModbusType Type => ModbusType.UDP;
    }
}
