sampler uImage0 : register(s0);

texture uMaskTexture;
sampler2D maskTexture = sampler_state
{
    texture = <uMaskTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};
bool usePartialAlpha;

float4 main(float4 drawColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float alpha = tex2D(maskTexture, uv).a;
    float4 image = tex2D(uImage0, uv) * drawColor;
    if (usePartialAlpha)
    {
        alpha = alpha > 0. && alpha < 1. ? 1. : 0;
    }
    else
    {
        alpha = alpha > 0. ? 1. : alpha;
    }
    
    return image * (1 - alpha);
}

technique Technique1
{
    pass MaskShader
    {
        PixelShader = compile ps_3_0 main();
    }
}
