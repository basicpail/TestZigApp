using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Helpers
{
    public class BuildErrorResponse
    {
        private byte[] ErrorResponse(ushort txId, byte unitId, byte functionCode, byte exceptionCode)
        {
            // Modbus 예외 응답 PDU:
            // Function Code | 0x80 비트 OR (오류 표시)
            // Exception Code

        byte[] pdu = new byte[]
        {
            (byte)(functionCode | 0x80), // 오류 표시를 위해 상위 비트 설정
            exceptionCode
        };

        ushort responseLength = (ushort)(pdu.Length + 1); // Unit ID 포함
        byte[] response = new byte[7 + pdu.Length];

        // MBAP Header
        response[0] = (byte)(txId >> 8);       // Transaction ID High
        response[1] = (byte)(txId & 0xFF);     // Transaction ID Low
        response[2] = 0x00;                    // Protocol ID High
        response[3] = 0x00;                    // Protocol ID Low
        response[4] = (byte)(responseLength >> 8);  // Length High
        response[5] = (byte)(responseLength & 0xFF);// Length Low
        response[6] = unitId;                       // Unit ID

        // PDU 복사
        Buffer.BlockCopy(pdu, 0, response, 7, pdu.Length);
        Console.WriteLine($"[ErrorResponse] Function=0x{functionCode:X2}, Exception=0x{exceptionCode:X2}");

        return response;
        }
    }
}
