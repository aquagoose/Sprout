using Silk.NET.Vulkan;
using Sprout.Graphics.Vulkan.VMA;

namespace Sprout.Graphics.Vulkan;

internal readonly unsafe struct VkImage
{
    public readonly Image Image;

    public readonly Allocation* Allocation;

    public VkImage(Image image, Allocation* allocation)
    {
        Image = image;
        Allocation = allocation;
    }
}