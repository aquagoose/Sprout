using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan.VMA;

public partial struct VmaAllocationInfo2
{
    public VmaAllocationInfo allocationInfo;

    [NativeTypeName("VkDeviceSize")]
    public nuint blockSize;

    [NativeTypeName("VkBool32")]
    public uint dedicatedMemory;
}
