#include "Base/TestBase.h"

class ClearColorTest : public TestBase
{
    void Loop(float dt) override
    {

    }
};

int main(int argc, char* argv[])
{
    ClearColorTest test;
    test.Run("Clear Color Test");

    return 0;
}