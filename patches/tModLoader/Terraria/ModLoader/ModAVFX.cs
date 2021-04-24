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
		public virtual AVFXPriority Priority => AVFXPriority.None;

		// Capture Camera property
		public virtual CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

		// Methods
		protected override void Register() {
			Type = Loaders.AVFXs.Register(this);
		}

		/// <summary>
		/// Forcefully registers the provided ModAVFX to Loaders.
		/// ModBiome and direct implementations call this.
		/// Does NOT cache the return type
		/// </summary>
		internal void RegisterAVFX(ModAVFX modAVFX) {
			Loaders.AVFXs.Register(this);
		}

		/// <summary>
		/// Returns the strength to which the AVFXEffects are active. 0% < val < 200%. Defaults to 100% (ie 100) 
		/// </summary>
		public virtual byte GetWeight(Player player) => 100;

		internal int GetCorrWeight(Player player) {
			return Math.Min(GetWeight(player), (byte)200) + 200 * (byte)Priority;
		}

		public virtual bool IsActive(Player player) => false;
	}
}
