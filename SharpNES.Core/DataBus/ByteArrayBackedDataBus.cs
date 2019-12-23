using SharpNES.Core.CPU;
using System;

namespace SharpNES.Core.DataBus {
  public class ByteArrayBackedDataBus : INESDataBus {
    private const int NESMemorySize = 64 * 2014;

    public INESCpu Cpu { get; }

    private readonly byte[] _memory;

    public ByteArrayBackedDataBus(INESCpu cpu) {
      _memory = new byte[NESMemorySize];
      Cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
      Cpu.ConnectToDataBus(this);
    }

    public byte ReadFromMemory(ushort address, bool readOnly = false) {
      if (address >= 0x0000 && address <= 0xFFFF) {
        return _memory[address];
      }

      return 0;
    }

    public void WriteToMemory(ushort address, byte dataToWrite) {
      if (address >= 0x0000 && address <= 0xFFFF) {
        _memory[address] = dataToWrite;
      }
    }
  }
}
