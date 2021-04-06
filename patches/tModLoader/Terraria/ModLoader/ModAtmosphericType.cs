using System;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	/// <summary>
	/// ModAtmosphericType is an abstract class that your classes can derive from. It serves as a container for handling exclusive atmospheric content such as backgrounds, music, and water styling. 
	/// </summary>
	public abstract partial class ModAtmosphericType : ModType
	{
		// Atmospheric properties
		public virtual ModWaterStyle WaterStyle => null;
		public virtual ModSurfaceBgStyle SurfaceBackgroundStyle => null;
		public virtual ModUgBgStyle UndergroundBackgroundStyle => null;
		public virtual int Music => -1;
		public virtual AtmosphericPriority Priority => AtmosphericPriority.None;

		// Capture Camera property
		public virtual CaptureBiome.TileColorStyle tileColorStyle => CaptureBiome.TileColorStyle.Normal;

		// Interface Methods
		protected override void Register() {
			ModTypeLookup<ModAtmosphericType>.Register(this);
			RegisterAtmosphere(this);
		}

		/// <summary>
		/// Forcefully registers this ModAtmosphericType to AtmosphericLoader.
		/// ModBiome and direct implementations call this.
		/// </summary>
		internal void RegisterAtmosphere(ModAtmosphericType type) {
			AtmosphericLoader.atmosphericTypes.Add(type);
		}

		/// <summary>
		/// Returns the strength to which the AtmosphericEffects are active. 0% < val < 200%. Defaults to 100% (ie 100) 
		/// </summary>
		public virtual byte GetWeight(Player player) => 100;

		internal int GetCorrWeight(Player player) {
			if (!IsActive(player)) {
				return 0;
			}
			return Math.Min(GetWeight(player), (byte)200) + 200 * (byte)Priority;
		}

		public virtual bool IsActive(Player player) => false;
	}
}
