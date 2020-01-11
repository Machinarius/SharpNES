using Microsoft.Extensions.Logging;
using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.CPU.Internal;
using Xunit;

namespace SharpNES.Core.Tests.CPU.Internals.MemoryAddressingModes {
  public class MemoryAddressingModesTests {
    private readonly MOS6502CpuMemoryAddressingModes _subject;

    private readonly Mock<INESCpu> _mockCpu;

    public MemoryAddressingModesTests() {
      _mockCpu = new Mock<INESCpu>(MockBehavior.Strict);
      _subject = new MOS6502CpuMemoryAddressingModes(_mockCpu.Object,
        new Mock<ILogger<MOS6502CpuMemoryAddressingModes>>(MockBehavior.Loose).Object);
    }

    [Fact]
    public void ImplicitAddressingModeMustSetTheALUInputToTheValueOfTheAccumulator() {
      byte expectedAccumulator = 123;
      _mockCpu
        .SetupGet(mock => mock.AccumulatorRegister)
        .Returns(expectedAccumulator);
      _mockCpu.SetupSet(mock => mock.ALUInputRegister = expectedAccumulator)
        .Verifiable();

      var requiresMoreCycles = _subject.Implicit();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void ImmediateAddressingModeMustSetTheAbsoluteAddressToThePCValueAndIncrementIt() {
      ushort originalPCValue = 123;
      _mockCpu
        .SetupGet(mock => mock.ProgramCounter)
        .Returns(originalPCValue);

      ushort expectedPCValue = originalPCValue++;
      _mockCpu
        .SetupSet(mock => mock.ProgramCounter = originalPCValue)
        .Verifiable();
      _mockCpu
        .SetupSet(mock => mock.AbsoluteAddress = expectedPCValue)
        .Verifiable();
      
      var requiresMoreCyles = _subject.Immediate();
      Check.That(requiresMoreCyles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void ZeroPageZeroAddressingMustReadFromMemoryAtPCAndSetTheAbsoluteAddressToTheLowestByte() {
      ushort mockPc = 0x4321;
      ushort expectedPc = 0x4322;
      byte valueAtPc = 0x23;

      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc))
        .Returns(valueAtPc);
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);
      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = valueAtPc)
        .Verifiable();

