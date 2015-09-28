using EntityFX.Core.Common.Cpu.Registers;

namespace EntityFX.Core.Common.Cpu
{
    public interface ICpuStack
    {
        void Push(WordRegister register);

        void Push(byte @byte);

        void Push(ushort word);

        void Pop(WordRegister register);

        void Pop(out byte @byte);

        void Pop(out ushort word);
    }
}