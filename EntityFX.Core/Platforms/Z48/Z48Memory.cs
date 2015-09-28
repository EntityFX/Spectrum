using EntityFX.Core.Common;
using System;

namespace EntityFX.Core.Platforms.Z48
{
    public class Z48Memory : IMemory
    {
        readonly byte[] _memory = new byte[0x10000];

        /// <summary>
        ///     Total size in byte of memory (include paged memory too)
        /// </summary>
        public int Size
        {
            get { return 0x10000; }
        }

        /// <summary>
        ///     Access to plain memory not using any pagination function.
        ///     It can be used for cheat search, for direct memory access
        ///     and so on
        /// </summary>
        public byte this[int address]
        {
            get
            {
                return _memory[address];
            }
            set
            {
                _memory[address] = value;
            }
        }

        /// <summary>
        ///     Byte array containing the whole memory.
        ///     It can be used for cheat search, for direct memory access,
        ///     for saving and loading images and so on
        /// </summary>
        public byte[] Raw
        {
            get { return _memory; }
        }

        public event OnReadHandler OnRead;
        public event OnWriteHandler OnWrite;

        /// <summary>
        ///     Read a single byte from memory
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>Byte readed</returns>
        public byte ReadByte(ushort address)
        {
            if (OnRead != null)
                OnRead(address);
            return _memory[address];
        }

        /// <summary>
        ///     Write a single byte in memory
        /// </summary>
        /// <param name="address">Address to write to</param>
        /// <param name="value">Byte to write</param>
        public void WriteByte(ushort address, byte value)
        {
            // Avoid ROM write
            if (address < 0x4000)
                return;

            _memory[address] = value;

            if (OnWrite != null)
                OnWrite(address, value);
        }

        /// <summary>
        ///     Read a word from memory
        /// </summary>
        /// <param name="address">
        ///     Address to read from. Z80 is big endian so bits 0-7 are readed from Address, bits 8-15 are readed
        ///     from Address + 1
        /// </param>
        /// <returns>Word readed</returns>
        public ushort ReadWord(ushort address)
        {
            return (ushort)(ReadByte((ushort)(address + 1)) << 8 | ReadByte(address));
        }

        /// <summary>
        ///     Write a word to memory
        /// </summary>
        /// <param name="address">
        ///     Address to write to. Z80 is big endian so bits 0-7 are writed to Address, bits 8-15 are writed to
        ///     Address + 1
        /// </param>
        /// <param name="value">Word to write</param>
        public void WriteWord(ushort address, ushort value)
        {
            WriteByte(address, (byte)(value & 0x00FF));
            WriteByte((ushort)(address + 1), (byte)((value & 0xFF00) >> 8));
        }

        public void LoadPage(byte[] page, int offset)
        {
            Array.Copy(page, 0, _memory, offset, page.Length);
        }
    }
}