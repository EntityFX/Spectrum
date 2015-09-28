using System.Diagnostics;
using EntityFX.Core.Common;
using EntityFX.Core.Common.Cpu;
using EntityFX.Core.Common.Cpu.Registers;
using EntityFX.Core.CPU.Registers;

namespace EntityFX.Core.CPU
{
    public class Alu : IAlu
    {
        private readonly IMemory _memory;
        private readonly ILookupTables _lookupTables;
        private readonly IRegisterFile _registerFile;
        private readonly ICpuStack _cpuStack;
        private int _tstates;

        public Alu(IMemory memory, IRegisterFile registerFile, ICpuStack cpuStack, ILookupTables lookupTables)
        {
            _memory = memory;
            _lookupTables = lookupTables;
            _registerFile = registerFile;
            _cpuStack = cpuStack;
        }

        public int CpuTicks
        {
            get { return _tstates; }
            set { _tstates = value; }
        }

        public void AND_r(byte op)
        {
            _registerFile.A &= op;
            _registerFile.F = (byte)
                (FlagRegisterDefinition.H |
                _lookupTables.Sz53P[_registerFile.A]);
        }

        public void ADC_A_r(byte op)
        {
            ushort result = (ushort)(_registerFile.A + op + ((_registerFile.F & FlagRegisterDefinition.C) != 0 ? 1 : 0));
            // Prepare the bits to perform the lookup
            byte lookup = (byte)(((_registerFile.A & 0x88) >> 3) | ((op & 0x88) >> 2) | ((result & 0x88) >> 1));
            _registerFile.A = (byte)result;

            _registerFile.F = (byte)
                (((result & 0x100) != 0 ? FlagRegisterDefinition.C : (byte)0) |
                _lookupTables.HalfcarryAdd[lookup & 0x07] |
                _lookupTables.OverflowAdd[lookup >> 4] |
                _lookupTables.Sz53[_registerFile.A]);
        }

        public void ADC_HL(ushort op)
        {
            uint result = (uint)(_registerFile.HL + op + ((_registerFile.F & FlagRegisterDefinition.C) != 0 ? 1 : 0));
            byte lookup = (byte)(
                (byte)((_registerFile.HL & 0x8800) >> 11) |
                (byte)((op & 0x8800) >> 10) |
                (byte)((result & 0x8800) >> 9));
            _registerFile.HL = (ushort)result;
            _registerFile.F = (byte)
                (((result & 0x10000) != 0 ? FlagRegisterDefinition.C : (byte)0) |
                _lookupTables.OverflowAdd[lookup >> 4] |
                (_registerFile.H & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5 | FlagRegisterDefinition.S)) |
                _lookupTables.HalfcarryAdd[lookup & 0x07] |
                (_registerFile.HL == 0 ? (byte)0 : FlagRegisterDefinition.Z));
        }

        public void ADD_A_r(byte op)
        {
            ushort result = (ushort)(_registerFile.A + op);
            byte lookup = (byte)(((_registerFile.A & 0x88) >> 3) | (((op) & 0x88) >> 2) | ((result & 0x88) >> 1));
            _registerFile.A = (byte)result;
            _registerFile.F = (byte)
                (((result & 0x100) != 0 ? FlagRegisterDefinition.C : (byte)0) |
                _lookupTables.HalfcarryAdd[lookup & 0x07] |
                _lookupTables.OverflowAdd[lookup >> 4] |
                _lookupTables.Sz53[_registerFile.A]);
        }

        public void ADD_16(WordRegister op1, ushort op2)
        {
            uint result = (uint)(op1.Word + op2);
            byte lookup = (byte)(
                (byte)((op1.Word & 0x0800) >> 11) |
                (byte)((op2 & 0x0800) >> 10) |
                (byte)((result & 0x0800) >> 9));
            op1.Word = (ushort)result;
            _registerFile.F = (byte)(
                (_registerFile.F & (FlagRegisterDefinition.V | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) |
                ((result & 0x10000) != 0 ? FlagRegisterDefinition.C : (byte)0) |
                (byte)((result >> 8) & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)) |
                _lookupTables.HalfcarryAdd[lookup]);
        }

