using Sprout.Graphics.OpenGL;

namespace Sprout.Graphics;

/// <summary>
/// Represents a rendering device that graphics commands can be issued to.
/// </summary>
public abstract class GraphicsDevice : IDisposable
{
    /// <summary>
    /// Gets if this <see cref="GraphicsDevice"/> has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; protected set; }
    
    /// <summary>
    /// Gets the <see cref="Sprout.Graphics.Backend"/> render API used for this <see cref="GraphicsDevice"/> instance.
    /// </summary>
    public abstract Backend Backend { get; }

    /// <summary>
    /// Present to the current window.
    /// </summary>
    public abstract void Present();

    /// <summary>
    /// Dispose of this <see cref="GraphicsDevice"/>.
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// Create a <see cref="GraphicsDevice"/>.
    /// </summary>
    /// <param name="sdlWindow">The SDL3 window to create the device with.</param>
    /// <param name="backend">The <see cref="Sprout.Graphics.Backend"/> to use, if any. Pass
    /// <see cref="Backend.Unknown"/> to automatically pick the best backend.</param>
    /// <returns>The created <see cref="GraphicsDevice"/>.</returns>]
    public static GraphicsDevice Create(IntPtr sdlWindow, Backend backend = Backend.Unknown)
    {
        return new GLGraphicsDevice(sdlWindow);
    }
}