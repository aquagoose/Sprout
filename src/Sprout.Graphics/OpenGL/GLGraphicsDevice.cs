using System.Drawing;
using SDL3;
using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed class GLGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private readonly IntPtr _sdlWindow;
    private readonly nint _glContext;
    private readonly GL _gl;

    private readonly Dictionary<int, uint> _framebuffers;

    private Size _swapchainSize;
    
    public bool HasRenderTextureSet;
    
    public override Backend Backend => Backend.OpenGL;

    public override Size SwapchainSize => _swapchainSize;

    public override Viewport Viewport
    {
        get => field;
        set
        {
            field = value;
            
            // OpenGL framebuffers are upside down for SOME reason
            // Anyway this makes it a complete pain in the arse to work with them.
            // Viewports in OpenGL start at the bottom left, whereas we want them to be the top left, so we must "flip"
            // the viewport to be at the top left.
            // HOWEVER if we're rendering to a framebuffer then we don't want to do this!
            // Hence why we only flip the viewport if the render texture is NOT set.
            int yOffset = HasRenderTextureSet ? 0 : (int) (_swapchainSize.Height - value.Height);
            _gl.Viewport(value.X, yOffset + value.Y, value.Width, value.Height);
        }
    }

    public GLGraphicsDevice(IntPtr sdlWindow)
    {
        _sdlWindow = sdlWindow;
        _framebuffers = [];

        _glContext = SDL.GLCreateContext(_sdlWindow);
        if (_glContext == 0)
            throw new Exception($"Failed to create GL context: {SDL.GetError()}");

        if (!SDL.GLMakeCurrent(_sdlWindow, _glContext))
            throw new Exception("Failed to make GL context current.");
        
        _gl = GL.GetApi(SDL.GLGetProcAddress);

        SDL.GetWindowSizeInPixels(_sdlWindow, out int w, out int h);
        _swapchainSize = new Size(w, h);
    }

    public override Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments)
    {
        return new GLShader(_gl, in attachments);
    }

    protected override unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, TextureUsage usage,
        void* data)
    {
        return new GLTexture(_gl, width, height, format, usage, data);
    }

    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        return new GLRenderable(_gl, this, in info);
    }

    public override void SetRenderTextures(ReadOnlySpan<Texture> colorTextures)
    {
        const FramebufferTarget fbTarget = FramebufferTarget.DrawFramebuffer;
        
        if (colorTextures.Length == 0)
        {
            _gl.BindFramebuffer(fbTarget, 0);
            HasRenderTextureSet = false;
            Viewport = new Viewport(0, 0, (uint) _swapchainSize.Width, (uint) _swapchainSize.Height);
            return;
        }
        
        HashCode hashCode = new HashCode();
        foreach (Texture texture in colorTextures)
            hashCode.Add(texture);

        if (!_framebuffers.TryGetValue(hashCode.ToHashCode(), out uint framebuffer))
        {
            Console.WriteLine("Creating new framebuffer!");
            framebuffer = _gl.GenFramebuffer();
            _gl.BindFramebuffer(fbTarget, framebuffer);

            int attachmentIndex = 0;
            foreach (Texture texture in colorTextures)
            {
                GLTexture glTexture = (GLTexture) texture;
                FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0 + attachmentIndex++;
                
                if (glTexture.IsRenderbuffer)
                {
                    _gl.FramebufferRenderbuffer(fbTarget, attachment, RenderbufferTarget.Renderbuffer,
                        glTexture.Texture);
                }
                else
                    _gl.FramebufferTexture2D(fbTarget, attachment, TextureTarget.Texture2D, glTexture.Texture, 0);
            }

            FramebufferStatus status = (FramebufferStatus) _gl.CheckFramebufferStatus(fbTarget);
            if (status != FramebufferStatus.Complete)
                throw new Exception($"Framebuffer is not complete: {status}");
            
            _framebuffers.Add(hashCode.ToHashCode(), framebuffer);
        }
        
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, framebuffer);
        HasRenderTextureSet = true;
        Viewport = new Viewport(0, 0, (uint) colorTextures[0].Size.Width, (uint) colorTextures[0].Size.Height);
    }

    public override void Clear(Color color)
    {
        _gl.ClearColor(color.R, color.G, color.B, color.A);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public override void Present()
    {
        SDL.GLSetSwapInterval(1);
        SDL.GLSwapWindow(_sdlWindow);
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _gl.Dispose();
        SDL.GLDestroyContext(_glContext);
    }
}