using DegaussingTestZigApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Interfaces
{
    public interface IModbusService<T>
    {
        bool IsConnected { get; }

        Task<bool> ConnectAsync(T settings);

        Task<bool> DisconnectAsync();

        Task<ModbusResponse> SendRequestAsync(ModbusRequest request);
    }
}
