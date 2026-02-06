struct VSInput
{
    float2 Position: POSITION0;
    float2 TexCoord: TEXCOORD0;
};

struct VSOutput
{
    float4 Position: SV_Position;
    float2 TexCoord: TEXCOORD0;
};

Texture2D Texture : register(t0);
SamplerState Sampler : register(s0);

VSOutput VSMain(const in VSInput input)
{
    VSOutput output;
    
    output.Position = float4(input.Position, 0.0, 1.0);
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 PSMain(const in VSOutput input): SV_Target0
{
    return Texture.Sample(Sampler, input.TexCoord);
}