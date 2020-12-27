using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.CPU.Internal;
using System;
using Xunit;

namespace SharpNES.Core.Tests.CPU.Internals {
  public class MOS6502LogicalTests {
    private readonly Mock<INESCpu> _mockCpu;
    private readonly Mock<ILogger<MOS6502CpuInstructionExecutor>> _mockLogger;
    private readonly MOS6502CpuInstructionExecutor _subject;

    public MOS6502LogicalTests() {
      _mockCpu = new Mock<INESCpu>(MockBehavior.Strict);
      _mockLogger = new Mock<ILogger<MOS6502CpuInstructionExecutor>>(MockBehavior.Loose);
      _subject = new MOS6502CpuInstructionExecutor(_mockCpu.Object, _mockLogger.Object);
    }

    #region AND
    [Theory]
    [InlineData(0x0, 0x1, 0x0)]
    [InlineData(0x2, 0x1, 0x0)]
    [InlineData(0x9, 0xB, 0x9)]
    [InlineData(0xFF, 0x1, 0x1)]
    public void AndMustFetchTheInputAndCompareItWithTheAccumulator(byte accumulator, byte input, byte expectedResult) {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(input);

      _mockCpu
        .SetupProperty(mock => mock.AccumulatorRegister, accumulator);

      _mockCpu
        .SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      _subject.AndWithAccumulator();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(expectedResult);
    }

    [Theory]
    [InlineData(0x0, 0x0, true)]
    [InlineData(0x1, 0x1, false)]
    public void AndMustSetTheZeroFlagProperly(byte accumulator, byte input, bool expectedFlag) {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(input);

      _mockCpu
        .SetupProperty(mock => mock.AccumulatorRegister, accumulator);

      _mockCpu
        .SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      _subject.AndWithAccumulator();
      Check.That(_mockCpu.Object.StatusRegister.HasFlag(NESCpuFlags.Zero)).Equals(expectedFlag);
    }

    [Theory]
    [InlineData(0xFF, 0x80, true)]
    [InlineData(0xFF, 0x90, true)]
    [InlineData(0xFF, 0x1, false)]
    public void AndMustSetTheNegativeFlagProperly(byte accumulator, byte input, bool expectedFlag) {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(input);

      _mockCpu
        .SetupProperty(mock => mock.AccumulatorRegister, accumulator);

      _mockCpu
        .SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      _subject.AndWithAccumulator();
      Check.That(_mockCpu.Object.StatusRegister.HasFlag(NESCpuFlags.Negative)).Equals(expectedFlag);
    }

    [Fact]
    public void AndMustAlwaysRequireAnExtraCycle() {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(0x1);

      _mockCpu
        .SetupProperty(mock => mock.AccumulatorRegister, (byte)0x1);

      _mockCpu
        .SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      Check.That(_subject.AndWithAccumulator()).IsEqualTo(1);
    }
    #endregion
  
    #region ASL
    [Fact]
    public void AslMustShiftTheALUInputRegisterLeftAndStoreItIntoTheAbsoluteMemoryAddress() {
      ushort expectedAbsAddress = 0x1245;
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0x2);
      _mockCpu.Setup(mock => mock.WriteToDataBus(expectedAbsAddress, 0x4)).Verifiable();
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress, expectedAbsAddress);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);
      _mockCpu.SetupGet(mock => mock.CurrentInstruction).Returns(
        new CpuInstruction(
          MOS6502Instruction.ASL, MOS6502AddressingMode.AbsoluteX, 
          () => 1, () => 1, 1
        )
      );

      _subject.ArithmeticShiftLeft();
      _mockCpu.Verify();
      Check.That(_mockCpu.Object.StatusRegister).Equals(NESCpuFlags.Null);
    }

    [Fact]
    public void AslMustComputeTheCarryFlagProperly() {
      ushort expectedAbsAddress = 0x1245;
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0xA0);
      _mockCpu.Setup(mock => mock.WriteToDataBus(expectedAbsAddress, 0x40)).Verifiable();
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress, expectedAbsAddress);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);
      _mockCpu.SetupGet(mock => mock.CurrentInstruction).Returns(
        new CpuInstruction(
          MOS6502Instruction.ASL, MOS6502AddressingMode.AbsoluteX, 
          () => 1, () => 1, 1
        )
      );

      _subject.ArithmeticShiftLeft();
      _mockCpu.Verify();
      Check.That(_mockCpu.Object.StatusRegister).Equals(NESCpuFlags.CarryBit);
    }

    [Fact]
    public void AslMustComputeTheNegativeFlagProperly() {
      ushort expectedAbsAddress = 0x1245;
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0x7F);
      _mockCpu.Setup(mock => mock.WriteToDataBus(expectedAbsAddress, 0xFE)).Verifiable();
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress, expectedAbsAddress);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);
      _mockCpu.SetupGet(mock => mock.CurrentInstruction).Returns(
        new CpuInstruction(
          MOS6502Instruction.ASL, MOS6502AddressingMode.AbsoluteX, 
          () => 1, () => 1, 1
        )
      );

      _subject.ArithmeticShiftLeft();
      _mockCpu.Verify();
      Check.That(_mockCpu.Object.StatusRegister).Equals(NESCpuFlags.Negative);
    }

    [Fact]
    public void AslMustComputeTheZeroFlagProperly() {
      ushort expectedAbsAddress = 0x1245;
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0x80);
      _mockCpu.Setup(mock => mock.WriteToDataBus(expectedAbsAddress, 0x00)).Verifiable();
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress, expectedAbsAddress);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);
      _mockCpu.SetupGet(mock => mock.CurrentInstruction).Returns(
        new CpuInstruction(
          MOS6502Instruction.ASL, MOS6502AddressingMode.AbsoluteX, 
          () => 1, () => 1, 1
        )
      );

      _subject.ArithmeticShiftLeft();
      _mockCpu.Verify();
      Check.That(_mockCpu.Object.StatusRegister).Equals(NESCpuFlags.Zero | NESCpuFlags.CarryBit);
    }

    [Fact]
    public void AslMustWriteTheResultToTheAccumulatorIfTheAddressingModeIsImplicit() {
      ushort expectedAbsAddress = 0x1245;
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0x2);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, (byte)0);
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress, expectedAbsAddress);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);
      _mockCpu.SetupGet(mock => mock.CurrentInstruction).Returns(
        new CpuInstruction(
          MOS6502Instruction.ASL, MOS6502AddressingMode.Implicit, 
          () => 1, () => 1, 1
        )
      );

      _subject.ArithmeticShiftLeft();

      Check.That(_mockCpu.Object.StatusRegister).Equals(NESCpuFlags.Null);
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(0x4);
    }

    [Fact]
    public void AslMayNeverRequireMoreCycles() {
      ushort expectedAbsAddress = 0x1245;
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0x2);
      _mockCpu.Setup(mock => mock.WriteToDataBus(expectedAbsAddress, 0x4)).Verifiable();
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress, expectedAbsAddress);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);
      _mockCpu.SetupGet(mock => mock.CurrentInstruction).Returns(
        new CpuInstruction(
          MOS6502Instruction.ASL, MOS6502AddressingMode.AbsoluteX, 
          () => 1, () => 1, 1
        )
      );

      Check.That(_subject.ArithmeticShiftLeft()).IsEqualTo(0);
    }
    #endregion
  }
}
