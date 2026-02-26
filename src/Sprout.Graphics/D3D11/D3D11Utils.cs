using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.D3D11_TEXTURE_ADDRESS_MODE;

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

    public static D3D11_TEXTURE_ADDRESS_MODE ToD3D(this TextureAddress address)
    {
        return address switch
        {
            TextureAddress.Repeat => D3D11_TEXTURE_ADDRESS_WRAP,
            TextureAddress.RepeatMirrored => D3D11_TEXTURE_ADDRESS_MIRROR,
            TextureAddress.ClampToEdge => D3D11_TEXTURE_ADDRESS_CLAMP,
            TextureAddress.ClampToBorder => D3D11_TEXTURE_ADDRESS_BORDER,
            _ => throw new ArgumentOutOfRangeException(nameof(address), address, null)
        };
    }
}