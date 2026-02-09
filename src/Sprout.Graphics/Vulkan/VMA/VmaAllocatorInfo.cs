using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan.VMA;

public partial struct VmaAllocatorInfo
{
    [NativeTypeName("VkInstance _Nonnull")]
    public Instance instance;

    [NativeTypeName("VkPhysicalDevice _Nonnull")]
    public PhysicalDevice physicalDevice;

    [NativeTypeName("VkDevice _Nonnull")]
    public Device device;
}
