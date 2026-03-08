#include "Base/TestBase.h"

class ClearColorTest : public TestBase
{
    void Loop(float dt) override
    {
        const auto device = Device();
        device->Clear({ 1.0f, 0.5f, 0.2f });
        device->Present();
    }
};

int main(int argc, char* argv[])
{
    ClearColorTest test;
    test.Run("Clear Color Test");

    return 0;
}