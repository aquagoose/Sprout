namespace Sprout.Graphics;

/// <summary>
/// Defines various miscellaneous extensions.
/// </summary>
public static class GraphicsExtensions
{
    extension(PixelFormat format)
    {
        public uint BytesPerPixel
        {
            get
            {
                return format switch
                {
                    PixelFormat.RGBA8 => 4,
                    _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
                };
            }
        }

        public uint BitsPerPixel => format.BytesPerPixel / 8;
    }
}