using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.CPU.Internal;
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

    #region BIT
    [Theory]
    [InlineData(0x00, 0xFF, true, false, false)]
    [InlineData(0x80, 0xFF, false, true, false)]
    [InlineData(0x40, 0xFF, false, false, true)]
    [InlineData(0xC0, 0xFF, false, true, true)]
    public void BitMustCompareTheAccumulatorToTheInputAndSetFlagsProperly(
      byte aluInput,
      byte accumulatorValue,
      bool zeroFlag,
      bool negativeFlag,
      bool overflowFlag
    ) {
      _mockCpu.Setup(cpu => cpu.ReadALUInputRegister()).Returns(aluInput);
      _mockCpu.SetupGet(cpu => cpu.AccumulatorRegister).Returns(accumulatorValue);
      _mockCpu.SetupProperty(cpu => cpu.StatusRegister, NESCpuFlags.Null);

      var expectedStatus = NESCpuFlags.Null;
      if (zeroFlag) {
        expectedStatus |= NESCpuFlags.Zero;
      }

      if (negativeFlag) {
        expectedStatus |= NESCpuFlags.Negative;
      }

      if (overflowFlag) {
        expectedStatus |= NESCpuFlags.Overflow;
      }

      Check.That(_subject.BitTest()).IsEqualTo(0);
      Check.That(_mockCpu.Object.StatusRegister).IsEqualTo(expectedStatus);
    }
    #endregion
  
    #region BRK
    [Fact]
    public void BrkMustCopyThePcAndStateIntoTheStackAndSetThePcToTheInterruptHandler() {
      _mockCpu.SetupProperty(mock => mock.StackPointer, (byte)0x10);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Negative);
      _mockCpu.SetupProperty(mock => mock.ProgramCounter, (ushort)0x21AF);

      _mockCpu.Setup(mock => mock.WriteToDataBus(0x110, 0x21)).Verifiable();
      _mockCpu.Setup(mock => mock.WriteToDataBus(0x10F, 0xB0)).Verifiable();
      _mockCpu.Setup(mock => 
        mock.WriteToDataBus(0x10E, (byte)(NESCpuFlags.Negative | NESCpuFlags.Break | NESCpuFlags.DisableInterrupts))
      ).Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromDataBus(0xFFFE)).Returns(0x32);
      _mockCpu.Setup(mock => mock.ReadFromDataBus(0xFFFF)).Returns(0xEA);

      var extraCycles = _subject.BreakInterrupt();
      Check.That(extraCycles).IsEqualTo(0);

      _mockCpu.Verify();
      Check.That(_mockCpu.Object.StackPointer).IsEqualTo(0xD);
      Check.That(_mockCpu.Object.StatusRegister).IsEqualTo(NESCpuFlags.Negative | NESCpuFlags.DisableInterrupts);
      Check.That(_mockCpu.Object.ProgramCounter).IsEqualTo(0xEA32);
    }
    #endregion
  }
}
