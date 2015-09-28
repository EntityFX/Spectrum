namespace EntityFX.Core.Common
{
    public interface IOutputDevice
    {
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
        bool WritePort(ushort port, byte value);
    }
}