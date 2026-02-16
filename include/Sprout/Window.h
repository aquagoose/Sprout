#pragma once

#include <string>

#include <SDL3/SDL.h>

namespace Sprout
{
    struct WindowInfo
    {
        std::string Title = "Sprout Window";
    };

    class Window final
    {
        SDL_Window* _window{};

    public:
        explicit Window(const WindowInfo& info);
        ~Window();
    };
}