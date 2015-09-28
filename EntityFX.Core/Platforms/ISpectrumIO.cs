using EntityFX.Core.Common;

namespace EntityFX.Core.Platforms
{
    public interface ISpectrumIo
    {
        IInputDevice Keyboard { get; set; }

        IOutputDevice Video { get; set; }

        IInputOutputDevice DefaultIo { get; set; }
    }
}