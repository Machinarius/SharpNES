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
  public class MOS6502CpuInstructionExecutorTests {
    private readonly Mock<INESCpu> _mockCpu;
    private readonly Mock<ILogger<MOS6502CpuInstructionExecutor>> _mockLogger;
    private readonly MOS6502CpuInstructionExecutor _subject;

    public MOS6502CpuInstructionExecutorTests() {
      _mockCpu = new Mock<INESCpu>(MockBehavior.Strict);
      _mockLogger = new Mock<ILogger<MOS6502CpuInstructionExecutor>>(MockBehavior.Loose);
      _subject = new MOS6502CpuInstructionExecutor(_mockCpu.Object, _mockLogger.Object);
    }

    [Theory]
    [ClassData(typeof(AddWithCarryTestData))]
    public void AddWithCarryMustSetRegistersAndFlagsCorrectly(
      byte accumulatorValue, byte memoryValue, short expectedResult,
      bool expectedCarry, bool expectedOverflow, bool expectedZero,
      bool expectedNegative
    ) {
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Verifiable();
      _mockCpu.SetupGet(mock => mock.ALUInputRegister).Returns(memoryValue);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, accumulatorValue);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var expectedFlags = NESCpuFlags.Null;
      if (expectedCarry) {
        expectedFlags &= NESCpuFlags.CarryBit;
      }

      if (expectedOverflow) {
        expectedFlags &= NESCpuFlags.Overflow;
      }

      if (expectedZero) {
        expectedFlags &= NESCpuFlags.Zero;
      }

      if (expectedNegative) {
        expectedFlags &= NESCpuFlags.Negative;
      }

      _subject.AddWithCarry();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(expectedResult & 0x00FF));
      Check.That(_mockCpu.Object.StatusRegister).Equals(expectedFlags);

      _mockCpu.Verify();
    }

    [Fact]
    public void AddWithCarryMustIncludeTheCarryBit() {
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Verifiable();
      _mockCpu.SetupGet(mock => mock.ALUInputRegister).Returns((byte)0);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, (byte)0);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.CarryBit);

      _subject.AddWithCarry();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(1));
    }

    [Fact]
    public void AddWithCarryMustAlwaysRequireAnAdditionalCycle() {
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Verifiable();
      _mockCpu.SetupGet(mock => mock.ALUInputRegister).Returns((byte)0);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, (byte)0);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var requiresMoreCycles = _subject.AddWithCarry();
      Check.That(requiresMoreCycles).IsTrue();
    }

    [Theory]
    [ClassData(typeof(SubtractWithCarryTestData))]
    public void SubtractithCarryMustSetRegistersAndFlagsCorrectly(
      byte accumulatorValue, byte memoryValue, short expectedResult,
      bool expectedCarry, bool expectedOverflow, bool expectedZero,
      bool expectedNegative
    ) {
      _mockCpu.Setup(mock => mock.ReadALUInputRegister()).Verifiable();
      _mockCpu.SetupGet(mock => mock.ALUInputRegister).Returns(memoryValue);
      _mockCpu.SetupProperty(mock => mock.AccumulatorRegister, accumulatorValue);
      _mockCpu.SetupProperty(mock => mock.StatusRegister, NESCpuFlags.Null);

      var expectedFlags = NESCpuFlags.Null;
      if (expectedCarry) {
        expectedFlags &= NESCpuFlags.CarryBit;
      }

      if (expectedOverflow) {
        expectedFlags &= NESCpuFlags.Overflow;
      }

      if (expectedZero) {
        expectedFlags &= NESCpuFlags.Zero;
      }

      if (expectedNegative) {
        expectedFlags &= NESCpuFlags.Negative;
      }

      _subject.SubtractWithCarry();
      Check.That(_mockCpu.Object.AccumulatorRegister).Equals(Convert.ToByte(expectedResult & 0x00FF));
      Check.That(_mockCpu.Object.StatusRegister).Equals(expectedFlags);

      _mockCpu.Verify();
    }

    // http://www.righto.com/2012/12/the-6502-overflow-flag-explained.html
    private class AddWithCarryTestData: IEnumerable<object[]> {
      public IEnumerator<object[]> GetEnumerator() {
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
          (byte)0, (byte)0, (short)0, false, false, true, false
        };
      }

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}
