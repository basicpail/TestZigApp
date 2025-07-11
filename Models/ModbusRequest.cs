using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Models
{
    public class ModbusRequest
    {
        public byte SlaveId { get; set; }
        public byte FunctionCode { get; set; }
        public ushort StartAddress { get; set; }
        public ushort NumberOfPoints { get; set; }
        public byte[]? WriteData { get; set; } // 쓰기일 경우
    }
}
