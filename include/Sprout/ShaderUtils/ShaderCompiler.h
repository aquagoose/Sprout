#pragma once

#include "../Graphics/Shader.h"

#include <vector>
#include <cstdint>
#include <string>

namespace Sprout::ShaderCompiler
{
    std::vector<uint8_t> CompileHLSLToSpirV(ShaderStage stage, const std::string& hlsl, const std::string& entryPoint);
}