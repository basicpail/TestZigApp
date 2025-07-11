using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Interfaces
{
    public enum ModbusType
    {
        UDP,
        RTU,
        TCP
    }
    internal interface IModbusSettings
    {
        string Address { get; set; }       // IP 주소 or COM 포트
        int Port { get; set; }             // UDP의 경우 사용
        int BaudRate { get; set; }         // RTU의 경우 사용
        int DataBits { get; set; }
        Parity Parity { get; set; }
        StopBits StopBits { get; set; }
        ModbusType Type { get; }
    }
}
