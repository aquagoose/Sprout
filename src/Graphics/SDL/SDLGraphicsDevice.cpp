#include "SDLGraphicsDevice.h"
#include "SDLUtils.h"

namespace Sprout::SDL
{
    SDLGraphicsDevice::SDLGraphicsDevice(SDL_Window* window) : _window(window)
    {
        const SDL_PropertiesID deviceProps = SDL_CreateProperties();
        SDL_SetBooleanProperty(deviceProps, SDL_PROP_GPU_DEVICE_CREATE_SHADERS_SPIRV_BOOLEAN, true);

        _device = SDL_CreateGPUDeviceWithProperties(deviceProps);
        SDL_CHECK(_device, "Create device");
        SDL_CHECK(SDL_ClaimWindowForGPUDevice(_device, _window), "Claim window for device");
    }

    SDLGraphicsDevice::~SDLGraphicsDevice()
    {
        SDL_ReleaseWindowFromGPUDevice(_device, _window);
        SDL_DestroyGPUDevice(_device);
    }
}
