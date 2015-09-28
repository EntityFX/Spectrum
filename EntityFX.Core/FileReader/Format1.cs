using System;
using EntityFX.Core.Common.Cpu.Registers;
using EntityFX.Core.CPU.Registers;

namespace EntityFX.Core.FileReader
{
    public class Format1
    {

        /// <summary>
        /// Z80 status readed from the file
        /// </summary>
        public IRegisterFile Status { get; set; }

        /// <summary>
        /// Border colour
        /// </summary>
        public byte BorderColour { get; set; }

        /// <summary>
        /// Basic SamRom switched in
        /// </summary>
        public bool BasicSamRom { get; set; }

        /// <summary>
        /// Memory compressed flag
        /// </summary>
        protected bool DataCompressed { get; set; }

        /// <summary>
        /// Byte at position 12 of the file
        /// </summary>
        protected byte byte12 { get; set; }

        /// <summary>
        /// Byte at position 29 of the file
        /// </summary>
        protected byte byte29 { get; set; }

        /// <summary>
        /// Gets or sets the file version
        /// </summary>
        public byte FileVersion = 1;

        /// <summary>
        /// RAM
        /// </summary>
        public byte[] RAM { get; set; }

        public virtual void Read(byte[] buffer)
        {
            ReadStatus(buffer);
            ReadMemory(buffer, 30, 0xC000);
        }

        protected virtual void ReadStatus(byte[] buffer)
        {
            Status = new RegisterFile();
            Status.A = buffer[0];
            Status.F = buffer[1];
            Status.BC = (ushort)(buffer[2] | buffer[3] << 8);
            Status.HL = (ushort)(buffer[4] | buffer[5] << 8);
            Status.PC = (ushort)(buffer[6] | buffer[7] << 8);
            Status.SP = (ushort)(buffer[8] | buffer[9] << 8);
            Status.I = buffer[10];
            Status.R = buffer[11];

            byte12 = buffer[12];
            Status.R7 = (byte)(byte12 & 0x80);
            BorderColour = (byte)((byte12 >> 1) & 0x07);
            BasicSamRom = (byte12 & 0x10) != 0;
            DataCompressed = (byte12 & 0x20) != 0;


            Status.DE = (ushort)(buffer[13] | buffer[14] << 8);

            Status.RegisterBC_.Word = (ushort)(buffer[15] | buffer[16] << 8);
            Status.RegisterDE_.Word = (ushort)(buffer[17] | buffer[18] << 8);
            Status.RegisterHL_.Word = (ushort)(buffer[19] | buffer[20] << 8);
            Status.RegisterAFAlt.High.Value = buffer[21];
            Status.RegisterAFAlt.Low.Value = buffer[22];

            Status.IY = (ushort)(buffer[23] | buffer[24] << 8);
            Status.IX = (ushort)(buffer[25] | buffer[26] << 8);

            Status.IFF1 = buffer[27] != 0;
            Status.IFF2 = buffer[28] != 0;


            byte29 = buffer[29];

            Status.IM = (byte)(byte29 & 0x03);


        }

        protected void ReadMemory(byte[] buffer, int offset, int uncompressedSize)
        {
            RAM = new byte[uncompressedSize];

            if (!DataCompressed)
            {
                // Data is not compressed. Copy array
                Array.Copy(buffer, offset, RAM, 0, buffer.Length - offset);
            }
            else
            {
                // Data is compressed and should be decompressed

                /*
          The compression method is very simple: it replaces repetitions of at least 
          five equal bytes by a four-byte code ED ED xx yy, which stands for "byte yy repeated xx times". Only
          sequences of length at least 5 are coded. The exception is sequences consisting of ED's; if they are 
          encountered, even two ED's are encoded into ED ED 02 ED. Finally, every byte directly following a 
          single ED is not taken into a block, for example ED 6*00 is not encoded into ED ED ED 06 00 but into 
          ED 00 ED ED 05 00. The block is terminated by an end marker, 00 ED ED 00.
                */
                int ramPointer = 0;


                while (offset <= buffer.Length)
                {

                    if (buffer.Length <= offset + 4 &&
                      buffer[offset] == 0x00 && buffer[offset + 1] == 0xED && buffer[offset + 2] == 0xED && buffer[offset + 3] == 0x00)
                        // End of stream
                        return;
                    if (buffer[offset] != 0xED)
                    {
                        RAM[ramPointer] = buffer[offset];
                        ramPointer++;
                        offset++;
                    }
                    else
                    {
                        if (buffer.Length <= offset)
                        {
                            RAM[ramPointer] = 0xED;
                            offset++;
                            ramPointer++;
                        }
                        else if (buffer[offset + 1] != 0xED)
                        {
                            RAM[ramPointer] = 0xED;
                            offset++;
                            ramPointer++;
                        }
                        else
                        {
                            // compressed section
                            for (int i = buffer[offset + 2]; i > 0; i--)
                            {
                                RAM[ramPointer] = buffer[offset + 3];
                                ramPointer++;
                            }
                            offset += 4;
                        }
                    }
                }

            }

        }


        /// <summary>
        /// Returns a string rapresenting the object
        /// </summary>
        /// <returns>A string rapresenting the object</returns>
        public override string ToString()
        {
            string returnValue = "";
            returnValue += string.Format("{0}\r\nBorder colour= {1}, Basic SamRom= {2}, Data compressed= {3}, byte12= {4}, byte29= {5}", Status, BorderColour, BasicSamRom, DataCompressed, byte12, byte29);
            return returnValue;
        }

    }
}