        public void DAA()
        {
            byte add = 0;
            byte carry = (byte)(_registerFile.F & FlagRegisterDefinition.C);
            if ((_registerFile.F & FlagRegisterDefinition.H) != 0 || ((_registerFile.A & 0x0F) > 9))
                add = 6;
            if (carry != 0 || (_registerFile.A > 0x9F))
                add |= 0x60;
            if (_registerFile.A > 0x99)
                carry = 1;
            if ((_registerFile.F & FlagRegisterDefinition.N) != 0)
            {
                SUB_r(add);
            }
            else
            {
                if ((_registerFile.A > 0x90) && ((_registerFile.A & 0x0F) > 9))
                    add |= 0x60;
                ADD_A_r(add);
            }
            _registerFile.F = (byte)((_registerFile.F & ~(FlagRegisterDefinition.C | FlagRegisterDefinition.P)) | carry | _lookupTables.Parity[_registerFile.A]);
        }

        public void DEC(ByteRegister op)
        {
            _registerFile.F = (byte)((_registerFile.F & FlagRegisterDefinition.C) | ((op.Value & 0x0F) != 0 ? (byte)0 : FlagRegisterDefinition.H) | FlagRegisterDefinition.N);
            op.Value--;
            _registerFile.F |= (byte)((op.Value == 0x79 ? FlagRegisterDefinition.V : (byte)0) | _lookupTables.Sz53[op.Value]);
        }

        public void INC(ByteRegister op)
        {
            op.Value++;
            _registerFile.F = (byte)(
                (_registerFile.F & FlagRegisterDefinition.C) |
                (op.Value == 0x80 ? FlagRegisterDefinition.V : (byte)0) |
                ((op.Value & 0x0F) != 0 ? (byte)0 : FlagRegisterDefinition.H) |
                (op.Value != 0 ? (byte)0 : FlagRegisterDefinition.Z) |
                _lookupTables.Sz53[op.Value]);
        }

        public void OR_r(byte op)
        {
            _registerFile.A |= op;
            _registerFile.F = _lookupTables.Sz53P[_registerFile.A];
        }

        public void RL(ByteRegister op)
        {
            byte _op = op.Value;
            op.Value = (byte)((op.Value << 1) | (_registerFile.F & FlagRegisterDefinition.C));
            _registerFile.F = (byte)((_op >> 7) | _lookupTables.Sz53P[op.Value]);
        }

        public void RLA()
        {
            byte _A = _registerFile.A;
            _registerFile.A = (byte)((_registerFile.A << 1) | (_registerFile.F & FlagRegisterDefinition.C));
            _registerFile.F = (byte)(
                (_registerFile.F & (FlagRegisterDefinition.P | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) |
                (_registerFile.A & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)) |
                (_A >> 7));
        }

