namespace EntityFX.Core.Common.Cpu.Registers
{
    public class WordRegister
    {
        private readonly ByteRegister _low = new ByteRegister();
        private readonly ByteRegister _high = new ByteRegister();

        public ByteRegister Low
        {
            get { return _low; }
        }

        public ByteRegister High
        {
            get { return _high; }
        }

        public ushort Word
        {
            get
            {
                return (ushort)((_high.Value << 8) | (_low.Value));
            }
            set
            {
                _high.Value = (byte)((value >> 8) & 0xFF);
                _low.Value = (byte)(value & 0xFF);
            }
        }

        /// <summary>
        /// Used to swap this register with another
        /// </summary>
        /// <param name="Register">Register to swap with this</param>
        public void Swap(WordRegister Register)
        {
            byte hValue = Register._high.Value;
            byte lValue = Register._low.Value;

            Register._high.Value = _high.Value;
            Register._low.Value = _low.Value;

            _high.Value = hValue;
            _low.Value = lValue;
        }
    }
}