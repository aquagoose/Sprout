using System.Diagnostics;
using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed unsafe class GLRenderable : Renderable
{
    public override bool IsDisposed { get; protected set; }

    private readonly GL _gl;
    private readonly uint _vao;
    private readonly GLShader _shader;

    private readonly uint _vbo;
    private readonly uint _ebo;
    
    public GLRenderable(GL gl, ref readonly RenderableInfo info)
    {
        _gl = gl;
        _shader = (GLShader) info.Shader;

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);
        
        if (info.NumVertices > 0)
        {
            Debug.Assert(info.VertexSize > 0);
            uint sizeInBytes = info.NumVertices * info.VertexSize;
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, sizeInBytes, null, BufferUsageARB.StaticDraw);
        }

        if (info.NumIndices > 0)
        {
            Debug.Assert(info.NumVertices > 0);
            uint sizeInBytes = info.NumIndices * sizeof(uint);
            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeInBytes, null, BufferUsageARB.StaticDraw);
        }
        
        _gl.UseProgram(_shader.Program);
        
        for (int i = 0; i < info.VertexInput.Length; i++)
        {
            ref readonly VertexAttribute attribute = ref info.VertexInput[i];
            uint stride = info.VertexSize;
            void* offset = (void*) attribute.Offset;
            uint location = attribute.Location;
            
            _gl.EnableVertexAttribArray(location);
            
            switch (attribute.Type)
            {
                case AttributeType.Float:
                    _gl.VertexAttribPointer(location, 1, VertexAttribPointerType.Float, false, stride, offset);
                    break;
                case AttributeType.Float2:
                    _gl.VertexAttribPointer(location, 2, VertexAttribPointerType.Float, false, stride, offset);
                    break;
                case AttributeType.Float3:
                    _gl.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, stride, offset);
                    break;
                case AttributeType.Float4:
                    _gl.VertexAttribPointer(location, 4, VertexAttribPointerType.Float, false, stride, offset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public override void UpdateVertices<T>(ReadOnlySpan<T> vertices)
    {
        _gl.BindVertexArray(_vao);
        fixed (T* pVertices = vertices)
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint) (vertices.Length * sizeof(T)), pVertices);
    }
    public override void UpdateIndices(ReadOnlySpan<uint> indices)
    {
        _gl.BindVertexArray(_vao);
        fixed (uint* pIndices = indices)
            _gl.BufferSubData(BufferTargetARB.ElementArrayBuffer, 0, (nuint) (indices.Length * sizeof(uint)), pIndices);
    }

    public override void Draw(uint numElements)
    {
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_shader.Program);
        
        if (_ebo == 0)
            _gl.DrawArrays(PrimitiveType.Triangles, 0, numElements);
        else
            _gl.DrawElements(PrimitiveType.Triangles, numElements, DrawElementsType.UnsignedInt, (void*) 0);
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
    }
}