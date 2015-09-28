using EntityFX.Core.Common.Cpu.Registers;

namespace EntityFX.Core.Common.Cpu
{
    public interface IAlu
    {
        //int CpuTicks { get; set; }
        void AND_r(byte op); //
        void ADC_A_r(byte op); //
        void ADC_HL(ushort op); //
        void ADD_A_r(byte op); //
        void ADD_16(WordRegister op1, ushort op2); //
        void DAA(); //
        void DEC(ByteRegister op); //
        void INC(ByteRegister op); //
        void OR_r(byte op); //
        void RL(ByteRegister op); //
        void RLA(); //
        void RLCA(); //
        void RLC(ByteRegister op); //
        void RLD(); //
        void RR(ByteRegister op); //
        void RRA(); //
        void RRC(ByteRegister op); //
        void RRCA(); //
        void RRD(); //
        void RST(byte op);
        void SBC_A_r(byte op); //
        void SBC_HL(ushort op); //
        void SLA(ByteRegister op); //
        void SLL(ByteRegister op); //
        void SRA(ByteRegister op); //
        void SRL(ByteRegister op); //
        void SUB_r(byte op); //
        void XOR_r(byte op); //
    }
}