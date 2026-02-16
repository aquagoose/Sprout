#pragma once

#include <memory>

#include <SDL3/SDL.h>

namespace Sprout
{
    class GraphicsDevice
    {
    public:
        virtual ~GraphicsDevice() = default;

        static std::unique_ptr<GraphicsDevice> Create(SDL_Window* window);
    };
}