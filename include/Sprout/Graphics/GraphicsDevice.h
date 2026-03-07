#pragma once

#include <SDL3/SDL.h>

#include <memory>

namespace Sprout
{
    enum class Backend
    {
        Unknown = 0,

        Vulkan = 1,

        D3D12 = 2,

        Metal = 3,

        D3D11 = 4,

        OpenGL = 5,

        OpenGLES = 6,

        SDL = 7
    };

    class GraphicsDevice
    {
    public:
        virtual ~GraphicsDevice() = default;

        static std::unique_ptr<GraphicsDevice> Create(SDL_Window* window, Backend backend = Backend::Unknown);
    };
}