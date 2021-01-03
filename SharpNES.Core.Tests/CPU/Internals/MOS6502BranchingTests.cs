using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.CPU.Internal;
using System;
using Xunit;
using Instructions = SharpNES.Core.CPU.Internal.MOS6502CpuInstructionExecutor;

namespace SharpNES.Core.Tests.CPU.Internals {
  public class MOS6502BranchingTests {
    private readonly Mock<INESCpu> _mockCpu;
    private readonly Mock<ILogger<MOS6502CpuInstructionExecutor>> _mockLogger;
    private readonly MOS6502CpuInstructionExecutor _subject;
    private readonly Type _subjectType;

    public MOS6502BranchingTests() {
      _mockCpu = new Mock<INESCpu>(MockBehavior.Strict);
      _mockLogger = new Mock<ILogger<MOS6502CpuInstructionExecutor>>(MockBehavior.Loose);
      _subject = new MOS6502CpuInstructionExecutor(_mockCpu.Object, _mockLogger.Object);
      _subjectType = _subject.GetType();
    }

    private object InvokeSubjectMethod(string methodName) {
      var method = _subjectType.GetMethod(methodName);
      if (method == null) {
        throw new ArgumentException("The supplied method name is not defined: " + methodName);
      }

      return method.Invoke(_subject, null);
    }

    [Theory]
    [InlineData(nameof(Instructions.BranchOnCarryClear), NESCpuFlags.Overflow | NESCpuFlags.CarryBit)]
    [InlineData(nameof(Instructions.BranchOnCarrySet), NESCpuFlags.Overflow | NESCpuFlags.DecimalMode)]
    [InlineData(nameof(Instructions.BranchOnEqual), NESCpuFlags.Overflow | NESCpuFlags.DecimalMode)]
    [InlineData(nameof(Instructions.BranchOnMinus), NESCpuFlags.Overflow | NESCpuFlags.DecimalMode)]
    [InlineData(nameof(Instructions.BranchOnNotEqual), NESCpuFlags.Overflow | NESCpuFlags.Zero)]
    [InlineData(nameof(Instructions.BranchOnPlus), NESCpuFlags.Overflow | NESCpuFlags.Negative)]
    public void BranchingMustSimplyReturnZeroIfTheRequiredFlagIsntSet(
      string methodName,
      NESCpuFlags desiredStatusRegister
    ) {
      _mockCpu
        .SetupProperty(cpu => cpu.StatusRegister, desiredStatusRegister);

      var extraCycles = InvokeSubjectMethod(methodName);
      Check.That(extraCycles).IsEqualTo(0);
      Check.That(_mockCpu.Object.StatusRegister).IsEqualTo(desiredStatusRegister);
    }

    [Theory]
    [InlineData(nameof(Instructions.BranchOnCarryClear), NESCpuFlags.Overflow | NESCpuFlags.DecimalMode)]
    [InlineData(nameof(Instructions.BranchOnCarrySet), NESCpuFlags.Overflow | NESCpuFlags.CarryBit)]
    [InlineData(nameof(Instructions.BranchOnEqual), NESCpuFlags.Overflow | NESCpuFlags.Zero)]
    [InlineData(nameof(Instructions.BranchOnMinus), NESCpuFlags.Overflow | NESCpuFlags.Negative)]
    [InlineData(nameof(Instructions.BranchOnNotEqual), NESCpuFlags.Overflow | NESCpuFlags.DecimalMode)]
    [InlineData(nameof(Instructions.BranchOnPlus), NESCpuFlags.Overflow | NESCpuFlags.Zero)]
    public void BranchingMustSetThePCAndAbsAddressRegistersToJumpToRelativeAddressIfTheRequiredFlagIsSet(
      string methodName,
      NESCpuFlags desiredStatusRegister
    ) {
      ushort initialProgramCounter = 0x20;
      ushort initialRelAddress = 0x40;
      ushort expectedAddress = 0x60;

      _mockCpu
        .SetupProperty(cpu => cpu.StatusRegister, desiredStatusRegister);
      _mockCpu
        .SetupProperty(cpu => cpu.AbsoluteAddress, (ushort)0x54); // Junk data that will be replaced
      _mockCpu
        .SetupProperty(cpu => cpu.RelativeAddress, initialRelAddress);
      _mockCpu
        .SetupProperty(cpu => cpu.ProgramCounter, initialProgramCounter);

      var extraCycles = InvokeSubjectMethod(methodName);
      Check.That(extraCycles).IsEqualTo(1);
      Check.That(_mockCpu.Object.StatusRegister).IsEqualTo(desiredStatusRegister);
      Check.That(_mockCpu.Object.AbsoluteAddress).IsEqualTo(expectedAddress);
      Check.That(_mockCpu.Object.ProgramCounter).IsEqualTo(expectedAddress);
    }

    [Theory]
    [InlineData(nameof(Instructions.BranchOnCarryClear), NESCpuFlags.Overflow | NESCpuFlags.DecimalMode)]
    [InlineData(nameof(Instructions.BranchOnCarrySet), NESCpuFlags.Overflow | NESCpuFlags.CarryBit)]
    [InlineData(nameof(Instructions.BranchOnEqual), NESCpuFlags.Overflow | NESCpuFlags.Zero)]
    [InlineData(nameof(Instructions.BranchOnMinus), NESCpuFlags.Overflow | NESCpuFlags.Negative)]
    [InlineData(nameof(Instructions.BranchOnNotEqual), NESCpuFlags.Overflow | NESCpuFlags.DecimalMode)]
    [InlineData(nameof(Instructions.BranchOnPlus), NESCpuFlags.Overflow | NESCpuFlags.Zero)]
    public void BranchingMustRequireAnAdditionalCycleIfThereIsAPageJump(
      string methodName,
      NESCpuFlags desiredStatusRegister
    ) {
      ushort initialProgramCounter = 0x20;
      ushort initialRelAddress = 0x160;

      _mockCpu
        .SetupProperty(cpu => cpu.StatusRegister, desiredStatusRegister);
      _mockCpu
        .SetupProperty(cpu => cpu.AbsoluteAddress, (ushort)0x54); // Junk data that will be replaced
      _mockCpu
        .SetupProperty(cpu => cpu.RelativeAddress, initialRelAddress);
      _mockCpu
        .SetupProperty(cpu => cpu.ProgramCounter, initialProgramCounter);

      var extraCycles = InvokeSubjectMethod(methodName);
      Check.That(extraCycles).IsEqualTo(2);
    }
  }
}