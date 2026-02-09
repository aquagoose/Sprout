using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan.VMA;

public partial struct VmaBudget
{
    public VmaStatistics statistics;

    [NativeTypeName("VkDeviceSize")]
    public nuint usage;

    [NativeTypeName("VkDeviceSize")]
    public nuint budget;
}
