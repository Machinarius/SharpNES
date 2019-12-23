using Moq;
using NFluent;
using SharpNES.Core.CPU;
using SharpNES.Core.DataBus;
using Xunit;

namespace SharpNES.Core.Tests.DataBus {
  public class ByteArrayBackedDataBusTests {
    private readonly ByteArrayBackedDataBus _subject;

    private readonly Mock<INESCpu> _mockCpu; 

    public ByteArrayBackedDataBusTests() {
      _mockCpu = new Mock<INESCpu>(MockBehavior.Loose);
      _subject = new ByteArrayBackedDataBus(_mockCpu.Object);
    }

    [Fact]
    public void TheDataBusMustAnnounceItselfToTheCpuImmediately() {
      _mockCpu.Verify(mock => mock.ConnectToDataBus(_subject), Times.Once);
    }

    [Theory]
    [InlineData(0xFFFF)]
    public void ReadingFromAValidAddressThatHadDataWrittenToMustReturnTheWrittenData(ushort address) {
      _subject.WriteToMemory(address, 123);
      Check.That(_subject.ReadFromMemory(address)).IsEqualTo(123);
    }
  }
}
