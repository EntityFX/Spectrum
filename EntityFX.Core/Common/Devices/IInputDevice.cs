namespace EntityFX.Core.Common
{
    public interface IInputDevice
    {
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
        bool ReadPort(ushort port, out byte value);
    }
}