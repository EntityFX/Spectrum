namespace EntityFX.Core.Common.Cpu.Registers
{
    public class ByteRegister
    {
        public byte Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private byte _value;

        public ByteRegister()
        {
            
        }

        public ByteRegister(byte value)
        {
            _value = value;
        }

    }
}