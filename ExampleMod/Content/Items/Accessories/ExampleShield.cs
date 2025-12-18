using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Shield)] // Load the spritesheet you create as a shield for the player when it is equipped.
	public class ExampleShield : ModItem
	{
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.value = Item.buyPrice(10);
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;

			Item.defense = 1000;
			Item.lifeRegen = 10;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetDamage(DamageClass.Generic) += 1f; // Increase ALL player damage by 100%
			player.endurance = 1f - (0.1f * (1f - player.endurance));  // The percentage of damage reduction
			player.GetModPlayer<ExampleDashPlayer>().dashAccessoryEquipped = true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}

	public class ExampleDashPlayer : ModPlayer
	{
		public bool dashAccessoryEquipped;

		private const int dashCooldown = 45; // Time before the player can dash again after a dash
		private int dashCooldownTimer = 0; // Time remaining until the player can dash again
		private const float dashVelocity = 10f; // The initial velocity of the dash

		private int dashTime = 0; // The time the player has to input the second press of left/right. Necessary for Player.DoCommonDashHandle.

		public override void ResetEffects() {
			// Reset our equipped flag
			dashAccessoryEquipped = false;
		}

		// PostDash is called right after vanilla dash movement, so we should handle our modded dash functionality here.
		public override void PostDash() {
			if (dashAccessoryEquipped) {
				// If we are on cooldown, decrement the cooldown timer
				if (dashCooldownTimer > 0) dashCooldownTimer--;

				// If we can dash, handle player dash input
				if (CanDash())
					Player.DoCommonDashHandle(out int dir, out bool dashing, ref dashTime, DoExampleDash);
			}
		}

		// Called by Player.DoCommonDashHandle when a dash is started, since we passed it as an argument
		private void DoExampleDash(int direction) {
			dashCooldownTimer = dashCooldown; // Set the cooldown timer

			// Give player horizontal velocity in the appropriate direction
			Vector2 newVelocity = Player.velocity;
			newVelocity.X = direction * dashVelocity;
			Player.velocity = newVelocity;

			// Here you'd be able to set an effect that happens when the dash first activates
			// Some examples include:  the larger smoke effect from the Master Ninja Gear and Tabi
		}

		private bool CanDash() {
			return dashAccessoryEquipped
				&& Player.dashType == DashID.None // player doesn't have a vanilla dash equipped (give priority to those dashes)
				&& dashCooldownTimer <= 0 // dash is not on cooldown
				&& !Player.mount.Active; // player isn't mounted, since dashes on a mount look weird
		} 
	}
}