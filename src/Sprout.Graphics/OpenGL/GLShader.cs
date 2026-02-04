using Silk.NET.OpenGL;
using Sprout.Graphics.Utils;

namespace Sprout.Graphics.OpenGL;

internal sealed class GLShader : Shader
{
    public override bool IsDisposed { get; protected set; }

    private readonly GL _gl;

    public readonly uint Program;
    
    public GLShader(GL gl, in ReadOnlySpan<ShaderAttachment> attachments)
    {
        _gl = gl;

        Program = _gl.CreateProgram();

        Span<uint> shaders = stackalloc uint[attachments.Length];
        for (int i = 0; i < attachments.Length; i++)
        {
            ref readonly ShaderAttachment attachment = ref attachments[i];
            string glsl = ShaderUtils.TranspileHLSL(Backend.OpenGL, attachment.Stage, attachment.Source,
                attachment.EntryPoint);

            ShaderType type = attachment.Stage switch
            {
                ShaderStage.Vertex => ShaderType.VertexShader,
                ShaderStage.Pixel => ShaderType.FragmentShader,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            uint shader = _gl.CreateShader(type);
            shaders[i] = shader;
            _gl.ShaderSource(shader, glsl);
            _gl.CompileShader(shader);

            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
            if (compileStatus != (int) GLEnum.True)
                throw new Exception($"Failed to compile shader: {_gl.GetShaderInfoLog(shader)}");
            
            _gl.AttachShader(Program, shader);
        }
        
        _gl.LinkProgram(Program);
        _gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus != (int) GLEnum.True)
            throw new Exception($"Failed to link program: {_gl.GetProgramInfoLog(Program)}");

        foreach (uint shader in shaders)
        {
            _gl.DetachShader(Program, shader);
            _gl.DeleteShader(shader);
        }
    }
    
    public override void Dispose()
    {
        _gl.DeleteProgram(Program);
    }
}