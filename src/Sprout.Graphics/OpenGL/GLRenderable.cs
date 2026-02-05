using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed class GLRenderable : Renderable
{
    public override bool IsDisposed { get; protected set; }

    private readonly GL _gl;
    private readonly uint _vao;
    private readonly GLShader _shader;
    
    public GLRenderable(GL gl, ref readonly RenderableInfo info)
    {
        _gl = gl;
        _shader = (GLShader) info.Shader;

        _vao = _gl.GenVertexArray();
    }

    public override void Draw(uint numElements)
    {
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_shader.Program);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, numElements);
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _gl.DeleteVertexArray(_vao);
    }
}