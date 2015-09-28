using System;
using EntityFX.Core.Common;

namespace EntityFX.Core.Devices
{
    public class KeyEventArgs : EventArgs
    {
        private readonly bool _shift;
        private readonly bool _control;
        private readonly int _keyValue;

        public KeyEventArgs(int keyValue, bool shift, bool control)
        {
            _control = control;
            _keyValue = keyValue;
            _shift = shift;
        }

        public bool Shift
        {
            get { return _shift; }
        }

        public bool Control
        {
            get { return _control; }
        }

        public int KeyValue
        {
            get { return _keyValue; }
        }
    }

    public class Keyboard : IInputDevice, IKeyboardInput
    {
        private readonly KeyLine _key15 = new KeyLine();
        private readonly KeyLine _key60 = new KeyLine();
        private readonly KeyLine _keyAg = new KeyLine();
        private readonly KeyLine _keyBSpc = new KeyLine();
        private readonly KeyLine _keyCapsV = new KeyLine();
        private readonly KeyLine _keyHEnt = new KeyLine();
        private readonly KeyLine _keyQt = new KeyLine();
        private readonly KeyLine _keyYp = new KeyLine();

        /// <summary>
        ///     A port access. When Z80 asks to read from a port
        ///     consumers (usually IIO implementers) should asks to every input device (IIDevice implementers)
        ///     to read the byte from the specified port. If the result of the function is true, this means
        ///     that the IInputDevice has handled the request so the consumer should not ask to other devices for the
        ///     input
        /// </summary>
        /// <param name="port">Port address</param>
        /// <param name="value">value readed</param>
        /// <returns>True if the port is handled by this device, false otherwise</returns>
        public bool ReadPort(ushort port, out byte value)
        {
            value = 0xFF;

            // Check if is a keyboard access
            if ((port & 0xFF) != 0xFE)
                return false;

            if ((port & 0x8000) == 0)
                value = (byte) (value & _keyBSpc.Value);

            if ((port & 0x4000) == 0)
                value = (byte) (value & _keyHEnt.Value);

            if ((port & 0x2000) == 0)
                value = (byte) (value & _keyYp.Value);

            if ((port & 0x1000) == 0)
                value = (byte) (value & _key60.Value);

            if ((port & 0x800) == 0)
                value = (byte) (value & _key15.Value);

            if ((port & 0x400) == 0)
                value = (byte) (value & _keyQt.Value);

            if ((port & 0x200) == 0)
                value = (byte) (value & _keyAg.Value);

            if ((port & 0x100) == 0)
                value = (byte) (value & _keyCapsV.Value);

            return true;
        }

