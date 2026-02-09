using Silk.NET.Vulkan;

namespace Sprout.Graphics.Vulkan.VMA;

public unsafe partial struct VmaDefragmentationPassMoveInfo
{
    [NativeTypeName("uint32_t")]
    public uint moveCount;

    [NativeTypeName("VmaDefragmentationMove * _Nullable")]
    public VmaDefragmentationMove* pMoves;
}
