namespace EntityFX.Core.CPU.Registers
{
    /// <summary>
    ///     Definition of F register content
    /// </summary>
    public static class FlagRegisterDefinition
    {
        /// <summary>
        ///     Carry flag
        /// </summary>
        public const byte C = 0x01;

        /// <summary>
        ///     Add/Subtract flag
        /// </summary>
        public const byte N = 0x02;

        /// <summary>
        ///     Parity flag
        /// </summary>
        public const byte P = 0x04;

        /// <summary>
        ///     Overflow flag
        /// </summary>
        public const byte V = 0x04;

        /// <summary>
        ///     Not used
        /// </summary>
        public const byte _3 = 0x08;

        /// <summary>
        ///     Half carry flag
        /// </summary>
        public const byte H = 0x10;

        /// <summary>
        ///     Not used
        /// </summary>
        public const byte _5 = 0x20;

        /// <summary>
        ///     Zero flag
        /// </summary>
        public const byte Z = 0x40;

        /// <summary>
        ///     Sign flag
        /// </summary>
        public const byte S = 0x80;
    }
}