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
}