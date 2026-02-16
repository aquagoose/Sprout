#include "GLGraphicsDevice.h"

#include <stdexcept>
#include <string>

#include <glad/glad.h>

namespace Sprout::GL
{
    GLGraphicsDevice::GLGraphicsDevice(SDL_Window* window) : _window(window)
    {
        _context = SDL_GL_CreateContext(_window);
        if (!_context)
            throw std::runtime_error("Failed to create GL context: " + std::string(SDL_GetError()));

        if (!SDL_GL_MakeCurrent(_window, _context))
            throw std::runtime_error("Failed to make current: " + std::string(SDL_GetError()));

        gladLoadGLLoader(reinterpret_cast<GLADloadproc>(SDL_GL_GetProcAddress));
    }

    GLGraphicsDevice::~GLGraphicsDevice()
    {
        SDL_GL_DestroyContext(_context);
    }

    void GLGraphicsDevice::Present()
    {
        SDL_GL_SetSwapInterval(1);
        SDL_GL_SwapWindow(_window);
    }
}
