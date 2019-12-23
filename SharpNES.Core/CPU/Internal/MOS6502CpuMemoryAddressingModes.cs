using Microsoft.Extensions.Logging;
using System;

namespace SharpNES.Core.CPU.Internal {
  public class MOS6502CpuMemoryAddressingModes : IMemoryAddressingModes {
    private readonly INESCpu _cpu;
    private readonly ILogger<MOS6502CpuMemoryAddressingModes> _logger;

    public MOS6502CpuMemoryAddressingModes(INESCpu cpu, ILogger<MOS6502CpuMemoryAddressingModes> logger) {
      _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool Absolute() {
      throw new NotImplementedException();
    }

    public bool AbsoluteX() {
      throw new NotImplementedException();
    }

    public bool AbsoluteY() {
      throw new NotImplementedException();
    }

    public bool Immediate() {
      throw new NotImplementedException();
    }

    public bool Implicit() {
      _cpu.ALUInputRegister = _cpu.AccumulatorRegister;
      return false;
    }

    public bool Indirect() {
      throw new NotImplementedException();
    }

    public bool IndirectX() {
      throw new NotImplementedException();
    }

    public bool IndirectY() {
      throw new NotImplementedException();
    }

    public bool Relative() {
      throw new NotImplementedException();
    }

    public bool ZeroPage() {
      throw new NotImplementedException();
    }

    public bool ZeroPageX() {
      throw new NotImplementedException();
    }

    public bool ZeroPageY() {
      throw new NotImplementedException();
    }
  }
}
