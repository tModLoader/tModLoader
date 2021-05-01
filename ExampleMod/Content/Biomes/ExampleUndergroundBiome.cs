using ExampleMod.Common.Systems;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	public class ExampleUndergroundBiome : ModBiome
	{
		// Select all the scenery
		public override ModUgBgStyle UndergroundBackgroundStyle => ModContent.Find<ModUgBgStyle>("ExampleMod/ExampleUgBgStyle");

		// Select Music
		public override int Music => Mod.GetSoundSlot(SoundType.Music, "Assets/Sounds/Music/MarbleGallery.ogg");

		// Sets how the AVFX associated with this biome will be displayed with respect to vanilla AVFX. For more information see AVFXPriority & its values.
		public override AVFXPriority Priority => AVFXPriority.BiomeLow; // We have set the AVFXPriority to be BiomeLow for purpose of example, however default behavour is BiomeLow.

		// Populate the Bestiary Filter
		public override string BestiaryIcon => base.BestiaryIcon;
		public override string BackgroundPath => base.BackgroundPath;
		public override Color? BackgroundColor => base.BackgroundColor;

		// Use SetStaticDefaults to assign the display name
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Underground");
		}

		// Calculate when the biome is active.
		public override bool IsBiomeActive(Player player) {
			// Limit the biome height to be underground in either rock layer or dirt layer
			return (player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight) && 
				// Check how many tiles of our biome are present, such that biome should be active
				ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount >= 40 && 
				// Limit our biome to be in only the horizontal center third of the world.
				Math.Abs(player.position.ToTileCoordinates().X - Main.maxTilesX / 2) < Main.maxTilesX / 6;
		}

		// In the event that both our biome AND one or more modded AVFX layers are active with the same AVFXPriority, 
		// this can be used to differentiate whether or not our AVFX should be displayed. See the GetWeight documentation.
		public override byte GetWeight(Player player) {
			// We declare that our biome should have be more likely than not to be active if at center of world, and decreases in need to be active as player gets further away.
			return (byte)Math.Max(0, 150 - Math.Abs(player.position.ToTileCoordinates().X - Main.maxTilesX / 2)); 
		}
	}
}
