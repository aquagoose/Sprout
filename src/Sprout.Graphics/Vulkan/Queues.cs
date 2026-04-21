using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan;

internal struct Queues
{
    public uint GraphicsIndex;
    public uint PresentIndex;

    public Queue Graphics;
    public Queue Present;

    public HashSet<uint> UniqueQueues => [GraphicsIndex, PresentIndex];
}