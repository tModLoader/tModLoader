using ExampleMod.Common.Systems;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	//Shows setting up two basic biomes. For a more complicated example, please request.
	public class ExampleSurfaceBiome : ModBiome	
	{
		public override bool IsPrimaryBiome => true; // Allows this biome to impact NPC prices

		public override ModWaterStyle WaterStyle => base.WaterStyle; // Sets a water style for when inside this biome
		public override ModSurfaceBgStyle SurfaceBackgroundStyle => base.SurfaceBackgroundStyle;
		public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Mushroom;

		public override int Music => Mod.GetSoundSlot(SoundType.Music, "Assets/Sounds/Music/MarbleGallery");

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Surface");
		}

		public override bool IsBiomeActive(Player player) {
			return ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount >= 40 && Math.Abs(player.position.X - Main.maxTilesX / 2) < 40;
		}

		public override void ModifyShopPrices(HelperInfo helperInfo, ShopHelper shopHelperInstance) {
			switch (helperInfo.npc.type) {
				case NPCID.Merchant: // The merchant does not like the example surface biome
					shopHelperInstance.DislikeBiome(helperInfo.PrimaryPlayerBiome);
					break;
				case NPCID.Cyborg: // The cyborg loves this biome
					shopHelperInstance.LoveBiome(helperInfo.PrimaryPlayerBiome);
					break;
			}
		}
	}
}
