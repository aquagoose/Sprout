using System.Drawing;
using Sprout.Graphics.D3D11;
using Sprout.Graphics.OpenGL;
using Sprout.Graphics.Vulkan;
using StbImageSharp;

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
    /// Gets the size of the swapchain.
    /// </summary>
    public abstract Size SwapchainSize { get; }

    public abstract Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments);

    protected abstract unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, void* data);

    public unsafe Texture CreateTexture(uint width, uint height, PixelFormat format)
        => CreateTexture(width, height, format, null);

    public unsafe Texture CreateTexture<T>(uint width, uint height, PixelFormat format, ReadOnlySpan<T> data)
        where T : unmanaged
    {
        fixed (void* pData = data)
            return CreateTexture(width, height, format, pData);
    }

    public Texture CreateTexture<T>(uint width, uint height, PixelFormat format, T[] data) where T : unmanaged
        => CreateTexture(width, height, format, data.AsSpan());

    public Texture CreateTexture(string path)
    {
        using FileStream stream = File.OpenRead(path);
        ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        return CreateTexture((uint) result.Width, (uint) result.Height, PixelFormat.RGBA8, result.Data);
    }

    public abstract Renderable CreateRenderable(in RenderableInfo info);

    /// <summary>
    /// Clear the current render target with the given clear color.
    /// </summary>
    /// <param name="color">The color to clear with.</param>
    public abstract void Clear(Color color);
    
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
    /// <param name="backend">The <see cref="Sprout.Graphics.Backend"/> to use.</param>
    /// <returns>The created <see cref="GraphicsDevice"/>.</returns>
    public static GraphicsDevice Create(IntPtr sdlWindow, Backend backend)
    {
        return backend switch
        {
            Backend.Vulkan => new VkGraphicsDevice(sdlWindow),
            Backend.OpenGL => new GLGraphicsDevice(sdlWindow),
            Backend.D3D11 => new D3D11GraphicsDevice(sdlWindow),
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null)
        };
    }
}