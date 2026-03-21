using System.Drawing;
using Sprout.Content;
using Sprout.Graphics.D3D11;
using Sprout.Graphics.OpenGL;
using Sprout.Graphics.SDLGPU;
using Sprout.Graphics.Vulkan;
using StbImageSharp;

namespace Sprout.Graphics;

/// <summary>
/// Represents a rendering device that graphics commands can be issued to.
/// </summary>
public abstract class GraphicsDevice : IDisposable
{
    internal const TextureUsage DefaultTextureUsage = TextureUsage.Shader | TextureUsage.GenerateMipmaps;

    internal static readonly Sampler DefaultSampler = Sampler.LinearWrap;
    
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
    
    /// <summary>
    /// Gets and sets the viewport.
    /// </summary>
    public abstract Viewport Viewport { get; set; }
    
    /// <summary>
    /// Gets and sets the current <see cref="Graphics.BlendMode"/>.
    /// </summary>
    public abstract BlendMode BlendMode { get; set; }

    /// <summary>
    /// Create a graphics <see cref="Shader"/>.
    /// </summary>
    /// <param name="info">The <see cref="ShaderInfo"/> to use when creating the shader.</param>
    /// <returns>The created <see cref="Shader"/>.</returns>
    public abstract Shader CreateShader(in ShaderInfo info);

    /// <summary>
    /// Create a <see cref="Texture"/>, with data from an unmanaged pointer.
    /// </summary>
    /// <param name="width">The width, in pixels.</param>
    /// <param name="height">The height, in pixels.</param>
    /// <param name="format">The texture's <see cref="PixelFormat"/>.</param>
    /// <param name="usage">The <see cref="TextureUsage"/> describing how the texture will be used on the GPU.</param>
    /// <param name="data">A pointer to some data. The size of this data MUST equal
    /// <b>width * height * format.BytesPerPixel</b>. Pass <see langword="null"/> to create an empty texture.</param>
    /// <returns>The created <see cref="Texture"/>.</returns>
    protected abstract unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, TextureUsage usage, void* data);

    /// <summary>
    /// Create an empty <see cref="Texture"/>.
    /// </summary>
    /// <param name="width">The width, in pixels.</param>
    /// <param name="height">The height, in pixels.</param>
    /// <param name="format">The texture's <see cref="PixelFormat"/>.</param>
    /// <param name="usage">The <see cref="TextureUsage"/> describing how the texture will be used on the GPU.</param>
    /// <returns>The created <see cref="Texture"/>.</returns>
    public unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, TextureUsage usage = DefaultTextureUsage)
        => CreateTexture(width, height, format, usage, null);

    /// <summary>
    /// Create a <see cref="Texture"/>.
    /// </summary>
    /// <param name="width">The width, in pixels.</param>
    /// <param name="height">The height, in pixels.</param>
    /// <param name="format">The texture's <see cref="PixelFormat"/>.</param>
    /// <param name="data">The texture's initial data.</param>
    /// <param name="usage">The <see cref="TextureUsage"/> describing how the texture will be used on the GPU.</param>
    /// <typeparam name="T">An unmanaged type that will be used as the texture's data type as described in the
    /// <paramref name="format"/>.</typeparam>
    /// <returns>The created <see cref="Texture"/>.</returns>
    public unsafe Texture CreateTexture<T>(uint width, uint height, PixelFormat format, ReadOnlySpan<T> data, TextureUsage usage = DefaultTextureUsage)
        where T : unmanaged
    {
        fixed (void* pData = data)
            return CreateTexture(width, height, format, usage, pData);
    }

    /// <summary>
    /// Create a <see cref="Texture"/>.
    /// </summary>
    /// <param name="width">The width, in pixels.</param>
    /// <param name="height">The height, in pixels.</param>
    /// <param name="format">The texture's <see cref="PixelFormat"/>.</param>
    /// <param name="data">The texture's initial data.</param>
    /// <param name="usage">The <see cref="TextureUsage"/> describing how the texture will be used on the GPU.</param>
    /// <typeparam name="T">An unmanaged type that will be used as the texture's data type as described in the
    /// <paramref name="format"/>.</typeparam>
    /// <returns>The created <see cref="Texture"/>.</returns>
    public Texture CreateTexture<T>(uint width, uint height, PixelFormat format, T[] data, TextureUsage usage = DefaultTextureUsage) where T : unmanaged
        => CreateTexture(width, height, format, data.AsSpan(), usage);

    /// <summary>
    /// Create a <see cref="Texture"/> from an image file.
    /// </summary>
    /// <param name="path">The path to the image file.</param>
    /// <param name="usage">The <see cref="TextureUsage"/> describing how the texture will be used on the GPU.</param>
    /// <returns>The created <see cref="Texture"/>.</returns>
    public Texture CreateTexture(string path, TextureUsage usage = DefaultTextureUsage)
    {
        using FileStream stream = File.OpenRead(PathUtils.GetFullPath(path));
        ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        return CreateTexture((uint) result.Width, (uint) result.Height, PixelFormat.RGBA8, result.Data, usage);
    }

    /// <summary>
    /// Create a <see cref="Renderable"/>.
    /// </summary>
    /// <param name="info">The <see cref="RenderableInfo"/> that describes the renderable to create.</param>
    /// <returns>The created <see cref="Renderable"/>.</returns>
    public abstract Renderable CreateRenderable(in RenderableInfo info);

    /// <summary>
    /// Set the render <see cref="Texture"/>s that will be used in subsequent draw calls. Pass an empty array (or
    /// better, pass <see langword="null"/> into <see cref="SetRenderTexture"/>) to use the default render texture.
    /// </summary>
    /// <param name="colorTextures">The color textures.</param>
    public abstract void SetRenderTextures(ReadOnlySpan<Texture> colorTextures);

    /// <summary>
    /// Set the render <see cref="Texture"/> that will be used in subsequent draw calls. Set to <see langword="null"/>
    /// to use the default render texture.
    /// </summary>
    public void SetRenderTexture(Texture? renderTexture)
    {
        if (renderTexture == null)
            SetRenderTextures([]);
        else
            SetRenderTextures([renderTexture]);
    }

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
    /// Resize the swapchain.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    public abstract void ResizeSwapchain(uint width, uint height);

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
            Backend.SDL => new SDLGraphicsDevice(sdlWindow),
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null)
        };
    }
}