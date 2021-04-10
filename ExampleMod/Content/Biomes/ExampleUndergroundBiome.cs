using ExampleMod.Common.Systems;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	public class ExampleUndergroundBiome : ModBiome
	{
		public override ModUgBgStyle UndergroundBackgroundStyle => ModContent.Find<ModUgBgStyle>("ExampleMod/ExampleUgBgStyle");

		public override int Music => Mod.GetSoundSlot(SoundType.Music, "Assets/Sounds/Music/MarbleGallery");

		public override AVFXPriority Priority => AVFXPriority.BiomeHigh;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Underground");
		}

		public override bool IsBiomeActive(Player player) {
			return false;
			return ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount >= 40 && Math.Abs(player.position.X - Main.maxTilesX / 2) < 40;
		}

		public override byte GetWeight(Player player) {
			return (byte)Math.Max(0, 100 - Math.Abs(player.position.X - Main.maxTilesX / 2));
		}
	}
}
