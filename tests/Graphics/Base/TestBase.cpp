#include "TestBase.h"

#include <stdexcept>

TestBase::~TestBase()
{
    SDL_DestroyWindow(_window);
    SDL_Quit();
}

void TestBase::Run(const std::string& testName)
{
    if (!SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS))
        throw std::runtime_error("Failed to initialize SDL: " + std::string(SDL_GetError()));

    _window = SDL_CreateWindow(testName.c_str(), 1280, 720, 0);
    if (!_window)
        throw std::runtime_error("Failed to create window: " + std::string(SDL_GetError()));

    Load();

    bool alive = true;
    while (alive)
    {
        SDL_Event event;
        while (SDL_PollEvent(&event))
        {
            switch (event.type)
            {
                case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                case SDL_EVENT_QUIT:
                    alive = false;
                    break;
            }
        }

        Loop(1 / 60.0f);
    }
}
