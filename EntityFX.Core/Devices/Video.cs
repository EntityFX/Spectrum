using System.Drawing;
using EntityFX.Core.Common;

namespace EntityFX.Core.Devices
{
    public class Video : IOutputDevice, IVideoOutput
    {
        private readonly IMemory _memory;
        private bool _flashStateInverse = false;

        private Bitmap _defaultOutputBitmap;
        private Color _backgroundColor;

        private readonly Color[] Table_Colors =
        {
            Color.Black,
            Color.Blue,
            Color.Red,
            Color.Magenta,
            Color.Green,
            Color.Cyan,
            Color.Yellow,
            Color.WhiteSmoke,
            Color.Black,
            Color.LightBlue,
            Color.Tomato,
            Color.LightPink,
            Color.LightGreen,
            Color.LightCyan,
            Color.LightYellow,
            Color.White
        };


        /// <summary>
        /// Create a new video output based on memory specified
        /// </summary>
        /// <param name="Memory">Z80 Memory</param>
        public Video(IMemory Memory)
        {
            _memory = Memory;
            _memory.OnWrite += new OnWriteHandler(_Memory_OnWrite);
        }

        /// <summary>
        /// A port access. When Z80 asks to write to a port
        /// consumers (usually IIO implementers) should asks to every input device (IODevice implementers)
        /// to write the byte to the specified port. If the result of the function is true, this means
        /// that the ODevice has handled the request so the consumer should not ask to other devices for the
        /// output
        /// </summary>
        /// <param name="port">Port address</param>
        /// <param name="value">Value to write</param>
        /// <returns>
        /// True if the port is handled by this device, false otherwise.
        /// This is different from input ports because an output port can be handled
        /// by more than one Device
        /// </returns>
        public bool WritePort(ushort port, byte value)
        {
            if ((port & 0x01) == 0)
            {
                _backgroundColor = Table_Colors[value & 0x07];
                return true;
            }
            return false;
        }

        private void _Memory_OnWrite(ushort Address, byte Value)
        {
            if (Address >= 0x5B00)
                // Address is over video memory
                return;
            else if (Address >= 0x5800)
            {
                // Address is in attribute memory

                // Memory still contains the old copy (so bad to do this!!!)
                _memory.Raw[Address] = Value;

                Color fgColor;
                Color bgColor;
                GetColorsFromAttributeAddress(Address, _flashStateInverse, out fgColor, out bgColor);

                Draw8Bytes(GetPixelAddressFromAttributeAddress(Address), fgColor, bgColor);
            }
            else if (Address >= 0x4000)
            {
                //Address is in pixel memory

                // Memory still contains the old copy (so bad to do this!!!)
                _memory.Raw[Address] = Value;
                DrawSingleByte(Address);
            }

        }

        /// <summary>
        /// Get ink and paper colors from attribute address
        /// </summary>
        /// <param name="Address">Address of attribute</param>
        /// <param name="FlashReversed">True if Spectrum is in the state to display flashing pixel in reverse</param>
        /// <param name="Foreground">Foreground attribute</param>
        /// <param name="Background">Background attribute</param>
        public void GetColorsFromAttributeAddress(ushort Address, bool FlashReversed,
            out Color Foreground, out Color Background)
        {
            GetColorsFromAttribute(_memory[Address], FlashReversed, out Foreground, out Background);
        }

        /// <summary>
        /// Get ink and paper colors from attribute
        /// </summary>
        /// <param name="Attribute">The attribute</param>
        /// <param name="FlashReversed">True if Spectrum is in the state to display flashing pixel in reverse</param>
        /// <param name="Foreground">Foreground attribute</param>
        /// <param name="Background">Background attribute</param>
        private void GetColorsFromAttribute(byte Attribute, bool FlashReversed, out Color Foreground,
            out Color Background)
        {
#warning Non funziona correttamente il bright

            /*			
			if((Attribute & 0x80) != 0 && FlashReversed) 
			{
				Foreground = NibbleToColor(_b);
				Background = NibbleToColor(_b >> 4);

				*ink  = (attr & ( 0x0f << 3 ) ) >> 3;
				*paper= (attr & 0x07) + ( (attr & 0x40) >> 3 );
			} 
			else 
			{
				Foreground = NibbleToColor(_b);
				Background = NibbleToColor(_b >> 4);

				*ink= (attr & 0x07) + ( (attr & 0x40) >> 3 );
				*paper= (attr & ( 0x0f << 3 ) ) >> 3;
			}
			*/
            Foreground = Table_Colors[(Attribute & 0x07)];
            Background = Table_Colors[((Attribute >> 3) & 0x0F)];

            if (FlashReversed && ((Attribute & 0x80) != 0))
            {
                Color c = Foreground;
                Foreground = Background;
                Background = c;
            }

        }

