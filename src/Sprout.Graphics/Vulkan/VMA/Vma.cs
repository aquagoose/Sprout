using Silk.NET.Vulkan;
using System.Runtime.InteropServices;

namespace Sprout.Graphics.Vulkan.VMA;

public static unsafe partial class Vma
{
    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateAllocator([NativeTypeName("const VmaAllocatorCreateInfo * _Nonnull")] AllocatorCreateInfo* pCreateInfo, [NativeTypeName("VmaAllocator  _Nullable * _Nonnull")] Allocator** pAllocator);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaDestroyAllocator([NativeTypeName("VmaAllocator _Nullable")] Allocator* allocator);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetAllocatorInfo([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocatorInfo * _Nonnull")] VmaAllocatorInfo* pAllocatorInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetPhysicalDeviceProperties([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkPhysicalDeviceProperties * _Nullable * _Nonnull")] PhysicalDeviceProperties** ppPhysicalDeviceProperties);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetMemoryProperties([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkPhysicalDeviceMemoryProperties * _Nullable * _Nonnull")] PhysicalDeviceMemoryProperties** ppPhysicalDeviceMemoryProperties);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetMemoryTypeProperties([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("uint32_t")] uint memoryTypeIndex, [NativeTypeName("VkMemoryPropertyFlags * _Nonnull")] uint* pFlags);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaSetCurrentFrameIndex([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("uint32_t")] uint frameIndex);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaCalculateStatistics([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaTotalStatistics * _Nonnull")] VmaTotalStatistics* pStats);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetHeapBudgets([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaBudget * _Nonnull")] VmaBudget* pBudgets);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaFindMemoryTypeIndex([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("uint32_t")] uint memoryTypeBits, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("uint32_t * _Nonnull")] uint* pMemoryTypeIndex);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaFindMemoryTypeIndexForBufferInfo([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkBufferCreateInfo * _Nonnull")] BufferCreateInfo* pBufferCreateInfo, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("uint32_t * _Nonnull")] uint* pMemoryTypeIndex);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaFindMemoryTypeIndexForImageInfo([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkImageCreateInfo * _Nonnull")] ImageCreateInfo* pImageCreateInfo, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("uint32_t * _Nonnull")] uint* pMemoryTypeIndex);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreatePool([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VmaPoolCreateInfo * _Nonnull")] VmaPoolCreateInfo* pCreateInfo, [NativeTypeName("VmaPool  _Nullable * _Nonnull")] VmaPool_T** pPool);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaDestroyPool([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaPool _Nullable")] VmaPool_T* pool);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetPoolStatistics([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaPool _Nonnull")] VmaPool_T* pool, [NativeTypeName("VmaStatistics * _Nonnull")] VmaStatistics* pPoolStats);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaCalculatePoolStatistics([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaPool _Nonnull")] VmaPool_T* pool, [NativeTypeName("VmaDetailedStatistics * _Nonnull")] VmaDetailedStatistics* pPoolStats);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCheckPoolCorruption([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaPool _Nonnull")] VmaPool_T* pool);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetPoolName([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaPool _Nonnull")] VmaPool_T* pool, [NativeTypeName("const char * _Nullable * _Nonnull")] sbyte** ppName);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaSetPoolName([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaPool _Nonnull")] VmaPool_T* pool, [NativeTypeName("const char * _Nullable")] sbyte* pName);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaAllocateMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkMemoryRequirements * _Nonnull")] MemoryRequirements* pVkMemoryRequirements, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pCreateInfo, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaAllocateDedicatedMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkMemoryRequirements * _Nonnull")] MemoryRequirements* pVkMemoryRequirements, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pCreateInfo, [NativeTypeName("void * _Nullable")] void* pMemoryAllocateNext, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaAllocateMemoryPages([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkMemoryRequirements * _Nonnull")] MemoryRequirements* pVkMemoryRequirements, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pCreateInfo, [NativeTypeName("size_t")] nuint allocationCount, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocations, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaAllocateMemoryForBuffer([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VkBuffer _Nonnull")] Silk.NET.Vulkan.Buffer buffer, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pCreateInfo, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaAllocateMemoryForImage([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VkImage _Nonnull")] Image image, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pCreateInfo, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaFreeMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nullable")] Allocation* allocation);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaFreeMemoryPages([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("size_t")] nuint allocationCount, [NativeTypeName("VmaAllocation  _Nullable const * _Nonnull")] Allocation** pAllocations);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetAllocationInfo([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VmaAllocationInfo * _Nonnull")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetAllocationInfo2([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VmaAllocationInfo2 * _Nonnull")] VmaAllocationInfo2* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaSetAllocationUserData([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("void * _Nullable")] void* pUserData);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaSetAllocationName([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("const char * _Nullable")] sbyte* pName);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetAllocationMemoryProperties([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkMemoryPropertyFlags * _Nonnull")] uint* pFlags);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaMapMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("void * _Nullable * _Nonnull")] void** ppData);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaUnmapMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaFlushAllocation([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkDeviceSize")] nuint offset, [NativeTypeName("VkDeviceSize")] nuint size);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaInvalidateAllocation([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkDeviceSize")] nuint offset, [NativeTypeName("VkDeviceSize")] nuint size);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaFlushAllocations([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("uint32_t")] uint allocationCount, [NativeTypeName("VmaAllocation  _Nonnull const * _Nullable")] Allocation** allocations, [NativeTypeName("const VkDeviceSize * _Nullable")] nuint* offsets, [NativeTypeName("const VkDeviceSize * _Nullable")] nuint* sizes);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaInvalidateAllocations([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("uint32_t")] uint allocationCount, [NativeTypeName("VmaAllocation  _Nonnull const * _Nullable")] Allocation** allocations, [NativeTypeName("const VkDeviceSize * _Nullable")] nuint* offsets, [NativeTypeName("const VkDeviceSize * _Nullable")] nuint* sizes);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCopyMemoryToAllocation([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const void * _Nonnull")] void* pSrcHostPointer, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* dstAllocation, [NativeTypeName("VkDeviceSize")] nuint dstAllocationLocalOffset, [NativeTypeName("VkDeviceSize")] nuint size);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCopyAllocationToMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* srcAllocation, [NativeTypeName("VkDeviceSize")] nuint srcAllocationLocalOffset, [NativeTypeName("void * _Nonnull")] void* pDstHostPointer, [NativeTypeName("VkDeviceSize")] nuint size);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCheckCorruption([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("uint32_t")] uint memoryTypeBits);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaBeginDefragmentation([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VmaDefragmentationInfo * _Nonnull")] VmaDefragmentationInfo* pInfo, [NativeTypeName("VmaDefragmentationContext  _Nullable * _Nonnull")] VmaDefragmentationContext_T** pContext);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaEndDefragmentation([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaDefragmentationContext _Nonnull")] VmaDefragmentationContext_T* context, [NativeTypeName("VmaDefragmentationStats * _Nullable")] VmaDefragmentationStats* pStats);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaBeginDefragmentationPass([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaDefragmentationContext _Nonnull")] VmaDefragmentationContext_T* context, [NativeTypeName("VmaDefragmentationPassMoveInfo * _Nonnull")] VmaDefragmentationPassMoveInfo* pPassInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaEndDefragmentationPass([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaDefragmentationContext _Nonnull")] VmaDefragmentationContext_T* context, [NativeTypeName("VmaDefragmentationPassMoveInfo * _Nonnull")] VmaDefragmentationPassMoveInfo* pPassInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaBindBufferMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkBuffer _Nonnull")] Silk.NET.Vulkan.Buffer buffer);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaBindBufferMemory2([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkDeviceSize")] nuint allocationLocalOffset, [NativeTypeName("VkBuffer _Nonnull")] Silk.NET.Vulkan.Buffer buffer, [NativeTypeName("const void * _Nullable")] void* pNext);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaBindImageMemory([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkImage _Nonnull")] Image image);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaBindImageMemory2([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkDeviceSize")] nuint allocationLocalOffset, [NativeTypeName("VkImage _Nonnull")] Image image, [NativeTypeName("const void * _Nullable")] void* pNext);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateBuffer([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkBufferCreateInfo * _Nonnull")] BufferCreateInfo* pBufferCreateInfo, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("VkBuffer  _Nullable * _Nonnull")] Silk.NET.Vulkan.Buffer* pBuffer, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateBufferWithAlignment([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkBufferCreateInfo * _Nonnull")] BufferCreateInfo* pBufferCreateInfo, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("VkDeviceSize")] nuint minAlignment, [NativeTypeName("VkBuffer  _Nullable * _Nonnull")] Silk.NET.Vulkan.Buffer* pBuffer, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateDedicatedBuffer([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkBufferCreateInfo * _Nonnull")] BufferCreateInfo* pBufferCreateInfo, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("void * _Nullable")] void* pMemoryAllocateNext, [NativeTypeName("VkBuffer  _Nullable * _Nonnull")] Silk.NET.Vulkan.Buffer* pBuffer, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateAliasingBuffer([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("const VkBufferCreateInfo * _Nonnull")] BufferCreateInfo* pBufferCreateInfo, [NativeTypeName("VkBuffer  _Nullable * _Nonnull")] Silk.NET.Vulkan.Buffer* pBuffer);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateAliasingBuffer2([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkDeviceSize")] nuint allocationLocalOffset, [NativeTypeName("const VkBufferCreateInfo * _Nonnull")] BufferCreateInfo* pBufferCreateInfo, [NativeTypeName("VkBuffer  _Nullable * _Nonnull")] Silk.NET.Vulkan.Buffer* pBuffer);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaDestroyBuffer([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VkBuffer _Nullable")] Silk.NET.Vulkan.Buffer buffer, [NativeTypeName("VmaAllocation _Nullable")] Allocation* allocation);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateImage([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkImageCreateInfo * _Nonnull")] ImageCreateInfo* pImageCreateInfo, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("VkImage  _Nullable * _Nonnull")] Image* pImage, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateDedicatedImage([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("const VkImageCreateInfo * _Nonnull")] ImageCreateInfo* pImageCreateInfo, [NativeTypeName("const VmaAllocationCreateInfo * _Nonnull")] AllocationCreateInfo* pAllocationCreateInfo, [NativeTypeName("void * _Nullable")] void* pMemoryAllocateNext, [NativeTypeName("VkImage  _Nullable * _Nonnull")] Image* pImage, [NativeTypeName("VmaAllocation  _Nullable * _Nonnull")] Allocation** pAllocation, [NativeTypeName("VmaAllocationInfo * _Nullable")] VmaAllocationInfo* pAllocationInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateAliasingImage([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("const VkImageCreateInfo * _Nonnull")] ImageCreateInfo* pImageCreateInfo, [NativeTypeName("VkImage  _Nullable * _Nonnull")] Image* pImage);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateAliasingImage2([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VmaAllocation _Nonnull")] Allocation* allocation, [NativeTypeName("VkDeviceSize")] nuint allocationLocalOffset, [NativeTypeName("const VkImageCreateInfo * _Nonnull")] ImageCreateInfo* pImageCreateInfo, [NativeTypeName("VkImage  _Nullable * _Nonnull")] Image* pImage);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaDestroyImage([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("VkImage _Nullable")] Image image, [NativeTypeName("VmaAllocation _Nullable")] Allocation* allocation);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaCreateVirtualBlock([NativeTypeName("const VmaVirtualBlockCreateInfo * _Nonnull")] VmaVirtualBlockCreateInfo* pCreateInfo, [NativeTypeName("VmaVirtualBlock  _Nullable * _Nonnull")] VmaVirtualBlock_T** pVirtualBlock);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaDestroyVirtualBlock([NativeTypeName("VmaVirtualBlock _Nullable")] VmaVirtualBlock_T* virtualBlock);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkBool32")]
    public static extern uint vmaIsVirtualBlockEmpty([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetVirtualAllocationInfo([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("VmaVirtualAllocation _Nonnull")] VmaVirtualAllocation_T* allocation, [NativeTypeName("VmaVirtualAllocationInfo * _Nonnull")] VmaVirtualAllocationInfo* pVirtualAllocInfo);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: NativeTypeName("VkResult")]
    public static extern Result vmaVirtualAllocate([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("const VmaVirtualAllocationCreateInfo * _Nonnull")] VmaVirtualAllocationCreateInfo* pCreateInfo, [NativeTypeName("VmaVirtualAllocation  _Nullable * _Nonnull")] VmaVirtualAllocation_T** pAllocation, [NativeTypeName("VkDeviceSize * _Nullable")] nuint* pOffset);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaVirtualFree([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("VmaVirtualAllocation _Nullable")] VmaVirtualAllocation_T* allocation);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaClearVirtualBlock([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaSetVirtualAllocationUserData([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("VmaVirtualAllocation _Nonnull")] VmaVirtualAllocation_T* allocation, [NativeTypeName("void * _Nullable")] void* pUserData);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaGetVirtualBlockStatistics([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("VmaStatistics * _Nonnull")] VmaStatistics* pStats);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaCalculateVirtualBlockStatistics([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("VmaDetailedStatistics * _Nonnull")] VmaDetailedStatistics* pStats);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaBuildVirtualBlockStatsString([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("char * _Nullable * _Nonnull")] sbyte** ppStatsString, [NativeTypeName("VkBool32")] uint detailedMap);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaFreeVirtualBlockStatsString([NativeTypeName("VmaVirtualBlock _Nonnull")] VmaVirtualBlock_T* virtualBlock, [NativeTypeName("char * _Nullable")] sbyte* pStatsString);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaBuildStatsString([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("char * _Nullable * _Nonnull")] sbyte** ppStatsString, [NativeTypeName("VkBool32")] uint detailedMap);

    [DllImport("VulkanMemoryAllocator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void vmaFreeStatsString([NativeTypeName("VmaAllocator _Nonnull")] Allocator* allocator, [NativeTypeName("char * _Nullable")] sbyte* pStatsString);
}
