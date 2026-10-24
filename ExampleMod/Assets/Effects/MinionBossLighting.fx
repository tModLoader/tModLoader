sampler2D uTexture : register(s0);

texture uLightMapTexture;
sampler2D uLightMap = sampler_state
{
    texture = <uLightMapTexture>;

    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;

    AddressU = wrap;
    AddressV = wrap;
};

#define TILE_SIZE (16.0)

float2 uLightMapSize;
float2 uViewportOrigin;
float2 uDrawOffset;

float4x4 uDrawMatrix;
float uGlobalBrightness;

float4 PixelShaderFunction(float2 svPos : SV_POSITION, float2 textureUv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 albedo = tex2D(uTexture, textureUv) * color;

    float2 viewportPosition = svPos - uViewportOrigin;
    float2 drawPosition = mul(float4(viewportPosition, 0.0, 1.0), uDrawMatrix).xy;
    float2 lightUv = (drawPosition + uDrawOffset) / (TILE_SIZE * uLightMapSize);
    
    float3 light = tex2D(uLightMap, lightUv).rgb;
    light = saturate(light * uGlobalBrightness);
    
    return float4(albedo.rgb * light, albedo.a);
}

technique Technique1
{
	pass Pass1
	{
        // EasyXNB cannot compile model 3 shaders, refer to tools like fxc
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}