using System;
using System.Data.SqlClient;
using System.Diagnostics;
using EntityFX.Core.Common;
using EntityFX.Core.Common.Cpu;
using EntityFX.Core.Common.Cpu.Registers;
using EntityFX.Core.CPU.Registers;

namespace EntityFX.Core.CPU
{
    public class ExecutionUnit : IExecutionUnit, IAlu, ICpuStack
    {
        public const int EventNextEvent = 69888;
        private readonly IAlu _alu;
        private readonly ICpuStack _cpuStack;
        private readonly ILookupTables _lookupTables;
        private readonly IMemory _memory;
        private readonly IInputOutputDevice _inputOutputDevice;
        private readonly IRegisterFile _registerFile;
        private int _cpuTicks;

        // By default fetching won't be stopped
        private int _statementsToFetch = -1;

        public ExecutionUnit(IMemory memory, IRegisterFile registerFile, ICpuStack cpuStack, IAlu alu,
            IInputOutputDevice outputDevice, ILookupTables lookupTables)
        {
            _memory = memory;
            _registerFile = registerFile;
            _cpuStack = cpuStack;
            _alu = alu;
            _inputOutputDevice = outputDevice;
            _lookupTables = lookupTables;
        }

        public int StatementsToFetch
        {
            get { return _statementsToFetch; }
            set { _statementsToFetch = value; }
        }

        public int CpuTicks
        {
            get { return _cpuTicks; }
            set { _cpuTicks = value; }
        }


        public void BIT(byte bit, byte op)
        {
            _registerFile.F = (byte)(
                (_registerFile.F & FlagRegisterDefinition.C) |
                FlagRegisterDefinition.H |
                (op & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)) |
                ((op & (0x01 << bit)) != 0 ? 0 : (FlagRegisterDefinition.P | FlagRegisterDefinition.Z)));
        }

