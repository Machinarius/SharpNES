using System;

namespace SharpNES.Core.CPU {
  public struct CpuInstruction {
    public string Name { get; set; }
    public Func<bool> OperatorFunc { get; set; }
    public Func<bool> AddressingModeFunc { get; set; }
    public byte CycleCount { get; set; }

    public bool AddressingModeIsImplicit { get; private set; }

    public CpuInstruction(string name, Func<bool> operatorFunc, Func<bool> addressingModeFunc, byte cycleCount) 
      : this(name , operatorFunc, addressingModeFunc, cycleCount, false) { }

    public CpuInstruction(string name, Func<bool> operatorFunc, Func<bool> addressingModeFunc, byte cycleCount, bool addrModeImp) {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      OperatorFunc = operatorFunc ?? throw new ArgumentNullException(nameof(operatorFunc));
      AddressingModeFunc = addressingModeFunc ?? throw new ArgumentNullException(nameof(addressingModeFunc));
      CycleCount = cycleCount;
      AddressingModeIsImplicit = addrModeImp;
    }
  }
}
