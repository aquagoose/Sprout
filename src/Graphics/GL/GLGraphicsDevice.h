#pragma once

#include "Sprout/Graphics/GraphicsDevice.h"

namespace Sprout::GL
{
    class GLGraphicsDevice final : public GraphicsDevice
    {
    public:
        explicit GLGraphicsDevice(SDL_Window* window);
        ~GLGraphicsDevice() override;
    };
}