using System.Diagnostics;
using System.Text;
using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal sealed unsafe class GLShader : Shader
{
    public override bool IsDisposed { get; protected set; }

    private readonly GL _gl;

    public readonly uint Program;
    
    public readonly Dictionary<uint, GLUniform>? Uniforms;
    
    public unsafe GLShader(GL gl, ref readonly ShaderInfo info)
    {
        _gl = gl;

        Program = _gl.CreateProgram();

        uint? vertexShader = null;
        uint? fragmentShader = null;
        
        if (info.VertexShader is ShaderAttachment vertexAttachment)
        {
            uint shader = CreateShader(in vertexAttachment, ShaderType.VertexShader);
            _gl.AttachShader(Program, shader);
            vertexShader = shader;
        }

        if (info.PixelShader is ShaderAttachment pixelAttachment)
        {
            uint shader = CreateShader(in pixelAttachment, ShaderType.FragmentShader);
            _gl.AttachShader(Program, shader);
            fragmentShader = shader;
        }
        
        _gl.LinkProgram(Program);
        _gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != (int) GLEnum.True)
            throw new Exception($"Failed to link program: {_gl.GetProgramInfoLog(Program)}");

        if (fragmentShader is uint fShader)
        {
            _gl.DetachShader(Program, fShader);
            _gl.DeleteShader(fShader);
        }

        if (vertexShader is uint vShader)
        {
            _gl.DetachShader(Program, vShader);
            _gl.DeleteShader(vShader);
        }
        
        if (info.Uniforms != null)
        {
            Uniforms = new Dictionary<uint, GLUniform>(info.Uniforms.Length);
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

                        uint index = _gl.GetUniformBlockIndex(Program, $"sp_Uniform_{uniform.Index}");
                        _gl.UniformBlockBinding(Program, index, uniform.Index);

                        glUniform = new GLUniform(UniformType.ConstantBuffer, uniform.ConstantBufferSize, buffer, 0);
                        break;
                    }
                    
                    case UniformType.Texture:
                    {
                        int location = _gl.GetUniformLocation(Program, $"sp_Texture_{uniform.Index}");
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
                
                Uniforms.Add(uniform.Index, glUniform);
            }
        }
    }
    
    public override void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        
        _gl.DeleteProgram(Program);
    }
    
    public override void PushUniformData(uint index, uint offset, uint sizeInBytes, void* pData)
    {
        if (!(Uniforms?.TryGetValue(index, out GLUniform uniform) ?? false))
            throw new Exception("Invalid uniform index!");
        
        Debug.Assert(uniform.Type == UniformType.ConstantBuffer, "Uniform index is not a Constant Buffer uniform");
        Debug.Assert(offset + sizeInBytes >= uniform.UniformBufferSize);
     
        // TODO: Defer updating the uniform? (in the same way the texture is not bound until Renderable.Draw)
        //   Saves an extra bind when calling this method.
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.UniformBuffer, uniform.UniformBuffer);
        _gl.BufferSubData(BufferTargetARB.UniformBuffer, (nint) offset, sizeInBytes, pData);
    }

    public override void PushTexture(uint index, Texture texture)
    {
        if (!(Uniforms?.TryGetValue(index, out GLUniform uniform) ?? false))
            throw new Exception("Invalid uniform index!");
        
        Debug.Assert(uniform.Type == UniformType.Texture, "Texture index is not a Texture uniform!");
        Debug.Assert((texture.Usage & TextureUsage.Shader) != 0, "Texture was not created with the 'Shader' usage flag!");
        
        GLTexture glTexture = (GLTexture) texture;
        uniform.CurrentTexture = glTexture.Texture;
    }

    private uint CreateShader(in ShaderAttachment attachment, ShaderType type)
    {
        uint shader = _gl.CreateShader(type);
            
        int sourceLength = attachment.Source.Length;
        fixed (byte* pSource = attachment.Source)
            _gl.ShaderSource(shader, 1, &pSource, &sourceLength);
            
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
        if (compileStatus != (int) GLEnum.True)
            throw new Exception($"Failed to compile shader: {_gl.GetShaderInfoLog(shader)}");

        return shader;
    }
    
    // TODO: Make this a classn't and use an array + dictionary of array indices, to be able to get ref values.
    public class GLUniform
    {
        public readonly UniformType Type;

        public readonly uint UniformBufferSize;
        
        public readonly uint UniformBuffer;

        public readonly int TextureUnit;
        
        public uint CurrentTexture;
        
        public GLUniform(UniformType type, uint uniformBufferSize, uint uniformBuffer, int textureUnit)
        {
            Type = type;
            UniformBufferSize = uniformBufferSize;
            UniformBuffer = uniformBuffer;
            TextureUnit = textureUnit;
        }
    }
}