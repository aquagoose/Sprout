using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.SPIRV;
using Silk.NET.SPIRV.Cross;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.DirectX;
using static TerraFX.Interop.Windows.CLSID;
using static TerraFX.Interop.Windows.Windows;
using SpvCompiler = Silk.NET.SPIRV.Cross.Compiler;
using SpvResult = Silk.NET.SPIRV.Cross.Result;

namespace Sprout.Graphics.ShaderUtils;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public static class Compiler
{
    private static Cross _spirv;

    static Compiler()
    {
        _spirv = Cross.GetApi();
    }

    public static byte[] TranspileHLSL(Backend backend, ShaderStage stage, string hlsl, string entryPoint, string? includeDirectory = null)
    {
        byte[] spirv = HlslToSpirv(stage, hlsl, entryPoint, includeDirectory);

        return backend switch
        {
            Backend.OpenGL => SpirvToGLSL(stage, spirv, entryPoint),
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null)
        };
    }
    
    public static unsafe byte[] HlslToSpirv(ShaderStage stage, string hlsl, string entryPoint, string? includeDirectory)
    {
        Guid clsidDxcUtils = CLSID_DxcUtils;
        Guid clsidDxcCompiler = CLSID_DxcCompiler;
        
        IDxcUtils* utils = null;
        IDxcCompiler3* compiler = null;
        IDxcCompilerArgs* compilerArgs = null;
        IDxcIncludeHandler* includeHandler = null;
        IDxcResult* result = null;
        IDxcBlob* compiled = null;
        IDxcBlobEncoding* errorBlob = null;

        try
        {
            CheckResult(DxcCreateInstance(&clsidDxcUtils, __uuidof<IDxcUtils>(), (void**) &utils), "Create IDxcUtils");

            CheckResult(DxcCreateInstance(&clsidDxcCompiler, __uuidof<IDxcCompiler3>(), (void**) &compiler),
                "Create IDxcCompiler3");

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

            using DxcString pEntryPoint = entryPoint;
            using DxcString pProfile = profile;
            using DxcStringArray pArgs = new(args);

            CheckResult(utils->BuildArguments(null, pEntryPoint, pProfile, pArgs, pArgs.Length, null, 0, &compilerArgs),
                "Build arguments");

            CheckResult(utils->CreateDefaultIncludeHandler(&includeHandler), "Create default include handler");

            DxcBuffer buffer = new()
            {
                Ptr = (void*) Marshal.StringToHGlobalAnsi(hlsl),
                Size = (nuint) hlsl.Length
            };

            CheckResult(
                compiler->Compile(&buffer, compilerArgs->GetArguments(), compilerArgs->GetCount(), includeHandler,
                    __uuidof<IDxcResult>(), (void**) &result), "Compile");

            HRESULT status;
            CheckResult(result->GetStatus(&status), "Get status");

            if (status.FAILED)
            {
                CheckResult(result->GetErrorBuffer(&errorBlob), "Get error buffer");
                throw new Exception($"Failed to compile HLSL: {new string((sbyte*) errorBlob->GetBufferPointer())}");
            }

            CheckResult(result->GetResult(&compiled), "Get result");

            byte[] spirv = new byte[compiled->GetBufferSize()];
            fixed (byte* pSpirv = spirv)
                Unsafe.CopyBlock(pSpirv, compiled->GetBufferPointer(), (uint) spirv.Length);

            return spirv;
        }
        finally
        {
            // Is this a good solution? Who knows! Does it work? Yes!
            if (errorBlob != null)
                errorBlob->Release();

            if (compiled != null)
                compiled->Release();

            if (result != null)
                result->Release();
            
            if (includeHandler != null)
                includeHandler->Release();

            if (compilerArgs != null)
                compilerArgs->Release();
            
            if (compilerArgs != null)
                compiler->Release();
            
            if (utils != null)
                utils->Release();
        }
    }

    public static unsafe byte[] SpirvToGLSL(ShaderStage stage, byte[] spirv, string entryPoint)
    {
        Context* context;
        CheckResult(_spirv.ContextCreate(&context), "Create compiler");

        try
        {
            ParsedIr* ir;
            fixed (byte* pSpirv = spirv)
            {
                SpvResult result = _spirv.ContextParseSpirv(context, (uint*) pSpirv, (nuint) spirv.Length / 4, &ir);
                if (result != SpvResult.Success)
                    throw new Exception($"Failed to parse Spir-V: {_spirv.ContextGetLastErrorStringS(context)}");
            }

            SpvCompiler* compiler;
            CheckResult(
                _spirv.ContextCreateCompiler(context, Silk.NET.SPIRV.Cross.Backend.Glsl, ir, CaptureMode.TakeOwnership,
                    &compiler), "Create compiler");

            CompilerOptions* options;
            CheckResult(_spirv.CompilerCreateCompilerOptions(compiler, &options), "Create compiler options");
            CheckResult(_spirv.CompilerOptionsSetUint(options, CompilerOption.GlslVersion, 330),
                "Set compiler version");
            CheckResult(_spirv.CompilerInstallCompilerOptions(compiler, options), "Install compiler options");

            ExecutionModel model = stage switch
            {
                ShaderStage.Vertex => ExecutionModel.Vertex,
                ShaderStage.Pixel => ExecutionModel.Fragment,
                _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
            };
            CheckResult(_spirv.CompilerSetEntryPoint(compiler, entryPoint, model), "Set entry point");
            
            byte* pOutput;
            CheckResult(_spirv.CompilerCompile(compiler, &pOutput), "Compile");

            byte[] output = new byte[strlen(pOutput)];
            fixed (byte* pOut = output)
                Unsafe.CopyBlock(pOut, pOutput, (uint) output.Length);

            return output;
        }
        finally
        {
            _spirv.ContextDestroy(context);
        }
    }

    public static void CheckResult(HRESULT result, string operation)
    {
        if (result.FAILED)
            throw new Exception($"DXC operation '{operation}' failed with HRESULT: {result.Value:x8}");
    }

    private static void CheckResult(SpvResult result, string operation)
    {
        if (result != Result.Success)
            throw new Exception($"SPIRV-Cross operation '{operation}' failed: {result}");
    }

    private static unsafe uint strlen(byte* str)
    {
        uint size = 0;
        while (str[size++] != 0) { }
        return size;
    }
}