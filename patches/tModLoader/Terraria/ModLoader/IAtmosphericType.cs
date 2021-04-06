using Microsoft.Xna.Framework;
using System;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	/// <summary>
	/// ModSystem is an abstract class that your classes can derive from. It contains general-use hooks, and, unlike Mod, can have unlimited amounts of types deriving from it. 
	/// </summary>
	public abstract partial class IAtmosphericType : ModType
	{
		/// <summary>
		/// Returns the strength to which the AtmosphericEffects are active. 0% < val < 200%. Defaults to 100% (ie 100) 
		/// </summary>
		public virtual byte GetWeight(Player player) => 100;

		public virtual bool IsActive(Player player) => false;

		/// <summary>
		/// Sets the category that this AtmosphericEffect will be prioritized under.
		/// </summary>
		public virtual AtmosphericPriority Priority => AtmosphericPriority.None;


		// Atmospheric properties
		public virtual ModWaterStyle WaterStyle => null;
		public virtual ModWaterfallStyle WaterfallStyle => null;

		public virtual ModSurfaceBgStyle SurfaceBackgroundStyle => null;

		public virtual ModUgBgStyle UndergroundBackgroundStyle => null;

		public virtual int Music => -1;


		// Capture Camera property
		public virtual CaptureBiome.TileColorStyle tileColorStyle => CaptureBiome.TileColorStyle.Normal;

		// Interface Methods
		internal int GetCorrWeight(Player player) {
			int corrWeight = Math.Min(GetWeight(player), (byte)200) + 200 * (byte)Priority;

			if (!IsActive(player)) {
				corrWeight = 0;
			}

			return corrWeight;
		}


		/// <summary>
		/// Registers the atmospheric objects associated to IAtmosphricType to AtmosphericLoader.
		/// ModBiome calls this automatically.
		/// </summary>
		public void RegisterAtmosphere() {
			AtmosphericLoader.atmosphericTypes.Add(this);
		}
	}
}
