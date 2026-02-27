using Silk.NET.OpenGL;

namespace Sprout.Graphics.OpenGL;

internal static class GLUtils
{
    public static TextureWrapMode ToGL(this TextureAddress address)
    {
        return address switch
        {
            TextureAddress.Repeat => TextureWrapMode.Repeat,
            TextureAddress.RepeatMirrored => TextureWrapMode.MirroredRepeat,
            TextureAddress.ClampToEdge => TextureWrapMode.ClampToEdge,
            TextureAddress.ClampToBorder => TextureWrapMode.ClampToBorder,
            _ => throw new ArgumentOutOfRangeException(nameof(address), address, null)
        };
    }

    public static BlendingFactor ToGL(this BlendFactor factor)
    {
        return factor switch
        {
            BlendFactor.Zero => BlendingFactor.Zero,
            BlendFactor.One => BlendingFactor.One,
            BlendFactor.SrcColor => BlendingFactor.SrcColor,
            BlendFactor.OneMinusSrcColor => BlendingFactor.OneMinusSrcColor,
            BlendFactor.DestColor => BlendingFactor.DstColor,
            BlendFactor.OneMinusDestColor => BlendingFactor.OneMinusDstColor,
            BlendFactor.SrcAlpha => BlendingFactor.SrcAlpha,
            BlendFactor.OneMinusSrcAlpha => BlendingFactor.OneMinusSrcAlpha,
            BlendFactor.DestAlpha => BlendingFactor.DstAlpha,
            BlendFactor.OneMinusDestAlpha => BlendingFactor.OneMinusDstAlpha,
            _ => throw new ArgumentOutOfRangeException(nameof(factor), factor, null)
        };
    }

    public static BlendEquationModeEXT ToGL(this BlendOperation operation)
    {
        return operation switch
        {
            BlendOperation.Add => BlendEquationModeEXT.FuncAdd,
            BlendOperation.Subtract => BlendEquationModeEXT.FuncSubtract,
            BlendOperation.ReverseSubtract => BlendEquationModeEXT.FuncReverseSubtract,
            BlendOperation.Min => BlendEquationModeEXT.Min,
            BlendOperation.Max => BlendEquationModeEXT.Max,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}