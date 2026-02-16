#pragma once

#include <memory>

#include <SDL3/SDL.h>

namespace Sprout
{
    class GraphicsDevice
    {
    public:
        virtual ~GraphicsDevice() = default;

        virtual void Present() = 0;

        static std::unique_ptr<GraphicsDevice> Create(SDL_Window* window);
    };
}