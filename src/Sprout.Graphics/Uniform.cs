namespace Sprout.Graphics;

public struct Uniform
{
    public uint Index;

    public UniformType Type;

    public uint ConstantBufferSize;

    public Uniform(uint index, UniformType type, uint constantBufferSize = 0)
    {
        Index = index;
        Type = type;
        ConstantBufferSize = constantBufferSize;
    }

    public static Uniform ConstantBuffer(uint index, uint sizeInBytes)
        => new Uniform(index, UniformType.ConstantBuffer, sizeInBytes);

    public static unsafe Uniform ConstantBuffer<T>(uint index) where T : unmanaged
        => new Uniform(index, UniformType.ConstantBuffer, (uint) sizeof(T));

    public static unsafe Uniform ConstantBuffer<T>(uint index, uint arrayLength) where T : unmanaged
        => new Uniform(index, UniformType.ConstantBuffer, (uint) (sizeof(T) * arrayLength));

    public static Uniform Texture(uint index)
        => new Uniform(index, UniformType.Texture, 0);
}