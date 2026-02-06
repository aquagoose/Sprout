namespace Sprout.Graphics;

/// <summary>
/// A texture is an image, stored in video memory, that can be used during graphics operations.
/// </summary>
public abstract class Texture : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="Texture"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }

    /// <summary>
    /// Dispose of this <see cref="Texture"/>.
    /// </summary>
    public abstract void Dispose();
}