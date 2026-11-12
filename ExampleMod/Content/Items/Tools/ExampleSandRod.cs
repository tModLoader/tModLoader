using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	// ExampleSandRod is a sand version of Dirt Rod
	// It can be used to move different sand blocks (including ExampleSand) around
	// To implement a tile relocating rod for other types of blocks, you'll need to make a custom projectile for them
	public class ExampleSandRod : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
		}

		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 28;

			// Copied from Dirt Rod
			Item.channel = true;
			Item.knockBack = 5f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item8;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.rare = ItemRarityID.Blue;
			Item.noMelee = true;
			Item.value = Item.buyPrice(gold: 5);
		}

		public override bool CanUseItem(Player player) {
			if (player.whoAmI != Main.myPlayer) {
				return true;
			}

			// Calculate the tile position where the mouse is on
			Point tilePos = Main.MouseWorld.ToTileCoordinates();
			Tile tile = Main.tile[tilePos];

			if (!tile.HasTile) {
				return false;
			}

			// If the tile is not sand, the item will not be used
			if (!Main.tileSand[tile.TileType]) {
				return false;
			}

			// Get which projectile the tile will create when falling
			if (TileID.Sets.FallingBlockProjectile[tile.TileType] is not TileID.Sets.FallingBlockProjectileInfo data) {
				return false;
			}

			// Try to kill the tile without item dropping
			WorldGen.KillTile(tilePos.X, tilePos.Y, noItem: true);

			// If for some reason the tile can't be killed, don't use the item
			if (Main.tile[tilePos].HasTile) {
				return false;
			}

			// If the tile was killed successfully,
			// set Item.shoot to data.FallingProjectileType to create the projectile corresponding to the killed tile
			Item.shoot = data.FallingProjectileType;

			// If it is on the multiplayer client, sync the tile destruction to the server
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				// 4 corresponds to the "KillTile (No Item)" message
				NetMessage.SendData(MessageID.TileManipulation, number: 4, number2: tilePos.X, number3: tilePos.Y);
			}

			return true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Make sure the spawn position of projectile is the same as mouse
			position = Main.MouseWorld;
			player.LimitPointToPlayerReachableArea(ref position);
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
