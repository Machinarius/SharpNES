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

    [Fact]
    public void ReadingFromTheBusThroughTheCpuMustCallForwardToTheBusWithReadOnlySetToFalse() {
      ushort mockAddress = 0x3541;
      byte expectedData = 0x43;

      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(mockAddress, false))
        .Returns(expectedData)
        .Verifiable();

      var actualData = _subject.ReadFromDataBus(mockAddress);
      Check.That(actualData).IsEqualTo(expectedData);

      _mockDataBus.Verify();
    }

    [Fact]
    public void WritingIntoTheDataBusThroughTheCpuMustCallThroughToTheBus() {
      ushort expectedAddress = 0x3541;
      byte expectedData = 0x54;

      _mockDataBus
        .Setup(mock => mock.WriteToMemory(expectedAddress, expectedData))
        .Verifiable();

      _subject.WriteToDataBus(expectedAddress, expectedData);
    }

    [Fact]
    public void ResettingTheCPUMustSetRegistersToKnownValues() {
      ushort lowPcAddress = 0xFFFC;
      ushort highPcAddress = 0xFFFD;
      byte pcValueLow = 0x19;
      byte pcValueHigh = 0x20;

      ushort expectedPcValue = 0x2019;

      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(lowPcAddress, It.IsAny<bool>()))
        .Returns(pcValueLow);

      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(highPcAddress, It.IsAny<bool>()))
        .Returns(pcValueHigh);

      _subject.Reset();

      // TODO: The MOS6502 reference states "the mask interrupt flag will be set" when a reset is complete,
      // but my aim is to replicage Javdix9's code for now; so the Status value is set to the Unused flag

      Check.That(_subject.ProgramCounter).IsEqualTo(expectedPcValue);
      Check.That(_subject.AccumulatorRegister).IsEqualTo(0);
      Check.That(_subject.XRegister).IsEqualTo(0);
      Check.That(_subject.YRegister).IsEqualTo(0);
      Check.That(_subject.StackPointer).IsEqualTo(0xFD);
      Check.That(_subject.StatusRegister).IsEqualTo(NESCpuFlags.Unused); // HasFlag could also work here?
      Check.That(_subject.AbsoluteAddress).IsEqualTo(0);
      Check.That(_subject.RelativeAddress).IsEqualTo(0);
      Check.That(_subject.ALUInputRegister).IsEqualTo(0);
      Check.That(_subject.ClockCyclesRemaining).IsEqualTo(8);
    }

    [Fact]
    public void InterruptRequestsMustBeIgnoredWhenTheDisableInterruptsFlagIsSet() {
      _subject.StatusRegister = NESCpuFlags.DisableInterrupts;
      _subject.OnInterruptRequested();
    }

    [Fact]
    public void InterruptRequestsMustStoreThePcAndStatusIntoTheStackAndSetThePcToTheValueInAKnownMemoryAddress() {
      // This flag is supposed to be cleared by the implementation
      _subject.StatusRegister = NESCpuFlags.Break; 
      _subject.ProgramCounter = 0x2019;
      _subject.StackPointer = 0x10;

      _mockDataBus
        .Setup(mock => mock.WriteToMemory(0x0110, 0x20))
        .Verifiable();

      _mockDataBus
        .Setup(mock => mock.WriteToMemory(0x010F, 0x19))
        .Verifiable();

      var expectedStatusValue = (byte)(NESCpuFlags.Unused | NESCpuFlags.DisableInterrupts);
      _mockDataBus
        .Setup(mock => mock.WriteToMemory(0x010E, expectedStatusValue))
        .Verifiable();

      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(0xFFFE, false))
        .Returns(0x37);

      _mockDataBus
        .Setup(mock => mock.ReadFromMemory(0xFFFF, false))
        .Returns(0x13);

      _subject.OnInterruptRequested();

      Check.That(_subject.ProgramCounter).IsEqualTo(0x1337);
      Check.That(_subject.StackPointer).IsEqualTo(0xD);
      Check.That(_subject.ClockCyclesRemaining).IsEqualTo(7);
      _mockDataBus.Verify();
    }
  }
}
