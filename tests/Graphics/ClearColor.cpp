#include "Base/TestBase.h"

using namespace Sprout;

class ClearColorTest : public TestBase
{
    float _timer{};
    int _currentColor{};
    Color _colors[8] = {
        { 1.0f, 0.0f, 0.0f }, // Red
        { 0.0f, 1.0f, 0.0f }, // Green
        { 0.0f, 0.0f, 1.0f }, // Blue
        { 1.0f, 1.0f, 0.0f }, // Yellow
        { 1.0f, 0.0f, 1.0f }, // Magenta
        { 0.0f, 1.0f, 1.0f }, // Cyan
        { 1.0f, 1.0f, 1.0f, }, // White
        { 0.0f, 0.0f, 0.0f } // Black
    };

    void Loop(float dt) override
    {
        _timer += dt;
        if (_timer >= 1.0f)
        {
            _timer -= 1.0f;
            _currentColor = (_currentColor + 1) % 8;
        }

        GraphicsDevice& device = Device();
        device.Clear(_colors[_currentColor]);
        device.Present();
    }
};

int main(int argc, char* argv[])
{
    ClearColorTest test;
    test.Run("Clear Color Test");

    return 0;
}