        public void BIT7(byte op)
        {
            _registerFile.F = (byte)(
                (_registerFile.F & FlagRegisterDefinition.C) |
                FlagRegisterDefinition.H |
                (op & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)) |
                ((op & 0x80) != 0 ? FlagRegisterDefinition.S : (byte)(FlagRegisterDefinition.P | FlagRegisterDefinition.Z)));
        }

        public void CALL_NN()
        {
            CpuTicks += 17;
            ushort address = _memory.ReadWord(_registerFile.PC);
            _cpuStack.Push((ushort)(_registerFile.PC + 2));
            _registerFile.PC = address;
        }

        public void CCF()
        {
            CpuTicks += 4;
            _registerFile.F = (byte)(
                (_registerFile.F & (FlagRegisterDefinition.P | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) |
                ((_registerFile.F & FlagRegisterDefinition.C) != 0 ? FlagRegisterDefinition.H : FlagRegisterDefinition.C) |
                (_registerFile.A & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)));
        }

        public void CP_r(byte op)
        {
            ushort result = (ushort)(_registerFile.A - op);
            byte lookup = (byte)((((_registerFile.A & 0x88) >> 3) | ((op & 0x88) >> 2) | ((result & 0x88) >> 1)));
            _registerFile.F = (byte)(
                ((result & 0x100) != 0 ? FlagRegisterDefinition.C : (result != 0 ? (byte)0 : FlagRegisterDefinition.Z)) |
                FlagRegisterDefinition.N |
                _lookupTables.HalfcarrySub[lookup & 0x07] |
                _lookupTables.OverflowSub[lookup >> 4] |
                (op & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)) |
                (result & FlagRegisterDefinition.S));
        }

        public void CPD()
        {
            CPx();
            _registerFile.HL--;
        }

        public void CPDR()
        {
            CPx();
            _registerFile.HL--;
            if ((_registerFile.F & (FlagRegisterDefinition.V | FlagRegisterDefinition.Z)) == FlagRegisterDefinition.V)
            {
                _cpuTicks += 5;
                _registerFile.PC -= 2;
            }
        }

        public void CPI()
        {
            CPx();
            _registerFile.HL++;
        }

        public void CPIR()
        {
            CPx();
            _registerFile.HL++;
            if ((_registerFile.F & (FlagRegisterDefinition.V | FlagRegisterDefinition.Z)) == FlagRegisterDefinition.V)
            {
                _cpuTicks += 5;
                _registerFile.PC -= 2;
            }
        }

        public void CPx()
        {
            byte _b = _memory.ReadByte(_registerFile.HL);
            byte result = (byte)(_registerFile.A - _b);
            byte lookup = (byte)(((_registerFile.A & 0x08) >> 3) | (((_b) & 0x08) >> 2) | ((result & 0x08) >> 1));
            _registerFile.BC--;
            _registerFile.F = (byte)((_registerFile.F & FlagRegisterDefinition.C) | (_registerFile.BC != 0 ? (byte)(FlagRegisterDefinition.V | FlagRegisterDefinition.N) : FlagRegisterDefinition.N) | _lookupTables.HalfcarrySub[lookup] | (result != 0 ? (byte)0 : FlagRegisterDefinition.Z) | (result & FlagRegisterDefinition.S));
            if ((_registerFile.F & FlagRegisterDefinition.H) != 0)
                result--;
            _registerFile.F |= (byte)((result & FlagRegisterDefinition._3) | ((result & 0x02) != 0 ? FlagRegisterDefinition._5 : (byte)0));
        }

        public void CPL()
        {
            CpuTicks += 4;
            _registerFile.A ^= 0xFF;
            _registerFile.F = (byte)((_registerFile.F & (FlagRegisterDefinition.C | FlagRegisterDefinition.P | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) | (_registerFile.A & (FlagRegisterDefinition._3 | FlagRegisterDefinition._5)) | (FlagRegisterDefinition.N | FlagRegisterDefinition.H));
        }

        public void IN(ByteRegister reg, ushort port)
        {
            reg.Value = _inputOutputDevice.ReadPort(port);
            _registerFile.F = (byte)((_registerFile.F & FlagRegisterDefinition.C) | _lookupTables.Sz53P[reg.Value]);
        }

        public void IND()
        {
            INx();
            _registerFile.HL--;
        }

        public void INDR()
        {
            INx();
            _registerFile.HL--;
            if (_registerFile.B != 0)
            {
                _cpuTicks += 5;
                _registerFile.PC -= 2;
            }
        }

        public void INI()
        {
            INx();
            _registerFile.HL++;
        }

        public void INIR()
        {
            INx();
            _registerFile.HL++;
            if (_registerFile.B != 0)
            {
                _cpuTicks += 5;
                _registerFile.PC -= 2;
            }
        }

        public void INx()
        {
            byte _b = _inputOutputDevice.ReadPort(_registerFile.BC);
            _memory.WriteByte(_registerFile.HL, _b);
            _registerFile.B--;
            _registerFile.F = (byte)(((_b & 0x80) != 0 ? FlagRegisterDefinition.N : (byte)0) | _lookupTables.Sz53[_registerFile.B]);
            Trace.TraceWarning("S,H and P/V flags not implemented");
        }

        public void LDD()
        {
            LDx();
            _registerFile.DE--;
            _registerFile.HL--;
        }

        public void LDDR()
        {
            LDx();
            _registerFile.HL--;
            _registerFile.DE--;
            if (_registerFile.BC != 0)
            {
                _cpuTicks += 4;
                _registerFile.PC -= 2;
            }
        }

        public void LDI()
        {
            LDx();
            _registerFile.DE++;
            _registerFile.HL++;
        }

        public void LDIR()
        {
            LDx();
            _registerFile.DE++;
            _registerFile.HL++;
            if (_registerFile.BC != 0)
            {
                _cpuTicks += 5;
                _registerFile.PC -= 2;
            }
        }

        public void LDx()
        {
            byte _b = _memory.ReadByte(_registerFile.HL);
            _memory.WriteByte(_registerFile.DE, _b);
            _registerFile.BC--;
            _b += _registerFile.A;
            _registerFile.F = (byte)((_registerFile.F & (FlagRegisterDefinition.C | FlagRegisterDefinition.Z | FlagRegisterDefinition.S)) | (_registerFile.BC != 0 ? FlagRegisterDefinition.V : (byte)0) | (_b & FlagRegisterDefinition._3) | ((_b & 0x02) != 0 ? FlagRegisterDefinition._5 : (byte)0));
        }

        public void LD_A_BCA()
        {
            CpuTicks += 7;
            _registerFile.A = _memory.ReadByte(_registerFile.BC);
        }

        public void LD_nndd(WordRegister register)
        {
            // Read write address from PC address
            ushort address = _memory.ReadWord(_registerFile.PC);
            _registerFile.PC += 2;

            _memory.WriteWord(address, register.Word);
        }

        public void LD_ddnn(WordRegister register)
        {
            // Read write address from PC address
            ushort address = _memory.ReadWord(_registerFile.PC);
            _registerFile.PC += 2;

            register.Word = _memory.ReadWord(address);
        }

        public void EX_AF_AFAlt()
        {
            CpuTicks += 4;
            // The 2-byte contents of the register pairs AF and AF are exchanged.
            // Register pair AF consists of registers A' and F'.


            // Tape saving trap: note this traps the EX AF,AF' at #04d0, not #04d1 as PC has already been incremented 
            if (_registerFile.PC == 0x04d1)
            {
                if (tape_save_trap() == 0)
                {
                    throw new Exception("EX Af, AF': saving trap at PC=0x04d1");
                }
            }
            _registerFile.RegisterAF.Swap(_registerFile.RegisterAFAlt);
        }

        public void RLCA()
        {
            CpuTicks += 4;
            _alu.RLCA();
        }

        public void JP()
        {
            CpuTicks += 10;
            _registerFile.PC = _memory.ReadWord(_registerFile.PC);
        }

        public void JR()
        {
            CpuTicks += 12;
            _registerFile.PC = (ushort)((int)_registerFile.PC + (sbyte)_memory.ReadByte(_registerFile.PC));
            _registerFile.PC++;
        }

        public void OUTD()
        {
            OUTx();
            _registerFile.HL--;
        }

        public void OTDR()
        {
            OUTx();
            _registerFile.HL--;
            if (_registerFile.B != 0)
            {
                _cpuTicks += 5;
                _registerFile.PC -= 2;
            }
        }

        public void OUTI()
        {
            OUTx();
            _registerFile.HL++;
        }

        public void OTIR()
        {
            OUTx();
            _registerFile.HL++;
            if (_registerFile.B != 0)
            {
                _cpuTicks += 5;
                _registerFile.PC -= 2;
            }
        }

        public void OUTx()
        {
            byte _b = _memory.ReadByte(_registerFile.HL);
            _registerFile.B--;
            _inputOutputDevice.WritePort(_registerFile.BC, _b);
            _registerFile.F = (byte)(((_b & 0x80) != 0 ? FlagRegisterDefinition.N : (byte)0) | _lookupTables.Sz53[_registerFile.B]);
            Trace.TraceWarning("S,H and P/V flags not implemented");
        }

        public void RET()
        {
            CpuTicks += 10;
            ushort pc;
            _cpuStack.Pop(out pc);
            _registerFile.PC = pc;
        }

        /// <summary>
        /// Main execution
        /// </summary>
        public void Execute()
        {
            ushort Address;


            while (CpuTicks < EventNextEvent)
            {

                byte opcode;

                // Check if Statement to fetch must be handled
                if (StatementsToFetch >= 0)
                {
                    if (StatementsToFetch == 0)
                    {
                        // Disable next break (just in case the main program forget to do it)
                        StatementsToFetch = -1;
                        return;
                    }
                    StatementsToFetch--;
                }

                // Check if someone is registered to receive Fetch event and eventually raise it
                if (OnFetch != null)
                    OnFetch();


                // If the z80 is HALTed, execute a NOP-equivalent and loop again
                if (_registerFile.Halted)
                {
                    CpuTicks += 4;
                    continue;
                }

                // Fetch next instruction
                opcode = _memory.ReadByte(_registerFile.PC++);

                // Increment refresh register
                _registerFile.R++;

                if (opcode == 0x76)     // HALT
                {
                    HALT();
                }
                else if ((opcode & 0xC0) == 0x40)   // 01 ddd rrr       LD d, r
                {
                    ByteRegister reg1 = GetByteRegisterByOpcode((byte)(opcode >> 3));
                    ByteRegister reg2 = GetByteRegisterByOpcode(opcode);

                    if (reg1 == null)
                    {
                        // The target is (HL)
                        CpuTicks += 7;
                        _memory.WriteByte(_registerFile.HL, reg2.Value);
                    }
                    else if (reg2 == null)
                    {
                        // The source is (HL)
                        CpuTicks += 7;
                        reg1.Value = _memory.ReadByte(_registerFile.HL);
                    }
                    else
                    {
                        // Source and target are normal registries
                        CpuTicks += 4;
                        reg1.Value = reg2.Value;
                    }
                }
                else if ((opcode & 0xC0) == 0x80)
                {
                    // Operation beetween accumulator and other registers
                    // Usually are identified by 10 ooo rrr where ooo is the operation and rrr is the source register
                    ByteRegister reg = GetByteRegisterByOpcode(opcode);
                    byte rrr;

                    if (reg == null)
                    {
                        // The source is (HL)
                        CpuTicks += 7;
                        rrr = _memory.ReadByte(_registerFile.HL);
                    }
                    else
                    {
                        // The source is a normal registry
                        CpuTicks += 4;
                        rrr = reg.Value;
                    }

                    switch (opcode & 0xF8)
                    {
                        case 0x80:  // 10 000 rrr       ADD A,r
                            _alu.ADD_A_r(rrr);
                            break;
                        case 0x88:  // 10 001 rrr       ADC A,r
                            _alu.ADC_A_r(rrr);
                            break;
                        case 0x90:  // 10 010 rrr       SUB r
                            _alu.SUB_r(rrr);
                            break;
                        case 0x98:  // 10 011 rrr       SBC A,r
                            _alu.SBC_A_r(rrr);
                            break;
                        case 0xA0:  // 10 100 rrr       AND r
                            _alu.AND_r(rrr);
                            break;
                        case 0xA8:  // 10 101 rrr       XOR r
                            _alu.XOR_r(rrr);
                            break;
                        case 0xB0:  // 10 110 rrr       OR r
                            _alu.OR_r(rrr);
                            break;
                        case 0xB8:  // 10 111 rrr       CP r
                            CP_r(rrr);
                            break;
                        default:
                            throw new Exception("10 ooo rrr exception");
                    }

                }
                else if ((opcode & 0xC7) == 0x04) // INC r
                {
                    INC_R(GetByteRegisterByOpcode((byte)(opcode >> 3)));
                }
                else if ((opcode & 0xC7) == 0x05) // DEC r
                {
                    DEC_R(GetByteRegisterByOpcode((byte) (opcode >> 3)));
                }
                else if ((opcode & 0xC7) == 0x06) // LD r,nn
                {
                    ByteRegister reg = GetByteRegisterByOpcode((byte)(opcode >> 3));
                    byte Value = _memory.ReadByte(_registerFile.PC++);

                    if (reg == null)
                    {
                        // The target is (HL)
                        CpuTicks += 10;
                        _memory.WriteByte(_registerFile.HL, Value);
                    }
                    else
                    {
                        // The target is a normal registry
                        CpuTicks += 7;
                        reg.Value = Value;
                    }
                }
                else if ((opcode & 0xC7) == 0xC0) // RET cc
                {
                    CpuTicks += 5;
                    if (opcode == 0xC0 && _registerFile.PC == 0x056C)
                    {
                        if (tape_load_trap() == 0)
                            break;
                    }
                    if (CheckFlag(opcode))
                    {
                        CpuTicks += 6;
                        RET();
                    }
                }
                else if ((opcode & 0xC7) == 0xC2) // JP cc,nn
                {
                    CpuTicks += 10;
                    if (CheckFlag(opcode))
                        JP();
                    else
                        _registerFile.PC += 2;
                }
                else if ((opcode & 0xC7) == 0xC4) // CALL cc,nn
                {
                    CpuTicks += 10;
                    if (CheckFlag(opcode))
                    {
                        CpuTicks += 7;
                        CALL_NN();
                    }
                    else
                        _registerFile.PC += 2;
                }
                else if ((opcode & 0xC7) == 0xC7) // RST p
                {
                    CpuTicks += 11;
                    _alu.RST((byte)(opcode & 0x38));
                }
                else if ((opcode & 0xCF) == 0x01) // LD dd,nn
                {
                    CpuTicks += 10;
                    WordRegister reg = GetWordRegister(opcode, true);
                    ushort Value = _memory.ReadWord(_registerFile.PC);
                    _registerFile.PC += 2;

                    reg.Word = Value;
                }
                else if ((opcode & 0xCF) == 0x03) // 00 rrr 100	            INC r
                {
                    INC(opcode);
                }
                else if ((opcode & 0xCF) == 0x09) // 00 RR1 001     	    ADD HL,RR
                {
                    ADD_HL_RR(opcode);
                }
                else if ((opcode & 0xCF) == 0x0B) // 00 rrr 101	            DEC r
                {
                    DEC(opcode);
                }
                else if ((opcode & 0xCF) == 0xC5) // PUSH qq
                {
                    Push(GetWordRegister(opcode, false));
                }

                else if ((opcode & 0xCF) == 0xC1) // POP qq
                {
                    Pop(GetWordRegister(opcode, false));
                }
                else
                {
                    switch (opcode)
                    {
                        case 0x00:      // 00 000 000                   NOP
                            NOP();
                            break;
                        case 0x02:      // 00 000 010                   LD (BC), A
                            LD_BCA_A();
                            break;
                        case 0x07:      // 00 000 111                   RLCA
                            RLCA();
                            break;
                        case 0x08:      // 00 001 000                   EX AF, AF'
                            EX_AF_AFAlt();
                            break;
                        case 0x0A:      // 00 001 010	                LD A, (BC)
                            LD_A_BCA();
                            break;
                        case 0x0F:      // 00 001 111	                RRCA
                            RRCA();
                            break;
                        case 0x10:      // 00 010 000 ssssssss	        DJNZ s
                            DJNZ();
                            break;
                        case 0x12:      // 00 010 010	                LD (DE), A
                            LD_DEA_A();
                            break;
                        case 0x17:      // 00 010 111	                RLA
                            RLA();
                            break;
                        case 0x18:      // 00 011 000 ssssssss	        JR s
                            JR();
                            break;
                        case 0x1A:      // 00 011 010	                LD A, (DE)
                            LD_A_DEA();
                            break;
                        case 0x1F:      // 00 011 111	                RRA
                            RRA();
                            break;
                        case 0x20:      // 00 100 000 ssssssss	        JR NZ, s
                            JR_NZ();
                            break;
                        case 0x22:      // 00 100 010 NNNNNNNN NNNNNNNN	LD (NN), HL
                            LD_NNA_HL();
                            break;
                        case 0x27:      // 00 100 111	                DAA	
                            DAA();
                            break;
                        case 0x28:      // 00 101 000 ssssssss	        JR Z, s
                            CpuTicks += 7;
                            JR_Z();
                            break;
                        case 0x2A:      // 00 101 010 NNNNNNNN NNNNNNNN	LD HL, (NN)
                            LD_HL_NNA();
                            break;
                        case 0x2F:      // 00 101 111	                CPL
                            CPL();
                            break;
                        case 0x30:      // 00 100 000 ssssssss	        JR NC, s
                            JR_NC();
                            break;
                        case 0x32:      // 00 110 010 NNNNNNNN NNNNNNNN	LD (NN), A
                            LD_NNA_A();
                            break;
                        case 0x37:      // 00 110 111	                SCF
                            SCF();
                            break;
                        case 0x38:      // 00 111 000 ssssssss	        JR C, s
                            JR_C();
                            break;
                        case 0x3A:      // 00 111 010 NNNNNNNN NNNNNNNN	LD A, (NN)
                            LD_A_NNA();
                            break;
                        case 0x3F:      // 00 111 111	                CCF
                            CCF();
                            break;
                        case 0xC3:      // 11 000 011 NNNNNNNN NNNNNNNN	JP NN
                            JP();
                            break;
                        case 0xC6:      // 11 000 110 NNNNNNNN	        ADD A, N
                            ADD_A_N();
                            break;
                        case 0xC9:      // 11 001 001	                RET
                            RET();
                            break;
                        case 0xCB:      // 11 001 011
                            _registerFile.R++;
                            Execute_CB_Prefix(_memory.ReadByte(_registerFile.PC++));
                            break;
                        case 0xCD:      // 11 001 101 NNNNNNNN NNNNNNNN	CALL NN
                            CALL_NN();
                            break;
                        case 0xCE:      // 11 001 110 NNNNNNNN	        ADC A, N
                            ADC_A_N();
                            break;
                        case 0xD3:      // 11 010 011 NNNNNNNN	        OUT (N), A
                            OUT_NN_A();
                            break;
                        case 0xD6:      // 11 010 110 NNNNNNNN	        SUB N
                            SUB_N();
                            break;
                        case 0xD9:      // 11 011 001	                EXX
                            EXX();
                            break;
                        case 0xDB:      // 11 011 011 NNNNNNNN	        IN A, (N)
                            IN_A_NA();
                            break;
                        case 0xDD:      // 11 011 101
                            _registerFile.R++;
                            Execute_DDFD_Prefix(_registerFile.RegisterIX, _memory.ReadByte(_registerFile.PC++));
                            break;
                        case 0xDE:      // 11 011 110 NNNNNNNN	        SBC A, N
                            SBC_A_N();
                            break;
                        case 0xE3:      // 11 100 011	                EX (SP), HL
                            EX_SPA_HL();
                            break;
                        case 0xE6:      // 11 100 110 NNNNNNNN	        AND N
                            AND_N();
                            break;
                        case 0xE9:      // 11 101 001	                JP (HL)
                            JP_HLA();
                            break;
                        case 0xEB:      // 11 101 011	                EX DE,HL
                            EX_DE_HL();
                            break;
                        case 0xed:      // EDxx opcodes
                            _registerFile.R++;
                            Execute_ED_Prefix(_memory.ReadByte(_registerFile.PC++));
                            break;
                        case 0xEE:      // 11 101 110 NNNNNNNN	        XOR N
                            XOR_N();
                            break;
                        case 0xF3:      // 11 110 011	                DI
                            DI();
                            break;
                        case 0xF6:      // 11 110 110 NNNNNNNN	        OR N
                            OR_N();
                            break;
                        case 0xF9:      // 11 111 001	                LD SP, HL
                            LD_SP_HL();
                            break;
                        case 0xFB:      // 11 111 011	                EI
                            EI();
                            break;
                        case 0xFD:      // FDxx opcodes
                            _registerFile.R++;
                            Execute_DDFD_Prefix(_registerFile.RegisterIY, _memory.ReadByte(_registerFile.PC++));
                            break;
                        case 0xFE:      // CP nn
                            CpuTicks += 7;
                            CP_r(_memory.ReadByte(_registerFile.PC++));
                            break;
                        default:
                            throw new Exception(string.Format("Internal execute error. Opcode {0} not implemented.", opcode));
                    }
                }
            }
        }

        private void INC(byte opcode)
        {
            CpuTicks += 6;
            WordRegister reg = GetWordRegister(opcode, true);

            // No flags affected
            reg.Word++;
        }

        private void DEC(byte opcode)
        {
            CpuTicks += 6;
            WordRegister reg = GetWordRegister(opcode, true);

            reg.Word--;
        }

        private void ADD_HL_RR(byte opcode)
        {
            CpuTicks += 11;
            WordRegister reg = GetWordRegister(opcode, true);

            _alu.ADD_16(_registerFile.RegisterHL, reg.Word);
        }

        private void DEC_R(ByteRegister reg)
        {
            if (reg == null)
            {
                // The target is (HL)
                CpuTicks += 7;
                reg = new ByteRegister(_memory.ReadByte(_registerFile.HL));
                _alu.DEC(reg);
                _memory.WriteByte(_registerFile.HL, reg.Value);
            }
            else
            {
                // The target is a normal registry
                CpuTicks += 4;
                _alu.DEC(reg);
            }
        }

        private void INC_R(ByteRegister reg)
        {
            if (reg == null)
            {
                // The target is (HL)
                CpuTicks += 7;
                reg = new ByteRegister(_memory.ReadByte(_registerFile.HL));
                _alu.INC(reg);
                _memory.WriteByte(_registerFile.HL, reg.Value);
            }
            else
            {
                // The target is a normal registry
                CpuTicks += 4;
                _alu.INC(reg);
            }
        }

        private void EI()
        {
            CpuTicks += 4;
            // The enable interrupt instruction sets both interrupt enable flip flops (IFF1
            // and IFF2) to a logic 1, allowing recognition of any maskable interrupt. Note
            // that during the execution of this instruction and the following instruction,
            // maskable interrupts are disabled.
            _registerFile.IFF1 = true;
            _registerFile.IFF2 = true;
        }

        private void LD_SP_HL()
        {
            CpuTicks += 6;
            _registerFile.SP = _registerFile.HL;
        }

        private void OR_N()
        {
            CpuTicks += 7;
            _alu.OR_r(_memory.ReadByte(_registerFile.PC++));
        }

        private void DI()
        {
            CpuTicks += 4;
            // DI disables the maskable interrupt by resetting the interrupt enable flip-flops
            // (IFF1 and IFF2). Note that this instruction disables the maskable
            // interrupt during its execution.
            _registerFile.IFF1 = false;
            _registerFile.IFF2 = false;
        }

        private void XOR_N()
        {
            CpuTicks += 7;
            _alu.XOR_r(_memory.ReadByte(_registerFile.PC++));
        }

        private void EX_DE_HL()
        {
            CpuTicks += 4;
            _registerFile.RegisterDE.Swap(_registerFile.RegisterHL);
        }

        private void JP_HLA()
        {
            CpuTicks += 4;
            _registerFile.PC = _registerFile.HL;
        }

        private void AND_N()
        {
            CpuTicks += 7;
            _alu.AND_r(_memory.ReadByte(_registerFile.PC++));
        }

        private void EX_SPA_HL()
        {
            //TODO: check this!
            CpuTicks += 19;
            {
                ushort word = _memory.ReadWord(_registerFile.SP);
                _memory.WriteWord(_registerFile.SP, _registerFile.HL);
                _registerFile.HL = word;
            }
        }

        private void SBC_A_N()
        {
            CpuTicks += 4;
            _alu.SBC_A_r(_memory.ReadByte(_registerFile.PC++));
        }

        private void IN_A_NA()
        {
            CpuTicks += 11;
            // The operand n is placed on the bottom half (A0 through A7) of the address
            // bus to select the I/O device at one of 256 possible ports. The contents of the
            // Accumulator also appear on the top half (A8 through A15) of the address
            // bus at this time. Then one byte from the selected port is placed on the data
            // bus and written to the Accumulator (register A) in the CPU.
            _registerFile.A = _inputOutputDevice.ReadPort((ushort)(_memory.ReadByte(_registerFile.PC++) | (_registerFile.A << 8)));
        }

        private void EXX()
        {
            CpuTicks += 4;
            // Each 2-byte value in register pairs BC, DE, and HL is exchanged with the
            // 2-byte value in BC', DE', and HL', respectively.

            _registerFile.RegisterBC.Swap(_registerFile.RegisterBC_);
            _registerFile.RegisterDE.Swap(_registerFile.RegisterDE_);
            _registerFile.RegisterHL.Swap(_registerFile.RegisterHL_);
        }

        private void SUB_N()
        {
            CpuTicks += 7;
            _alu.SUB_r(_memory.ReadByte(_registerFile.PC++));
        }

        private void OUT_NN_A()
        {
            CpuTicks += 11;
            // The operand n is placed on the bottom half (A0 through A7) of the address
            // bus to select the I/O device at one of 256 possible ports. The contents of the
            // Accumulator (register A) also appear on the top half (A8 through A15) of
            // the address bus at this time. Then the byte contained in the Accumulator is
            // placed on the data bus and written to the selected peripheral device.
            _inputOutputDevice.WritePort((ushort)(_memory.ReadByte(_registerFile.PC++) | (_registerFile.A << 8)), _registerFile.A);
        }

        private void ADC_A_N()
        {
            CpuTicks += 7;
            _alu.ADC_A_r(_memory.ReadByte(_registerFile.PC++));
        }

        private void ADD_A_N()
        {
            CpuTicks += 7;
            _alu.ADD_A_r(_memory.ReadByte(_registerFile.PC++));
        }

        private void LD_A_NNA()
        {
            CpuTicks += 13;
            var address = _memory.ReadWord(_registerFile.PC);
            _registerFile.PC += 2;
            _registerFile.A = _memory.ReadByte(address);
        }

        private void JR_C()
        {
            CpuTicks += 7;
            if ((_registerFile.F & FlagRegisterDefinition.C) != 0)
            {
                CpuTicks += 5;
                _registerFile.PC = (ushort)((int)_registerFile.PC + (sbyte)_memory.ReadByte(_registerFile.PC));
            }
            _registerFile.PC++;
        }

        private void SCF()
        {
            CpuTicks += 4;
            _registerFile.F |= FlagRegisterDefinition.C;
        }

        private void LD_NNA_A()
        {
            CpuTicks += 13;
            var address = _memory.ReadWord(_registerFile.PC);
            _registerFile.PC += 2;
            _memory.WriteByte(address, _registerFile.A);
        }

        private void JR_NC()
        {
            CpuTicks += 7;
            if ((_registerFile.F & FlagRegisterDefinition.C) == 0)
            {
                CpuTicks += 5;
                _registerFile.PC = (ushort)((int)_registerFile.PC + (sbyte)_memory.ReadByte(_registerFile.PC));
            }
            _registerFile.PC++;
        }

        private void LD_HL_NNA()
        {
            CpuTicks += 16;
            LD_ddnn(_registerFile.RegisterHL);
        }

        private void JR_Z()
        {
            if ((_registerFile.F & FlagRegisterDefinition.Z) != 0)
            {
                CpuTicks += 5;
                _registerFile.PC = (ushort)((int)_registerFile.PC + (sbyte)_memory.ReadByte(_registerFile.PC));
            }
            _registerFile.PC++;
        }

        private void LD_NNA_HL()
        {
            CpuTicks += 16;
            LD_nndd(_registerFile.RegisterHL);
        }

        private void JR_NZ()
        {
            CpuTicks += 7;
            if ((_registerFile.F & FlagRegisterDefinition.Z) == 0)
            {
                CpuTicks += 5;
                _registerFile.PC = (ushort)((int)_registerFile.PC + (sbyte)_memory.ReadByte(_registerFile.PC));
            }
            _registerFile.PC++;
        }

        private void LD_A_DEA()
        {
            CpuTicks += 7;
            _registerFile.A = _memory.ReadByte(_registerFile.DE);
        }

        public void RLA()
        {
            CpuTicks += 4;
            _alu.RLA();
        }

        private void LD_DEA_A()
        {
            CpuTicks += 7;
            _memory.WriteByte(_registerFile.DE, _registerFile.A);
        }

        private void DJNZ()
        {
            CpuTicks += 8;
            _registerFile.B--;
            if (_registerFile.B != 0)
            {
                CpuTicks += 5;
                JR();
            }
            _registerFile.PC++;
        }

        public void RRCA()
        {
            CpuTicks += 4;
            _alu.RRCA();
        }


        public event OnFetchHandler OnFetch;

        private int tape_load_trap()
        {
            return 0;
        }

        private int tape_save_trap()
        {
            return 0;
        }

        /// <summary>
        ///     From an opcode returns a half register following the rules
        ///     Reg opcode
        ///     A xxxxx111
        ///     B xxxxx000
        ///     C xxxxx001
        ///     D xxxxx010
        ///     E xxxxx011
        ///     H xxxxx100
        ///     L xxxxx101
        /// </summary>
        /// <param name="opcode">opcode</param>
        /// <returns>The half register or null if opcode is 110 (it is the ID for (HL))</returns>
        private ByteRegister GetByteRegisterByOpcode(byte opcode)
        {
            switch (opcode & 0x07)
            {
                case 0x00:
                    return _registerFile.RegisterBC.High;
                case 0x01:
                    return _registerFile.RegisterBC.Low;
                case 0x02:
                    return _registerFile.RegisterDE.High;
                case 0x03:
                    return _registerFile.RegisterDE.Low;
                case 0x04:
                    return _registerFile.RegisterHL.High;
                case 0x05:
                    return _registerFile.RegisterHL.Low;
                case 0x06:
                    return null;
                case 0x07:
                    return _registerFile.RegisterAF.High;
            }
            throw new Exception("Exception flow");
        }

        /// <summary>
        ///     From an opcode return a register following the rules
        ///     Reg         opcode
        ///     BC         xx00xxxx
        ///     DE         xx01xxxx
        ///     HL         xx10xxxx
        ///     SP or AF   xx11xxxx
        /// </summary>
        /// <param name="opcode">opcode</param>
        /// <param name="ReturnSP">Checked if opcode is 11. If this parameter is true then SP is returned else AF is returned</param>
        /// <returns>The register</returns>
        private WordRegister GetWordRegister(byte opcode, bool ReturnSP)
        {
            switch (opcode & 0x30)
            {
                case 0x00:
                    return _registerFile.RegisterBC;
                case 0x10:
                    return _registerFile.RegisterDE;
                case 0x20:
                    return _registerFile.RegisterHL;
                case 0x30:
                    if (ReturnSP)
                        return _registerFile.RegisterSP;
                    return _registerFile.RegisterAF;
            }
            throw new Exception("What's happening to me?");
        }

        /// <summary>
        ///     Execution of ED xx codes
        /// </summary>
        /// <param name="opcode">opcode to execute</param>
        private void Execute_ED_Prefix(byte opcode)
        {
            if ((opcode & 0xC7) == 0x40) // IN r,(C)
            {
                ByteRegister reg = GetByteRegisterByOpcode((byte) (opcode >> 3));

                // In this case 110 does not write in (HL) but affects only the flags
                if (reg == null)
                    reg = new ByteRegister();

                _cpuTicks += 12;
                IN(reg, _registerFile.BC);
            }
            else if ((opcode & 0xC7) == 0x41) // OUT (C),r
            {
                // The contents of register C are placed on the bottom half (A0 through A7) of
                // the address bus to select the I/O device at one of 256 possible ports. The
                // contents of Register B are placed on the top half (A8 through A15) of the
                // address bus at this time. Then the byte contained in register r is placed on
                // the data bus and written to the selected peripheral device.
                ByteRegister reg = GetByteRegisterByOpcode((byte) (opcode >> 3));

                // In this case 110 outputs 0 in out port
                if (reg == null)
                    reg = new ByteRegister(0);

                _cpuTicks += 12;
                _inputOutputDevice.WritePort(_registerFile.BC, reg.Value);
            }
            else if ((opcode & 0xC7) == 0x42) // ALU operations with HL
            {
                _cpuTicks += 15;
                WordRegister reg = GetWordRegister(opcode, true);
                switch (opcode & 0x08)
                {
                    case 0: // SBC HL,ss
                        _alu.SBC_HL(reg.Word);
                        break;
                    case 8: // ADC HL,ss
                        _alu.ADC_HL(reg.Word);
                        break;
                    default:
                        throw new Exception("No no no!!!");
                }
            }
            else if ((opcode & 0xC7) == 0x43) // Load register from to memory address
            {
                _cpuTicks += 20;
                WordRegister reg = GetWordRegister(opcode, true);
                switch (opcode & 0x08)
                {
                    case 0: // LD (nnnn),ss
                        LD_nndd(reg);
                        break;
                    case 8: // LD ss,(nnnn)
                        LD_ddnn(reg);
                        break;
                    default:
                        throw new Exception("No no no!!!");
                }
            }
            else
            {
                switch (opcode)
                {
                    case 0x44:
                    case 0x4c:
                    case 0x54:
                    case 0x5c:
                    case 0x64:
                    case 0x6c:
                    case 0x74:
                    case 0x7c: // NEG
                        // The contents of the Accumulator are negated (two’s complement). This is
                        // the same as subtracting the contents of the Accumulator from zero. Note
                        // that 80H is left unchanged.
                        // Condition Bits Affected:
                        // S is set if result is negative; reset otherwise
                        // Z is set if result is 0; reset otherwise
                        // H is set if borrow from bit 4; reset otherwise
                        // P/V is set if Accumulator was 80H before operation; reset otherwise
                        // N is set
                        // C is set if Accumulator was not 00H before operation; reset otherwise
                        _cpuTicks += 8;
                    {
                        byte _b = _registerFile.A;
                        _registerFile.A = 0;
                        _alu.SUB_r(_b);
                    }
                        break;

                    case 0x45:
                    case 0x4d: // RETI
                        // This instruction is used at the end of a maskable interrupt service routine to:
                        // • Restore the contents of the Program Counter (PC) (analogous to the
                        // RET instruction)
                        // • Signal an I/O device that the interrupt routine is completed. The RETI
                        // instruction also facilitates the nesting of interrupts, allowing higher
                        // priority devices to temporarily suspend service of lower priority
                        // service routines. However, this instruction does not enable interrupts
                        // that were disabled when the interrupt routine was entered. Before
                        // doing the RETI instruction, the enable interrupt instruction (EI)
                        // should be executed to allow recognition of interrupts after completion
                        // of the current service routine.

                        // TODO: Reading Z80 specs this instruction should not copy IFF2 to IFF1 but
                        // in real Z80 seems that the operation is done.
                    case 0x55:
                    case 0x5d:
                    case 0x65:
                    case 0x6d:
                    case 0x75:
                    case 0x7d: // RETN
                        // This instruction is used at the end of a non-maskable interrupts service
                        // routine to restore the contents of the Program Counter (PC) (analogous to
                        // the RET instruction). The state of IFF2 is copied back to IFF1 so that
                        // maskable interrupts are enabled immediately following the RETN if they
                        // were enabled before the nonmaskable interrupt.
                        _cpuTicks += 14;
                        _registerFile.IFF1 = _registerFile.IFF2;
                        RET();
                        break;

                    case 0x46:
                    case 0x4e:
                    case 0x66:
                    case 0x6e: // IM 0
                        // The IM 0 instruction sets interrupt mode 0. In this mode, the interrupting
                        // device can insert any instruction on the data bus for execution by the
                        // CPU. The first byte of a multi-byte instruction is read during the interrupt
                        // acknowledge cycle. Subsequent bytes are read in by a normal memory
                        // read sequence.
                        _cpuTicks += 8;
                        _registerFile.IM = 0;
                        break;

                    case 0x47: // LD I,A
                        _cpuTicks += 9;
                        _registerFile.I = _registerFile.A;
                        break;

                    case 0x4F: // LD R,A
                        _cpuTicks += 9;
                        _registerFile.R = _registerFile.R7 = _registerFile.A;
                        break;

                    case 0x56:
                    case 0x76: // IM 1
                        _cpuTicks += 8;
                        // The IM 1 instruction sets interrupt mode 1. In this mode, the processor
                        // responds to an interrupt by executing a restart to location 0038H.
                        _registerFile.IM = 1;
                        break;
                    case 0x57: // LD A,I
                        _cpuTicks += 9;
                        // The contents of the Interrupt Vector Register I are loaded to the Accumulator.
                        // Condition Bits Affected:
                        // S is set if I-Register is negative; reset otherwise
                        // Z is set if I-Register is zero; reset otherwise
                        // H is reset
                        // P/V contains contents of IFF2
                        // N is reset
                        // C is not affected
                        // If an interrupt occurs during execution of this instruction, the Parity
                        // flag contains a 0.
                        _registerFile.A = _registerFile.I;
                        _registerFile.F =
                            (byte)
                                ((_registerFile.F & FlagRegisterDefinition.C) |
                                 _lookupTables.Sz53[_registerFile.A] |
                                 (_registerFile.IFF2 ? FlagRegisterDefinition.V : 0));
                        break;
                    case 0x5E:
                    case 0x7E: // IM 2
                        // The IM 2 instruction sets the vectored interrupt mode 2. This mode allows
                        // an indirect call to any memory location by an 8-bit vector supplied from the
                        // peripheral device. This vector then becomes the least-significant eight bits
                        // of the indirect pointer, while the I register in the CPU provides the most-significant
                        // eight bits. This address points to an address in a vector table that
                        // is the starting address for the interrupt service routine.
                        _cpuTicks += 8;
                        _registerFile.IM = 2;
                        break;

                    case 0x5F: // LD A,R
                        _cpuTicks += 9;
                        _registerFile.A = (byte) ((_registerFile.R & 0x7F) | (_registerFile.R7 & 0x80));
                        _registerFile.F =
                            (byte)
                                ((_registerFile.F & FlagRegisterDefinition.C) |
                                 _lookupTables.Sz53[_registerFile.A] |
                                 (_registerFile.IFF2 ? FlagRegisterDefinition.V : 0));
                        break;

                    case 0x67: // RRD
                        _cpuTicks += 18;
                        _alu.RRD();
                        break;
                    case 0x6F: // RLD
                        _cpuTicks += 18;
                        _alu.RLD();
                        break;
                    case 0xA0: // LDI
                        _cpuTicks += 16;
                        LDI();
                        break;
                    case 0xA1: // CPI
                        _cpuTicks += 16;
                        CPI();
                        break;

                    case 0xA2: // INI
                        _cpuTicks += 16;
                        INI();
                        break;

                    case 0xA3: // OUTI
                        _cpuTicks += 16;
                        OUTI();
                        break;

                    case 0xA8: // LDD
                        _cpuTicks += 16;
                        LDD();
                        break;

                    case 0xA9: // CPD
                        _cpuTicks += 16;
                        CPD();
                        break;
                    case 0xAA: // IND
                        _cpuTicks += 16;
                        IND();
                        break;

                    case 0xAB: // OUTD
                        _cpuTicks += 16;
                        OUTD();
                        break;

                    case 0xB0: // LDIR
                        _cpuTicks += 16;
                        LDIR();
                        break;

                    case 0xB1: // CPIR
                        _cpuTicks += 16;
                        CPIR();
                        break;

                    case 0xB2: // INIR
                        _cpuTicks += 16;
                        INIR();
                        break;

                    case 0xB3: // OTIR
                        _cpuTicks += 16;
                        OTIR();
                        break;

                    case 0xB8: // LDDR
                        _cpuTicks += 17;
                        LDDR();
                        break;

                    case 0xB9: // CPDR
                        _cpuTicks += 16;
                        CPDR();
                        break;

                    case 0xba: // INDR
                        _cpuTicks += 16;
                        INDR();
                        break;

                    case 0xbb: // OTDR
                        _cpuTicks += 16;
                        OTDR();
                        break;

                    default: // All other opcodes are NOPD
                        _cpuTicks += 8;
                        break;
                }
            }
        }

        /// <summary>
        ///     Execution of CB xx codes
        /// </summary>
        /// <param name="opcode">opcode to execute</param>
        private void Execute_CB_Prefix(byte opcode)
        {
            // Operations with single byte register
            // The format is 00 ooo rrr where ooo is the operation and rrr is the register
            ByteRegister reg = GetByteRegisterByOpcode(opcode);
            ByteRegister _HL_ = null;

            // Check if the source/target is (HL)
            if (reg == null)
                reg = _HL_ = new ByteRegister(_memory.ReadByte(_registerFile.HL));

            Execute_CB_on_reg(opcode, reg);

            if (reg == _HL_)
            {
                // The target is (HL)
                _cpuTicks += 15;
                _memory.WriteByte(_registerFile.HL, _HL_.Value); //We should not do this when we check bits (BIT n,r)!
            }
            else
            {
                // The source is a normal registry
                _cpuTicks += 8;
            }
        }

        private void Execute_DDFD_Prefix(WordRegister RegisterI_, byte opcode)
        {
            ByteRegister _I__;
            ushort Address;

            if (opcode == 0x76)     // HALT
            {
                // The first check is for HALT otherwise it could be
                // interpreted as LD (I_ + d),(I_ + d)
                _cpuTicks += 4;
                _registerFile.Halted = true;
            }
            else if ((opcode & 0xC0) == 0x40)   // LD r,r'
            {
                ByteRegister reg1 = GetByteRegisterByOpcode((byte)(opcode >> 3));
                ByteRegister reg2 = GetByteRegisterByOpcode(opcode);

                if (reg1 == null)
                {
                    // The target is (I_ + d)
                    _cpuTicks += 19;
                    Address = (ushort)(RegisterI_.Word + (sbyte)_memory.ReadByte(_registerFile.PC++));
                    _memory.WriteByte(Address, reg2.Value);
                }
                else if (reg2 == null)
                {
                    // The source is (I_ + d)
                    _cpuTicks += 19;
                    Address = (ushort)(RegisterI_.Word + (sbyte)_memory.ReadByte(_registerFile.PC++));
                    reg1.Value = _memory.ReadByte(Address);
                }
                else
                {
                    // Source and target are normal registers but HL is now substituted by I_
                    if (reg1 == _registerFile.RegisterHL.High)
                        reg1 = RegisterI_.High;
                    if (reg1 == _registerFile.RegisterHL.Low)
                        reg1 = RegisterI_.Low;

                    if (reg2 == _registerFile.RegisterHL.High)
                        reg2 = RegisterI_.High;
                    if (reg2 == _registerFile.RegisterHL.Low)
                        reg2 = RegisterI_.Low;

                    _cpuTicks += 8;
                    reg1.Value = reg2.Value;
                }
            }
            else if ((opcode & 0xC0) == 0x80)
            {
                // Operation beetween accumulator and other registers
                // Usually are identified by 10 ooo rrr where ooo is the operation and rrr is the source register
                ByteRegister reg = GetByteRegisterByOpcode(opcode);
                byte _Value;

                if (reg == null)
                {
                    // The source is (I_ + d)
                    _cpuTicks += 19;
                    _Value = _memory.ReadByte((ushort)(RegisterI_.Word + (sbyte)_memory.ReadByte(_registerFile.PC++)));
                }
                else
                {
                    // The source is a normal registry but HL is substituted by I_
                    _cpuTicks += 8;
                    if (reg == _registerFile.RegisterHL.High)
                        _Value = RegisterI_.High.Value;
                    else if (reg == _registerFile.RegisterHL.Low)
                        _Value = RegisterI_.Low.Value;
                    else
                        _Value = reg.Value;
                }

                switch (opcode & 0xF8)
                {
                    case 0x80:  // ADD A,r
                        _alu.ADC_A_r(_Value);
                        break;
                    case 0x88:  // ADC A,r
                        _alu.ADC_A_r(_Value);
                        break;
                    case 0x90:  // SUB r
                        _alu.SUB_r(_Value);
                        break;
                    case 0x98:  // SBC A,r
                        _alu.SBC_A_r(_Value);
                        break;
                    case 0xA0:  // AND r
                        _alu.AND_r(_Value);
                        break;
                    case 0xA8:  // XOR r
                        _alu.XOR_r(_Value);
                        break;
                    case 0xB0:  // OR r
                        _alu.OR_r(_Value);
                        break;
                    case 0xB8:  // CP r
                        CP_r(_Value);
                        break;
                    default:
                        throw new Exception("Wrong place in the right time...");
                }

            }
            else
            {

                switch (opcode)
                {
                    case 0x09:      // ADD I_,BC
                        _cpuTicks += 15;
                        ADD_16(RegisterI_, _registerFile.BC);
                        break;

                    case 0x19:      // ADD I_,DE
                        _cpuTicks += 15;
                        ADD_16(RegisterI_, _registerFile.DE);
                        break;

                    case 0x21:      // LD I_,nnnn
                        _cpuTicks += 14;
                        RegisterI_.Word = _memory.ReadWord(_registerFile.PC);
                        _registerFile.PC += 2;
                        break;

                    case 0x22:      // LD (nnnn),I_
                        _cpuTicks += 20;
                        LD_nndd(RegisterI_);
                        break;

                    case 0x23:      // INC I_
                        _cpuTicks += 10;
                        RegisterI_.Word++;
                        break;

                    case 0x24:      // INC I_.h
                        _cpuTicks += 8;
                        INC(RegisterI_.High);
                        break;

                    case 0x25:      // DEC I_.h
                        _cpuTicks += 8;
                        DEC(RegisterI_.High);
                        break;

                    case 0x26:      // LD I_.h,nn
                        _cpuTicks += 11;
                        RegisterI_.High.Value = _memory.ReadByte(_registerFile.PC++);
                        break;

                    case 0x29:      // ADD I_,I_
                        _cpuTicks += 15;
                        ADD_16(RegisterI_, RegisterI_.Word);
                        break;

                    case 0x2A:      // LD I_,(nnnn)
                        _cpuTicks += 20;
                        LD_ddnn(RegisterI_);
                        break;

                    case 0x2B:      // DEC I_
                        _cpuTicks += 10;
                        RegisterI_.Word--;
                        break;

                    case 0x2C:      // INC I_.l
                        _cpuTicks += 8;
                        INC(RegisterI_.Low);
                        break;

                    case 0x2D:      // DEC I_.l
                        _cpuTicks += 8;
                        DEC(RegisterI_.Low);
                        break;

                    case 0x2E:      // LD I_.l,nn
                        _cpuTicks += 11;
                        RegisterI_.Low.Value = _memory.ReadByte(_registerFile.PC++);
                        break;

                    case 0x34:      // INC (I_ + d)
                        _cpuTicks += 23;
                        Address = (ushort)(RegisterI_.Word + (sbyte)_memory.ReadByte(_registerFile.PC++));
                        _I__ = new ByteRegister(_memory.ReadByte(Address));
                        INC(_I__);
                        _memory.WriteByte(Address, _I__.Value);
                        break;

                    case 0x35:      // DEC (I_ + d)
                        _cpuTicks += 23;
                        Address = (ushort)(RegisterI_.Word + (sbyte)_memory.ReadByte(_registerFile.PC++));
                        _I__ = new ByteRegister(_memory.ReadByte(Address));
                        DEC(_I__);
                        _memory.WriteByte(Address, _I__.Value);
                        break;

                    case 0x36:      // LD (I_ + d),nn
                        _cpuTicks += 19;
                        Address = (ushort)(RegisterI_.Word + (sbyte)_memory.ReadByte(_registerFile.PC++));
                        _memory.WriteByte(Address, _memory.ReadByte(_registerFile.PC++));
                        break;

                    case 0x39:      // ADD I_,SP
                        _cpuTicks += 15;
                        ADD_16(RegisterI_, _registerFile.SP);
                        break;




                    case 0xCB:      // {DD|FD}CBxx opcodes
                        {
                            Address = (ushort)(RegisterI_.Word + (sbyte)_memory.ReadByte(_registerFile.PC++));
                            Execute_DDFD_CB_Prefix(Address, _memory.ReadByte(_registerFile.PC++));
                        }
                        break;
                    case 0xE1:      // POP I_
                        _cpuTicks += 14;
                        Pop(RegisterI_);
                        break;

                    case 0xE3:      // EX (SP),I_
                        _cpuTicks += 23;
                        {
                            ushort _w = _memory.ReadWord(_registerFile.SP);
                            _memory.WriteWord(_registerFile.SP, RegisterI_.Word);
                            RegisterI_.Word = _w;
                        }
                        break;
                    case 0xE5:      // PUSH I_
                        _cpuTicks += 15;
                        Push(RegisterI_);
                        break;

                    case 0xE9:      // JP I_
                        _cpuTicks += 8;
                        _registerFile.PC = RegisterI_.Word;
                        break;

                    // Note EB (EX DE,HL) does not get modified to use either IX or IY;
                    // this is because all EX DE,HL does is switch an internal flip-flop
                    // in the Z80 which says which way round DE and HL are, which can't
                    // be used with IX or IY. (This is also why EX DE,HL is very quick
                    // at only 4 T states).

                    case 0xF9:      // LD SP,I_
                        _cpuTicks += 10;
                        _registerFile.SP = RegisterI_.Word;
                        break;

                    default:
                        // Instruction did not involve H or L, so backtrack one instruction and parse again
                        _cpuTicks += 4;
                        _registerFile.PC--;
                        break;

                }
            }

        }

        /// <summary>
        ///     Execution of DD CB xx codes or FD CB xx codes
        /// </summary>
        /// <param name="Address">Address to act on - Address = I_ + d</param>
        /// <param name="opcode">opcode</param>
        private void Execute_DDFD_CB_Prefix(ushort Address, byte opcode)
        {
            // This is a mix of DD/FD opcodes (Normal operation but access to 
            // I_ register instead of HL register) and CB op codes.
            // Behaviour is a little different:
            // if (Opcodes use B, C, D, E, H, L)  -  opcodes with rrr different from 110
            //   r = (I_ + d)
            //   execute_op r
            //   (I_ + d) = r
            // if (Opcodes use (HL))              -  opcodes with rrr = 110
            //   execute_op (I_ + d)
            //
            // if execute_op is a bit checking operation BIT n,r no assignement are done


            ByteRegister reg;

            // Check if the operation is a bit checking operation
            // The format is 01 bbb rrr
            if (opcode >> 6 == 0x01)
            {
                reg = new ByteRegister();
                _cpuTicks += 20;
            }
            else
            {
                // Retrieve the register from opcode xxxxx rrr
                reg = GetByteRegisterByOpcode(opcode);

                // Check if the source is (I_ + d) so the op will not act on any register
                // but only on memory
                if (reg == null)
                {
                    _cpuTicks += 23;
                    reg = new ByteRegister();
                }
                else
                    _cpuTicks += 23;
                // In case reg is not null the timings are not documented. I think the operation 
                // take at least 23 CpuTicks (the same of the operation without storing the 
                // result in register too).
            }

            // Assign (I_ + d) value to reg
            reg.Value = _memory.ReadByte(Address);


            Execute_CB_on_reg(opcode, reg);

            _memory.WriteByte(Address, reg.Value);
        }

        /// <summary>
        ///     This is the low level function called within a CB opcode fetch
        ///     (single byte or DD CB or FD CB)
        ///     It must be called after the execution unit has determined on
        ///     wich register act
        /// </summary>
        /// <param name="opcode">opcode</param>
        /// <param name="reg">Register to act on</param>
        private void Execute_CB_on_reg(byte opcode, ByteRegister reg)
        {
            switch (opcode >> 3)
            {
                case 0: // RLC r
                    _alu.RLC(reg);
                    break;
                case 1: // RRC r
                    _alu.RRC(reg);
                    break;
                case 2: // RL r
                    _alu.RL(reg);
                    break;
                case 3: // RR r
                    _alu.RR(reg);
                    break;
                case 4: // SLA r
                    _alu.SLA(reg);
                    break;
                case 5: // SRA r
                    _alu.SRA(reg);
                    break;
                case 6: // SLL r
                    _alu.SLL(reg);
                    break;
                case 7: // SRL r
                    _alu.SRL(reg);
                    break;
                default:
                    // Work on bits

                    // The format is oo bbb rrr
                    // oo is the operation (01 BIT, 10 RES, 11 SET)
                    // bbb is the bit number
                    // rrr is the register
                    var bit = (byte) ((opcode >> 3) & 0x07);

                    switch (opcode >> 6)
                    {
                        case 1: // BIT n,r
                            if (bit == 7)
                                BIT7(reg.Value);
                            else
                                BIT(bit, reg.Value);
                            break;
                        case 2: // RES n,r
                            reg.Value &= (byte) ~(1 << bit);
                            break;
                        case 3: // SET n,r
                            reg.Value |= (byte) (1 << bit);
                            break;
                        default:
                            throw new Exception("What am I doing here?!?");
                    }

                    break;
            }
        }

        /// <summary>
        ///     Check the flag related to opcode according with the following table:
        ///     Cond opcode   Flag Description
        ///     NZ   xx000xxx  Z   Not Zero
        ///     Z   xx001xxx  Z   Zero
        ///     NC   xx010xxx  C   Not Carry
        ///     C   xx011xxx  C   Carry
        ///     PO   xx100xxx  P/V Parity odd  (Not parity)
        ///     PE   xx101xxx  P/V Parity even (Parity)
        ///     P   xx110xxx  S   Sign positive
        ///     M   xx111xxx  S   Sign negative
        /// </summary>
        /// <param name="opcode">The opcode</param>
        /// <returns>True if the condition is satisfied otherwise false</returns>
        private bool CheckFlag(byte opcode)
        {
            bool not = false;
            byte flag;

            // Find the right flag and the condition
            switch ((opcode >> 3) & 0x07)
            {
                case 0:
                    not = true;
                    flag = FlagRegisterDefinition.Z;
                    break;
                case 1:
                    flag = FlagRegisterDefinition.Z;
                    break;
                case 2:
                    not = true;
                    flag = FlagRegisterDefinition.C;
                    break;
                case 3:
                    flag = FlagRegisterDefinition.C;
                    break;
                case 4:
                    not = true;
                    flag = FlagRegisterDefinition.P;
                    break;
                case 5:
                    flag = FlagRegisterDefinition.P;
                    break;
                case 6:
                    not = true;
                    flag = FlagRegisterDefinition.S;
                    break;
                case 7:
                    flag = FlagRegisterDefinition.S;
                    break;
                default:
                    throw new Exception("I'm feeling bad");
            }

            // Check flag and condition
            if (not)
                return ((_registerFile.F & flag) == 0);
            return ((_registerFile.F & flag) != 0);
        }

        public void NOP()
        {
            CpuTicks += 4;
        }

        public void HALT()
        {
            // The first check is for HALT otherwise it could be
            // interpreted as LD (HL),(HL)
            CpuTicks += 4;
            _registerFile.Halted = true;
        }

        public void LD_d_r(ByteRegister destinationRegister, ByteRegister sourceRegister)
        {
            throw new NotImplementedException();
        }

        public void LD_BCA_A()
        {
            CpuTicks += 7;
            _memory.WriteByte(_registerFile.BC, _registerFile.A);
        }

        public void AND_r(byte op)
        {
            _alu.AND_r(op);
        }

        public void ADC_A_r(byte op)
        {
            _alu.ADC_A_r(op);
        }

        public void ADC_HL(ushort op)
        {
            _alu.ADC_HL(op);
        }

        public void ADD_A_r(byte op)
        {
            _alu.ADD_A_r(op);
        }

        public void ADD_16(WordRegister op1, ushort op2)
        {
            _alu.ADD_16(op1, op2);
        }

        public void DAA()
        {
            CpuTicks += 4;
            _alu.DAA();
        }

        public void DEC(ByteRegister op)
        {
            _alu.DEC(op);
        }

        public void INC(ByteRegister op)
        {
            _alu.INC(op);
        }

        public void OR_r(byte op)
        {
            _alu.OR_r(op);
        }

        public void RL(ByteRegister op)
        {
            _alu.RL(op);
        }

        public void RLC(ByteRegister op)
        {
            _alu.RLC(op);
        }

        public void RLD()
        {
            _alu.RLD();
        }

        public void RR(ByteRegister op)
        {
            _alu.RR(op);
        }

        public void RRA()
        {
            CpuTicks += 4;
            _alu.RRA();
        }

        public void RRC(ByteRegister op)
        {
            _alu.RRC(op);
        }


        public void RRD()
        {
            throw new NotImplementedException();
        }

        public void RST(byte op)
        {
            throw new NotImplementedException();
        }

        public void SBC_A_r(byte op)
        {
            throw new NotImplementedException();
        }

        public void SBC_HL(ushort op)
        {
            throw new NotImplementedException();
        }

        public void SLA(ByteRegister op)
        {
            throw new NotImplementedException();
        }

        public void SLL(ByteRegister op)
        {
            throw new NotImplementedException();
        }

        public void SRA(ByteRegister op)
        {
            throw new NotImplementedException();
        }

        public void SRL(ByteRegister op)
        {
            throw new NotImplementedException();
        }

        public void SUB_r(byte op)
        {
            throw new NotImplementedException();
        }

        public void XOR_r(byte op)
        {
            throw new NotImplementedException();
        }

        public void Push(WordRegister register)
        {
            CpuTicks += 11;
            _cpuStack.Push(register);
        }

        public void Push(byte @byte)
        {
            throw new NotImplementedException();
        }

        public void Push(ushort word)
        {
            throw new NotImplementedException();
        }

        public void Pop(WordRegister register)
        {
            CpuTicks += 10;
            _cpuStack.Pop(register);
        }

        public void Pop(out byte @byte)
        {
            throw new NotImplementedException();
        }

        public void Pop(out ushort word)
        {
            throw new NotImplementedException();
        }
    }
}