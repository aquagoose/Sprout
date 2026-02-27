using System.Diagnostics.CodeAnalysis;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.D3D11_BLEND_OP;
using static TerraFX.Interop.DirectX.D3D11_BLEND;
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

    public static D3D11_BLEND ToD3D(this BlendFactor factor)
    {
        return factor switch
        {
            BlendFactor.Zero => D3D11_BLEND_ZERO,
            BlendFactor.One => D3D11_BLEND_ONE,
            BlendFactor.SrcColor => D3D11_BLEND_SRC_COLOR,
            BlendFactor.OneMinusSrcColor => D3D11_BLEND_INV_SRC_COLOR,
            BlendFactor.DestColor => D3D11_BLEND_DEST_COLOR,
            BlendFactor.OneMinusDestColor => D3D11_BLEND_INV_DEST_COLOR,
            BlendFactor.SrcAlpha => D3D11_BLEND_SRC_ALPHA,
            BlendFactor.OneMinusSrcAlpha => D3D11_BLEND_INV_SRC_ALPHA,
            BlendFactor.DestAlpha => D3D11_BLEND_DEST_ALPHA,
            BlendFactor.OneMinusDestAlpha => D3D11_BLEND_INV_DEST_ALPHA,
            _ => throw new ArgumentOutOfRangeException(nameof(factor), factor, null)
        };
    }

    public static D3D11_BLEND_OP ToD3D(this BlendOperation operation)
    {
        return operation switch
        {
            BlendOperation.Add => D3D11_BLEND_OP_ADD,
            BlendOperation.Subtract => D3D11_BLEND_OP_SUBTRACT,
            BlendOperation.ReverseSubtract => D3D11_BLEND_OP_REV_SUBTRACT,
            BlendOperation.Min => D3D11_BLEND_OP_MIN,
            BlendOperation.Max => D3D11_BLEND_OP_MAX,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}