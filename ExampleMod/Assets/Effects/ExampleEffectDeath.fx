sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity : register(C0);
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;

// This is a shader. You are on your own with shaders. Compile shaders in an XNB project.

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coords);
	if (!any(color))
		return color;
	float4 color1= tex2D( uImage1 , coords.xy);

	float readRed = uOpacity * 1.1;

	if(color1.r > readRed){
		color.rgba = 0;
	}else if(color1.r > uOpacity ){
		color =  float4(1, 105.0/255, 180.0/255, 1);
	}
	return color;
}

technique Technique1
{
	pass DeathAnimation
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}