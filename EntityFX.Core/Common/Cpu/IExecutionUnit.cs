using EntityFX.Core.Common.Cpu.Registers;

namespace EntityFX.Core.Common.Cpu
{
    public interface IExecutionUnit
    {
        int StatementsToFetch { get; set; }

        int CpuTicks { get; set; }

        void Execute();
        #region Execution commands

        void NOP();
        void HALT();
        void BIT(byte bit, byte op);
        void BIT7(byte op);
        void CALL_NN();
        void CCF();
        void CP_r(byte op);
        void CPD();
        void CPDR();
        void CPI();
        void CPIR();
        void CPx();
        void CPL();
        void IN(ByteRegister reg, ushort port);
        void IND();
        void INDR();
        void INI();
        void INIR();
        void INx();

        void LDD();
        void LDDR();
        void LDI();
        void LDIR();
        void LDx();

        void LD_BCA_A();
        void LD_A_BCA();

        void LD_nndd(WordRegister register);
        void LD_ddnn(WordRegister register);
        void LD_d_r(ByteRegister destinationRegister, ByteRegister sourceRegister);

        void JP();
        void JR();
        void OUTD();
        void OTDR();
        void OUTI();
        void OTIR();
        void OUTx();
        void RET();

        void EX_AF_AFAlt();
        #endregion

        #region ALU commands
        void RLCA();
        #endregion

        /// <summary>
        /// Fetch event (used during debug)
        /// </summary>
        event OnFetchHandler OnFetch;
    }
}