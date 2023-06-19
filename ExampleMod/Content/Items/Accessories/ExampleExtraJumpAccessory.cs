using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;

namespace ExampleMod.Content.Items.Accessories
{
	// Showcases a basic extra jump
	public class ExampleExtraJumpAccessory : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(silver: 50));
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetExtraJump<SimpleExtraJump>().Active = true;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(20)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}

	public class SimpleExtraJump : ExtraJump
	{
		public override Position GetDefaultPosition() => new After(BlizzardInABottle);

		public override IEnumerable<Position> GetModdedConstraints() {
			// By default, modded extra jumps set to be between two vanilla extra jumps (via After and Before) are ordered in load order.
			// This hook allows you to organize where this extra jump is located relative to other modded extra jumps that are also
			// placed between the same two vanila extra jumps.
			yield return new Before(ModContent.GetInstance<MultipleUseExtraJump>());
		}

		public override float GetJumpDuration(Player player) {
			// Use this hook to set the duration of the extra jump
			// The XML summary for this hook mentions the values used by the vanilla extra jumps
			return 2.25f;
		}

		public override void ModifyHorizontalSpeeds(Player player) {
			// Use this hook to modify "player.runAcceleration" and "player.maxRunSpeed"
			// The XML summary for this hook mentions the values used by the vanilla extra jumps
			player.runAcceleration *= 1.75f;
			player.maxRunSpeed *= 2f;
		}

		public override void OnJumpStarted(Player player, ref bool playSound) {
			// Use this hook to trigger effects that should appear at the start of the extra jump
			// This example mimicks the logic for spawning the puff of smoke from the Cloud in a Bottle
			int offsetY = player.height;
			if (player.gravDir == -1f)
				offsetY = 0;

			offsetY -= 16;

			for (int i = 0; i < 10; i++) {
				Dust dust = Dust.NewDustDirect(player.position + new Vector2(-34f, offsetY), 102, 32, DustID.Cloud, -player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 100, Color.Gray, 1.5f);
				dust.velocity = dust.velocity * 0.5f - player.velocity * new Vector2(0.1f, 0.3f);
			}

			Gore gore = Gore.NewGoreDirect(player.GetSource_FromThis(), player.Top + new Vector2(-16f, offsetY), -player.velocity, Main.rand.Next(11, 14));
			gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
			gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f;

			gore = Gore.NewGoreDirect(player.GetSource_FromThis(), player.position + new Vector2(-36f, offsetY), -player.velocity, Main.rand.Next(11, 14));
			gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
			gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f;

			gore = Gore.NewGoreDirect(player.GetSource_FromThis(), player.TopRight + new Vector2(4f, offsetY), -player.velocity, Main.rand.Next(11, 14));
			gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
			gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f;
		}

		public override void JumpVisuals(Player player) {
			// Use this hook to trigger effects that should appear throughout the duration of the extra jump
			// This example mimics the logic for spawning the dust from the Blizzard in a Bottle
			int offsetY = player.height - 6;
			if (player.gravDir == -1f)
				offsetY = 6;

			Vector2 spawnPos = new Vector2(player.position.X, player.position.Y + offsetY);

			for (int i = 0; i < 2; i++) {
				Dust dust = Dust.NewDustDirect(spawnPos, player.width, 12, DustID.Snow, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, newColor: Color.Gray);
				dust.velocity *= 0.1f;
				if (i == 0)
					dust.velocity += player.velocity * 0.03f;
				else
					dust.velocity -= player.velocity * 0.03f;

				dust.velocity -= player.velocity * 0.1f;
				dust.noGravity = true;
				dust.noLight = true;
			}

			for (int i = 0; i < 3; i++) {
				Dust dust = Dust.NewDustDirect(spawnPos, player.width, 12, DustID.Snow, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, newColor: Color.Gray);
				dust.fadeIn = 1.5f;
				dust.velocity *= 0.6f;
				dust.velocity += player.velocity * 0.8f;
				dust.noGravity = true;
				dust.noLight = true;
			}

			for (int i = 0; i < 3; i++) {
				Dust dust = Dust.NewDustDirect(spawnPos, player.width, 12, DustID.Snow, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, newColor: Color.Gray);
				dust.fadeIn = 1.5f;
				dust.velocity *= 0.6f;
				dust.velocity -= player.velocity * 0.8f;
				dust.noGravity = true;
				dust.noLight = true;
			}
		}
	}
}
