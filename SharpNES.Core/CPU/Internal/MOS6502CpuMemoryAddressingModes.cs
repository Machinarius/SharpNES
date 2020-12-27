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

    public int Absolute() {
      var lowBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      var highBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      _cpu.AbsoluteAddress = (ushort)((highBits << 8) | lowBits);

      return 0;
    }

    public int AbsoluteX() {
      var lowBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      var highBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      _cpu.AbsoluteAddress = (ushort)((highBits << 8) | lowBits);
      _cpu.AbsoluteAddress += _cpu.XRegister;

      var pageChanged = (_cpu.AbsoluteAddress & Masks.HigherByte) != highBits << 8;
      return pageChanged ? 1 : 0;
    }

    public int AbsoluteY() {
      var lowBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      var highBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      _cpu.AbsoluteAddress = (ushort)((highBits << 8) | lowBits);
      _cpu.AbsoluteAddress += _cpu.YRegister;

      var pageChanged = (_cpu.AbsoluteAddress & Masks.HigherByte) != highBits << 8;
      return pageChanged ? 1 : 0;
    }

    public int Immediate() {
      _cpu.AbsoluteAddress = _cpu.ProgramCounter++;
      return 0;
    }

    public int Implicit() {
      return 0;
    }

    public int Indirect() {
      var pointerLowBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      var pointerHighBits = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      var pointerBase = (ushort)((pointerHighBits << 8) | pointerLowBits);
      ushort highAddress;

      if (pointerLowBits == Masks.LowerByte) {
        highAddress = (ushort)(pointerBase & Masks.HigherByte);
      } else {
        highAddress = (ushort)(pointerBase + 1);
      }

      var lowValue = _cpu.ReadFromDataBus(pointerBase);
      var highValue = _cpu.ReadFromDataBus(highAddress);

      var absoluteAddress = (highValue << 8) | lowValue;
      _cpu.AbsoluteAddress = (ushort)absoluteAddress;

      return 0;
    }

    public int IndirectX() {
      var pointerBase = (ushort)_cpu.ReadFromDataBus(_cpu.ProgramCounter++);
      var xRegister = (ushort)_cpu.XRegister;
      
      var lowAddress = (ushort)((pointerBase + xRegister) & Masks.LowerByte);
      var highAddress = (ushort)((pointerBase + (xRegister + 1)) & Masks.LowerByte);

      var lowValue = _cpu.ReadFromDataBus(lowAddress);
      var highValue = _cpu.ReadFromDataBus(highAddress);
      var absoluteAddress = (highValue << 8) | lowValue;

      _cpu.AbsoluteAddress = (ushort)absoluteAddress;
      return 0;
    }

    public int IndirectY() {
      var pointerBase = _cpu.ReadFromDataBus(_cpu.ProgramCounter++);

      var lowAddress = (ushort)(pointerBase & Masks.LowerByte);
      var highAddress = (ushort)((pointerBase + 1) & Masks.LowerByte);

      var lowBits = _cpu.ReadFromDataBus(lowAddress);
      var highBits = _cpu.ReadFromDataBus(highAddress);

      var absoluteAddress = (highBits << 8) | lowBits;
      absoluteAddress += _cpu.YRegister;
      _cpu.AbsoluteAddress = (ushort)absoluteAddress;
      
      var pageChanged = (absoluteAddress & Masks.HigherByte) != (highBits << 8);
      return pageChanged ? 1 : 0;
    }

    public int Relative() {
      _cpu.RelativeAddress = _cpu.ReadFromDataBus(_cpu.ProgramCounter);
      _cpu.ProgramCounter++;

      // Range: -128 to +127
      var relOutsideOfRange = (_cpu.RelativeAddress & Masks.EighthHighestBit) > 0;
      if (relOutsideOfRange) {
        _cpu.RelativeAddress |= Masks.HigherByte;
      }

      return 0;
    }

    public int ZeroPageZero() {
      _cpu.AbsoluteAddress = _cpu.ReadFromDataBus(_cpu.ProgramCounter);
      _cpu.ProgramCounter++;
      _cpu.AbsoluteAddress &= Masks.LowerByte;

      return 0;
    }

    public int ZeroPageX() {
      var valueAtPc = _cpu.ReadFromDataBus(_cpu.ProgramCounter);
      _cpu.AbsoluteAddress = (ushort)(valueAtPc + _cpu.XRegister);
      _cpu.ProgramCounter++;
      _cpu.AbsoluteAddress &= Masks.LowerByte;

      return 0;
    }

    public int ZeroPageY() {
      var addressAtPc = _cpu.ReadFromDataBus(_cpu.ProgramCounter);
      _cpu.AbsoluteAddress = (ushort)(addressAtPc + _cpu.YRegister);
      _cpu.ProgramCounter++;
      _cpu.AbsoluteAddress &= Masks.LowerByte;

      return 0;
    }

    private static class Masks {
      public const int EighthHighestBit = 0x80; // Decimal: 128
      public const int LowerByte = 0x00FF;
      public const int HigherByte = 0xFF00;
    }
  }
}
