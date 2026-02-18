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

    private Size _swapchainSize;
    
    public override Backend Backend => Backend.OpenGL;

    public override Size SwapchainSize => _swapchainSize;

    public GLGraphicsDevice(IntPtr sdlWindow)
    {
        _sdlWindow = sdlWindow;

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

    protected override unsafe Texture CreateTexture(uint width, uint height, PixelFormat format, void* data)
    {
        return new GLTexture(_gl, width, height, format, data);
    }

    public override Renderable CreateRenderable(in RenderableInfo info)
    {
        return new GLRenderable(_gl, in info);
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