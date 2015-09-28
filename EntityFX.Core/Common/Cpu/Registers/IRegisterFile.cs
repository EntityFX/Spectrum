using System;
using System.IO;
using System.Runtime.Serialization;

namespace EntityFX.Core.Common.Cpu.Registers
{
    public interface IRegisterFile : ICloneable, ISerializable
    {
        #region WordRegister AF

        /// <summary>
        ///     Accumulator and flags register
        /// </summary>
        WordRegister RegisterAF { get; set; }

        /// <summary>
        ///     Access to AF register as word
        /// </summary>
        ushort AF { get; set; }

        /// <summary>
        ///     Access to bits 8-15 of AF
        /// </summary>
        byte A { get; set; }

        /// <summary>
        ///     Access to bits 0-7 of AF
        /// </summary>
        byte F { get; set; }

        #endregion

        #region WordRegister BC

        /// <summary>
        ///     BC Register
        /// </summary>
        WordRegister RegisterBC { get; set; }


        /// <summary>
        ///     Access to BC register as word
        /// </summary>
        ushort BC { get; set; }

        /// <summary>
        ///     Access to 8-15 bits of BC
        /// </summary>
        byte B { get; set; }

        /// <summary>
        ///     Access to 0-7 bits of BC
        /// </summary>
        byte C { get; set; }

        #endregion

        #region WordRegister DE

        /// <summary>
        ///     DE Register
        /// </summary>
        WordRegister RegisterDE { get; set; }

        /// <summary>
        ///     Access to DE WordRegister as word
        /// </summary>
        ushort DE { get; set; }

        /// <summary>
        ///     Access to bit 8-15 of DE
        /// </summary>
        byte D { get; set; }

        /// <summary>
        ///     Access to bit 0-7 of DE
        /// </summary>
        byte E { get; set; }

        #endregion

        #region WordRegister HL

        /// <summary>
        ///     HL Register
        /// </summary>
        WordRegister RegisterHL { get; set; }


        /// <summary>
        ///     Access to HL register as word
        /// </summary>
        ushort HL { get; set; }

        /// <summary>
        ///     Access to bits 8-15 of HL
        /// </summary>
        byte H { get; set; }

        /// <summary>
        ///     Access to bits 0-7 of HL
        /// </summary>
        byte L { get; set; }

        #endregion

        #region WordRegister SP

        /// <summary>
        ///     Stack pointer register
        /// </summary>
        WordRegister RegisterSP { get; set; }

        /// <summary>
        ///     Access to SP register as word
        /// </summary>
        ushort SP { get; set; }

        #endregion

        ushort PC
        {
            get; set; }

        #region Alternate registers (AF', BC', DE', HL')

        /// <summary>
        ///     Alternate Accumulator and Flags Register
        /// </summary>
        WordRegister RegisterAFAlt { get; set; }

        /// <summary>
        ///     Alternate BC Register
        /// </summary>
        WordRegister RegisterBC_ { get; set; }

        /// <summary>
        ///     Alternate DE Register
        /// </summary>
        WordRegister RegisterDE_ { get; set; }

        /// <summary>
        ///     Alternate HL Register
        /// </summary>
        WordRegister RegisterHL_ { get; set; }

        #endregion

        #region Index Registers

        /// <summary>
        ///     Index register IX
        /// </summary>
        WordRegister RegisterIX { get; set; }

        /// <summary>
        ///     Access to IX register as word
        /// </summary>
        ushort IX { get; set; }


        /// <summary>
        ///     Index register IY
        /// </summary>
        WordRegister RegisterIY { get; set; }

        /// <summary>
        ///     Access to IY register as word
        /// </summary>
        ushort IY { get; set; }

        #endregion

        #region IR Register

        /// <summary>
        ///     Interrupt register
        /// </summary>
        byte I { get; set; }

        /// <summary>
        ///     Refresh register
        /// </summary>
        byte R { get; set; }

        /// <summary>
        ///     Refresh register Bit 7
        /// </summary>
        byte R7 { get; set; }

        #endregion

        #region Program Counter

        /// <summary>
        ///     Program counter
        /// </summary>
        ushort RegisterPc { get; set; }

        #endregion

        #region Other states holders

        /// <summary>
        ///     CPU Halted
        /// </summary>
        bool Halted { get; set; }

        /// <summary>
        ///     Main interrupts flip flop
        /// </summary>
        bool IFF1 { get; set; }

        /// <summary>
        ///     Temporary storage for IFF1
        /// </summary>
        bool IFF2 { get; set; }


        /// <summary>
        ///     Interrupt Mode (can be 0, 1, 2)
        /// </summary>
        byte IM { get; set; }

        #endregion

        void Reset();

        void Serialize(TextWriter stream);
    }
}