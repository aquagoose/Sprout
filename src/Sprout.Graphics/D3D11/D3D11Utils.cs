using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.Windows;

namespace Sprout.Graphics.D3D11;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal static class D3D11Utils
{
    public static void Check(this HRESULT result, string operation)
    {
        if (result.FAILED)
            throw new Exception($"D3D11 operation '{operation}' failed with HRESULT: 0x{result.Value:x8}");
    }

    public static string SemanticToString(Semantic semantic)
    {
        return semantic switch
        {
            Semantic.TexCoord => "TEXCOORD",
            Semantic.Position => "POSITION",
            Semantic.Color => "COLOR",
            Semantic.Tangent => "TANGENT",
            Semantic.BiTangent => "BITANGENT",
            _ => throw new ArgumentOutOfRangeException(nameof(semantic), semantic, null)
        };
    }
}