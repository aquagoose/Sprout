#include "Base/TestBase.h"

const char* Shader = R"(
float4 VSMain(const in uint id: SV_VertexID): SV_Position
{
    // Triangle
    const float2 vertices[] =
    {
        float2(-0.5, -0.5),
        float2( 0.0,  0.5),
        float2( 0.5, -0.5),
    };

    return float4(vertices[id], 0.0, 1.0);
}


float4 PSMain(): SV_Target0
{
    return float4(1.0, 0.5, 0.25, 1.0);
}
)";

class SimpleShaderTest : public TestBase
{
    std::unique_ptr<Sprout::Shader> _shader;

    void Load() override
    {

    }

    void Loop(float dt) override
    {
        auto& device = Device();
        device.Clear({ 0.25f, 0.5f, 1.0f });
        device.Present();
    }
};

int main(int argc, char* argv[])
{
    SimpleShaderTest test;
    test.Run("Simple Shader Test");

    return 0;
}