using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// The classes in this file showcase making and using a custom use style. A use style controls the movement and hitbox of an item when used.
	// This custom use style swings the sword up from below instead of the usual down swing of ItemUseStyleID.Swing.
	// In addition, this custom use style can swing at any angle.
	// A separate example, ExampleCustomSwingSword, showcases an even more advanced custom swing using a held projectile instead of using custom use style code. It can be easier to implement advanced movements using a held projectile, but some may prefer the use style approach.

	// ExampleCustomUseStyleGlobalItem contains the actual custom UseStyle logic. It is possible to put custom use style code directly in a ModItem, but we use a GlobalItem in this example because we use the same custom use style for the vanilla Cutlass weapon as well.
	// ExampleCustomUseStyleItemSets stores extra data used by the custom item use style on a per-item type basis.
	// ExampleCustomUseStylePlayer facilitates our custom use style.

	public class ExampleCustomUseStyleWeapon : ModItem
	{
		public override void SetDefaults() {
			// Here, we set the Item's useStyle to the custom use style value registered in ExampleCustomUseStyleGlobalItem.
			// This will allow the custom logic in ExampleCustomUseStyleGlobalItem to run instead of the default logic for one of the existing useStyles.
			// If instead of sharing custom use style code in a GlobalItem we wanted to make a custom use style for just this item, we could instead set Item.useStyle to -1 and implement the UseStyle, UseItemHitbox, and UseItemFrame methods in this class.
			Item.useStyle = ExampleCustomUseStyleGlobalItem.ExampleCustomUseStyle;

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

		public override void SetStaticDefaults() {
			// This controls how far out the weapon should be held from the hand. This weapon uses 0 (so we don't actually need to set it) but the logic in ExampleCustomUseStyleGlobalItem works for other values as well. We can set it here or in the CreateIntSet method, both work.
			//ExampleCustomUseStyleItemSets.HandOffsets[Type] = 0;
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

	// See CustomItemSets.cs to learn more about ReinitializeDuringResizeArrays and working with custom item sets.
	[ReinitializeDuringResizeArrays]
	public static class ExampleCustomUseStyleItemSets
	{
		// Stores custom hold offsets if needed. Defaults to 0.
		// Cutlass, for example, is held slightly closer to the player.
		// ExampleCustomUseStyleWeapon is included here as an example even though it would default to 0 anyway.
		public static int[] HandOffsets = ItemID.Sets.Factory.CreateIntSet(0, ItemID.Cutlass, -6, ModContent.ItemType<ExampleCustomUseStyleWeapon>(), 0);
	}

	// This class contains the actual custom UseStyle logic.
	public class ExampleCustomUseStyleGlobalItem : GlobalItem
	{
		public static int ExampleCustomUseStyle;

		public override void Load() {
			// We register a custom use style ID in Load so that the value is set and ready to use in ModItem/GlobalItem.SetDefaults.
			ExampleCustomUseStyle = ItemLoader.RegisterUseStyle(Mod, "ExampleCustomUseStyle");
		}

		// Rather than checking item.useStyle in each method, we use AppliesToEntity to have the logic in this class only run for items set to use our custom use style.
		// Checking ItemID.Cutlass is necessary here because SetDefaults below won't run for Cutlass to change its Item.useStyle otherwise.
		public override bool AppliesToEntity(Item item, bool lateInstantiation) {
			return lateInstantiation && (item.type == ItemID.Cutlass || item.useStyle == ExampleCustomUseStyle);
		}

		public override void SetDefaults(Item item) {
			// Cutlass will now use out custom use style, making it swing up instead of the normal swing.
			if (item.type == ItemID.Cutlass) {
				item.useStyle = ExampleCustomUseStyle;
			}
		}

		public override void SetStaticDefaults() {
			//ExampleCustomUseStyleItemSets.HandOffsets[ItemID.Cutlass] = -6; // Alternate approach to setting HandOffsets
		}

		// We use the UseStyle method to determine where the item will be drawn during the weapon animation
		public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) {
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

			// Here we set the front arm drawing parameters. This uses the newer arm rendering approach.
			// The normal vanilla Swing doesn't use this approach and instead only uses the old approach of setting player.bodyFrame.Y during UseItemFrame.
			// The old approach has discrete arm positions while the new approach can draw the arm at any angle.
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);
			// We could also use SetCompositeArmBack to set the drawing parameters of the other arm.

			// We set itemLocation to indicate where the item should be drawn
			player.itemLocation = player.MountedCenter + currentAngle.ToRotationVector2() * new Vector2(ExampleCustomUseStyleItemSets.HandOffsets[item.type]);

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
		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
			// Calculate the direction of the hand
			Vector2 handDirection = (player.compositeFrontArm.rotation + MathHelper.PiOver2).ToRotationVector2() * player.gravDir;

			// Calculate the item hitbox. The hitbox parameter passed in already has item scale applied so recalculating it here is more flexible.
			Rectangle drawHitbox = Item.GetDrawHitbox(item.type, player);
			Vector2 itemSize = Main.dedServ ? new Vector2(32) : new Vector2(drawHitbox.Width, drawHitbox.Height);

			// Calculate the distance from the handle to the tip of the weapon, taking into account item scaling effects.
			float itemLength = (itemSize * player.GetAdjustedItemScale(item)).Length(); // We don't use Item.Size/width/height because those values are for the item hitbox when dropped.

			// Calculate the handle and tip positions
			Vector2 handlePosition = handDirection * new Vector2(ExampleCustomUseStyleItemSets.HandOffsets[item.type]) + player.MountedCenter;
			Vector2 tipPosition = handlePosition + handDirection * itemLength;

			// Now we use those values to create the item hitbox
			hitbox = Utils.CornerRectangle(handlePosition.ToPoint(), tipPosition.ToPoint());
			hitbox.Inflate(1, 1); // Make the hitbox slightly bigger.
		}

		// We use UseItemFrame to drive the player animation during the weapon animation
		public override void UseItemFrame(Item item, Player player) {
			// Even though we are using SetCompositeArmFront to set the arm animation, we still need to set player.bodyFrame.Y to appropriate values to avoid visual issues in rare situations.
			// One example is animating attacks during the wolf transformation (Lilith's Necklace)
			player.bodyFrame.Y = player.bodyFrame.Height;
		}
	}
}
