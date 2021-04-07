using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	//Shows setting up two basic biomes. For a more complicated example, please request.
	public class ExampleSurfaceBiome : ModBiome	
	{
		public override bool IsPrimaryBiome => true;
		public override ModWaterStyle WaterStyle => base.WaterStyle;
		public override ModSurfaceBgStyle SurfaceBackgroundStyle => base.SurfaceBackgroundStyle;
		public override CaptureBiome.TileColorStyle tileColorStyle => CaptureBiome.TileColorStyle.Mushroom;
		public override int Music => base.Music;

		public override bool IsBiomeActive(Player player) {
			return Math.Abs(player.position.X - Main.maxTilesX / 2) < 40;
		}

		public override void ModifyShopPrices(HelperInfo helperInfo, ShopHelper shopHelperInstance) {
			switch (helperInfo.npc.type) {
				case 17:
					shopHelperInstance.DislikeBiome(helperInfo.PrimaryPlayerBiome);
					break;
				case 209:
					shopHelperInstance.LoveBiome(helperInfo.PrimaryPlayerBiome);
					break;
			}
		}
	}

	public class ExampleUndergroundBiome : ModBiome
	{
		public override ModUgBgStyle UndergroundBackgroundStyle => base.UndergroundBackgroundStyle;

		public override int Music => base.Music;

		public override AtmosphericPriority Priority => AtmosphericPriority.BiomeHigh;

		public override bool IsBiomeActive(Player player) {
			return Math.Abs(player.position.X - Main.maxTilesX / 2) < 40;
		}

		public override byte GetWeight(Player player) {
			return (byte)Math.Max(0, 100 - Math.Abs(player.position.X - Main.maxTilesX / 2));
		}
	}
}
