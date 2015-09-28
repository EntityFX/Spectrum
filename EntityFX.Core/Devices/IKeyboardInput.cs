namespace EntityFX.Core.Devices
{
    public interface IKeyboardInput
    {
        void ParseKey(bool down, KeyEventArgs e);
    }
}