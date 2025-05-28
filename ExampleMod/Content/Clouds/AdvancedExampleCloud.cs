using ExampleMod.Content.Biomes;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Content.Clouds
{
	// This class showcases advanced usage of a modded cloud. A ModCloud class is only needed for clouds with custom logic.
	// Typical clouds can be autoloaded automatically from any "Clouds" folder or manually loaded via CloudLoader.AddCloudFromTexture method for greater control.
	public class AdvancedExampleCloud : ModCloud
	{
		public override bool RareCloud => true;

		public override float SpawnChance() {
			// If this cloud represents a defeated boss, this would be a typical approach
			// return Common.Systems.DownedBossSystem.downedMinionBoss ? 1f : 0f;

			if (!Main.gameMenu && Main.LocalPlayer.InModBiome<ExampleSurfaceBiome>()) {
				return 10f;
			}
			// Since this is a rare cloud, remember that this default spawn chance value is in relation to other rare clouds. If you would like to force this cloud to be more common to see it in action, change RareCloud above to false.
			return 1f;
		}

		public override void OnSpawn(Cloud cloud) {
			// AdvancedExampleCloud.png has text, so we need to force the cloud to not be flipped once spawned.
			cloud.spriteDir = SpriteEffects.None;
		}

		public override bool Draw(SpriteBatch spriteBatch, Cloud cloud, int cloudIndex, ref DrawData drawData) {
			// Manual draw code can happen here. This example draws an after-image while in ExampleSurfaceBiome.
			if (!Main.gameMenu && Main.LocalPlayer.InModBiome<ExampleSurfaceBiome>()) {
				var drawDataCopy = drawData;
				drawDataCopy.color *= 0.5f;
				drawDataCopy.position += Utils.NextVector2Circular(Main.rand, 5, 5);
				drawDataCopy.Draw(spriteBatch);
			}
			return true;
		}
	}
}
