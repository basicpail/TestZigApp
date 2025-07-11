using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Models
{
    public class ModbusResponse
    {
        public bool IsSuccess { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
