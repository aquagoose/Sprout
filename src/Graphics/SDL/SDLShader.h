#pragma once

#include "Sprout/Graphics/Shader.h"

#include <SDL3/SDL.h>

namespace Sprout::SDL
{
    class SDLShader final : public Shader
    {
        SDL_GPUDevice* _device;

    public:
        SDL_GPUShader* VertexShader{};
        SDL_GPUShader* PixelShader{};

        SDLShader(SDL_GPUDevice* device, const ShaderInfo& info);
        ~SDLShader() override;
    };
}