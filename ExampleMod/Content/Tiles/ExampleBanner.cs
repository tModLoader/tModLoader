using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Tiles
{
	public class ExampleBanner : ModTile
	{
		// TODO: Dictionary mapping from style to Item/NPC?

		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0); // Note: added | AnchorType.PlanterBox since 1.3

			TileObjectData.newTile.StyleWrapLimit = 111; // not needed, I think?

			// Does the preview flip? Can we improve on vanilla placement preview behavior??

			// All this is new:
			/* comments to test out DrawYOffset
			TileObjectData.newTile.DrawYOffset = -2; // Did banners move up since 1.3?
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.DrawYOffset = -10;
			TileObjectData.addAlternate(0);
			*/

			// TODO: Make note of changes.
			TileObjectData.addTile(Type);

			DustType = -1; // do banners not have any dust? Is 0 dirt?

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(13, 88, 130), name); // TODO: double check vanilla color ingame
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			int style = frameX / 18;
			string item;
			switch (style) {
				case 0:
					item = "SarcophagusBanner";
					break;
				case 1:
					item = "OctopusBanner";
					break;
				default:
					return;
			}
			Item.NewItem(i * 16, j * 16, 16, 48, Mod.Find<ModItem>(item).Type);
		}

		public override void NearbyEffects(int i, int j, bool closer) {
			// Todo: is closer backwards? Are banners supposed to actually be !closer?

			if (closer) {
				Player player = Main.LocalPlayer;
				int style = Main.tile[i, j].TileFrameX / 18;
				string type;
				switch (style) {
					case 0:
						type = "Sarcophagus";
						break;
					case 1:
						type = "Octopus";
						break;
					default:
						return;
				}
				player.NPCBannerBuff[Mod.Find<ModNPC>(type).Type] = true;
				player.hasBanner = true;

				int style2 = TileObjectData.GetTileStyle(Main.tile[i, j]);
				int bannerID = ModContent.NPCType<ExampleCustomAISlimeNPC>();
				//TileObjectData.GetTileInfo(Main.tile[i, j], ref int style, ref int _)
				int item = Item.BannerToItem(bannerID);
				item = TileLoader.GetItemDropFromTypeAndStyle(Type, style2);

				if (ItemID.Sets.BannerStrength.IndexInRange(item) && ItemID.Sets.BannerStrength[item].Enabled) {
					Main.SceneMetrics.NPCBannerBuff[bannerID] = true;
					Main.SceneMetrics.hasBanner = true;
				}
			}
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}
	}
}
