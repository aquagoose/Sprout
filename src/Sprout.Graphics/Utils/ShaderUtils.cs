using Silk.NET.SPIRV.Cross;
using Vortice.Dxc;
using Result = SharpGen.Runtime.Result;
using SpvResult = Silk.NET.SPIRV.Cross.Result;

namespace Sprout.Graphics.Utils;

internal static class ShaderUtils
{
    private static Cross _spirv;

    static ShaderUtils()
    {
        _spirv = Cross.GetApi();
    }

    public static string TranspileHLSL(Backend backend, ShaderStage stage, string hlsl, string entryPoint, string? includeDirectory = null)
    {
        byte[] spirv = HlslToSpirv(stage, hlsl, entryPoint, includeDirectory);

        return backend switch
        {
            Backend.OpenGL => SpirvToGLSL(stage, spirv, entryPoint),
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null)
        };
    }
    
    public static byte[] HlslToSpirv(ShaderStage stage, string hlsl, string entryPoint, string? includeDirectory)
    {
        IDxcUtils utils = Dxc.CreateDxcUtils();
        IDxcCompiler3 compiler = Dxc.CreateDxcCompiler<IDxcCompiler3>();

        string profile = stage switch
        {
            ShaderStage.Vertex => "vs_6_0",
            ShaderStage.Pixel => "ps_6_0",
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
        };

        List<string> args = ["-spirv"];
        if (includeDirectory != null)
        {
            args.Add("-I");
            args.Add(includeDirectory);
        }

        IDxcCompilerArgs compilerArgs = utils.BuildArguments(null, entryPoint, profile, args.ToArray(), null);
        IDxcIncludeHandler includeHandler = utils.CreateDefaultIncludeHandler();

        IDxcResult result = compiler.Compile(hlsl, compilerArgs.Arguments, includeHandler);
        Result status = result.GetStatus();

        if (status.Failure)
            throw new Exception($"Compilation failure: {result.GetErrors()}");

        return result.GetObjectBytecodeArray();
    }

    public static unsafe string SpirvToGLSL(ShaderStage stage, byte[] spirv, string entryPoint)
    {
        Context* context;
        CheckResult(_spirv.ContextCreate(&context), "Create compiler");

        try
        {
            ParsedIr* ir;
            fixed (byte* pSpirv = spirv)
            {
                SpvResult result = _spirv.ContextParseSpirv(context, (uint*) pSpirv, (nuint) spirv.Length, &ir);
                if (result != SpvResult.Success)
                    throw new Exception($"Failed to parse Spir-V: {_spirv.ContextGetLastErrorStringS(context)}");
            }

            Compiler* compiler;
            CheckResult(
                _spirv.ContextCreateCompiler(context, Silk.NET.SPIRV.Cross.Backend.Glsl, ir, CaptureMode.TakeOwnership,
                    &compiler), "Create compiler");

            CompilerOptions* options;
            CheckResult(_spirv.CompilerCreateCompilerOptions(compiler, &options), "Create compiler options");
            CheckResult(_spirv.CompilerOptionsSetUint(options, CompilerOption.GlslVersion, 330),
                "Set compiler version");
            
            CheckResult(_spirv.CompilerInstallCompilerOptions(compiler, options), "Install compiler options");

            byte* output;
            CheckResult(_spirv.CompilerCompile(compiler, &output), "Compile");

            return new string((sbyte*) output);
        }
        finally
        {
            _spirv.ContextDestroy(context);
        }
    }

    private static void CheckResult(SpvResult result, string operation)
    {
        if (result != Silk.NET.SPIRV.Cross.Result.Success)
            throw new Exception($"SPIRV-Cross operation '{operation}' failed: {result}");
    }
}