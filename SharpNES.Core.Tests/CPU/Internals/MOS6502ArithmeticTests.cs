using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.CPU.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace SharpNES.Core.Tests.CPU.Internals {
  public class MOS6502ArithmeticTests {
    private readonly Mock<INESCpu> _mockCpu;
    private readonly Mock<ILogger<MOS6502CpuInstructionExecutor>> _mockLogger;
    private readonly MOS6502CpuInstructionExecutor _subject;

    public MOS6502ArithmeticTests() {
      _mockCpu = new Mock<INESCpu>(MockBehavior.Strict);
      _mockLogger = new Mock<ILogger<MOS6502CpuInstructionExecutor>>(MockBehavior.Loose);
      _subject = new MOS6502CpuInstructionExecutor(_mockCpu.Object, _mockLogger.Object);
    }

    #region ADC
    [Theory]
    [ClassData(typeof(AddWithCarryTestData))]
    public void AddWithCarryMustSetRegistersAndFlagsCorrectly(
      byte accumulatorValue, byte memoryValue, short expectedResult,
      bool expectedCarry, bool expectedOverflow, bool expectedZero,
      bool expectedNegative
    ) {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(memoryValue)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(memoryValue);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, accumulatorValue);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var expectedFlags = NESCpuFlags.Null;
      if (expectedCarry) {
        expectedFlags |= NESCpuFlags.CarryBit;
      }

      if (expectedOverflow) {
        expectedFlags |= NESCpuFlags.Overflow;
      }

      if (expectedZero) {
        expectedFlags |= NESCpuFlags.Zero;
      }

      if (expectedNegative) {
        expectedFlags |= NESCpuFlags.Negative;
      }

      _subject.AddWithCarry();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(expectedResult & 0x00FF));
      Check.That(_mockCpu.Object.StatusRegister).Equals(expectedFlags);

      _mockCpu.Verify();
    }

    [Fact]
    public void AddWithCarryMustIncludeTheCarryBit() {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(0)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, (byte)0);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.CarryBit);

      _subject.AddWithCarry();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(1));
    }

    [Fact]
    public void AddWithCarryMustAlwaysRequireAnAdditionalCycle() {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(0)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, (byte)0);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var requiresMoreCycles = _subject.AddWithCarry();
      Check.That(requiresMoreCycles).IsEqualTo(1);
    }
    #endregion

    #region SBC
    [Theory]
    [ClassData(typeof(SubtractWithCarryTestData))]
    public void SubtractWithCarryMustSetRegistersAndFlagsCorrectly(
      byte accumulatorValue, byte memoryValue, short expectedResult,
      bool expectedCarry, bool expectedOverflow, bool expectedZero,
      bool expectedNegative
    ) {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(0)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(memoryValue);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, accumulatorValue);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var expectedFlags = NESCpuFlags.Null;
      if (expectedCarry) {
        expectedFlags |= NESCpuFlags.CarryBit;
      }

      if (expectedOverflow) {
        expectedFlags |= NESCpuFlags.Overflow;
      }

      if (expectedZero) {
        expectedFlags |= NESCpuFlags.Zero;
      }

      if (expectedNegative) {
        expectedFlags |= NESCpuFlags.Negative;
      }

      _subject.SubtractWithCarry();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(expectedResult & 0x00FF));
      Check.That(_mockCpu.Object.StatusRegister).Equals(expectedFlags);

      _mockCpu.Verify();
    }

    [Fact]
    public void SubtractWithCarryMustIncludeTheCarryBit() {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(0)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, (byte)0);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.CarryBit);

      // I am definitely suspicious of this 1 here.
      // This may be a bug.
      _subject.SubtractWithCarry();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(1));
    }

    [Fact]
    public void SubtractWithCarryMustAlwaysRequireAnAdditionalCycle() {
      _mockCpu
        .Setup(mock => mock.ReadALUInputRegister())
        .Returns(0)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(0);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, (byte)0);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var requiresMoreCycles = _subject.SubtractWithCarry();
      Check.That(requiresMoreCycles).IsEqualTo(1);
    }
    #endregion

    #region AND
    [Theory]
    [InlineData(0x13, 0x37, 0x13, false, false)]
    [InlineData(0x80, 0xFF, 0x80, false, true)]
    [InlineData(0x0F, 0xF0, 0x00, true, false)]
    public void AndMustCompareOperandsAndSetFlagsCorrectly(
      byte accumulatorValue, byte memoryValue, byte expectedAccumulator,
      bool expectedZero, bool expectedNegative
    ) {
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Returns(memoryValue);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, accumulatorValue);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var expectedFlags = NESCpuFlags.Null;

      if (expectedZero) {
        expectedFlags |= NESCpuFlags.Zero;
      }

      if (expectedNegative) {
        expectedFlags |= NESCpuFlags.Negative;
      }

      _subject.AndWithAccumulator();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(expectedAccumulator & 0x00FF));
      Check.That(_mockCpu.Object.StatusRegister).Equals(expectedFlags);
    }
    #endregion

    #region TestData
    #pragma warning disable CA1812
    // Taken from http://www.righto.com/2012/12/the-6502-overflow-flag-explained.html
    private class AddWithCarryTestData: IEnumerable<object[]> {
      public IEnumerator<object[]> GetEnumerator() {
        // A, M, A_r, C, V, Z, N 
        yield return new object[] {
          (byte)80, (byte)16, (short)96, false, false, false, false
        };
        yield return new object[] {
          (byte)80, (byte)80, (short)160, false, true, false, true
        };
        yield return new object[] {
          (byte) 80, (byte) 144, (short) 224, false, false, false, true
        };
        yield return new object[] {
          (byte)80, (byte)208, (short)288, true, false, false, false
        };
        yield return new object[] {
          (byte)208, (byte)16, (short)224, false, false, false, true
        };
        yield return new object[] {
          (byte)208, (byte)80, (short)288, true, false, false, false
        };
        yield return new object[] {
          (byte)208, (byte)144, (short)352, true, true, false, false
        };
        yield return new object[] {
          (byte)208, (byte)208, (short)416, true, false, false, true
        };
        yield return new object[] {
          (byte)0, (byte)0, (short)0, false, false, true, false
        };
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // Taken from http://www.righto.com/2012/12/the-6502-overflow-flag-explained.html
    private class SubtractWithCarryTestData: IEnumerable<object[]> {
      public IEnumerator<object[]> GetEnumerator() {
        // A, M, A_r, C, V, Z, N 
        yield return new object[] {
          (byte)80, (byte)240, (short)96, false, false, false, false
        };
        yield return new object[] {
          (byte)80, (byte)176, (short)160, false, true, false, true
        };
        yield return new object[] {
          (byte) 80, (byte) 112, (short) 224, false, false, false, true
        };
        yield return new object[] {
          (byte)80, (byte)48, (short)32, true, false, false, false
        };
        yield return new object[] {
          (byte)208, (byte)240, (short)224, false, false, false, true
        };
        yield return new object[] {
          (byte)208, (byte)176, (short)32, true, false, false, false
        };
        yield return new object[] {
          (byte)208, (byte)112, (short)96, true, true, false, false
        };
        yield return new object[] {
          (byte)208, (byte)48, (short)160, true, false, false, true
        };
        yield return new object[] {
          (byte)0, (byte)0, (short)0, true, false, true, false
        };
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    #pragma warning restore CA1812
    #endregion
  }
}
