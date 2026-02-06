namespace Sprout.Graphics;

public struct VertexAttribute
{
    public uint Location;

    public AttributeType Type;

    public uint Offset;

    public VertexAttribute(uint location, AttributeType type, uint offset)
    {
        Location = location;
        Type = type;
        Offset = offset;
    }
}