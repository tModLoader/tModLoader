using ExampleMod.Backgrounds;
using ExampleMod.Common.Systems;
using ExampleMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	// Shows setting up two basic biomes. For a more complicated example, please request.
	public class ExampleSurfaceBiome : ModBiome
	{
		// Select all the scenery
		public override ModWaterStyle WaterStyle => ModContent.GetInstance<ExampleWaterStyle>(); // Sets a water style for when inside this biome
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<ExampleSurfaceBackgroundStyle>();
		public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Crimson;

		// Select Music
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MysteriousMystery");
		/* If you need the music choice to be conditional, such as supporting the Otherworld soundtrack toggle, you can use this approach:
		public override int Music {
			get {
				if (!Main.swapMusic == Main.drunkWorld && !Main.remixWorld) {
					return MusicID.OtherworldlyEerie;
				}
				return MusicLoader.GetMusicSlot(Mod, "Assets/Music/MysteriousMystery");
			}
		}
		*/

		public override int BiomeTorchItemType => ModContent.ItemType<ExampleTorch>();
		public override int BiomeCampfireItemType => ModContent.ItemType<ExampleCampfire>();

		// Populate the Bestiary Filter
		public override string BestiaryIcon => base.BestiaryIcon;
		public override string BackgroundPath => base.BackgroundPath;
		public override Color? BackgroundColor => base.BackgroundColor;
		public override string MapBackground => BackgroundPath; // Re-uses Bestiary Background for Map Background

		// Calculate when the biome is active.
		public override bool IsBiomeActive(Player player) {
			// First, we will use the exampleBlockCount from our added ModSystem for our first custom condition
			bool b1 = ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount >= 40;

			// Second, we will limit this biome to the inner horizontal third of the map as our second custom condition
			bool b2 = Math.Abs(player.position.ToTileCoordinates().X - Main.maxTilesX / 2) < Main.maxTilesX / 6;

			// Finally, we will limit the height at which this biome can be active to above ground (ie sky and surface). Most (if not all) surface biomes will use this condition.
			bool b3 = player.ZoneSkyHeight || player.ZoneOverworldHeight;
			return b1 && b2 && b3;
		}

		// Declare biome priority. The default is BiomeLow so this is only necessary if it needs a higher priority.
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
	}
}
