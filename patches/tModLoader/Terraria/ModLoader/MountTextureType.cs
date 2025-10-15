namespace Terraria.ModLoader;

/// <summary>
/// This is an enum of all possible types of extra mount textures for custom mounts.
/// <br/> The enum's keys are used in default texture autoloading lookup paths, which can be overriden in <see cref="ModMount.GetExtraTexture"/>.
/// </summary>
public enum MountTextureType
{
	Back,
	BackGlow,
	BackExtra,
	BackExtraGlow,
	Front,
	FrontGlow,
	FrontExtra,
	FrontExtraGlow
}
