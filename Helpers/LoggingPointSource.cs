using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Helpers
{
    public class LoggingPointSource
    {
        private readonly IPointSource<ushort> _inner;
        private readonly Action<string> _log;

        public LoggingPointSource(IPointSource<ushort> inner, Action<string> log)
        {
            _inner = inner;
            _log = log;
        }

        public ushort[] ReadPoints(ushort startAddress, ushort numberOfPoints)
        {
            var values = _inner.ReadPoints(startAddress, numberOfPoints);
            _log($"[응답] Addr {startAddress}~{startAddress + numberOfPoints - 1}: {string.Join(", ", values)}");
            return values;
        }

        public void WritePoints(ushort startAddress, ushort[] points)
        {
            _inner.WritePoints(startAddress, points);
        }
    }
}