        /// <summary>
        ///     Parse the key and set the state for input and output answers
        /// </summary>
        /// <param name="down">True if the key has been pressed (KeyDown event), False if the key has been released (KeyUp event)</param>
        /// <param name="e">The event arg passed to the event</param>
        public void ParseKey(bool down, KeyEventArgs e)
        {
            bool SetBit = !down;

            bool CAPS = e.Shift;
            bool SYMB = e.Control;

            int ascii = e.KeyValue;

            // Change control versions of keys to lower case
            if ((ascii >= 1) && (ascii <= 0x27) && SYMB)
                ascii = ascii + 97 - 1;

            if (CAPS)
                _keyCapsV.Reset(0);
            else
                _keyCapsV.Set(0);

            if (SYMB)
                _keyBSpc.Reset(1);
            else
                _keyBSpc.Set(1);

            switch (ascii)
            {
                case 8: // Backspace
                    if (down)
                    {
                        _key60.Reset(0);
                        _keyCapsV.Reset(0);
                    }
                    else
                    {
                        _key60.Set(0);
                        if (!CAPS)
                            _keyCapsV.Set(1);
                    }
                    break;


                case 65: // A
                    _keyAg.Set(0, SetBit);
                    break;
                case 66: // B
                    _keyBSpc.Set(4, SetBit);
                    break;
                case 67: // C
                    _keyCapsV.Set(3, SetBit);
                    break;
                case 68: // D
                    _keyAg.Set(2, SetBit);
                    break;
                case 69: // E
                    _keyQt.Set(2, SetBit);
                    break;
                case 70: // F
                    _keyAg.Set(3, SetBit);
                    break;
                case 71: // G
                    _keyAg.Set(4, SetBit);
                    break;
                case 72: // H
                    _keyHEnt.Set(4, SetBit);
                    break;
                case 73: // I
                    _keyYp.Set(2, SetBit);
                    break;
                case 74: // J
                    _keyHEnt.Set(3, SetBit);
                    break;
                case 75: // K
                    _keyHEnt.Set(2, SetBit);
                    break;
                case 76: // L
                    _keyHEnt.Set(1, SetBit);
                    break;
                case 77: // M
                    _keyBSpc.Set(2, SetBit);
                    break;
                case 78: // N
                    _keyBSpc.Set(3, SetBit);
                    break;
                case 79: // O
                    _keyYp.Set(1, SetBit);
                    break;
                case 80: // P
                    _keyYp.Set(0, SetBit);
                    break;
                case 81: // Q
                    _keyQt.Set(0, SetBit);
                    break;
                case 82: // R
                    _keyQt.Set(3, SetBit);
                    break;
                case 83: // S
                    _keyAg.Set(1, SetBit);
                    break;
                case 84: // T
                    _keyQt.Set(4, SetBit);
                    break;
                case 85: // U
                    _keyYp.Set(3, SetBit);
                    break;
                case 86: // V
                    _keyCapsV.Set(4, SetBit);
                    break;
                case 87: // W
                    _keyQt.Set(1, SetBit);
                    break;
                case 88: // X
                    _keyCapsV.Set(2, SetBit);
                    break;
                case 89: // Y
                    _keyYp.Set(4, SetBit);
                    break;
                case 90: // Z
                    _keyCapsV.Set(1, SetBit);
                    break;
                case 48: // 0
                    _key60.Set(0, SetBit);
                    break;
                case 49: // 1
                    _key15.Set(0, SetBit);
                    break;
                case 50: // 2
                    _key15.Set(1, SetBit);
                    break;
                case 51: // 3
                    _key15.Set(2, SetBit);
                    break;
                case 52: // 4
                    _key15.Set(3, SetBit);
                    break;
                case 53: // 5
                    _key15.Set(4, SetBit);
                    break;
                case 54: // 6
                    _key60.Set(4, SetBit);
                    break;
                case 55: // 7
                    _key60.Set(3, SetBit);
                    break;
                case 56: // 8
                    _key60.Set(2, SetBit);
                    break;
                case 57: // 9
                    _key60.Set(1, SetBit);
                    break;
                case 96: // Keypad 0
                    _key60.Set(0, SetBit);
                    break;
                case 97: // Keypad 1
                    _key15.Set(0, SetBit);
                    break;
                case 98: // Keypad 2
                    _key15.Set(1, SetBit);
                    break;
                case 99: // Keypad 3
                    _key15.Set(2, SetBit);
                    break;
                case 100: // Keypad 4
                    _key15.Set(3, SetBit);
                    break;
                case 101: // Keypad 5
                    _key15.Set(4, SetBit);
                    break;
                case 102: // Keypad 6
                    _key60.Set(4, SetBit);
                    break;
                case 103: // Keypad 7
                    _key60.Set(3, SetBit);
                    break;
                case 104: // Keypad 8
                    _key60.Set(2, SetBit);
                    break;
                case 105: // Keypad 9
                    _key60.Set(1, SetBit);
                    break;


                case 106: // Keypad *
                    if (down)
                    {
                        _keyBSpc.Reset(2);
                        _keyBSpc.Reset(4);
                    }
                    else
                    {
                        if (SYMB)
                            _keyBSpc.Set(4);
                        else
                        {
                            _keyBSpc.Set(2);
                            _keyBSpc.Set(4);
                        }
                    }
                    break;

                case 107: // Keypad +
                    if (down)
                    {
                        _keyHEnt.Reset(2);
                        _keyBSpc.Reset(1);
                    }
                    else
                    {
                        _keyHEnt.Set(2);
                        if (!SYMB)
                            _keyBSpc.Set(1);
                    }
                    break;

                case 109: // Keypad -
                    if (down)
                    {
                        _keyHEnt.Reset(3);
                        _keyBSpc.Reset(1);
                    }
                    else
                    {
                        _keyHEnt.Set(3);
                        if (!SYMB)
                            _keyBSpc.Set(1);
                    }
                    break;


                case 110: // Keypad .
                    if (down)
                    {
                        _keyBSpc.Reset(2);
                        _keyBSpc.Reset(1);
                    }
                    else
                    {
                        _keyBSpc.Set(2);
                        if (!SYMB)
                            _keyBSpc.Set(1);
                    }
                    break;

                case 111: // Keypad /
                    if (down)
                    {
                        _keyCapsV.Reset(4);
                        _keyBSpc.Reset(1);
                    }
                    else
                    {
                        _keyCapsV.Set(4);
                        if (!SYMB)
                            _keyBSpc.Set(1);
                    }
                    break;


                case 37: // Left
                    if (down)
                    {
                        _key15.Reset(4);
                        _keyCapsV.Reset(0);
                    }
                    else
                    {
                        _key15.Set(4);
                        if (!SYMB)
                            _keyBSpc.Set(1);
                    }
                    break;

                case 38: // Up
                    if (down)
                    {
                        _key60.Reset(3);
                        _keyCapsV.Reset(0);
                    }
                    else
                    {
                        _key60.Set(3);
                        if (!SYMB)
                            _keyCapsV.Set(1);
                    }
                    break;

                case 39: // Right
                    if (down)
                    {
                        _key60.Reset(2);
                        _keyCapsV.Reset(0);
                    }
                    else
                    {
                        _key60.Set(2);
                        if (!SYMB)
                            _keyCapsV.Set(1);
                    }
                    break;

                case 40: // Down
                    if (down)
                    {
                        _key60.Reset(4);
                        _keyCapsV.Reset(0);
                    }
                    else
                    {
                        _key60.Set(4);
                        if (!SYMB)
                            _keyCapsV.Set(1);
                    }
                    break;


                case 13: // RETURN
                    _keyHEnt.Set(0, SetBit);
                    break;
                case 32: // SPACE BAR
                    _keyBSpc.Set(0, SetBit);
                    break;
                case 187: // =/+ key
                    if (down)
                    {
                        if (CAPS)
                            _keyHEnt.Reset(2);
                        else
                            _keyHEnt.Reset(1);
                        _keyBSpc.Reset(1);
                        _keyCapsV.Set(0);
                    }
                    else
                    {
                        _keyHEnt.Set(2);
                        _keyHEnt.Set(1);
                        _keyBSpc.Set(1);
                    }
                    break;

                case 189: // -/_ key
                    if (down)
                    {
                        if (CAPS)
                            _key60.Reset(0);
                        else
                            _keyHEnt.Reset(3);

                        _keyBSpc.Reset(1);
                        _keyCapsV.Set(0);
                    }
                    else
                    {
                        _key60.Set(0);
                        _keyHEnt.Set(3);
                        _keyBSpc.Set(1);
                    }
                    break;

                case 186: // ;/: keys
                    if (down)
                    {
                        if (CAPS)
                            _keyCapsV.Reset(1);
                        else
                            _keyYp.Reset(1);

                        _keyBSpc.Reset(1);
                        _keyCapsV.Set(0);
                    }
                    else
                    {
                        _keyCapsV.Set(1);
                        _keyYp.Set(1);
                        _keyBSpc.Set(1);
                    }
                    break;

                default:
                    // ???
                    break;
            }
        }

        /// <summary>
        ///     A line of keys readed with a single input operation
        /// </summary>
        private class KeyLine
        {
            private byte _value = 0xFF;

            /// <summary>
            ///     Gets or set the value of the key pattern
            /// </summary>
            public byte Value
            {
                get { return _value; }
                set { _value = value; }
            }


            /// <summary>
            ///     Set a bit
            /// </summary>
            /// <param name="bit">Bit to set</param>
            public void Set(byte bit)
            {
                _value = (byte) (_value | (1 << bit));
            }

            /// <summary>
            ///     Reset a bit
            /// </summary>
            /// <param name="bit">Bit to reset</param>
            public void Reset(byte bit)
            {
                _value = (byte) (_value & (~(1 << bit)));
            }

            /// <summary>
            ///     Sets or resets a bit
            /// </summary>
            /// <param name="bit">Bit to change</param>
            /// <param name="value">True if the bit have to be set else false</param>
            public void Set(byte bit, bool value)
            {
                if (value)
                    Set(bit);
                else
                    Reset(bit);
            }
        }
    }
}