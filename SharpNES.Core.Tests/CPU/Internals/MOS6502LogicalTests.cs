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

      Check.That(_subject.AndWithAccumulator()).IsTrue();
    }
    #endregion
  }
}
