using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan.VMA;

public partial struct VmaDetailedStatistics
{
    public VmaStatistics statistics;

    [NativeTypeName("uint32_t")]
    public uint unusedRangeCount;

    [NativeTypeName("VkDeviceSize")]
    public nuint allocationSizeMin;

    [NativeTypeName("VkDeviceSize")]
    public nuint allocationSizeMax;

    [NativeTypeName("VkDeviceSize")]
    public nuint unusedRangeSizeMin;

    [NativeTypeName("VkDeviceSize")]
    public nuint unusedRangeSizeMax;
}
