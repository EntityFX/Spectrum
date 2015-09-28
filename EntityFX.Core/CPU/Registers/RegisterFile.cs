using System.IO;
using System.Runtime.Serialization;
using System.Text;
using EntityFX.Core.Common.Cpu.Registers;

namespace EntityFX.Core.CPU.Registers
{
    /// <summary>
    ///     Z80 RegisterFile
    /// </summary>
    public class RegisterFile : IRegisterFile
    {
        private bool _halted;
        private bool _iff1;
        private bool _iff2;
        private byte _im;
        private WordRegister _registerAf = new WordRegister();
        private WordRegister _registerAft = new WordRegister();
        private WordRegister _registerBc = new WordRegister();
        private WordRegister _registerBct = new WordRegister();
        private WordRegister _registerDe = new WordRegister();
        private WordRegister _registerDet = new WordRegister();
        private WordRegister _registerHl = new WordRegister();
        private WordRegister _registerHlt = new WordRegister();
        private byte _registerI;

        private WordRegister _registerIx = new WordRegister();
        private WordRegister _registerIy = new WordRegister();
        private ushort _registerPc;

        private byte _r;
        private byte _r7;

        private WordRegister _registerSp = new WordRegister();
        private ushort _pc;
        private WordRegister _registerAfAlt;

        #region WordRegister AF

        /// <summary>
        ///     Accumulator and flags register
        /// </summary>
        public WordRegister RegisterAF
        {
            get { return _registerAf; }
            set { _registerAf = value; }
        }

        /// <summary>
        ///     Access to AF register as word
        /// </summary>
        public ushort AF
        {
            get { return _registerAf.Word; }
            set { _registerAf.Word = value; }
        }

        /// <summary>
        ///     Access to bits 8-15 of AF
        /// </summary>
        public byte A
        {
            get { return _registerAf.High.Value; }
            set { _registerAf.High.Value = value; }
        }

        /// <summary>
        ///     Access to bits 0-7 of AF
        /// </summary>
        public byte F
        {
            get { return _registerAf.Low.Value; }
            set { _registerAf.Low.Value = value; }
        }

        #endregion

        #region WordRegister BC

        /// <summary>
        ///     BC Register
        /// </summary>
        public WordRegister RegisterBC
        {
            get { return _registerBc; }
            set { _registerBc = value; }
        }


        /// <summary>
        ///     Access to BC register as word
        /// </summary>
        public ushort BC
        {
            get { return _registerBc.Word; }
            set { _registerBc.Word = value; }
        }

        /// <summary>
        ///     Access to 8-15 bits of BC
        /// </summary>
        public byte B
        {
            get { return _registerBc.High.Value; }
            set { _registerBc.High.Value = value; }
        }

        /// <summary>
        ///     Access to 0-7 bits of BC
        /// </summary>
        public byte C
        {
            get { return _registerBc.Low.Value; }
            set { _registerBc.Low.Value = value; }
        }

        #endregion

        #region WordRegister DE

        /// <summary>
        ///     DE Register
        /// </summary>
        public WordRegister RegisterDE
        {
            get { return _registerDe; }
            set { _registerDe = value; }
        }


        /// <summary>
        ///     Access to DE WordRegister as word
        /// </summary>
        public ushort DE
        {
            get { return _registerDe.Word; }
            set { _registerDe.Word = value; }
        }

        /// <summary>
        ///     Access to bit 8-15 of DE
        /// </summary>
        public byte D
        {
            get { return _registerDe.High.Value; }
            set { _registerDe.High.Value = value; }
        }

        /// <summary>
        ///     Access to bit 0-7 of DE
        /// </summary>
        public byte E
        {
            get { return _registerDe.Low.Value; }
            set { _registerDe.Low.Value = value; }
        }

        #endregion

        #region WordRegister HL

        /// <summary>
        ///     HL Register
        /// </summary>
        public WordRegister RegisterHL
        {
            get { return _registerHl; }
            set { _registerHl = value; }
        }


        /// <summary>
        ///     Access to HL register as word
        /// </summary>
        public ushort HL
        {
            get { return _registerHl.Word; }
            set { _registerHl.Word = value; }
        }

        /// <summary>
        ///     Access to bits 8-15 of HL
        /// </summary>
        public byte H
        {
            get { return _registerHl.High.Value; }
            set { _registerHl.High.Value = value; }
        }

        /// <summary>
        ///     Access to bits 0-7 of HL
        /// </summary>
        public byte L
        {
            get { return _registerHl.Low.Value; }
            set { _registerHl.Low.Value = value; }
        }

        #endregion

        #region WordRegister SP

        /// <summary>
        ///     Stack pointer register
        /// </summary>
        public WordRegister RegisterSP
        {
            get { return _registerSp; }
            set { _registerSp = value; }
        }

        /// <summary>
        ///     Access to SP register as word
        /// </summary>
        public ushort SP
        {
            get { return _registerSp.Word; }
            set { _registerSp.Word = value; }
        }

        public ushort PC
        {
            get { return _pc; }
            set { _pc = value; }
        }


        #endregion

        #region Alternate registers (AF', BC', DE', HL')

        /// <summary>
        ///     Alternate Accumulator and Flags Register
        /// </summary>
        public WordRegister RegisterAFAlt
        {
            get { return _registerAft; }
            set { _registerAft = value; }
        }

        /// <summary>
        ///     Alternate BC Register
        /// </summary>
        public WordRegister RegisterBC_
        {
            get { return _registerBct; }
            set { _registerBct = value; }
        }

