using ExampleMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
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
			return 1f;
		}

		public override void OnSpawn(Cloud cloud) {
			// AdvancedExampleCloud.png has text, so we need to force the cloud to not be flipped once spawned.
			cloud.spriteDir = SpriteEffects.None;
		}

		public override bool Draw(SpriteBatch spriteBatch, Vector2 position, ref Color color, int cloudIndex) {
			Cloud cloud = Main.cloud[cloudIndex];

			if (Main.rand.NextBool(2)) {
				spriteBatch.Draw(
					TextureAssets.Cloud[cloud.type].Value,
					position,
					new Rectangle(0, 0, TextureAssets.Cloud[cloud.type].Width(), TextureAssets.Cloud[cloud.type].Height()),
					color /** num7*/,
					cloud.rotation,
					new Vector2((float)TextureAssets.Cloud[cloud.type].Width() * 0.5f, (float)TextureAssets.Cloud[cloud.type].Height() * 0.5f),
					cloud.scale,
					cloud.spriteDir,
					0f);
				return false;
			}

			return true;
		}
	}
}
