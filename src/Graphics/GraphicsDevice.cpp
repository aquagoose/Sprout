#include <Sprout/Graphics/GraphicsDevice.h>

#ifdef SPROUT_RENDERER_OPENGL
#include "GL/GLGraphicsDevice.h"
#endif

#include <stdexcept>

namespace Sprout
{
    std::unique_ptr<GraphicsDevice> GraphicsDevice::Create(SDL_Window* window)
    {
#ifdef SPROUT_RENDERER_OPENGL
        return std::make_unique<GL::GLGraphicsDevice>(window);
#endif

        throw std::runtime_error("No backends available");
    }
}