        /// <summary>
        ///     Alternate DE Register
        /// </summary>
        public WordRegister RegisterDE_
        {
            get { return _registerDet; }
            set { _registerDet = value; }
        }

        /// <summary>
        ///     Alternate HL Register
        /// </summary>
        public WordRegister RegisterHL_
        {
            get { return _registerHlt; }
            set { _registerHlt = value; }
        }

        #endregion

        #region Index Registers

        /// <summary>
        ///     Index register IX
        /// </summary>
        public WordRegister RegisterIX
        {
            get { return _registerIx; }
            set { _registerIx = value; }
        }

        /// <summary>
        ///     Access to IX register as word
        /// </summary>
        public ushort IX
        {
            get { return _registerIx.Word; }
            set { _registerIx.Word = value; }
        }


        /// <summary>
        ///     Index register IY
        /// </summary>
        public WordRegister RegisterIY
        {
            get { return _registerIy; }
            set { _registerIy = value; }
        }

        /// <summary>
        ///     Access to IY register as word
        /// </summary>
        public ushort IY
        {
            get { return _registerIy.Word; }
            set { _registerIy.Word = value; }
        }

        #endregion

        #region IR Register

        /// <summary>
        ///     Interrupt register
        /// </summary>
        public byte I
        {
            get { return _registerI; }
            set { _registerI = value; }
        }

        /// <summary>
        ///     Refresh register
        /// </summary>
        public byte R
        {
            get { return _r; }
            set { _r = value; }
        }

        /// <summary>
        ///     Refresh register Bit 7
        /// </summary>
        public byte R7
        {
            get { return _r7; }
            set { _r7 = value; }
        }

        #endregion

        #region Program Counter

        /// <summary>
        ///     Program counter
        /// </summary>
        public ushort RegisterPc
        {
            get { return _registerPc; }
            set { _registerPc = value; }
        }

        #endregion

        #region Other states holders

        /// <summary>
        ///     CPU Halted
        /// </summary>
        public bool Halted
        {
            get { return _halted; }
            set { _halted = value; }
        }

        /// <summary>
        ///     Main interrupts flip flop
        /// </summary>
        public bool IFF1
        {
            get { return _iff1; }
            set { _iff1 = value; }
        }

        /// <summary>
        ///     Temporary storage for IFF1
        /// </summary>
        public bool IFF2
        {
            get { return _iff2; }
            set { _iff2 = value; }
        }


        /// <summary>
        ///     Interrupt Mode (can be 0, 1, 2)
        /// </summary>
        public byte IM
        {
            get { return _im; }
            set { _im = value; }
        }

        #endregion

        /// <summary>
        ///     Make a copy of this class
        /// </summary>
        /// <returns>A new status class containing this status</returns>
        public object Clone()
        {
            return new RegisterFile
            {
                AF = AF,
                BC = BC,
                DE = DE,
                HL = HL,
                RegisterAFAlt = {Word = RegisterAFAlt.Word},
                RegisterBC_ = {Word = RegisterBC_.Word},
                RegisterDE_ = {Word = RegisterDE_.Word},
                RegisterHL_ = {Word = RegisterHL_.Word},
                IX = IX,
                IY = IY,
                _registerI = _registerI,
                _r = _r,
                _r7 = _r7,
                SP = SP,
                _registerPc = _registerPc,
                _iff1 = _iff1,
                _iff2 = _iff2,
                _im = _im,
                _halted = _halted
            };
        }

        /// <summary>
        ///     Resets the Z80 status
        /// </summary>
        public void Reset()
        {
            AF = 0;
            BC = 0;
            DE = 0;
            HL = 0;

            RegisterAFAlt.Word = 0;
            RegisterBC_.Word = 0;
            RegisterDE_.Word = 0;
            RegisterHL_.Word = 0;

            IX = 0;
            IY = 0;

            _registerI = 0;
            _r = 0;
            _r7 = 0;

            SP = 0xFFFF;
            _registerPc = 0;

            _iff1 = false;
            _iff2 = false;
            _im = 0;

            _halted = false;
        }

        /// <summary>
        ///     Serializes the status in a text stream
        /// </summary>
        /// <param name="stream"></param>
        public void Serialize(TextWriter stream)
        {
            stream.WriteLine("RegisterPc= {0}, SP= {1}", PC, SP);
            stream.WriteLine("A= {0}, F= {1}, I= {2}, R= {3}, BC= {4}, DE= {5}, HL= {6}", A, F, I, R, BC,
                DE, HL);
            stream.WriteLine("AF'= {0}, BC'= {1}, DE'= {2}, HL'= {3}", RegisterAFAlt.Word, RegisterBC_.Word,
                RegisterDE_.Word, RegisterHL_.Word);
            stream.WriteLine("IX= {0}, IY= {1}", IX, IY);
            stream.WriteLine("IM= {0}, IFF1= {1}, IFF2= {2}", IM, IFF1 ? 1 : 0, IFF2 ? 1 : 0);
            stream.WriteLine();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            var streamWriter = new StringWriter(stringBuilder);
            Serialize(streamWriter);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Заполняет объект <see cref="T:System.Runtime.Serialization.SerializationInfo"/> данными, необходимыми для сериализации целевого объекта.
        /// </summary>
        /// <param name="info"><see cref="T:System.Runtime.Serialization.SerializationInfo"/> для заполнения данными.</param><param name="context">Конечный объект (см. <see cref="T:System.Runtime.Serialization.StreamingContext"/>) для этой сериализации.</param><exception cref="T:System.Security.SecurityException">У вызывающего объекта отсутствует необходимое разрешение.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}