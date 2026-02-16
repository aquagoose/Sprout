#pragma once

#include "Sprout/Graphics/GraphicsDevice.h"

namespace Sprout::GL
{
    class GLGraphicsDevice final : public GraphicsDevice
    {
        SDL_Window* _window;
        SDL_GLContext _context;

    public:
        explicit GLGraphicsDevice(SDL_Window* window);
        ~GLGraphicsDevice() override;

        void Present() override;
    };
}