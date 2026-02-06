float4 VSMain(const in uint vertexID: SV_VertexID): SV_Position
{
    const float2 vertices[] =
    {
        float2(-0.5, -0.5),
        float2( 0.0,  0.5),
        float2( 0.5, -0.5)
    };
    
    return float4(vertices[vertexID], 0.0, 1.0);
}

float4 PSMain(): SV_Target0
{
    return float4(0.25, 0.5, 1.0, 1.0);
}