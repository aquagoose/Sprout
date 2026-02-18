cmake_minimum_required(VERSION 3.20)
project(VulkanMemoryAllocator)

add_library(VulkanMemoryAllocator SHARED
	../VulkanMemoryAllocator/include/vk_mem_alloc.h
	../VulkanMemoryAllocator/src/VmaUsage.h
	../VulkanMemoryAllocator/src/VmaUsage.cpp
)

target_include_directories(VulkanMemoryAllocator PUBLIC ../VulkanMemoryAllocator/include/)

find_package(Vulkan REQUIRED)
target_link_libraries(VulkanMemoryAllocator PUBLIC ${Vulkan_LIBRARIES})
target_include_directories(VulkanMemoryAllocator PUBLIC ${Vulkan_INCLUDE_DIRS})