        /// <summary>
        /// Draw 8 Bytes (8x8 pixels). It can be used when an attribute changes or during screen blink
        /// </summary>
        /// <param name="Address">Absolute address of the first pixel. The address of the other 7 pixels blocks are aa001aaaaaaaa aa010aaaaaaaa and so on.</param>
        /// <param name="fgColor">Foreground color (ink)</param>
        /// <param name="bgColor">Background color (paper)</param>
        public void Draw8Bytes(ushort Address, Color fgColor, Color bgColor)
        {
            for (ushort y = 0; y < 8; y++)
                DrawSingleByte((ushort)(Address | (y << 8)), fgColor, bgColor);
        }


        /// <summary>
        /// Draw a single byte of video memory to specified bitmap
        /// </summary>
        /// <param name="Address">The absolute address (0x4000 + relative address)</param>
        public void DrawSingleByte(ushort Address)
        {
            DrawSingleByte(_defaultOutputBitmap, Address);
        }

        /// <summary>
        /// This version of DrawSingleByte can be used when the bgColor and the fgColor is well known
        /// It can be used for blinking management and during incremental screen refresh.
        /// For details see other versions
        /// </summary>
        /// <param name="Address">Absolute address of pixel</param>
        /// <param name="fgColor">Foreground color (ink)</param>
        /// <param name="bgColor">Background color (paper)</param>
        public void DrawSingleByte(ushort Address, Color fgColor, Color bgColor)
        {
            byte _b = _memory.Raw[Address];


            ushort RelativeAddress = (ushort)(Address - 0x4000);

            byte x8 = (byte)(RelativeAddress & 0x001F);
            byte memory_y = (byte)((RelativeAddress >> 5));

            byte y = (byte)(memory_y & 0xC0 | ((memory_y & 0x07) << 3) | (memory_y >> 3) & 0x07);

            for (byte x1 = 0; x1 < 8; x1++)
            {
                // x position
                byte x = (byte)((x8 << 3) | x1);
                // mask to use to check bit
                byte x1mask = (byte)(0x80 >> x1);
                if ((_b & x1mask) != 0)
                    _defaultOutputBitmap.SetPixel(x, y, fgColor);
                else
                    _defaultOutputBitmap.SetPixel(x, y, bgColor);
            }

        }

        /// <summary>
        /// Draw a single byte of video memory  to specified bitmap
        /// </summary>
        /// <param name="Video">Bitmap</param>
        /// <param name="Address">The absolute address (0x4000 + relative address)</param>
        public void DrawSingleByte(Bitmap Video, ushort Address)
        {
            byte _b = _memory.Raw[Address];

            // _Address rappresents the relative address.
            // It's in the form yyyyyyyy xxxxx. yyyyyyyy does not really rappresent
            // y coordinate.
            //
            // The video is splitted in 3 sections, up, middle low section. The
            // section is determined by the first 2 bit of memory_y. In each
            // section the y rows are 8/interleaved.
            // Example
            // Memory_y   y
            //        0   0
            //        1   8
            //        2   16
            //        .
            //        8   1
            //        9   9
            //       10  17
            //
            // The format of memory_y is bb yyy zzz where bb is the block, yyy
            // is the y of the pixel in block and zzz is the block
            // to determine y we can invert yyy with zzz.

            ushort RelativeAddress = (ushort)(Address - 0x4000);

            byte x8 = (byte)(RelativeAddress & 0x001F);
            byte memory_y = (byte)((RelativeAddress >> 5));

            byte y = (byte)(memory_y & 0xC0 | ((memory_y & 0x07) << 3) | (memory_y >> 3) & 0x07);

            Color fgColor;
            Color bgColor;
            GetColorsFromPixelAddress(Address, _flashStateInverse, out fgColor, out bgColor);

            for (byte x1 = 0; x1 < 8; x1++)
            {
                // x position
                byte x = (byte)((x8 << 3) | x1);
                // mask to use to check bit
                byte x1mask = (byte)(0x80 >> x1);
                if ((_b & x1mask) != 0)
                    Video.SetPixel(x, y, fgColor);
                else
                    Video.SetPixel(x, y, bgColor);
            }

        }

