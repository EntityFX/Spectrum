using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFX.Core.Common;
using EntityFX.Core.Devices;

namespace EntityFX.Core.Platforms.Z48
{
    class Z48IO : IInputOutputDevice, ISpectrumIo
    {
        private IInputDevice _keyboard;
        private IOutputDevice _video;
        private IInputOutputDevice _DefaultIO = null;
        private IInputOutputDevice _defaultIo;

        public Z48IO(IInputDevice keyboard, IOutputDevice video)
        {
            _keyboard = keyboard;
            _video = video;
        }

        /// <summary>
        /// A port access. When Z80 asks to read from a port
        /// consumers (usually IIO implementers) should asks to every input device (IIDevice implementers)
        /// to read the byte from the specified port. If the result of the function is true, this means
        /// that the IInputDevice has handled the request so the consumer should not ask to other devices for the
        /// input
        /// </summary>
        /// <param name="port">Port address</param>
        /// <param name="value">Value readed</param>
        /// <returns>True if the port is handled by this device, false otherwise</returns>
        public bool ReadPort(ushort port, out byte value)
        {
            if (_keyboard.ReadPort(port, out value))
            {
                return true;
            }

            if (_DefaultIO != null)
            {
                _DefaultIO.ReadPort(port);
                return true;
            }


            return false;
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
            _video.WritePort(port, value);

            if (_DefaultIO != null)
            {
                _DefaultIO.WritePort(port, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Read a single byte from a port
        /// </summary>
        /// <param name="port">Port to read from</param>
        /// <returns>Byte readed</returns>
        public byte ReadPort(ushort port)
        {
            byte value;

            if (_keyboard.ReadPort(port, out value))
                return value;

            if (_DefaultIO != null)
                return _DefaultIO.ReadPort(port);

            return 0xFF;
        }

        public IInputDevice Keyboard
        {
            get { return _keyboard; }
            set { _keyboard = value; }
        }

        public IOutputDevice Video
        {
            get { return _video; }
            set { _video = value; }
        }

        public IInputOutputDevice DefaultIo
        {
            get { return _defaultIo; }
            set { _defaultIo = value; }
        }
    }
}
