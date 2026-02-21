using System.Diagnostics;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed unsafe class GLRenderable : Renderable
{
    public override bool IsDisposed { get; protected set; }

    private readonly GL _gl;
    private readonly GLGraphicsDevice _device;
    private readonly uint _vao;
    private readonly GLShader _shader;

    private readonly uint _vbo;
    private readonly uint _ebo;

    private readonly Dictionary<uint, GLUniform>? _uniforms;

    private readonly uint _numDraws;
    
    public GLRenderable(GL gl, GLGraphicsDevice device, ref readonly RenderableInfo info)
    {
        _gl = gl;
        _device = device;
        _shader = (GLShader) info.Shader;

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        BufferUsageARB usage = info.Dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StaticDraw;
        
        if (info.NumVertices > 0)
        {
            Debug.Assert(info.VertexSize > 0);
            uint sizeInBytes = info.NumVertices * info.VertexSize;
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, sizeInBytes, null, usage);

            _numDraws = info.NumVertices;
        }

        if (info.NumIndices > 0)
        {
            Debug.Assert(info.NumVertices > 0);
            uint sizeInBytes = info.NumIndices * sizeof(uint);
            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeInBytes, null, usage);

            _numDraws = info.NumIndices;
        }

        if (info.VertexInput != null)
        {
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

        if (info.Uniforms != null)
        {
            _gl.UseProgram(_shader.Program);
            
            _uniforms = new Dictionary<uint, GLUniform>(info.Uniforms.Length);
            int currentTextureUnit = 0;
            for (int i = 0; i < info.Uniforms.Length; i++)
            {
                ref readonly Uniform uniform = ref info.Uniforms[i];

                GLUniform glUniform;
                switch (uniform.Type)
                {
                    case UniformType.ConstantBuffer:
                    {
                        uint buffer = _gl.GenBuffer();
                        _gl.BindBuffer(BufferTargetARB.UniformBuffer, buffer);
                        _gl.BufferData(BufferTargetARB.UniformBuffer, uniform.ConstantBufferSize, null,
                            BufferUsageARB.DynamicDraw);

                        uint index = _gl.GetUniformBlockIndex(_shader.Program, $"sp_Uniform_{uniform.Index}");
                        _gl.UniformBlockBinding(_shader.Program, index, uniform.Index);

                        glUniform = new GLUniform(UniformType.ConstantBuffer, uniform.ConstantBufferSize, buffer, 0);
                        break;
                    }
                    
                    case UniformType.Texture:
                    {
                        int location = _gl.GetUniformLocation(_shader.Program, $"sp_Texture_{uniform.Index}");
                        if (location == -1)
                            throw new Exception("Internal error: Couldn't find texture uniform!");
                        
                        _gl.Uniform1(location, currentTextureUnit);
                        glUniform = new GLUniform(UniformType.Texture, 0, 0, currentTextureUnit);
                        currentTextureUnit++;
                        break;
                    }
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                _uniforms.Add(uniform.Index, glUniform);
            }
        }
    }

    public override void UpdateVertices<T>(uint offset, ReadOnlySpan<T> vertices)
    {
        _gl.BindVertexArray(_vao);
        fixed (T* pVertices = vertices)
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint) offset, (nuint) (vertices.Length * sizeof(T)),
                pVertices);
        }
    }
    public override void UpdateIndices(uint offset, ReadOnlySpan<uint> indices)
    {
        _gl.BindVertexArray(_vao);
        fixed (uint* pIndices = indices)
        {
            _gl.BufferSubData(BufferTargetARB.ElementArrayBuffer, (nint) offset,
                (nuint) (indices.Length * sizeof(uint)), pIndices);
        }
    }

    public override void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        if (!(_uniforms?.TryGetValue(index, out GLUniform uniform) ?? false))
            throw new Exception("Invalid uniform index!");
        
        Debug.Assert(uniform.Type == UniformType.ConstantBuffer, "Uniform index is not a Constant Buffer uniform");
        Debug.Assert(offset + sizeInBytes >= uniform.UniformBufferSize);
     
        _gl.BindVertexArray(_vao);
        _gl.BindBufferBase(BufferTargetARB.UniformBuffer, index, uniform.UniformBuffer);
        _gl.BufferSubData(BufferTargetARB.UniformBuffer, (nint) offset, sizeInBytes, pData);
    }

    public override void PushTexture(uint index, Texture texture)
    {
        if (!(_uniforms?.TryGetValue(index, out GLUniform uniform) ?? false))
            throw new Exception("Invalid uniform index!");
        
        Debug.Assert(uniform.Type == UniformType.Texture, "Texture index is not a Texture uniform!");
        Debug.Assert((texture.Usage & TextureUsage.Shader) != 0, "Texture was not created with the 'Shader' usage flag!");
     
        _gl.BindVertexArray(_vao);
        
        GLTexture glTexture = (GLTexture) texture;
        _gl.ActiveTexture(TextureUnit.Texture0 + uniform.TextureUnit);
        _gl.BindTexture(TextureTarget.Texture2D, glTexture.Texture);
    }

    public override void Draw()
        => Draw(_numDraws);

    public override void Draw(uint numElements)
    {
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_shader.Program);

        int vertexMultiplierLocation = _gl.GetUniformLocation(_shader.Program, "SP_VertexMultiplier");
        if (vertexMultiplierLocation < 0)
            throw new Exception("Couldn't find a uniform named 'SP_VertexMultiplier' in the vertex shader!");
        
        Vector4 vertexMultiplier = Vector4.One;
        if (_device.HasRenderTextureSet)
            vertexMultiplier.Y = -1;
        _gl.Uniform4(vertexMultiplierLocation, 1, &vertexMultiplier.X);
        
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

    private readonly struct GLUniform
    {
        public readonly UniformType Type;

        public readonly uint UniformBufferSize;
        
        public readonly uint UniformBuffer;

        public readonly int TextureUnit;

        public GLUniform(UniformType type, uint uniformBufferSize, uint uniformBuffer, int textureUnit)
        {
            Type = type;
            UniformBufferSize = uniformBufferSize;
            UniformBuffer = uniformBuffer;
            TextureUnit = textureUnit;
        }
    }
}