using ExampleMod.Common.Systems;
using Microsoft.Xna.Framework;
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

		// Select all the scenery
		public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("ExampleMod/ExampleWaterStyle"); // Sets a water style for when inside this biome
		public override ModSurfaceBgStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBgStyle>("ExampleMod/ExampleSurfaceBgStyle");
		public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Mushroom;

		// Select Music
		public override int Music => Mod.GetSoundSlot(SoundType.Music, "Assets/Sounds/Music/MarbleGallery");

		// Populate the Bestiary Filter
		public override string BestiaryIcon => base.BestiaryIcon;
		public override string BackgroundPath => base.BackgroundPath;
		public override Color? BackgroundColor => base.BackgroundColor;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Surface");
		}

		public override bool IsBiomeActive(Player player) {
			return true;
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
