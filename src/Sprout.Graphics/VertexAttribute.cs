namespace Sprout.Graphics;

public struct VertexAttribute
{
    public uint Location;

    public Semantic Semantic;

    public uint SemanticIndex;
    
    public AttributeType Type;

    public uint Offset;

    public VertexAttribute(uint location, Semantic semantic, uint semanticIndex, AttributeType type, uint offset)
    {
        Location = location;
        Semantic = semantic;
        SemanticIndex = semanticIndex;
        Type = type;
        Offset = offset;
    }
}