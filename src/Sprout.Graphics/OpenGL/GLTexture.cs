using System.Diagnostics;
using System.Drawing;
using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed unsafe class GLTexture : Texture
{
    public override bool IsDisposed { get; protected set; }

    private readonly GL _gl;
    
    public readonly uint Texture;
    public readonly bool IsRenderbuffer;

    public override Sampler Sampler
    {
        get => field;
        set
        {
            field = value;

            TextureMinFilter minFilter;
            if ((Usage & TextureUsage.GenerateMipmaps) != 0)
            {
                minFilter = (value.MinFilter, value.MipFilter) switch
                {
                    (Filter.Linear, Filter.Linear) => TextureMinFilter.LinearMipmapLinear,
                    (Filter.Linear, Filter.Point) => TextureMinFilter.LinearMipmapNearest,
                    (Filter.Point, Filter.Linear) => TextureMinFilter.NearestMipmapLinear,
                    (Filter.Point, Filter.Point) => TextureMinFilter.NearestMipmapNearest,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
            {
                minFilter = value.MinFilter switch
                {
                    Filter.Linear => TextureMinFilter.Linear,
                    Filter.Point => TextureMinFilter.Nearest,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            TextureMagFilter magFilter = value.MagFilter switch
            {
                Filter.Linear => TextureMagFilter.Linear,
                Filter.Point => TextureMagFilter.Nearest,
                _ => throw new ArgumentOutOfRangeException()
            };

            TextureWrapMode wrapS = value.AddressU.ToGL();
            TextureWrapMode wrapT = value.AddressV.ToGL();
            
            _gl.BindTexture(TextureTarget.Texture2D, Texture);
            
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) minFilter);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) magFilter);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) wrapS);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) wrapT);
        }
    }

    public GLTexture(GL gl, uint width, uint height, PixelFormat format, TextureUsage usage, void* pData)
        : base(new Size((int) width, (int) height), format, usage)
    {
        Debug.Assert((usage & TextureUsage.Shader) != 0 || (usage & TextureUsage.RenderTexture) != 0,
            "Texture must be sampled in a shader or used as a render texture.");
        
        _gl = gl;

        (Silk.NET.OpenGL.PixelFormat fmt, InternalFormat iFmt, PixelType type) = format switch
        {
            PixelFormat.RGBA8 => (Silk.NET.OpenGL.PixelFormat.Rgba, InternalFormat.Rgba8, PixelType.UnsignedByte),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        // If the texture is only used as a render texture, and isn't used in the shader, we can instead create a
        // RenderBuffer, which is slightly more efficient.
        if ((Usage & TextureUsage.RenderTexture) != 0 && (Usage & TextureUsage.Shader) == 0)
        {
            Debug.Assert((Usage & TextureUsage.GenerateMipmaps) == 0, "Cannot generate mipmaps for render texture that is not sampled!");
            
            IsRenderbuffer = true;
            Texture = _gl.GenRenderbuffer();
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Texture);
            _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, iFmt, width, height);
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }
        else
        {
            Texture = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, Texture);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, iFmt, width, height, 0, fmt, type, pData);
            
            if ((Usage & TextureUsage.GenerateMipmaps) != 0 && (Usage & TextureUsage.RenderTexture) == 0)
                _gl.GenerateMipmap(TextureTarget.Texture2D);
            
            Sampler = GraphicsDevice.DefaultSampler;
            _gl.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        if (IsRenderbuffer)
            _gl.DeleteRenderbuffer(Texture);
        else
            _gl.DeleteTexture(Texture);
    }
}