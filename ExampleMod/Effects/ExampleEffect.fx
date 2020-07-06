sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
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

	int choice = uTime % 4;

	if(choice == 0)
		color.r = 1;
	else if(choice == 1)
		color.g = 1;
	else if (choice == 2)
		color.b = 1;
		//color = float4(0, 0, 1, 1);
	//else if (choice == 3)
	//   color = color;

	return color;

	//return color * tex2D(uImage0, coords).a;
}

technique Technique1
{
	pass ExampleDyePass
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}