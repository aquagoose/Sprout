#pragma once

#include <SDL3/SDL.h>
#include <Sprout/Graphics/GraphicsDevice.h>

#include <string>

class TestBase
{
    SDL_Window* _window{};
    std::unique_ptr<Sprout::GraphicsDevice> _graphicsDevice;

protected:
    [[nodiscard]] Sprout::GraphicsDevice* Device() const { return _graphicsDevice.get(); }

    virtual void Load() {}
    virtual void Loop(float dt) {}

public:
    virtual ~TestBase();
    void Run(const std::string& testName);
};