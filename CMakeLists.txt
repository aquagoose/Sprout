cmake_minimum_required(VERSION 4.1)
project(Sprout)

set(CMAKE_CXX_STANDARD 17)

option(SPROUT_RENDERER_SDLGPU "Enable the SDL_GPU render backend" ON)

file(GLOB SPROUT_SRC CONFIGURE_DEPENDS
        include/Sprout/*.h
        src/*.h
        src/*.cpp
)

file(GLOB SPROUT_GRAPHICS_SRC CONFIGURE_DEPENDS
        include/Sprout/Graphics/*.h
        src/Graphics/*.h
        src/Graphics/*.cpp
)
list(APPEND SPROUT_SRC ${SPROUT_GRAPHICS_SRC})

if (SPROUT_RENDERER_SDLGPU)
    file(GLOB SPROUT_SDLGPU_SRC CONFIGURE_DEPENDS src/Graphics/SDL/*.h src/Graphics/SDL/*.cpp)
    list(APPEND SPROUT_SRC ${SPROUT_SDLGPU_SRC})
endif ()

add_library(Sprout STATIC ${SPROUT_SRC})
target_include_directories(Sprout PUBLIC include)

add_subdirectory(external/SDL)
target_link_libraries(Sprout PUBLIC SDL3::SDL3)

if (SPROUT_RENDERER_SDLGPU)
    target_compile_definitions(Sprout PUBLIC SPROUT_RENDERER_SDLGPU)
endif ()