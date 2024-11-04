using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles.Banners
{
	// This tile is for enemy banners (https://terraria.wiki.gg/wiki/Banners_(enemy)). Several ModNPC in ExampleMod share an existing BannerID, but the enemies represented in this tile have their own.
	// When placed, this tile will provide bonus damage to specific BannerIDs. For individual enemies, a BannerID is usually the same as their NPCID, but some enemies share a BannerID with similar NPC.
	// This example uses an automated approach to implementing a banner tile to reduce code repetition, but comments show how to do things manually if the code shown is too advanced or confusing.
	// To support a new NPC, simply add an item texture to the Content/Items/Placeable/Banners folder, a tile sprite to Content/Tiles/Banners/EnemyBanner.png, set ModNPC.Banner and ModNPC.BannerItem on the ModNPC, and add an entry to EnemyBanner.StyleIDs.
	public class EnemyBanner : ModTile
	{
		public class StyleIDs {
			public const int ExampleWormHead = 0;
			public const int ExampleCustomAISlimeNPC = 1;
			public static readonly IdDictionary Search = IdDictionary.Create<StyleIDs, int>();
		}

		public enum StyleIDsEnum
		{
			ExampleWormHead, // TODO: "= 0", not forced, so this approach might end up with mistakes if modder rearranges?
			ExampleCustomAISlimeNPC
		}


		public override void SetStaticDefaults() {
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

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			if (TileObjectData.IsTopLeft(tile)) {
				// Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}
			// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
			return false;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY){
			// Due to MultiTileVine rendering the tile 2 tiles higher than expected for modded tiles using TileObjectData.DrawYOffset, we need to add 2 to fix the math for correct drawing
			offsetY += 2;
			return;
		}

		public override void NearbyEffects(int i, int j, bool closer) {
			// TODO: Porting notes: Old example used closer incorrectly. Tell everyone this was wrong!
			if (closer) {
				return;
			}

			// Calculate the tile place style, then map that place style to a BannerID.
			int tileStyle = TileObjectData.GetTileStyle(Main.tile[i, j]);
			int bannerID = EnemyBannerSystem.GetBannerID(tileStyle);

			/*
			// Mapping bannerID to tile style can be done manually.
			// Each item needs to know the tile style anyway, so a const or enum helps prevent bugs from typos.

			bannerID = tileStyle switch {
				StyleIDs.ExampleWormHead => ModContent.NPCType<ExampleWormHead>(),
				StyleIDs.ExampleCustomAISlimeNPC => ModContent.NPCType<ExampleCustomAISlimeNPC>(),
				_ => -1,
			};

			bannerID = (StyleIDsEnum)tileStyle switch {
				StyleIDsEnum.ExampleWormHead => ModContent.NPCType<ExampleWormHead>(),
				StyleIDsEnum.ExampleCustomAISlimeNPC => ModContent.NPCType<ExampleCustomAISlimeNPC>(),
				_ => -1,
			};
			*/

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
}