        public void RLCA()
        {
            _registerFile.A = (byte)((_registerFile.A << 1) | (_registerFile.A >> 7));
            _registerFile.F = (byte)(
                (_registerFile.F & (FlagRegisterDefinition.P | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) |
                (_registerFile.A & (FlagRegisterDefinition.C | FlagRegisterDefinition._3 | FlagRegisterDefinition._5)));
        }

        public void RLC(ByteRegister op)
        {
            op.Value = (byte)((op.Value << 1) | (op.Value >> 7));
            _registerFile.F = (byte)(
                (op.Value & FlagRegisterDefinition.C) |
                _lookupTables.Sz53P[op.Value]);
        }

        public void RLD()
        {
            byte _b = _memory.ReadByte(_registerFile.HL);
            _memory.WriteByte(_registerFile.HL, (byte)((_b << 4) | (_registerFile.A & 0x0F)));
            _registerFile.A = (byte)((_registerFile.A & 0xF0) | (_b >> 4));
            _registerFile.F = (byte)((_registerFile.F & FlagRegisterDefinition.C) | _lookupTables.Sz53P[_registerFile.A]);
        }

        public void RR(ByteRegister op)
        {
            byte _op = op.Value;
            op.Value = (byte)((op.Value >> 1) | (_registerFile.F << 7));
            _registerFile.F = (byte)(
                (_op & FlagRegisterDefinition.C) |
                _lookupTables.Sz53P[op.Value]);
        }

        public void RRA()
        {
            byte _A = _registerFile.A;
            _registerFile.A = (byte)((_registerFile.A >> 1) | (_registerFile.F << 7));
            _registerFile.F = (byte)(
                (_registerFile.F & (FlagRegisterDefinition.P | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) |
                (_registerFile.A & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)) |
                (_A & FlagRegisterDefinition.C));
        }

        public void RRC(ByteRegister op)
        {
            _registerFile.F = (byte)(op.Value & FlagRegisterDefinition.C);
            op.Value = (byte)((op.Value >> 1) | (op.Value << 7));
            _registerFile.F |= _lookupTables.Sz53P[op.Value];
        }

        public void RRCA()
        {
            _registerFile.F = (byte)((_registerFile.F & (FlagRegisterDefinition.P | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) | (_registerFile.A & FlagRegisterDefinition.C));
            _registerFile.A = (byte)((_registerFile.A >> 1) | (_registerFile.A << 7));
            _registerFile.F |= (byte)(_registerFile.A & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5));
        }

        public void RRD()
        {
            byte _b = _memory.ReadByte(_registerFile.HL);
            _memory.WriteByte(_registerFile.HL, (byte)((_registerFile.A << 4) | (_b >> 4)));
            _registerFile.A = (byte)((_registerFile.A & 0xF0) | (_b & 0x0F));
            _registerFile.F = (byte)((_registerFile.F & FlagRegisterDefinition.C) | _lookupTables.Sz53P[_registerFile.A]);
        }

        public void RST(byte op)
        {
            _cpuStack.Push(_registerFile.PC);
            _registerFile.PC = op;
        }

        public void SBC_A_r(byte op)
        {
            ushort result = (ushort)(_registerFile.A - op - (_registerFile.F & FlagRegisterDefinition.C));
            byte lookup = (byte)(((_registerFile.A & 0x88) >> 3) | ((op & 0x88) >> 2) | ((result & 0x88) >> 1));
            _registerFile.A = (byte)result;
            _registerFile.F = (byte)(((result & 0x100) != 0 ? FlagRegisterDefinition.C : (byte)0) | FlagRegisterDefinition.N | _lookupTables.HalfcarrySub[lookup & 0x07] | _lookupTables.OverflowSub[lookup >> 4] | _lookupTables.Sz53[_registerFile.A]);
        }

        public void SBC_HL(ushort op)
        {
            uint result = (uint)(_registerFile.HL - op - ((_registerFile.F & FlagRegisterDefinition.C) != 0 ? 1 : 0));
            byte lookup = (byte)((byte)((_registerFile.HL & 0x8800) >> 11) | (byte)((op & 0x8800) >> 10) | (byte)((result & 0x8800) >> 9));
            _registerFile.HL = (ushort)result;
            _registerFile.F = (byte)(((result & 0x10000) != 0 ? FlagRegisterDefinition.C : (byte)0) | FlagRegisterDefinition.N | _lookupTables.OverflowSub[lookup >> 4] | (_registerFile.H & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5 | FlagRegisterDefinition.S)) | _lookupTables.HalfcarrySub[lookup & 0x07] | (_registerFile.HL != 0 ? (byte)0 : FlagRegisterDefinition.Z));
        }

        public void SLA(ByteRegister op)
        {
            _registerFile.F = (byte)(op.Value >> 7);
            op.Value <<= 1;
            _registerFile.F |= _lookupTables.Sz53P[op.Value];
        }

        public void SLL(ByteRegister op)
        {
            _registerFile.F = (byte)(op.Value >> 7);
            op.Value = (byte)((op.Value << 1) | 0x01);
            _registerFile.F |= _lookupTables.Sz53P[op.Value];
        }

        public void SRA(ByteRegister op)
        {
            _registerFile.F = (byte)(op.Value & FlagRegisterDefinition.C);
            op.Value = (byte)((op.Value & 0x80) | (op.Value >> 1));
            _registerFile.F |= _lookupTables.Sz53P[op.Value];
        }

        public void SRL(ByteRegister op)
        {
            _registerFile.F = (byte)(op.Value & FlagRegisterDefinition.C);
            op.Value >>= 1;
            _registerFile.F |= _lookupTables.Sz53P[op.Value];
        }

        public void SUB_r(byte op)
        {
            ushort result = (ushort)(_registerFile.A - op);
            byte lookup = (byte)(((_registerFile.A & 0x88) >> 3) | ((op & 0x88) >> 2) | ((result & 0x88) >> 1));
            _registerFile.A = (byte)result;
            _registerFile.F = (byte)(((result & 0x100) != 0 ? FlagRegisterDefinition.C : (byte)0) | FlagRegisterDefinition.N | _lookupTables.HalfcarrySub[lookup & 0x07] | _lookupTables.OverflowSub[lookup >> 4] | _lookupTables.Sz53[_registerFile.A]);
        }

        public void XOR_r(byte op)
        {
            _registerFile.A ^= op;
            _registerFile.F = _lookupTables.Sz53P[_registerFile.A];
        }
    }
}