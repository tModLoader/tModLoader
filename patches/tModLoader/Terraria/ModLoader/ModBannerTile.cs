using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Terraria.ModLoader;

/// <summary>
/// Extension to <seealso cref="ModTile"/> that streamlines the process of creating an enemy banner tile. Behaves the same as <see cref="TileID.Banners"/> except it does not set StyleWrapLimit to 111.
/// <para/> Handles applying banner buffs for <see cref="ModNPC"/>. Each <see cref="ModNPC"/> should call <see cref="RegisterStyle(int, int)"/> in <see cref="ModNPC.SetDefaults"/> to register a mapping of tile placement style to BannerID. If multiple ModNPC share a BannerID, only the representative NPC needs to call RegisterStyle.
/// </summary>
public abstract class ModBannerTile : ModTile
{
	private Dictionary<int, int> tileStyleToBannerIDMapping = new();

	/// <summary>
	/// Registers a tile place style to a BannerID. Necessary to facilitate automatically applying enemy banner buffs.
	/// </summary>
	public void RegisterStyle(int tileStyle, int bannerID) => tileStyleToBannerIDMapping[tileStyle] = bannerID;

	/// <summary>
	/// Given an tile place style, returns the corresponding BannerID. Returns -1 if not found.
	/// </summary>
	public int GetBannerID(int tileStyle) => tileStyleToBannerIDMapping.TryGetValue(tileStyle, out var id) ? id : -1;

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.MultiTileSway[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0);
		// TODO: Note: Porting Notes: added | AnchorType.PlanterBox since 1.3
		// TODO: Make note of changes: Vanilla banners don't flip, but 1.3 example used SetSpriteEffects to flip

		// TODO: Note: Porting Notes: All this is new:
		TileObjectData.newTile.DrawYOffset = -2; // Draw this tile 2 pixels up, allowing the banner pole to align visually with the bottom of the tile it is anchored to.

		// This alternate placement supports placing on un-hammered platform tiles. Note how the DrawYOffset accounts for the height adjustment needed for the tile to look correctly attached.
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, TileObjectData.newTile.Width, 0);
		TileObjectData.newAlternate.DrawYOffset = -10;
		TileObjectData.addAlternate(0);

		TileObjectData.addTile(Type);

		DustType = -1; // No dust when mined
		AddMapEntry(new Color(13, 88, 130), Language.GetText("MapObject.Banner"));
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		if (TileObjectData.IsTopLeft(tile)) {
			// Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
		}
		// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
		return false;
	}

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
	{
		// Due to MultiTileVine rendering the tile 2 tiles higher than expected for modded tiles using TileObjectData.DrawYOffset, we need to add 2 to fix the math for correct drawing
		offsetY += 2;
		return;
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		// TODO: Porting notes: Old example used closer incorrectly. Tell everyone this was wrong!
		if (closer) {
			return;
		}

		// Calculate the tile place style, then map that place style to a BannerID.
		int tileStyle = TileObjectData.GetTileStyle(Main.tile[i, j]);

		int itemType2 = TileLoader.GetItemDropFromTypeAndStyle(Type, tileStyle);
		int bannerID2 = NPCLoader.itemToBanner[itemType2];

		int bannerID = GetBannerID(tileStyle);

		if (bannerID == -1) {
			return;
		}

		int itemType = Item.BannerToItem(bannerID);

		// Once the BannerID and Item type have been calculated, we apply the banner buff
		if (ItemID.Sets.BannerStrength.IndexInRange(itemType) && ItemID.Sets.BannerStrength[itemType].Enabled) {
			Main.SceneMetrics.NPCBannerBuff[bannerID] = true;
			Main.SceneMetrics.hasBanner = true;
		}
	}
}
