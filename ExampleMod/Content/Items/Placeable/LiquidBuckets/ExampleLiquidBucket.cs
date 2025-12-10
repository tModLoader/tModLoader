using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.LiquidBuckets
{
	//This is an example of a modded bucket
	//Here we do some extra logic to make our bucket dispense liquid
	//While also using an ID Set to allow buckets to create this item
	public class ExampleLiquidBucket : ModItem
	{
		//The SetStaticDefaults of a bucket
		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
			ItemID.Sets.AlsoABuildingItem[Type] = true; //Unused ID Set, but may be useful for other modders or when the game updates
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ExampleBottomlessBucket>();
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;

			//Here is how we make buckets create this item
			//Using the CreateLiquidBucketItem ID Set, we are able to set which liquid creates an item as well as which item it creates
			//NOTE: a liquid can only create 1 bucket item at a time
			LiquidID.Sets.CreateLiquidBucketItem[ModContent.LiquidType<ExampleLiquid>()] = Type;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
		}

		//The normal SetDefaults for a bucket
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 24;
			Item.maxStack = Item.CommonMaxStack;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		//We use HoldItem for bucket logic, as it's the hook ran just before the bucket logic is done.
		//This is so no other extra item logic is run inbetween this hook and when we would normally have bucket logic called
		public override void HoldItem(Player player) {
			PlaceLiquid(player, Item, ModContent.LiquidType<ExampleLiquid>());
		}

		//Here is how we do the bucket logic. Calculating when the player is able to place a liquid tile or not. Mainly taken from the vanilla bucket logic
		//This is a seperate method as this mod contains multiple bucket items, and this reduces the amount of repeated code
		public static void PlaceLiquid(Player player, Item item, int liquidType, bool isBottomless = false) {
			if (!player.JustDroppedAnItem)//make sure the player hasn't just dropped an item recently
			{
				if (player.whoAmI != Main.myPlayer) //make sure to only run on clients (we sync multiplayer data later down the line)
				{
					return;
				}
				//We make sure the player is in range to be able to place the liquid tile
				if (player.noBuilding ||
					!(player.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost <= (float)Player.tileTargetX) ||
					!((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f >= (float)Player.tileTargetX) ||
					!(player.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost <= (float)Player.tileTargetY) ||
					!((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)item.tileBoost - 2f >= (float)Player.tileTargetY)) {
					return;
				}
				//We set the cursor's item to be of the bucket item. To show the player that they are able to place a liquid tile at that coordinate normally
				if (!Main.GamepadDisableCursorItemIcon) {
					player.cursorItemIconEnabled = true;
					Main.ItemIconCacheUpdate(item.type);
				}
				//Make sure that the item is being used to place liquid
				if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem) {
					return;
				}
				//Get the tile position of the cursor
				Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
				if (tile.LiquidAmount >= 200) {
					return;
				}
				//Do additional tile check to make sure the target tile isn't a grate
				if (tile.HasUnactuatedTile) {
					bool[] tileSolid = Main.tileSolid;
					if (tileSolid[tile.TileType]) {
						bool[] tileSolidTop = Main.tileSolidTop;
						if (!tileSolidTop[tile.TileType]) {
							if (tile.TileType != TileID.Grate) {
								return;
							}
						}
					}
				}
				//We then check if the liquid at the position is ours and if the liquid amount isn't zero
				if (tile.LiquidAmount != 0) {
					if (tile.LiquidType != liquidType) {
						return;
					}
				}

				//After all of that, we are able to place the liquid 
				//In which we...
				SoundEngine.PlaySound(SoundID.SplashWeak, player.position); //...play a sound
				tile.LiquidType = liquidType; //...create a liquid tile...
				tile.LiquidAmount = byte.MaxValue; //...at full liquid capacity
				WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY); //...frame the tile to update the liquid
				if (!isBottomless) {
					item.stack--; //...remove the item's count
					player.PutItemInInventoryFromItemUsage(ItemID.EmptyBucket, player.selectedItem); //...create a bucket item
				}
				player.ApplyItemTime(item); //...do item usetime

				if (Main.netMode == NetmodeID.MultiplayerClient)//...lastly, if the game is MP, then sync the liquid plavement
				{
					NetMessage.sendWater(Player.tileTargetX, Player.tileTargetY);
				}
			}
		}
	}
}
