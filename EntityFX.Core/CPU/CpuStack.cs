using EntityFX.Core.Common;
using EntityFX.Core.Common.Cpu;
using EntityFX.Core.Common.Cpu.Registers;

namespace EntityFX.Core.CPU
{
    public class CpuStack : ICpuStack
    {
        private readonly IMemory _memory;
        private readonly IRegisterFile _registerFile;

        public CpuStack(IMemory memory, IRegisterFile registerFile)
        {
            _memory = memory;
            _registerFile = registerFile;
        }

        public void Push(WordRegister register)
        {
            Push(register.Word);
        }

        public void Push(byte @byte)
        {
            _registerFile.SP--;
            _memory.WriteByte(_registerFile.SP, @byte);
        }

        public void Push(ushort word)
        {
            _registerFile.SP -= 2;
            _memory.WriteWord(_registerFile.SP, word);
        }

        public void Pop(WordRegister register)
        {
            ushort memoryContent;
            Pop(out memoryContent);
            register.Word = memoryContent;
        }

        public void Pop(out byte @byte)
        {
            @byte = _memory.ReadByte(_registerFile.SP);
            _registerFile.SP++;
        }

        public void Pop(out ushort word)
        {
            word = _memory.ReadWord(_registerFile.SP);
            _registerFile.SP += 2;
        }
    }
}