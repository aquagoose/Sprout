#pragma once

#include "Color.h"
#include "Shader.h"

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

        virtual std::unique_ptr<Shader> CreateShader(const ShaderInfo& info) = 0;

        virtual void Clear(const Color& color) = 0;
        virtual void Present() = 0;

        static std::unique_ptr<GraphicsDevice> Create(SDL_Window* window, Backend backend = Backend::Unknown);
    };
}