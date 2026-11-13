using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static class LiquidLoader
{
	private static int nextLiquid = LiquidID.Count;

	internal static readonly IList<ModLiquid> liquids = new List<ModLiquid>();

	internal static readonly IList<GlobalLiquid> globalLiquids = new List<GlobalLiquid>();

	private static bool loaded = false;

	public static int LiquidCount => nextLiquid;

	public static Asset<Texture2D>[] LiquidAssets = new Asset<Texture2D>[4];
	public static Asset<Texture2D>[] LiquidBlockAssets = new Asset<Texture2D>[4];
	public static Asset<Texture2D>[] LiquidSlopeAssets = new Asset<Texture2D>[4];

	internal static int ReserveLiquidID()
	{
		if (nextLiquid >= LiquidData.MaxLiquidTypes) {
			throw new Exception($"Cannot add more than {LiquidData.MaxLiquidTypes} liquid types");
		}

		int result = nextLiquid;
		nextLiquid++;
		return result;
	}

	/// <summary>
	/// Gets the ModLiquid instance with the given type. If no ModLiquid with the given type exists, returns null.
	/// </summary>
	public static ModLiquid GetLiquid(int type)
	{
		if (type < LiquidID.Count || type >= LiquidCount) {
			return null;
		}
		return liquids[type - LiquidID.Count];
	}

	internal static void ResizeArrays(bool unloading = false)
	{
		if (!Main.dedServ) {
			LiquidRenderer.Instance._cache = new LiquidRenderer.LiquidCache[1];
			LiquidRenderer.Instance._drawCache = new LiquidRenderer.LiquidDrawCache[1];
		}
		Array.Resize(ref LiquidAssets, nextLiquid);
		Array.Resize(ref LiquidBlockAssets, nextLiquid);
		Array.Resize(ref LiquidSlopeAssets, nextLiquid);
		Array.Resize(ref LiquidRenderer.WATERFALL_LENGTH, nextLiquid);
		Array.Resize(ref LiquidRenderer.DEFAULT_OPACITY, nextLiquid);
		Array.Resize(ref LiquidRenderer.WAVE_MASK_STRENGTH, nextLiquid);
		Array.Resize(ref LiquidRenderer.VISCOSITY_MASK, nextLiquid);

		LoaderUtils.ResetStaticMembers(typeof(LiquidID));

		if (!unloading) {
			loaded = true;
		}
	}

	internal static void Unload()
	{
		loaded = false;
		liquids.Clear();
		nextLiquid = LiquidID.Count;
		globalLiquids.Clear();
	}

	// Mainly taken from the vanilla bucket logic. TODO: Is this Player.tml.cs or LiquidLoader? How confident are we in this?
	// This is a seperate helper method as reduces the amount of repeated code in mods
	public static void PlaceLiquidViaBucket(Player player, Item item, int liquidType, bool isBottomless = false)
	{
		if (player.JustDroppedAnItem)// make sure the player hasn't just dropped an item recently
			return;

		if (player.whoAmI != Main.myPlayer) // make sure to only run on clients (we sync multiplayer data later down the line)
			return;

		// We make sure the player is in range to be able to place the liquid tile
		if (player.noBuilding ||
			!(player.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost <= (float)Player.tileTargetX) ||
			!((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f >= (float)Player.tileTargetX) ||
			!(player.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost <= (float)Player.tileTargetY) ||
			!((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)item.tileBoost - 2f >= (float)Player.tileTargetY)) {
			return;
		}

		// We set the cursor's item to be of the bucket item. To show the player that they are able to place a liquid tile at that coordinate normally
		if (!Main.GamepadDisableCursorItemIcon) {
			player.cursorItemIconEnabled = true;
			Main.ItemIconCacheUpdate(item.type);
		}

		// Make sure that the item is being used to place liquid
		if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem) {
			return;
		}

		// Get the tile position of the cursor
		Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
		if (tile.LiquidAmount >= 200) {
			return;
		}

		// Do additional tile check to make sure the target tile isn't a grate
		if (tile.HasUnactuatedTile) {
			bool[] tileSolid = Main.tileSolid;

			if (tileSolid[tile.TileType]) {
				bool[] tileSolidTop = Main.tileSolidTop;

				if (!tileSolidTop[tile.TileType] && tile.TileType != TileID.Grate) {
					return;
				}
			}
		}

		// We then check if the liquid at the position is ours and if the liquid amount isn't zero
		if (tile.LiquidAmount != 0) {
			if (tile.LiquidType != liquidType) {
				return;
			}
		}

		// After all of that, we are able to place the liquid 

		// In which we...
		SoundEngine.PlaySound(SoundID.SplashWeak, player.position); //...play a sound
		tile.LiquidType = liquidType; //...create a liquid tile...
		tile.LiquidAmount = byte.MaxValue; //...at full liquid capacity

		WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY); //...frame the tile to update the liquid

		if (!isBottomless) {
			item.stack--; //...remove the item's count
			player.PutItemInInventoryFromItemUsage(ItemID.EmptyBucket); //...create a bucket item
		}
		player.ApplyItemTime(item); //...do item usetime

		if (Main.netMode == NetmodeID.MultiplayerClient)//...lastly, if the game is MP, then sync the liquid placement
		{
			NetMessage.sendWater(Player.tileTargetX, Player.tileTargetY);
		}
	}

}
