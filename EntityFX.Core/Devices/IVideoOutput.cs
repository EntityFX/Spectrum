using System.Drawing;

namespace EntityFX.Core.Devices
{
    public interface IVideoOutput
    {
        /// <summary>
        /// Bitmap where output will be flushed if no other bitmap is specified
        /// </summary>
        Bitmap OutputBitmap
        {
            get; set; }

        /// <summary>
        /// Draw complete video from memory to default bitmap
        /// </summary>
        void Refresh();

        /// <summary>
        /// Draw complete video from memory to specified bitmap
        /// </summary>
        /// <param name="Video">Bitmap</param>
        void Refresh(Bitmap Video);

        /// <summary>
        /// Redraw the screen inverting the ink and the paper color
        /// </summary>
        void Flash();
    }
}