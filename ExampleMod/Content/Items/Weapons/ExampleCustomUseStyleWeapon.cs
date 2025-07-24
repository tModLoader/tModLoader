using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// This item showcases a custom use style. A use style controls the movement and hitbox of an item when used.
	// This custom use style swings the sword up from below instead of the usual down swing of ItemUseStyleID.Swing.
	// In addition, this custom use style can swing at any angle. 
	// A separate example, ExampleCustomSwingSword, showcases an even more advanced custom swing using a held projectile instead of using custom use style code. It can be easier to implement advanced movements using a held projectile, but some may prefer the use style approach.
	public class ExampleCustomUseStyleWeapon : ModItem
	{
		// This controls how far out the weapon should be held from the hand. This weapon uses 0 but the logic in this example works for other values as well.
		private const int Offset = 0;

		public override void SetDefaults() {
			// Here, we set the Item's useStyle to a positive value that is NOT used by vanilla
			// We do this because we want the item to still be used, but we do not want Terraria to run logic for existing useStyles.
			Item.useStyle = 100;

			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.DamageType = DamageClass.Melee;
			Item.damage = 20;
			Item.width = 58;
			Item.height = 58;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;
		}

		// We use the UseStyle method to determine where the item will be drawn during the weapon animation
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			// Due to this use style being able to swing in any direction, we need to use a ModPlayer to store and sync the swing direction to properly sync the weapon animation.
			ExampleCustomUseStylePlayer useStylePlayer = player.GetModPlayer<ExampleCustomUseStylePlayer>();

			// Find how far through out swing we are, between 0 and 1 (0% to 100%)
			// player.itemAnimation starts at its highest value (player.itemAnimationMax), and ticks down to 0
			// When it hits 0, the player is (usually) finished with their item animation
			float percentDone = 1 - (float)player.itemAnimation / player.itemAnimationMax;

			// The total angle that the item will cover throughout its swing
			float swingArcRange = MathHelper.ToRadians(115);

			// When the animation starts, determine the swing direction. This code must only run on the local player since it involves the mouse cursor location.
			if (player.ItemAnimationJustStarted && player.whoAmI == Main.myPlayer) { 
				 // Calculate the angle towards the cursor. Note that this code properly handles reverse gravity.
				useStylePlayer.swingAngle = ((Main.MouseWorld - player.MountedCenter) * new Vector2(1, player.gravDir)).ToRotation();

				if (Main.netMode == NetmodeID.MultiplayerClient) {
					// Send this value to other players so they see the correct swing angle.
					useStylePlayer.SyncDirection(Main.myPlayer);
				}
			}

			// Set the player facing left or right depending on the target angle.
			player.direction = Utils.ToDirectionInt(useStylePlayer.swingAngle.ToRotationVector2().X >= 0);

			// Calculate start and end rotational values
			float start = useStylePlayer.swingAngle + (swingArcRange * .5f * player.direction);
			float end = useStylePlayer.swingAngle - (swingArcRange * .5f * player.direction);

			// and use them to calculate the current rotational value based on how long the weapon animation has been playing
			float currentAngle = MathHelper.Lerp(start, end, percentDone);

			// Here we set the rotation of the item. We need to add 45 degrees (PiOver4) because the weapon sprite is oriented that way. When facing left we add more rotation to account for the sprite being flipped.
			if (player.direction > 0) {
				player.itemRotation = currentAngle + MathHelper.PiOver4;
			}
			else {
				player.itemRotation = currentAngle + (MathHelper.PiOver4 * 3);
			}

			// Setting the player's primary arm so that it matches the swing animation
			//

			// Here we set the front arm drawing parameters. This uses the newer arm rendering approach.
			// The normal vanilla Swing doesn't use this approach and instead only uses the old approach of setting player.bodyFrame.Y during UseItemFrame.
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);
			// We could also use SetCompositeArmBack to set the drawing parameters of the other arm.

			// We set itemLocation to indicate where the item should be drawn
			player.itemLocation = player.MountedCenter + currentAngle.ToRotationVector2() * Offset;

			// This FlipItemLocationAndRotationForGravity method handles adjusting itemRotation and itemLocation to account for reversed gravity.
			player.FlipItemLocationAndRotationForGravity();
			/* This is what the method does:
			if (player.gravDir == -1f) {
				player.itemRotation = 0f - player.itemRotation;
				player.itemLocation.Y = player.position.Y + (float)player.height + (player.position.Y - player.itemLocation.Y);
			}
			*/
		}

		// We use UseItemHitbox to determine the hitbox of the item during the weapon animation
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			// Helper method to sort coordinates later
			(int, int) Order(float v1, float v2) {
				if (v1 < v2) {
					return ((int)v1, (int)v2);
				}

				return ((int)v2, (int)v1);
			}

			// Calculate the direction of the hand
			Vector2 handDirection = (player.compositeFrontArm.rotation + MathHelper.PiOver2).ToRotationVector2() * player.gravDir;

			// Calculate the distance from the handle to the tip of the weapon, taking into account item scaling effects.
			float itemLength = (Item.Size * player.GetAdjustedItemScale(Item)).Length();

			// Calculate the handle and tip positions
			Vector2 handlePosition = handDirection * Offset + player.MountedCenter;
			Vector2 tipPosition = handlePosition + handDirection * itemLength;

			// Now we use those values to create the item hitbox
			(int X1, int X2) = Order(handlePosition.X, tipPosition.X);
			(int Y1, int Y2) = Order(handlePosition.Y, tipPosition.Y);
			hitbox = new Rectangle(X1, Y1, X2 - X1, Y2 - Y1);
			hitbox.Inflate(1, 1); // Make the hitbox slightly bigger.
		}

		// We use UseItemFrame to drive the player animation during the weapon animation
		public override void UseItemFrame(Player player) {
			// Even though we are using SetCompositeArmFront to set the arm animation, we still need to set player.bodyFrame.Y to appropriate values to avoid visual issues in rare situations.
			// One example is animating attacks during the wolf transformation (Lilith's Necklace)
			player.bodyFrame.Y = player.bodyFrame.Height;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}

	// We need a corresponding ModPlayer to store and sync the swing angle because our custom use style uses the mouse location to determine the swing angle.
	// Without this, all other players will see the player swinging directly to the right.
	public class ExampleCustomUseStylePlayer : ModPlayer
	{
		public float swingAngle;

		public void SyncDirection(int whoAmI) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)ExampleMod.MessageType.SendCustomUseStylePlayerDirection);
			packet.Write((byte)whoAmI);
			packet.Write(swingAngle);
			packet.Send(ignoreClient: whoAmI);
		}

		public static void ReceiveDirection(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
			}

			ExampleCustomUseStylePlayer useStylePlayer = Main.player[player].GetModPlayer<ExampleCustomUseStylePlayer>();
			useStylePlayer.swingAngle = reader.ReadSingle();

			if (Main.netMode == NetmodeID.Server) {
				useStylePlayer.SyncDirection(player);
			}
		}
	}
}
