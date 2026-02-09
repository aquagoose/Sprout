using Silk.NET.Vulkan;
using System.Runtime.CompilerServices;

namespace Sprout.Graphics.Vulkan.VMA;

public partial struct VmaTotalStatistics
{
    [NativeTypeName("VmaDetailedStatistics[32]")]
    public _memoryType_e__FixedBuffer memoryType;

    [NativeTypeName("VmaDetailedStatistics[16]")]
    public _memoryHeap_e__FixedBuffer memoryHeap;

    public VmaDetailedStatistics total;

    [InlineArray(32)]
    public partial struct _memoryType_e__FixedBuffer
    {
        public VmaDetailedStatistics e0;
    }

    [InlineArray(16)]
    public partial struct _memoryHeap_e__FixedBuffer
    {
        public VmaDetailedStatistics e0;
    }
}
