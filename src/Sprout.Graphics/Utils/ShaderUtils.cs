using SharpGen.Runtime;
using Vortice.Dxc;

namespace Sprout.Graphics.Utils;

internal static class ShaderUtils
{
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
}