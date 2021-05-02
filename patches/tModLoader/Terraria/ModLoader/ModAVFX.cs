using System;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	/// <summary>
	/// ModAVFX is an abstract class that your classes can derive from. It serves as a container for handling exclusive AVFX content such as backgrounds, music, and water styling. 
	/// </summary>
	public abstract partial class ModAVFX : ModType
	{
		public int Type { get; internal set; }

		// AVFX properties
		public virtual ModWaterStyle WaterStyle => null;
		public virtual ModSurfaceBgStyle SurfaceBackgroundStyle => null;
		public virtual ModUgBgStyle UndergroundBackgroundStyle => null;
		public virtual int Music => -1;

		/// <summary>
		/// The <see cref="AVFXPriority"/> of this AVFX layer. Determines the relative postion compared to vanilla AVFX.
		/// Analogously, if AVFX were competing in a wrestling match, this would be the 'Weight Class' that this AVFX is competing in. 
		/// </summary>
		public virtual AVFXPriority Priority => AVFXPriority.None;

		/// <summary>
		/// Used to apply secondary colour shading for the capture camera. For example, darkening the background with the GlowingMushroom style.
		/// </summary>
		public virtual CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

		// Methods
		protected override void Register() {
			Type = Loaders.AVFXs.Register(this);
		}

		/// <summary>
		/// Forcefully registers the provided ModAVFX to Loaders.
		/// ModBiome and direct implementations call this.
		/// Does NOT cache the return type.
		/// </summary>
		internal void RegisterAVFX(ModAVFX modAVFX) {
			Loaders.AVFXs.Register(this);
		}

		/// <summary>
		/// Is invoked when two or more modded AVFX layers are active within the same <see cref="Priority"/> group to attempt to determine which one should take precedence, if it matters.
		/// It's uncommon that need to assign a weight - you'd have to specifically believe that you don't need higher AVFXPriority, but do need to be the active AVFX within the priority you designated
		/// Analogously, if AVFX were competing in a wrestling match, this would be how likely the AVFX should win within its weight class.
		/// Is intentionally bounded at a max of 100% (1) to reduce complexity. Defaults to 50% (0.5).
		/// Typical calculations may include: 1) how many tiles are present as a percentage of target amount; 2) how far away you are from the cause of the AVFX
		/// </summary>
		public virtual float GetWeight(Player player) => 0.5f;

		/// <summary>
		/// Combines Priority and Weight to determine what AVFX should be active. 
		/// Priority is used to do primary sorting with respect to vanilla AVFX. 
		/// Weight will be used if multiple AVFX have the same AVFXPriority so as to attempt to distinguish them based on their needs.
		/// </summary>
		internal float GetCorrWeight(Player player) {
			return Math.Max(Math.Min(GetWeight(player), 1), 0) + (float)Priority;
		}

		public virtual bool IsAVFXActive(Player player) => false;
	}
}
