using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.DataBus;
using Xunit;

namespace SharpNES.Core.Tests.CPU {
  public class MOS6502CpuTests {
    private readonly MOS6502Cpu _subject;

    private readonly Mock<INESDataBus> _mockDataBus;
    private readonly Mock<ICpuInstructionExecutor> _mockExecutor;
    private readonly Mock<IMemoryAddressingModes> _mockAddressing;
    private readonly Mock<IInstructionLookupTable> _mockInstructionsTable;

    public MOS6502CpuTests() {
      _mockDataBus = new Mock<INESDataBus>(MockBehavior.Strict);
      _mockExecutor = new Mock<ICpuInstructionExecutor>(MockBehavior.Strict);
      _mockAddressing = new Mock<IMemoryAddressingModes>(MockBehavior.Strict);
      _mockInstructionsTable = new Mock<IInstructionLookupTable>(MockBehavior.Strict);

      _subject = new MOS6502Cpu(new Mock<ILogger<MOS6502Cpu>>(MockBehavior.Loose).Object,
        _mockExecutor.Object, _mockAddressing.Object, _mockInstructionsTable.Object);
      _subject.ConnectToDataBus(_mockDataBus.Object);
    }

    [Fact]
    public void TheCpuMustFetchAndIncrementPcAndExecuteOpCodesOnClockZero() {
      ushort originalPC = _subject.ProgramCounter;
      byte expectedOpCode = 10;
      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(originalPC, false))
        .Returns(expectedOpCode)
        .Verifiable();

      _mockExecutor
        .Setup(mock => mock.BreakInterrupt())
        .Returns(false)
        .Verifiable();

      _mockAddressing
        .Setup(mock => mock.Immediate())
        .Returns(false)
        .Verifiable();

      var expectedInstruction = new CpuInstruction("BRK", _mockExecutor.Object.BreakInterrupt, _mockAddressing.Object.Immediate, 2);
      _mockInstructionsTable
        .Setup(mock => mock.GetInstructionForOpCode(expectedOpCode))
        .Returns(expectedInstruction)
        .Verifiable();

      _subject.OnClockTick();
      _mockDataBus.Verify();
      _mockExecutor.Verify();
      _mockAddressing.Verify();
      _mockInstructionsTable.Verify();

      Check.That(_subject.ProgramCounter).Equals(originalPC + 1);
    }

    [Fact]
    public void TheCpuMustntFetchNorExecuteUntilTheCyclesForTheInstructionsAreComplete() {
      ushort originalPC = _subject.ProgramCounter;
      byte expectedOpCode = 10;
      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(originalPC, false))
        .Returns(expectedOpCode);

      _mockExecutor
        .Setup(mock => mock.BreakInterrupt())
        .Returns(false);

      _mockAddressing
        .Setup(mock => mock.Immediate())
        .Returns(false);

      var expectedInstruction = new CpuInstruction("BRK", _mockExecutor.Object.BreakInterrupt, _mockAddressing.Object.Immediate, 2);
      _mockInstructionsTable
        .Setup(mock => mock.GetInstructionForOpCode(expectedOpCode))
        .Returns(expectedInstruction)
        .Verifiable();

