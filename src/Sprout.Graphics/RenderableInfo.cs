namespace Sprout.Graphics;

/// <summary>
/// Describes how a <see cref="Renderable"/> should be created.
/// </summary>
public struct RenderableInfo
{
    /// <summary>
    /// The initial number of vertices. This is multiplied by <see cref="VertexSize"/> to calculate the size in bytes
    /// of the vertex buffer. Set to 0 to disable the vertex buffer.
    /// </summary>
    public uint NumVertices;

    /// <summary>
    /// The size, in bytes, of a single vertex. This is multiplied by <see cref="NumVertices"/> to calculate the size in
    /// bytes of the vertex buffer.
    /// </summary>
    public uint VertexSize;
    
    /// <summary>
    /// The number of indices. Set to 0 to disable the index buffer.
    /// </summary>
    public uint NumIndices;

    /// <summary>
    /// The vertex shader input layout.
    /// </summary>
    public VertexAttribute[]? VertexInput;
    
    /// <summary>
    /// The <see cref="Sprout.Graphics.Shader"/> to use.
    /// </summary>
    public Shader Shader;
}