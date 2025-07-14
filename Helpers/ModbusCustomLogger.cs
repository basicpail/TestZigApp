using NModbus;
using System;

namespace DegaussingTestZigApp.Helpers
{
    public class CustomModbusLogger : IModbusLogger
    {
        public void Debug(string message) => Console.WriteLine($"[DEBUG] {message}");
        public void Error(string message) => Console.WriteLine($"[ERROR] {message}");
        public void Information(string message) => Console.WriteLine($"[INFO] {message}");
        public void Warning(string message) => Console.WriteLine($"[WARN] {message}");

        public void Log(LoggingLevel level, string message)
        {
            switch (level)
            {
                case LoggingLevel.Debug: Debug(message); break;
                case LoggingLevel.Information: Information(message); break;
                case LoggingLevel.Warning: Warning(message); break;
                case LoggingLevel.Error: Error(message); break;
                default: Console.WriteLine($"[UNKNOWN] {message}"); break;
            }
        }

        public bool ShouldLog(LoggingLevel level)
        {
            // 필요에 따라 로그 레벨 필터링 가능 (여기선 모두 출력)
            return true;
        }
    }
}