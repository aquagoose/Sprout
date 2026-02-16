#include <Sprout/Sprout.h>

using namespace Sprout;

int main(int argc, char* argv[])
{
    WindowInfo info;
    info.Title = "Clear Color Test";

    auto window = std::make_unique<Window>(info);
    auto device = GraphicsDevice::Create(window->Handle());

    bool alive = true;
    while (alive)
    {
        SDL_Event event;
        while (SDL_PollEvent(&event))
        {
            switch (event.type)
            {
                case SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                    alive = false;
                    break;
            }
        }

        device->Clear({ 1.0f, 0.5f, 0.25f });
        device->Present();
    }

    return 0;
}