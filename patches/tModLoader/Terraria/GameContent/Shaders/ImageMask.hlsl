sampler uImage0 : register(s0);

texture uMaskTexture;
sampler2D maskTexture = sampler_state
{
    texture = <uMaskTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

bool invert;

float4 main(float4 drawColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float alpha = tex2D(maskTexture, uv).a;
    return tex2D(uImage0, uv) * drawColor * (invert ? 1 - alpha : alpha);
}

technique Technique1
{
    pass MaskShader
    {
        PixelShader = compile ps_3_0 main();
    }
}
