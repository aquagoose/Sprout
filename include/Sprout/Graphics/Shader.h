#pragma once

#include <string>
#include <cstdint>
#include <optional>

namespace Sprout
{
    enum class ShaderStage
    {
        Vertex,
        Pixel
    };

    struct ShaderAttachment
    {
        uint8_t* Code;
        size_t CodeSize;
        std::string EntryPoint;
    };

    struct ShaderInfo
    {
        std::optional<ShaderAttachment> VertexShader;
        std::optional<ShaderAttachment> PixelShader;
    };

    class Shader
    {
    public:
        virtual ~Shader() = default;
    };
}