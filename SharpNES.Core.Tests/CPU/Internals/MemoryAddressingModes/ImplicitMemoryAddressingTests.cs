using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.CPU.Internal;
using Xunit;

namespace SharpNES.Core.Tests.CPU.Internals.MemoryAddressingModes {
  public class ImplicitMemoryAddressingTests {
    private readonly MOS6502CpuMemoryAddressingModes _subject;

    private readonly Mock<INESCpu> _mockCpu;

    public ImplicitMemoryAddressingTests() {
      _mockCpu = new Mock<INESCpu>(MockBehavior.Strict);
      _subject = new MOS6502CpuMemoryAddressingModes(_mockCpu.Object,
        new Mock<ILogger<MOS6502CpuMemoryAddressingModes>>(MockBehavior.Loose).Object);
    }

    [Fact]
    public void ImplicitAddressingModeMayNeverRequireMoreCycles() {
      byte expectedAccValue = 123;
      _mockCpu
        .SetupGet(mock => mock.AccumulatorRegister)
        .Returns(expectedAccValue);
      _mockCpu.SetupSet(mock => mock.ALUInputRegister = expectedAccValue);

      var requiresMoreCycles = _subject.Implicit();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.VerifySet(mock => mock.ALUInputRegister = expectedAccValue);
    }
  }
}
