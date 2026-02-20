namespace Sprout.Graphics;

/// <summary>
/// Describes how <see cref="Texture"/>s are used.
/// </summary>
[Flags]
public enum TextureUsage
{
    /// <summary>
    /// This texture is not used.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// This texture is sampled in a shader.
    /// </summary>
    Shader = 1 << 0,
    
    /// <summary>
    /// This texture is a render texture/render target.
    /// </summary>
    RenderTexture = 1 << 1,
    
    /// <summary>
    /// This texture will have mipmaps generated.
    /// </summary>
    GenerateMipmaps = 1 << 16
}