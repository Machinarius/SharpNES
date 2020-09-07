using System;

namespace SharpNES.Core.CPU.Exceptions {
  public class AddressingModeException : Exception {
    public AddressingModeException(): this("The Addressing Mode is not valid for the desired operation") { }
    public AddressingModeException(string message): base(message) { }
  }
}
