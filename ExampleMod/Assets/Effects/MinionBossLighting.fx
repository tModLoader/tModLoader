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
float2 uScreenSize;
float2 uScreenPosition;

float uGlobalBrightness;

float2 uDrawOffset;
matrix uDrawMatrix;
bool uDrawFlipped;

// This is a shader. You are on your own with shaders. Compile shaders in an XNB project.

float4 PixelShaderFunction(float2 svPos : SV_POSITION, float2 textureUv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 albedo = tex2D(uTexture, textureUv) * color;
    
    svPos.y *= uDrawFlipped ? -1 : 1;
    svPos.y += uScreenSize.y * uDrawFlipped;
    
    float2 screenPosTiles = (svPos + uDrawOffset) / TILE_SIZE;
    float2 lightUv = screenPosTiles / uLightMapSize;
    
    lightUv = mul(lightUv, uDrawMatrix);
    
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