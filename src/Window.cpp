#include "Sprout/Window.h"

#include <stdexcept>

namespace Sprout
{
    Window::Window(const WindowInfo& info)
    {
        if (!SDL_Init(SDL_INIT_VIDEO))
            throw std::runtime_error("Failed to initialize SDL: " + std::string(SDL_GetError()));

        SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
        SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 3);
        SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);
        //SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG);

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
