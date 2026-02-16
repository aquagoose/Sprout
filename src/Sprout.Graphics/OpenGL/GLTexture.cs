using System.Drawing;
using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed unsafe class GLTexture : Texture
{
    public override bool IsDisposed { get; protected set; }

    private readonly GL _gl;
    
    public readonly uint Texture;

    public GLTexture(GL gl, uint width, uint height, PixelFormat format, void* pData) : base(new Size((int) width, (int) height))
    {
        _gl = gl;

        (Silk.NET.OpenGL.PixelFormat fmt, InternalFormat iFmt, PixelType type) = format switch
        {
            PixelFormat.RGBA8 => (Silk.NET.OpenGL.PixelFormat.Rgba, InternalFormat.Rgba8, PixelType.UnsignedByte),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        Texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, Texture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, iFmt, width, height, 0, fmt, type, pData);
        
        _gl.GenerateMipmap(TextureTarget.Texture2D);
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
    }
}