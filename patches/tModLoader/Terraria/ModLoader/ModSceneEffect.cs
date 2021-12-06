using System;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	/// <summary>
	/// ModSceneEffect is an abstract class that your classes can derive from. It serves as a container for handling exclusive SceneEffect content such as backgrounds, music, and water styling. 
	/// </summary>
	public abstract partial class ModSceneEffect : ModType
	{
		public int Type { get; internal set; }

		// SceneEffect properties
		public virtual ModWaterStyle WaterStyle => null;
		public virtual ModSurfaceBackgroundStyle SurfaceBackgroundStyle => null;
		public virtual ModUndergroundBackgroundStyle UndergroundBackgroundStyle => null;
		public virtual int Music => -1;

		/// <summary>
		/// The <see cref="SceneEffectPriority"/> of this SceneEffect layer. Determines the relative postion compared to vanilla SceneEffect.
		/// Analogously, if SceneEffect were competing in a wrestling match, this would be the 'Weight Class' that this SceneEffect is competing in. 
		/// </summary>
		public virtual SceneEffectPriority Priority => SceneEffectPriority.None;

		/// <summary>
		/// Used to apply secondary colour shading for the capture camera. For example, darkening the background with the GlowingMushroom style.
		/// </summary>
		public virtual CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

		// Methods
		protected override void Register() {
			Type = LoaderManager.Get<SceneEffectLoader>().Register(this);
		}

		/// <summary>
		/// Forcefully registers the provided ModSceneEffect to LoaderManager.
		/// ModBiome and direct implementations call this.
		/// Does NOT cache the return type.
		/// </summary>
		internal void RegisterSceneEffect(ModSceneEffect modSceneEffect) {
			LoaderManager.Get<SceneEffectLoader>().Register(this);
		}

		/// <summary>
		/// Is invoked when two or more modded SceneEffect layers are active within the same <see cref="Priority"/> group to attempt to determine which one should take precedence, if it matters.
		/// It's uncommon that need to assign a weight - you'd have to specifically believe that you don't need higher SceneEffectPriority, but do need to be the active SceneEffect within the priority you designated
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
		internal float GetCorrWeight(Player player) {
			return Math.Max(Math.Min(GetWeight(player), 1), 0) + (float)Priority;
		}

		public virtual bool IsSceneEffectActive(Player player) => false;
	}
}
