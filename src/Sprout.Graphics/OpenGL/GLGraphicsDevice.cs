using SDL3;
using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed class GLGraphicsDevice : GraphicsDevice
{
    public override bool IsDisposed { get; protected set; }

    private readonly IntPtr _sdlWindow;
    private readonly nint _glContext;
    private readonly GL _gl;
    
    public override Backend Backend => Backend.OpenGL;

    public GLGraphicsDevice(IntPtr sdlWindow)
    {
        _sdlWindow = sdlWindow;

        _glContext = SDL.GLCreateContext(_sdlWindow);
        if (_glContext == 0)
            throw new Exception($"Failed to create GL context: {SDL.GetError()}");

        if (!SDL.GLMakeCurrent(_sdlWindow, _glContext))
            throw new Exception("Failed to make GL context current.");
        
        _gl = GL.GetApi(SDL.GLGetProcAddress);
    }

    public override Shader CreateShader(params ReadOnlySpan<ShaderAttachment> attachments)
    {
        return new GLShader(_gl, in attachments);
    }

    public override void Clear(float r, float g, float b, float a = 1)
    {
        _gl.ClearColor(r, g, b, a);
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