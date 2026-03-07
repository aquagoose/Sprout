#include "Sprout/Graphics/GraphicsDevice.h"
#include "SDL/SDLGraphicsDevice.h"

#include <stdexcept>

namespace Sprout
{
    std::unique_ptr<GraphicsDevice> GraphicsDevice::Create(SDL_Window* window, Backend backend)
    {
#ifdef SPROUT_RENDERER_SDLGPU
        return std::make_unique<SDL::SDLGraphicsDevice>(window);
#endif

        throw std::runtime_error("No renderer backends available!");
    }
}
