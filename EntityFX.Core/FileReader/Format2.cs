using System;

namespace EntityFX.Core.FileReader
{
    public class Format2 : Format1
    {

        /// <summary>
        /// Pages of RAM
        /// </summary>
        public byte[][] Pages = new byte[11][];

        /// <summary>
        /// Gets or sets the length of the additional header block.
        /// </summary>
        /// <value>The length of the additional header block.</value>
        public int AdditionalHeaderBlockLength { get; set; }

        /// <summary>
        /// Gets or sets the hardware.
        /// </summary>
        /// <value>The hardware.</value>
        public byte HardwareMode { get; set; }

        /// <summary>
        /// Reads from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public override void Read(byte[] buffer)
        {
            ReadStatus(buffer);
        }

        /// <summary>
        /// Reads the status.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        protected override void ReadStatus(byte[] buffer)
        {
            base.ReadStatus(buffer);

            // Check if is really a V2 file
            if (Status.PC != 0)
            {
                // It's a V1
                base.ReadMemory(buffer, 30, 0xC000);
                return;
            }


            base.FileVersion = 2;

            AdditionalHeaderBlockLength = (buffer[30] | buffer[31] << 8);
            Status.PC = (ushort)(buffer[32] | buffer[33] << 8);
            HardwareMode = buffer[34];


            /*      
            * 35      1       If in SamRam mode, bitwise state of 74ls259.
                              For example, bit 6=1 after an OUT 31,13 (=2*6+1)
                              If in 128 mode, contains last OUT to 0x7ffd
                              If in Timex mode, contains last OUT to 0xf4
            * 36      1       Contains 0xff if Interface I rom paged
                              If in Timex mode, contains last OUT to 0xff
            * 37      1       Bit 0: 1 if R register emulation on
                              Bit 1: 1 if LDIR emulation on
            Bit 2: AY sound in use, even on 48K machines
            Bit 6: (if bit 2 set) Fuller Audio Box emulation
            Bit 7: Modify hardware (see below)
            * 38      1       Last OUT to port 0xfffd (soundchip register number)
            * 39      16      Contents of the sound chip registers
              55      2       Low T state counter
              57      1       Hi T state counter
              58      1       Flag byte used by Spectator (QL spec. emulator)
                              Ignored by Z80 when loading, zero when saving
              59      1       0xff if MGT Rom paged
              60      1       0xff if Multiface Rom paged. Should always be 0.
              61      1       0xff if 0-8191 is ROM, 0 if RAM
              62      1       0xff if 8192-16383 is ROM, 0 if RAM
              63      10      5 x keyboard mappings for user defined joystick
              73      10      5 x ASCII word: keys corresponding to mappings above
              83      1       MGT type: 0=Disciple+Epson,1=Disciple+HP,16=Plus D
              84      1       Disciple inhibit button status: 0=out, 0ff=in
              85      1       Disciple inhibit flag: 0=rom pageable, 0ff=not
           ** 86      1       Last OUT to port 0x1ffd
             * */

            int memoryOffset = 32 + AdditionalHeaderBlockLength;
            ReadMemory(buffer, memoryOffset);

        }

        /// <summary>
        /// Reads the memory.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <param name="offset">The offset.</param>
        protected virtual void ReadMemory(byte[] buffer, int offset)
        {
            /*
      Hereafter a number of memory blocks follow, each containing the compressed data of a 16K block. The compression is according to the old scheme, except for the end-marker, which is now absent. The structure of a memory block is:

              Byte    Length  Description
              ---------------------------
              0       2       Length of compressed data (without this 3-byte header)
                              If length=0xffff, data is 16384 bytes long and not compressed
              2       1       Page number of block
              3       [0]     Data

             The pages are numbered, depending on the hardware mode, in the following way:

              Page    In '48 mode     In '128 mode    In SamRam mode
              ------------------------------------------------------
               0      48K rom         rom (basic)     48K rom
               1      Interface I, Disciple or Plus D rom, according to setting
               2      -               rom (reset)     samram rom (basic)
               3      -               page 0          samram rom (monitor,..)
               4      8000-bfff       page 1          Normal 8000-bfff
               5      c000-ffff       page 2          Normal c000-ffff
               6      -               page 3          Shadow 8000-bfff
               7      -               page 4          Shadow c000-ffff
               8      4000-7fff       page 5          4000-7fff
               9      -               page 6          -
              10      -               page 7          -
              11      Multiface rom   Multiface rom   -In 48K mode, pages 4,5 and 8 are saved. In SamRam mode, pages 4 to 8 are saved. In 128K mode, all pages from 3 to 10 are saved. Pentagon snapshots are very similar to 128K snapshots, while Scorpion snapshots have the 16 RAM pages saved in pages 3 to 18. There is no end marker.       * */

            while (offset < buffer.Length)
            {
                int compressedBlockSize = buffer[offset] | buffer[offset + 1] << 8;
                byte pageNumber = buffer[offset + 2];
                Pages[pageNumber] = new byte[16384];
                offset += 3;
                UncompressBlock(buffer, offset, compressedBlockSize, Pages[pageNumber]);
                if (compressedBlockSize == 0xFFFF)
                    offset += 16384;
                else
                    offset += compressedBlockSize;
            }
        }

        protected virtual void UncompressBlock(byte[] buffer, int offset, int compressedBlockSize, byte[] page)
        {

            // Data is compressed and should be decompressed

            /*
      The compression method is very simple: it replaces repetitions of at least 
      five equal bytes by a four-byte code ED ED xx yy, which stands for "byte yy repeated xx times". Only
      sequences of length at least 5 are coded. The exception is sequences consisting of ED's; if they are 
      encountered, even two ED's are encoded into ED ED 02 ED. Finally, every byte directly following a 
      single ED is not taken into a block, for example ED 6*00 is not encoded into ED ED ED 06 00 but into 
      ED 00 ED ED 05 00. No end marker for the block in V2 and V3 files
            */

            if (compressedBlockSize == 0xFFFF)
            {
                // Data is 16384 and is not compressed
                Array.Copy(buffer, offset, page, 0, 16384);
                return;
            }

            int ramPointer = 0;

            while (compressedBlockSize > 0)
            {

                if (buffer[offset] != 0xED)
                {
                    page[ramPointer] = buffer[offset];
                    ramPointer++;
                    offset++;
                    compressedBlockSize--;
                }
                else
                {
                    if (buffer.Length <= offset)
                    {
                        page[ramPointer] = 0xED;
                        offset++;
                        ramPointer++;
                        compressedBlockSize--;
                    }
                    else if (buffer[offset + 1] != 0xED)
                    {
                        page[ramPointer] = 0xED;
                        offset++;
                        ramPointer++;
                        compressedBlockSize--;
                    }
                    else
                    {
                        // compressed section
                        for (int i = buffer[offset + 2]; i > 0; i--)
                        {
                            page[ramPointer] = buffer[offset + 3];
                            ramPointer++;
                        }
                        offset += 4;
                        compressedBlockSize -= 4;
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
            string sPages;

            if (base.FileVersion == 1)
                sPages = "RAM";
            else
            {
                sPages = "";
                for (int i = 0; i < Pages.Length; i++)
                    if (Pages[i] != null)
                        sPages += string.Format("{0} ", i);

            }

            returnValue = string.Format("Version= {0}, Hardware= {1}\r\n{2}\r\nLoaded pages: {3}", base.FileVersion, HardwareMode, base.ToString(), sPages);

            return returnValue;
        }

    }
}