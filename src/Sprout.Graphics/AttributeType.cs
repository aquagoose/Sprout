namespace Sprout.Graphics;

/// <summary>
/// Represents various supported types that a shader <see cref="Attribute"/> supports.
/// </summary>
public enum AttributeType
{
    /// <summary>
    /// A single float.
    /// </summary>
    Float,
    
    /// <summary>
    /// A 2-component float2/vec2
    /// </summary>
    Float2,
    
    /// <summary>
    /// A 3-component float3/vec3
    /// </summary>
    Float3,
    
    /// <summary>
    /// A 4-component float4/vec4
    /// </summary>
    Float4
}