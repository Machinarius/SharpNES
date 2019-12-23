using SharpNES.Core.CPU;

namespace SharpNES.Core.DataBus {
  public interface INESDataBus {
    INESCpu Cpu { get; }

    void WriteToMemory(ushort address, byte dataToWrite);
    byte ReadFromMemory(ushort address, bool readOnly = false);
  }
}
