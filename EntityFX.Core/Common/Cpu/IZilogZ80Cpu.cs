using System.Xml.Serialization;
using EntityFX.Core.Common.Cpu.Registers;

namespace EntityFX.Core.Common.Cpu
{

    public interface IZilogZ80Cpu
    {
        IMemory Memory { get;  }
        IInputOutputDevice IO { get; }
        IRegisterFile Status { get; set; }
        int States { get; set; }

        int StatementsToFetch { get; set; }

        void Execute();
        void Interrupt();
        void Reset();

    }
}