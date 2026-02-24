using Sprout.Graphics.Vulkan.VMA;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Sprout.Graphics.Vulkan;

public unsafe struct VkBuffer
{
    public readonly Buffer Buffer;
    public readonly Allocation* Allocation;
    public readonly void* MappedPtr;

    public VkBuffer(Buffer buffer, Allocation* allocation, void* mappedPtr)
    {
        Buffer = buffer;
        Allocation = allocation;
        MappedPtr = mappedPtr;
    }
}