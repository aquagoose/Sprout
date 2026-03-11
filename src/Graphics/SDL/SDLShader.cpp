#include "SDLShader.h"
#include "SDLUtils.h"

SDL_GPUShader* CreateShader(SDL_GPUDevice* device, const SDL_GPUShaderFormat format, const Sprout::ShaderAttachment& attachment)
{
    SDL_GPUShaderCreateInfo shaderInfo{};
    shaderInfo.code = attachment.Code;
    shaderInfo.code_size = attachment.CodeSize;
    shaderInfo.entrypoint = attachment.EntryPoint.c_str();
    shaderInfo.format = format;

    SDL_GPUShader* shader = SDL_CreateGPUShader(device, &shaderInfo);
    SDL_CHECK(shader, "Create shader");

    return shader;
}

namespace Sprout::SDL
{
    SDLShader::SDLShader(SDL_GPUDevice* device, const ShaderInfo& info) : _device(device)
    {
        SDL_GPUShaderFormat format = SDL_GetGPUShaderFormats(_device);

        if (info.VertexShader.has_value())
            VertexShader = CreateShader(_device, format, info.VertexShader.value());

        if (info.PixelShader.has_value())
            PixelShader = CreateShader(_device, format, info.PixelShader.value());
    }

    SDLShader::~SDLShader()
    {
        if (PixelShader)
            SDL_ReleaseGPUShader(_device, PixelShader);
        if (VertexShader)
            SDL_ReleaseGPUShader(_device, VertexShader);
    }
}
