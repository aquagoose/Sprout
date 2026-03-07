#pragma once

#include "Sprout/Graphics/GraphicsDevice.h"

#include <SDL3/SDL.h>

namespace Sprout::SDL
{
    class SDLGraphicsDevice : public GraphicsDevice
    {
        SDL_Window* _window;
        SDL_GPUDevice* _device;

    public:
        explicit SDLGraphicsDevice(SDL_Window* window);
        ~SDLGraphicsDevice() override;
    };
}