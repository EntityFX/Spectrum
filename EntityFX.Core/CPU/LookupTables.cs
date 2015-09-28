using EntityFX.Core.Common.Cpu;
using EntityFX.Core.CPU.Registers;

namespace EntityFX.Core.CPU
{
    public class LookupTables : ILookupTables
    {
        private byte[] _halfcarryAdd = { 0, FlagRegisterDefinition.H, FlagRegisterDefinition.H, FlagRegisterDefinition.H, 0, 0, 0, FlagRegisterDefinition.H };
        private byte[] _halfcarrySub = { 0, 0, FlagRegisterDefinition.H, 0, FlagRegisterDefinition.H, 0, FlagRegisterDefinition.H, FlagRegisterDefinition.H };
        private byte[] _overflowAdd = { 0, 0, 0, FlagRegisterDefinition.V, FlagRegisterDefinition.V, 0, 0, 0 };
        private byte[] _overflowSub = { 0, FlagRegisterDefinition.V, 0, 0, 0, 0, FlagRegisterDefinition.V, 0 };
        private byte[] _sz53 = new byte[0x100];
        private byte[] _parity = new byte[0x100];
        private byte[] _sz53P = new byte[0x100];

        public byte[] HalfcarryAdd
        {
            get { return _halfcarryAdd; }
        }

        public byte[] HalfcarrySub
        {
            get { return _halfcarrySub; }
        }

        public byte[] OverflowAdd
        {
            get { return _overflowAdd; }
        }

        public byte[] OverflowSub
        {
            get { return _overflowSub; }
        }

        public byte[] Sz53
        {
            get { return _sz53; }
        }

        public byte[] Parity
        {
            get { return _parity; }
        }

        public byte[] Sz53P
        {
            get { return _sz53P; }
        }

        public void Init()
        {
            int i, j, k;
            byte parity;

            for (i = 0; i < 0x100; i++)
            {
                Sz53[i] = (byte)(i & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5 | FlagRegisterDefinition.S));

                j = i;
                parity = 0;
                for (k = 0; k < 8; k++)
                {
                    parity ^= (byte)(j & 1);
                    j >>= 1;
                }
                Parity[i] = (parity != 0 ? (byte)0 : FlagRegisterDefinition.P);
                Sz53P[i] = (byte)(Sz53[i] | Parity[i]);
            }

            Sz53[0] |= FlagRegisterDefinition.Z;
            Sz53P[0] |= FlagRegisterDefinition.Z;
        }
    }
}