      _subject.OnClockTick();
      _subject.OnClockTick();
      _mockDataBus.Verify(mock => mock.ReadFromMemory(originalPC, false), Times.Once);
      _mockExecutor.Verify(mock => mock.BreakInterrupt(), Times.Once);
      _mockAddressing.Verify(mock => mock.Immediate(), Times.Once);
      _mockInstructionsTable.Verify(mock => mock.GetInstructionForOpCode(expectedOpCode), Times.Once);
    }

    [Fact]
    public void TheCpuMustExtendTheInstructionCyclesShouldTheOpCodeRequestSo() {
      ushort originalPC = _subject.ProgramCounter;
      byte expectedOpCode = 10;
      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(originalPC, false))
        .Returns(expectedOpCode);

      _mockExecutor
        .Setup(mock => mock.BreakInterrupt())
        .Returns(true);

      _mockAddressing
        .Setup(mock => mock.Immediate())
        .Returns(false);

      var expectedInstruction = new CpuInstruction("BRK", _mockExecutor.Object.BreakInterrupt, _mockAddressing.Object.Immediate, 2);
      _mockInstructionsTable
        .Setup(mock => mock.GetInstructionForOpCode(expectedOpCode))
        .Returns(expectedInstruction)
        .Verifiable();

      _subject.OnClockTick();
      _subject.OnClockTick();
      _subject.OnClockTick();
      _mockDataBus.Verify(mock => mock.ReadFromMemory(originalPC, false), Times.Once);
      _mockExecutor.Verify(mock => mock.BreakInterrupt(), Times.Once);
      _mockAddressing.Verify(mock => mock.Immediate(), Times.Once);
      _mockInstructionsTable.Verify(mock => mock.GetInstructionForOpCode(expectedOpCode), Times.Once);
    }

    [Fact]
    public void TheCpuMustExtendTheInstructionCyclesShouldTheAddressingModeRequestSo() {
      ushort originalPC = _subject.ProgramCounter;
      byte expectedOpCode = 10;
      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(originalPC, false))
        .Returns(expectedOpCode);

      _mockExecutor
        .Setup(mock => mock.BreakInterrupt())
        .Returns(false);

      _mockAddressing
        .Setup(mock => mock.Immediate())
        .Returns(true);

      var expectedInstruction = new CpuInstruction("BRK", _mockExecutor.Object.BreakInterrupt, _mockAddressing.Object.Immediate, 2);
      _mockInstructionsTable
        .Setup(mock => mock.GetInstructionForOpCode(expectedOpCode))
        .Returns(expectedInstruction)
        .Verifiable();

      _subject.OnClockTick();
      _subject.OnClockTick();
      _subject.OnClockTick();
      _mockDataBus.Verify(mock => mock.ReadFromMemory(originalPC, false), Times.Once);
      _mockExecutor.Verify(mock => mock.BreakInterrupt(), Times.Once);
      _mockAddressing.Verify(mock => mock.Immediate(), Times.Once);
      _mockInstructionsTable.Verify(mock => mock.GetInstructionForOpCode(expectedOpCode), Times.Once);
    }

    [Fact]
    public void TheCpuMustExecuteAnotherInstructionOnceCyclesAreComplete() {
      ushort originalPC = _subject.ProgramCounter;
      ushort secondExpectedPC = (ushort)(originalPC + 1);

      byte expectedOpCode = 10;
      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(originalPC, false))
        .Returns(expectedOpCode);
      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(secondExpectedPC, false))
        .Returns(expectedOpCode);

      _mockExecutor
        .Setup(mock => mock.BreakInterrupt())
        .Returns(false);

      _mockAddressing
        .Setup(mock => mock.Immediate())
        .Returns(false);

      var expectedInstruction = new CpuInstruction("BRK", _mockExecutor.Object.BreakInterrupt, _mockAddressing.Object.Immediate, 2);
      _mockInstructionsTable
        .Setup(mock => mock.GetInstructionForOpCode(expectedOpCode))
        .Returns(expectedInstruction)
        .Verifiable();

      _subject.OnClockTick();
      _subject.OnClockTick();
      _subject.OnClockTick();
      _mockDataBus.Verify(mock => mock.ReadFromMemory(originalPC, false), Times.Once);
      _mockDataBus.Verify(mock => mock.ReadFromMemory(secondExpectedPC, false), Times.Once);
      _mockExecutor.Verify(mock => mock.BreakInterrupt(), Times.Exactly(2));
      _mockAddressing.Verify(mock => mock.Immediate(), Times.Exactly(2));
      _mockInstructionsTable.Verify(mock => mock.GetInstructionForOpCode(expectedOpCode), Times.Exactly(2));

      Check.That(_subject.ProgramCounter).IsEqualTo(originalPC + 2);
    }
  }
}
