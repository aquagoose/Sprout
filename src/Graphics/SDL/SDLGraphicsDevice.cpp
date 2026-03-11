#include "SDLGraphicsDevice.h"
#include "SDLUtils.h"
#include "SDLShader.h"

namespace Sprout::SDL
{
    bool SDLGraphicsDevice::EnsureInFrame()
    {
        if (_hasSkippedRenderingThisFrame)
            return false;

        _commandBuffer = SDL_AcquireGPUCommandBuffer(_device);
        SDL_CHECK(_commandBuffer, "Acquire command buffer");

        SDL_CHECK(SDL_WaitAndAcquireGPUSwapchainTexture(_commandBuffer, _window, &_swapchainTexture, nullptr, nullptr),
                  "Acquire swapchain texture");

        // Texture is not available yet!
        if (!_swapchainTexture)
        {
            SDL_CancelGPUCommandBuffer(_commandBuffer);
            _commandBuffer = nullptr;
            _hasSkippedRenderingThisFrame = true;
            return false;
        }

        return true;
    }

    bool SDLGraphicsDevice::EnsureInRenderPass()
    {
        if (!EnsureInFrame())
            return false;

        if (_renderPass)
            return true;

        SDL_GPUColorTargetInfo targetInfo{};
        targetInfo.texture = _swapchainTexture;
        targetInfo.clear_color = { _clearColor.R, _clearColor.G, _clearColor.B, _clearColor.A };
        targetInfo.load_op = _clearHasBeenRequested ? SDL_GPU_LOADOP_CLEAR : SDL_GPU_LOADOP_LOAD;
        targetInfo.store_op = SDL_GPU_STOREOP_STORE;

        _renderPass = SDL_BeginGPURenderPass(_commandBuffer, &targetInfo, 1, nullptr);
        SDL_CHECK(_renderPass, "Begin render pass");

        return true;
    }

    void SDLGraphicsDevice::ResetPerFrameState()
    {
        _renderPass = nullptr;
        _hasSkippedRenderingThisFrame = false;
    }

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

    std::unique_ptr<Shader> SDLGraphicsDevice::CreateShader(const ShaderInfo& info)
    {
        return std::make_unique<SDLShader>(_device, info);
    }

    void SDLGraphicsDevice::Clear(const Color& color)
    {
        // Don't directly begin a render pass here.
        // All of this is to support this hypothetical situation:
        //   Device->Clear();
        //   Device->SetRenderTexture();
        //   <draw commands>
        //   Device->SetRenderTexture(nullptr);
        //   <draw commands involving main render target>
        // We must support this out-of-order clearing, by only clearing when the target itself is actually being
        // rendered to.
        _clearColor = color;
        _clearHasBeenRequested = true;
    }

    void SDLGraphicsDevice::Present()
    {
        if (!EnsureInRenderPass())
        {
            ResetPerFrameState();
            return;
        }

        SDL_EndGPURenderPass(_renderPass);
        SDL_SubmitGPUCommandBuffer(_commandBuffer);

        ResetPerFrameState();
    }
}
