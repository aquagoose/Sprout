#include "Sprout/Window.h"

#include <stdexcept>

namespace Sprout
{
    Window::Window(const WindowInfo& info)
    {
        if (!SDL_Init(SDL_INIT_VIDEO))
            throw std::runtime_error("Failed to initialize SDL: " + std::string(SDL_GetError()));

        _window = SDL_CreateWindow(info.Title.c_str(), 1280, 720, 0);
        if (!_window)
            throw std::runtime_error("Failed to create window: " + std::string(SDL_GetError()));
    }

    Window::~Window()
    {
        SDL_DestroyWindow(_window);
        SDL_Quit();
    }
}
