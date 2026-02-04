using System.Drawing;
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

    public abstract Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments);

    /// <summary>
    /// Clear the current render target with the given clear color.
    /// </summary>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    /// <param name="a">The alpha component.</param>
    public abstract void Clear(float r, float g, float b, float a = 1.0f);

    /// <summary>
    /// Clear the current render target with the given clear color.
    /// </summary>
    /// <param name="color">The color to clea with.</param>
    public void Clear(Color color)
        => Clear(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    
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