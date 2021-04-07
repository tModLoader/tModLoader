using System;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	/// <summary>
	/// ModAVFX is an abstract class that your classes can derive from. It serves as a container for handling exclusive AVFX content such as backgrounds, music, and water styling. 
	/// </summary>
	public abstract partial class ModAVFX : ModType
	{
		// AVFX properties
		public virtual ModWaterStyle WaterStyle => null;
		public virtual ModSurfaceBgStyle SurfaceBackgroundStyle => null;
		public virtual ModUgBgStyle UndergroundBackgroundStyle => null;
		public virtual int Music => -1;
		public virtual AVFXPriority Priority => AVFXPriority.None;

		// Capture Camera property
		public virtual CaptureBiome.TileColorStyle tileColorStyle => CaptureBiome.TileColorStyle.Normal;

		// Interface Methods
		protected override void Register() {
			ModTypeLookup<ModAVFX>.Register(this);
			RegisterAtmosphere(this);
		}

		/// <summary>
		/// Forcefully registers this ModAVFX to AVFXLoader.
		/// ModBiome and direct implementations call this.
		/// </summary>
		internal void RegisterAtmosphere(ModAVFX type) {
			AVFXLoader.AVFXs.Add(type);
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
