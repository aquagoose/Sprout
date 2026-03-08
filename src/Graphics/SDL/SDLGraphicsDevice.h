#pragma once

#include "Sprout/Graphics/GraphicsDevice.h"

#include <SDL3/SDL.h>

namespace Sprout::SDL
{
    class SDLGraphicsDevice : public GraphicsDevice
    {
        SDL_Window* _window;
        SDL_GPUDevice* _device;

        // The command buffer for this current frame. If nullptr, it means the frame has not yet been started.
        SDL_GPUCommandBuffer* _commandBuffer{};

        // The swapchain texture for this current frame. If nullptr, it means either the frame has not yet been started,
        // or that it's not ready to be rendered to. Either way, don't render to it, it won't end well. I hope you like
        // segfaults!
        SDL_GPUTexture* _swapchainTexture{};
        bool _hasSkippedRenderingThisFrame{};

        SDL_GPURenderPass* _renderPass{};
        Color _clearColor;
        bool _clearHasBeenRequested;

        bool EnsureInFrame();
        bool EnsureInRenderPass();
        void ResetPerFrameState();

    public:
        explicit SDLGraphicsDevice(SDL_Window* window);
        ~SDLGraphicsDevice() override;

        void Clear(const Color& color) override;

        void Present() override;
    };
}