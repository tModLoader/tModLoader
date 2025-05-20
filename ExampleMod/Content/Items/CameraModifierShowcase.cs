using ExampleMod.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	// Showcases a usage of ExampleCameraModifier.cs in-game.
	public class CameraModifierShowcase : ModItem
	{
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.JimsDrone}";

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 300;
			Item.useTime = 300;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = false;
			Item.useTurn = false;
		}

		public override bool? UseItem(Player player) {
			// Pans the camera targeting the current mouse location, lasting 300 frames
			Main.instance.CameraModifiers.Add(new ExampleCameraModifier(Main.MouseWorld, 300, FullName));
			return true;
		}
	}
}
