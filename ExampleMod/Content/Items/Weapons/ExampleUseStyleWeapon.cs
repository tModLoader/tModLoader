using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ExampleMod.Content.Items.Weapons
{
	
	public class ExampleUseStyleWeapon : ModItem
	{
		public override void SetDefaults() {
			// Here, we set the Item's UseStyle to a positive value that is NOT used by vanilla
			// We do this because we want the item to still be used, but we DO NOT want Terraria to use their UseStyle logic
			Item.useStyle = 15;

			// These will affect how fast the item is in this usestyle
			Item.useAnimation = 20;
			Item.useTime = 20;
			// This adds a brief (8 tick) delay to item being reused
			Item.reuseDelay = 8;

			// Item.useTurn allows the player to turn while the item is being used
			// Since we set the direction the player is facing with our usestyle, we leave this as false
			Item.useTurn = false;

			// Everything below this point is fairly typical
			Item.DamageType = DamageClass.Melee;
			Item.damage = 20;
			Item.width = 58;
			Item.height = 58;
			Item.knockBack = 2f;

			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			// Getting and using a modplayer to help with this usestyle
			UseStylePlayer modPlayer = player.GetModPlayer<UseStylePlayer>();

			// Find how far through out swing we are, between 0 and 1 (0% to 100%)
			// player.itemAnimation starts at its highest value (player.itemAnimationMax), and ticks down to 0
			// When it hits 0, the player is (usually) finished with their item animation
			float percentDone = 1 - (float)player.itemAnimation / (float)player.itemAnimationMax;

			// Keeps animation at completion state if item has a reuse delay
			if (player.reuseDelay == 0) {
				percentDone = 1f;
			}

			// This value represets the total angle that the item will cover throughout its swing
			float angle = MathHelper.ToRadians(115);

			// The following steps combine all the information we have now, and produce a behaviour as seen ingame

			// 1 ... Get the baseline rotation (angle to cursor)
			if (player.ItemAnimationJustStarted && player.whoAmI == Main.myPlayer) {
				modPlayer.direction = player.MountedCenter.AngleTo(Main.MouseWorld);
			}

			float baseAngle = modPlayer.direction;

			// Assign which direction the player should face
			// If the player 
			if ((modPlayer.direction - MathHelper.PiOver2 + MathHelper.TwoPi) % MathHelper.TwoPi < MathHelper.Pi) {
				player.direction = -1;
			}
			else {
				player.direction = 1;
			}

			// 2 ... Get the start and end rotational values
			// You can swap addition and subtraction here for the swing to rotate the opposite direction
			float start = baseAngle + (angle * .5f * player.direction);
			float end = baseAngle - (angle * .5f * player.direction);

			// 3 ... Given the previous 2 values, and the percent way through the animation...
			// Get the current rotational value
			float currentAngle = MathHelper.Lerp(start, end, percentDone);

			// 4 ... Finally, apply these maths to the item rotation and player arm
			// Item rotation.
			// Terraria applies a sprite-flipping effect when the player is facing another direction, which is accounted for here
			if (player.direction > 0) {
				player.itemRotation = currentAngle + MathHelper.PiOver4;
			}
			else {
				player.itemRotation = currentAngle + (MathHelper.PiOver4 * 3);
			}

			// Setting the player's primary arm so that it matches the swing animation
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);

			// 5 ... Set current item's location so it is drawn correctly
			player.itemLocation = player.MountedCenter + Vector2.UnitX.RotatedBy(currentAngle);
		}

		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			//Helper method to sort coordinates later
			(int, int) Order(float v1, float v2) {
				if (v1 < v2) {
					return ((int)v1, (int)v2);
				}

				return ((int)v2, (int)v1);
			}

			//Disabling the item's "melee" hitbox if the player's animation just started or ended
			if (player.reuseDelay == 0 || player.ItemAnimationJustStarted) {
				noHitbox = true;
			}

			//Get the direction of the weapon, and the distance from the player to the hilt
			Vector2 offset = this.HoldoutOffset() ?? Vector2.Zero;
			offset.X += 1f;
			float distance = offset.Length();
			Vector2 handPos = offset.RotatedBy(player.compositeFrontArm.rotation + MathHelper.PiOver2);

			//Use afforementioned direction, and get the distance from the player to the tip of the weapon
			float length = (Item.Size * player.GetAdjustedItemScale(Item)).Length();
			Vector2 endPos = handPos;

			//Use values obtained above to construct an approximation of where the hitbox should be
			handPos = handPos * distance;
			endPos = handPos + (endPos * length);
			handPos += player.MountedCenter;
			endPos += player.MountedCenter;

			//Use helper method to get coordinates and size for the rectangle
			(int X1, int X2) = Order(handPos.X, endPos.X);
			(int Y1, int Y2) = Order(handPos.Y, endPos.Y);

			//Here we get the coordinates of the new hitbox. For each of these, we add/subtract 2...
			//This is done to ensure there is never a hitbox with height or width 0.
			int topLeftX = X1 - 2;
			int topLeftY = Y1 - 2;
			int bottomRightX = X2 - X1 + 2;
			int bottomRightY = Y2 - Y1 + 2;

			//Create the new bounds of the hitbox
			hitbox = new Rectangle(topLeftX, topLeftY, bottomRightX, bottomRightY);
		}

		//Adding recipes so the item is obtainable ingame
		public override void AddRecipes() {
			CreateRecipe().AddIngredient(ItemID.GoldBroadsword, 2).AddIngredient<ExampleItem>(5).AddTile(TileID.WorkBenches).Register();
			CreateRecipe().AddIngredient(ItemID.PlatinumBroadsword, 2).AddIngredient<ExampleItem>(5).AddTile(TileID.WorkBenches).Register();
		}
	}

	public class UseStylePlayer : ModPlayer
	{
		public float direction;
	}
}
