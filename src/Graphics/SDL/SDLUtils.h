#pragma once

#include <SDL3/SDL.h>

#include <stdexcept>

// TODO: fmtlib
#define SDL_CHECK(Result, Operation) \
    if (!Result) \
        throw std::runtime_error("SDL operation '" + std::string(Operation) + "' failed: " + std::string(SDL_GetError()));

namespace Sprout {}