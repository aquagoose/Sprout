using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan.VMA;

public unsafe partial struct VmaDefragmentationMove
{
    public VmaDefragmentationMoveOperation operation;

    [NativeTypeName("VmaAllocation _Nonnull")]
    public Allocation* srcAllocation;

    [NativeTypeName("VmaAllocation _Nonnull")]
    public Allocation* dstTmpAllocation;
}
