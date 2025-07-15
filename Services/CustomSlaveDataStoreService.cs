using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DegaussingTestZigApp.Services
{
    public class RandomHoldingRegisterSource : IPointSource<ushort>
    {
        private readonly Random _random = new();
        public event EventHandler<ushort[]>? RandomDataList;


        //ReadHoldingRegister 할 때 마다 ReadPoints가 호출되어서 DataStore 값이 초기화 된다.
        public ushort[] ReadPoints(ushort startAddress, ushort numberOfPoints)
        {
            ushort[] values = new ushort[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                values[i] = (ushort)_random.Next(0, 201); // 0~200
            }
            RandomDataList?.Invoke(this, values);//DashvoardViewModel로 값 전달

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
    public class CustomSlaveDataStoreService : ISlaveDataStore
    {

        public IPointSource<bool> Coils { get; }
        public IPointSource<bool> DiscreteInputs { get; }
        public IPointSource<ushort> HoldingRegisters { get; }
        public IPointSource<ushort> InputRegisters { get; }

        public IPointSource<bool> CoilDiscretes => throw new NotImplementedException();

        public IPointSource<bool> CoilInputs => throw new NotImplementedException();

        public CustomSlaveDataStoreService(RandomHoldingRegisterSource randomSource)
        {
            Coils = new EmptyPointSource<bool>();
            DiscreteInputs = new EmptyPointSource<bool>();
            //HoldingRegisters = new RandomHoldingRegisterSource(); // 랜덤응답
            HoldingRegisters = randomSource; // 랜덤응답
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
