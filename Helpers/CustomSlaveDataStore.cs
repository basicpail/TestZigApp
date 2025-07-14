using NModbus;
using NModbus.Data;
using System;

namespace DegaussingTestZigApp.Helpers
{
    /// <summary>
    /// Holding Register에 대해 Read 요청이 들어올 때마다 0~200 범위의 랜덤 값을 반환하는 포인트 소스
    /// </summary>
    public class RandomHoldingRegisterSource : IPointSource<ushort>
    {
        public event EventHandler<ushort[]>? RandomDataList;
        private readonly Random _random = new();

        //ReadHoldingRegister 할 때 마다 ReadPoints가 호출되어서 DataStore 값이 초기화 된다.
        public ushort[] ReadPoints(ushort startAddress, ushort numberOfPoints)
        {
            ushort[] values = new ushort[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                values[i] = (ushort)_random.Next(0, 201); // 0~200
            }
            RandomDataList?.Invoke(this, values);
            return values;
        }

        public void WritePoints(ushort startAddress, ushort[] points)
        {
            // Write 요청은 무시
        }
    }

    /// <summary>
    /// 모든 영역(Coils, Inputs 등)을 초기화하되 HoldingRegister만 랜덤 값을 반환하도록 구성된 Slave 데이터 스토어
    /// </summary>
    public class CustomSlaveDataStore : ISlaveDataStore
    {
        public IPointSource<bool> Coils { get; }
        public IPointSource<bool> DiscreteInputs { get; }
        public IPointSource<ushort> HoldingRegisters { get; }
        public IPointSource<ushort> InputRegisters { get; }

        public IPointSource<bool> CoilDiscretes => throw new NotImplementedException();

        public IPointSource<bool> CoilInputs => throw new NotImplementedException();

        public CustomSlaveDataStore()
        {
            Coils = new EmptyPointSource<bool>();
            DiscreteInputs = new EmptyPointSource<bool>();
            HoldingRegisters = new RandomHoldingRegisterSource(); // 핵심: 랜덤 응답
            InputRegisters = new EmptyPointSource<ushort>();
        }
    }

    /// <summary>
    /// 기본값으로만 응답하고 아무 동작도 하지 않는 빈 포인트 소스
    /// </summary>
    public class EmptyPointSource<T> : IPointSource<T>
    {
        public T[] ReadPoints(ushort startAddress, ushort numberOfPoints)
        {
            return new T[numberOfPoints]; // 기본값 배열 반환
        }

        public void WritePoints(ushort startAddress, T[] points)
        {
            // 저장하지 않음
        }
    }
}
