namespace EntityFX.Core.Common
{
    public interface IInputOutputDevice : IInputDevice, IOutputDevice
    {
        /// <summary>
        /// Read a single byte from a port
        /// </summary>
        /// <param name="port">Port to read from</param>
        /// <returns>Byte readed</returns>
        byte ReadPort(ushort port);
    }
}