      var requiresMoreCycles = _subject.ZeroPageZero();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void ZeroPageXAddressingMustReadFromMemoryAtPCAndSetTheAbsoluteAddressToTheLowestBytePlusXRegister() {
      ushort mockPc = 0x4321;
      ushort expectedPc = 0x4322;
      byte xRegister = 0x10;
      byte valueAtPc = 0x45;
      byte expectedAddress = 0x55;

      _mockCpu.SetupGet(mock => mock.XRegister).Returns(xRegister);
      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc))
        .Returns(valueAtPc);
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);
      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.ZeroPageX();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void ZeroPageYAddressingMustReadFromMemoryAtPCAndSetTheAbsoluteAddressToTheLowestBytePlusYRegister() {
      ushort mockPc = 0x4321;
      ushort expectedPc = 0x4322;
      byte yRegister = 0x10;
      byte valueAtPc = 0x45;
      byte expectedAddress = 0x55;

      _mockCpu.SetupGet(mock => mock.YRegister).Returns(yRegister);
      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc))
        .Returns(valueAtPc);
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);
      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.ZeroPageY();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void RelativeAddressingModeMustSetTheRelativeAddressToTheValueAtPCValueAndAdvanceThePC() {
      ushort mockPc = 0x4321;
      ushort expectedPc = 0x4322;
      byte valueAtPc = 0x45;

      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc))
        .Returns(valueAtPc);
      _mockCpu.SetupProperty(mock => mock.RelativeAddress);
      _mockCpu.SetupSet(mock => mock.RelativeAddress = valueAtPc)
        .Verifiable();

      var requiresMoreCycles = _subject.Relative();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void RelativeAddressingMustSetTheHighestBitsOfTheRelativeAddressIfItsOutsideThe8BitRange() {
      ushort mockPc = 0x4321;
      ushort expectedPc = 0x4322;
      byte valueAtPc = 0x81;
      ushort expectedAddress = 0xFF81;

      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc))
        .Returns(valueAtPc);
      _mockCpu.SetupProperty(mock => mock.RelativeAddress);
      _mockCpu.SetupSet(mock => mock.RelativeAddress = expectedAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.Relative();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void AbsoluteModeMustSetTheAbsoluteAddressToTheComponentsAtTheNextTwoValuesOfPc() {
      ushort firstMockPc = 0x2376;
      ushort secondMockPc = 0x2377;
      ushort expectedPc = 0x2378;

      byte firstValueAtPc = 0x19;
      byte secondValueAtPc = 0x20;
      ushort expectedAbsoluteAddress = 0x2019;

      _mockCpu.SetupProperty(mock => mock.ProgramCounter, firstMockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(firstMockPc)).Returns(firstValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(secondMockPc)).Returns(secondValueAtPc);
      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);

      var requiresMoreCycles = _subject.Absolute();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();

      Check.That(_mockCpu.Object.AbsoluteAddress).IsEqualTo(expectedAbsoluteAddress);
    }

    [Fact]
    public void AbsoluteModeXMustSetTheAbsoluteAddressToTheComponentsAtTheNextTwoValuesOfPcPlusXRegister() {
      ushort firstMockPc = 0x2376;
      ushort secondMockPc = 0x2377;
      ushort expectedPc = 0x2378;

      byte firstValueAtPc = 0x19;
      byte secondValueAtPc = 0x20;
      ushort expectedAbsoluteAddress = 0x201A;

      byte mockXRegister = 0x1;

      _mockCpu.SetupGet(mock => mock.XRegister).Returns(mockXRegister);

      _mockCpu.SetupProperty(mock => mock.ProgramCounter, firstMockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(firstMockPc)).Returns(firstValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(secondMockPc)).Returns(secondValueAtPc);

      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);

      var requiresMoreCycles = _subject.AbsoluteX();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();

      Check.That(_mockCpu.Object.AbsoluteAddress).IsEqualTo(expectedAbsoluteAddress);
    }

    [Fact]
    public void AbsoluteModeXMustRequireAnAdditionalClockCylceIfTheresAPageChange() {
      ushort firstMockPc = 0x2376;
      ushort secondMockPc = 0x2377;
      ushort expectedPc = 0x2378;

      byte firstValueAtPc = 0xFF;
      byte secondValueAtPc = 0x01;
      ushort expectedAbsoluteAddress = 0x027F;

      byte mockXRegister = 0x80;

      _mockCpu.SetupGet(mock => mock.XRegister).Returns(mockXRegister);

      _mockCpu.SetupProperty(mock => mock.ProgramCounter, firstMockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(firstMockPc)).Returns(firstValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(secondMockPc)).Returns(secondValueAtPc);

      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);

      var requiresMoreCycles = _subject.AbsoluteX();
      Check.That(requiresMoreCycles).IsTrue();
      _mockCpu.Verify();

      Check.That(_mockCpu.Object.AbsoluteAddress).IsEqualTo(expectedAbsoluteAddress);
    }

    [Fact]
    public void AbsoluteModeYMustSetTheAbsoluteAddressToTheComponentsAtTheNextTwoValuesOfPcPlusYRegister() {
      ushort firstMockPc = 0x2376;
      ushort secondMockPc = 0x2377;
      ushort expectedPc = 0x2378;

      byte firstValueAtPc = 0x19;
      byte secondValueAtPc = 0x20;
      ushort expectedAbsoluteAddress = 0x201A;

      byte mockYRegister = 0x1;

      _mockCpu.SetupGet(mock => mock.YRegister).Returns(mockYRegister);

      _mockCpu.SetupProperty(mock => mock.ProgramCounter, firstMockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(firstMockPc)).Returns(firstValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(secondMockPc)).Returns(secondValueAtPc);

      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);

      var requiresMoreCycles = _subject.AbsoluteY();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();

      Check.That(_mockCpu.Object.AbsoluteAddress).IsEqualTo(expectedAbsoluteAddress);
    }

    [Fact]
    public void AbsoluteModeYMustRequireAnAdditionalClockCylceIfTheresAPageChange() {
      ushort firstMockPc = 0x2376;
      ushort secondMockPc = 0x2377;
      ushort expectedPc = 0x2378;

      byte firstValueAtPc = 0xFF;
      byte secondValueAtPc = 0x01;
      ushort expectedAbsoluteAddress = 0x027F;

      byte mockYRegister = 0x80;

      _mockCpu.SetupGet(mock => mock.YRegister).Returns(mockYRegister);

      _mockCpu.SetupProperty(mock => mock.ProgramCounter, firstMockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(firstMockPc)).Returns(firstValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(secondMockPc)).Returns(secondValueAtPc);

      _mockCpu.SetupProperty(mock => mock.AbsoluteAddress);

      var requiresMoreCycles = _subject.AbsoluteY();
      Check.That(requiresMoreCycles).IsTrue();
      _mockCpu.Verify();

      Check.That(_mockCpu.Object.AbsoluteAddress).IsEqualTo(expectedAbsoluteAddress);
    }

    [Fact]
    public void IndirectModeMustReadTheMemoryPointedToByTheNextToPcValues() {
      ushort firstMockPc = 0x2376;
      ushort secondMockPc = 0x2377;
      ushort expectedPc = 0x2378;

      byte firstValueAtPc = 0x01;
      byte secondValueAtPc = 0x02;
      ushort expectedPointer = 0x0201;
      ushort expectedPointerOffset = 0x0202;

      byte pointerValue = 0x19;
      byte pointerOffsetValue = 0x20;
      ushort expectedAbsoluteAddress = 0x2019;

      _mockCpu.SetupProperty(mock => mock.ProgramCounter, firstMockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(firstMockPc)).Returns(firstValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(secondMockPc)).Returns(secondValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedPointer)).Returns(pointerValue);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedPointerOffset)).Returns(pointerOffsetValue);

      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAbsoluteAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.Indirect();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void IndirectModeMustAccuratelyReproduceAHardwareBugWhenAPageBoundaryIsCrossed() {
      ushort firstMockPc = 0x2376;
      ushort secondMockPc = 0x2377;
      ushort expectedPc = 0x2378;

      byte firstValueAtPc = 0xFF;
      byte secondValueAtPc = 0x02;
      ushort expectedPointer = 0x02FF;
      ushort expectedHighAddress = 0x0200;

      byte pointerValue = 0x19;
      byte highAddressValue = 0x20;
      ushort expectedAbsoluteAddress = 0x2019;

      _mockCpu.SetupProperty(mock => mock.ProgramCounter, firstMockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(firstMockPc)).Returns(firstValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(secondMockPc)).Returns(secondValueAtPc);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedPointer)).Returns(pointerValue);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedHighAddress)).Returns(highAddressValue);

      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAbsoluteAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.Indirect();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void IndirectXModeMustReadTheMemoryAtPcAndTreatTheContentsAsAPointerBaseWithXRegisterAsTheOffset() {
      ushort mockPc = 0x1276;
      ushort expectedPc = 0x1277;

      byte mockPointerBase = 0x63;
      byte mockXRegister = 0x54;
      byte expectedLowAddress = 0xB7;
      byte expectedHighAddress = 0xB8;

      byte lowValue = 0x19;
      byte highValue = 0x20;
      ushort expectedAbsoluteAddress = 0x2019;

      _mockCpu.SetupGet(mock => mock.XRegister).Returns(mockXRegister);
      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc)).Returns(mockPointerBase);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedLowAddress)).Returns(lowValue);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedHighAddress)).Returns(highValue);

      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAbsoluteAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.IndirectX();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void IndirectXModeMayOnlyReadValuesOffTheFirstMemoryPage() {
      ushort mockPc = 0x1276;
      ushort expectedPc = 0x1277;

      byte mockPointerBase = 0xFA;
      byte mockXRegister = 0x29;
      byte expectedLowAddress = 0x23;
      byte expectedHighAddress = 0x24;

      byte lowValue = 0x19;
      byte highValue = 0x20;
      ushort expectedAbsoluteAddress = 0x2019;

      _mockCpu.SetupGet(mock => mock.XRegister).Returns(mockXRegister);
      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc)).Returns(mockPointerBase);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedLowAddress)).Returns(lowValue);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedHighAddress)).Returns(highValue);

      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAbsoluteAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.IndirectX();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void IndirectYModeMustReadTheMemoryAtPcAndTreatTheContentsAsAPointerToLoadAbsAddressAndAddYRegister() {
      ushort mockPc = 0x1276;
      ushort expectedPc = 0x1277;

      byte mockPointerBase = 0x63;
      byte expectedHighAddress = 0x64;
      byte mockYRegister = 0x54;

      byte lowValue = 0x19;
      byte highValue = 0x20;
      ushort expectedAbsoluteAddress = 0x206D; // HEX: 2019 + 54

      _mockCpu.SetupGet(mock => mock.YRegister).Returns(mockYRegister);
      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc)).Returns(mockPointerBase);
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPointerBase)).Returns(lowValue);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedHighAddress)).Returns(highValue);

      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAbsoluteAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.IndirectY();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void IndirectYModeMayOnlyReadValuesBasedOffTheFirstMemoryPage() {
      ushort mockPc = 0x1276;
      ushort expectedPc = 0x1277;

      byte mockPointerBase = 0xFF;
      byte expectedHighAddress = 0x00;
      byte mockYRegister = 0x54;

      byte lowValue = 0x19;
      byte highValue = 0x20;
      ushort expectedAbsoluteAddress = 0x206D; // HEX: 2019 + 54

      _mockCpu.SetupGet(mock => mock.YRegister).Returns(mockYRegister);
      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc)).Returns(mockPointerBase);
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPointerBase)).Returns(lowValue);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedHighAddress)).Returns(highValue);

      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAbsoluteAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.IndirectY();
      Check.That(requiresMoreCycles).IsFalse();
      _mockCpu.Verify();
    }

    [Fact]
    public void IndirectYModeMustRequestAnAdditionalClockCycleIfAPageChangeOccurs() {
      ushort mockPc = 0x1276;
      ushort expectedPc = 0x1277;

      byte mockPointerBase = 0x63;
      byte expectedHighAddress = 0x64;
      byte mockYRegister = 0x25;

      byte lowValue = 0xFA;
      byte highValue = 0x20;
      ushort expectedAbsoluteAddress = 0x211F; // HEX: 20FA + 25

      _mockCpu.SetupGet(mock => mock.YRegister).Returns(mockYRegister);
      _mockCpu.SetupGet(mock => mock.ProgramCounter).Returns(mockPc);
      _mockCpu.SetupSet(mock => mock.ProgramCounter = expectedPc)
        .Verifiable();

      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPc)).Returns(mockPointerBase);
      _mockCpu.Setup(mock => mock.ReadFromMemory(mockPointerBase)).Returns(lowValue);
      _mockCpu.Setup(mock => mock.ReadFromMemory(expectedHighAddress)).Returns(highValue);

      _mockCpu.SetupSet(mock => mock.AbsoluteAddress = expectedAbsoluteAddress)
        .Verifiable();

      var requiresMoreCycles = _subject.IndirectY();
      Check.That(requiresMoreCycles).IsTrue();
      _mockCpu.Verify();
    }
  }
}
