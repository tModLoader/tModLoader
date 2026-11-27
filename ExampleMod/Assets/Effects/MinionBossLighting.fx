// It's expected that you are already familiar with shaders, so this example
// will not go into detail about shader syntax or other commonly-understood
// concepts. It will just focus on the logic behind sampling the lightmap.
// See MinionBossBody.cs for the C# side of the implementation, and how to use
// this shader.

// Textures drawn with SpriteBatch are put into texture register 0 (t0). The
// corresponding sampler register (s0) will implicitly sample from that.
sampler2D uTexture : register(s0);

// We must pass in the lightmap buffer this way. We manually define a linear
// sampler, but SamplerState.LinearWrap and SamplerState.LinearClamp are just
// as effective.
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

// The size of the lightmap buffer.
float2 uLightMapSize;
// The top-left origin of the viewport. This will generally always be (0, 0).
float2 uViewportOrigin;
// The offset of the lightmap. See MinionBossBody for how to calculate it, this
// is important for sampling the correct pixel in LightMap since it may be
// offset from the screen itself.
float2 uDrawOffset;

// This is the inversion of the transformation matrix currently-rendered quads
// are using. We this to undo the transformation to convert the transformed
// coordinates back to something absolute for the lightmap.
float4x4 uDrawMatrix;

// Lighting.GlobalBrightness needs to be manually accounted for here.
float uGlobalBrightness;

float4 PixelShaderFunction(float2 svPos : SV_POSITION, float2 textureUv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
	// Sample the actual texture to get the base color, before lighting is
	// applied.
	float4 albedo = tex2D(uTexture, textureUv) * color;

	// Offset SV_POSITION based on the viewport origin to work within viewport
	// bounds.
	float2 viewportPosition = svPos - uViewportOrigin;
	// Get the real "draw" position by applying the inverted matrix. This gets
	// us the original position in world space.
	float2 drawPosition = mul(float4(viewportPosition, 0.0, 1.0), uDrawMatrix).xy;
	// This converts the screen pixels to lightmap pixels, since the lightmap
	// is in tiles. uDrawOffset is applied to account for the part of the
	// lightmap from the top-left that is off-screen.
	float2 lightUv = (drawPosition + uDrawOffset) / (TILE_SIZE * uLightMapSize);
	
	// Sample the lightbuffer and apply it multiplicatively.
	float3 light = tex2D(uLightMap, lightUv).rgb;
	light = saturate(light * uGlobalBrightness);
	return float4(albedo.rgb * light, albedo.a);
}

technique Technique1
{
	pass Pass1
	{
		// Common tools shared around the community such as EasyXNB and
		// fxcompiler cannot handle Shader Model 3.0 shaders (ps_3_0).
		// Some brief documentation on fxc is available here:
		// https://learn.microsoft.com/en-us/windows/win32/direct3dtools/fxc
		// Community tools such as fxcs and KBO.FXC also package the necessary
		// binaries, and fix a couple bugs (refer to their READMEs):
		// - https://github.com/steviegt6/fxcs
		// - https://github.com/ProjectSlowRush/KBO.FXC
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}