        /// <summary>
        /// From the address of an attribute retrieve the address of the first 8 pixels associated with the attribute
        /// The address returned is in the form aa000aaaaaaaa. The address of the other 7 pixels blocks are aa001aaaaaaaa
        /// aa010aaaaaaaa and so on.
        /// </summary>
        /// <param name="AttributeAddress">Address of the attribute</param>
        /// <returns>The address of the first 8 pixels</returns>
        public ushort GetPixelAddressFromAttributeAddress(ushort AttributeAddress)
        {
            ushort AttributeRelativeAddress = (ushort)(AttributeAddress - 0x5800);
            ushort PixelRelativeAddress =
                (ushort)(((AttributeRelativeAddress & 0x300) << 3) | AttributeRelativeAddress & 0xFF);
            return (ushort)(0x4000 + PixelRelativeAddress);
        }

        /// <summary>
        /// From a memory address of an eight pixel set retrieve the atribute
        /// </summary>
        /// <param name="Address">The absolute address (0x4000 + relative address)</param>
        /// <param name="FlashReversed">True if Spectrum is in the state to display flashing pixel in reverse</param>
        /// <param name="Foreground">Foreground attribute</param>
        /// <param name="Background">Background attribute</param>
        public void GetColorsFromPixelAddress(ushort Address, bool FlashReversed, out Color Foreground,
            out Color Background)
        {
            ushort RelativeAddress = (ushort)(Address - 0x4000);

            // The color of a pixel is determined with a byte at the end of the screen memory
            // Each byte determines the color of an 8x8 pixel group so of a block.
            // To determine the address of an attribute (so the color was called on speccy)
            // from the memory address in the form of bb yyy zzz xxxxx we can use
            // bb zzz xxxxx (section + block address).
            ushort AttributeRelativeAddress = (ushort)((RelativeAddress >> 3) & 0x300 | RelativeAddress & 0xFF);

            GetColorsFromAttribute(_memory[AttributeRelativeAddress + 0x5800], FlashReversed, out Foreground,
                out Background);
        }

        /// <summary>
        /// Draw complete video from memory to specified bitmap
        /// </summary>
        /// <param name="Video">Bitmap</param>
        public void Refresh(Bitmap Video)
        {
            // Main x cicle
            for (ushort Address = 0x4000; Address < 0x5800; Address++)
                DrawSingleByte(Video, Address);

        }

        /// <summary>
        /// Draw complete video from memory to default bitmap
        /// </summary>
        public void Refresh()
        {
            Refresh(_defaultOutputBitmap);
        }

        /// <summary>
        /// Redraw the screen inverting the ink and the paper color
        /// </summary>
        public void Flash()
        {
            // Invert the flashing state
            _flashStateInverse = !_flashStateInverse;

            // Look for an an attribute with flash bit set
            for (ushort Address = 0x5800; Address < 0x5B00; Address++)
                if ((_memory.Raw[Address] & 0x80) != 0)
                {
                    Color fgColor;
                    Color bgColor;
                    GetColorsFromAttributeAddress(Address, _flashStateInverse, out fgColor, out bgColor);

                    Draw8Bytes(GetPixelAddressFromAttributeAddress(Address), fgColor, bgColor);

                }
        }

        /// <summary>
        /// Bitmap where output will be flushed if no other bitmap is specified
        /// </summary>
        public Bitmap OutputBitmap
        {
            get
            {
                return _defaultOutputBitmap;
            }
            set
            {
                _defaultOutputBitmap = value;
            }
        }
    }
}