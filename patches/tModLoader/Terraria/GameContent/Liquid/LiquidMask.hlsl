sampler uImage0 : register(s0);

float4 main(float4 drawColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float alpha = (tex2D(uImage0, uv) * drawColor).a;
    return (alpha > 0) ? 1 : 0;
}

technique Technique1
{
    pass MaskShader
    {
        PixelShader = compile ps_3_0 main();
    }
}
