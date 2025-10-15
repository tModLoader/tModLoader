using System;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Terraria.ModLoader;

/// <summary>
/// ModSceneEffect is an abstract class that your classes can derive from. It serves as a container for handling exclusive SceneEffect content such as backgrounds, music, and water styling.
/// </summary>
public abstract partial class ModSceneEffect : ModType
{
	public int Type { get; internal set; }

	// SceneEffect properties
	/// <summary>
	/// The ModWaterStyle that will apply to water.
	/// </summary>
	public virtual ModWaterStyle WaterStyle => null;

	/// <summary>
	/// The ModSurfaceBackgroundStyle that will draw its background when the player is on the surface.
	/// </summary>
	public virtual ModSurfaceBackgroundStyle SurfaceBackgroundStyle => null;

	/// <summary>
	/// The ModUndergroundBackgroundStyle that will draw its background when the player is underground.
	/// </summary>
	public virtual ModUndergroundBackgroundStyle UndergroundBackgroundStyle => null;

	/// <SharedSummary>
	/// The music that will play. -1 for letting other music play, 0 for no music, >0 for the given music to play (using <see cref="MusicLoader.GetMusicSlot(Mod, string)"/> or <see cref="ID.MusicID"/>).
	/// <br/><br/> See <see cref="Main.swapMusic"/> for information about playing alternate music when the Otherworld soundtrack is enabled.
	/// </SharedSummary>
	/// <summary>
	/// <inheritdoc cref="Music" path="/SharedSummary/node()"/>
	/// <para/> Defaults to -1.
	/// </summary>
	public virtual int Music => -1;

	/// <summary>
	/// The path to the texture that will display behind the map. Should be 115x65.
	/// </summary>
	public virtual string MapBackground => null;

	/// <summary>
	/// If true, the map background (<see cref="MapBackground"/>) will be forced to be drawn at full brightness (White). For example, the background map of the Mushroom biome draws at full brightness even when above ground.
	/// <para/> By default, this returns false, indicating that the sky color should be used if above surface level and full brightness otherwise. 
	/// <para/> Use <see cref="MapBackgroundColor(ref Color)"/> instead to fully customize the map background draw color.
	/// </summary>
	public virtual bool MapBackgroundFullbright => false;

	/// <SharedSummary>
	/// The <see cref="SceneEffectPriority"/> of this SceneEffect layer. Determines the relative position compared to a vanilla SceneEffect.
	/// Analogously, if SceneEffect were competing in a wrestling match, this would be the 'Weight Class' that this SceneEffect is competing in.
	/// </SharedSummary>
	/// <summary>
	/// <inheritdoc cref="Priority" path="/SharedSummary/node()"/>
	/// <para/> Defaults to <see cref="SceneEffectPriority.None"/>.
	/// </summary>
	public virtual SceneEffectPriority Priority => SceneEffectPriority.None;

	/// <summary>
	/// Used to apply secondary color shading for the capture camera. For example, darkening the background with the GlowingMushroom style.
	/// </summary>
	public virtual CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

	// Methods
	protected override void Register()
	{
		Type = LoaderManager.Get<SceneEffectLoader>().Register(this);
	}

	/// <summary>
	/// Forcefully registers the provided ModSceneEffect to LoaderManager.
	/// ModBiome and direct implementations call this.
	/// Does NOT cache the return type.
	/// </summary>
	internal void RegisterSceneEffect(ModSceneEffect modSceneEffect)
	{
		LoaderManager.Get<SceneEffectLoader>().Register(modSceneEffect);
	}

	/// <summary>
	/// Is invoked when two or more modded SceneEffect layers are active within the same <see cref="Priority"/> group to attempt to determine which one should take precedence, if it matters.
	/// It's uncommon to have the need to assign a weight - you'd have to specifically believe that you don't need higher SceneEffectPriority, but do need to be the active SceneEffect within the priority you designated.
	/// Analogously, if SceneEffect were competing in a wrestling match, this would be how likely the SceneEffect should win within its weight class.
	/// Is intentionally bounded at a max of 100% (1) to reduce complexity. Defaults to 50% (0.5).
	/// Typical calculations may include: 1) how many tiles are present as a percentage of target amount; 2) how far away you are from the cause of the SceneEffect
	/// </summary>
	public virtual float GetWeight(Player player) => 0.5f;

	/// <summary>
	/// Combines Priority and Weight to determine what SceneEffect should be active.
	/// Priority is used to do primary sorting with respect to vanilla SceneEffect.
	/// Weight will be used if multiple SceneEffect have the same SceneEffectPriority so as to attempt to distinguish them based on their needs.
	/// </summary>
	internal float GetCorrWeight(Player player)
	{
		return Math.Max(Math.Min(GetWeight(player), 1), 0) + (float)Priority;
	}

	/// <summary>
	/// Return true to make the SceneEffect apply its effects (as long as its priority and weight allow that).
	/// </summary>
	public virtual bool IsSceneEffectActive(Player player) => false;

	/// <summary>
	/// Allows you to create special visual effects in the area around the player. For example, the Blood Moon's red filter on the screen or the Slime Rain's falling slime in the background. You must create classes that override <see cref="ScreenShaderData"/> or <see cref="CustomSky"/>, add them in a Load hook, then call <see cref="Player.ManageSpecialBiomeVisuals"/>. See the ExampleMod if you do not have access to the source code.
	/// <br/> This runs even if <see cref="IsSceneEffectActive"/> returns false. Check <paramref name="isActive"/> for the active status.
	/// </summary>
	public virtual void SpecialVisuals(Player player, bool isActive) { }

	/// <summary>
	/// Uses to customize the draw color of the map background (<see cref="MapBackground"/>) drawn on the fullscreen map. <see cref="MapBackgroundFullbright"/> can be used for typical effects, but this method can be used if further customization is needed.
	/// </summary>
	/// <param name="color">White or Main.ColorOfTheSkies depending on if above ground and MapBackgroundUsesSkyColor value.</param>
	public virtual void MapBackgroundColor(ref Color color) {
	} 
}
