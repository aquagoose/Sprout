using System.Drawing;

namespace Sprout.Graphics;

/// <summary>
/// A texture is an image, stored in video memory, that can be used during graphics operations.
/// </summary>
public abstract class Texture : IDisposable
{
    public readonly Size Size;

    public readonly PixelFormat Format;

    public readonly TextureUsage Usage;
    
    public abstract Sampler Sampler { get; set; }

    protected Texture(Size size, PixelFormat format, TextureUsage usage)
    {
        Size = size;
        Format = format;
        Usage = usage;
    }
    
    /// <summary>
    /// Gets if this <see cref="Texture"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    /// <summary>
    /// Dispose of this <see cref="Texture"/>.
    /// </summary>
    public abstract void Dispose();
}