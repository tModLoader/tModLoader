using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class ExampleSign : ModTile
	{
		public static LocalizedText DefaultSignText { get; private set; }

		public override void SetStaticDefaults() {
			// These are all used by TileID.Signs, hover over them to read their documentation and see the purpose of each
			Main.tileSign[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.AvoidedByNPCs[Type] = true;
			TileID.Sets.TileInteractRead[Type] = true;
			TileID.Sets.InteractibleByNPCs[Type] = true;

			VanillaFallbackOnModDeletion = TileID.Signs;

			// TileObjectData assignment
			// The TileID.Signs TileObjectData doesn't set StyleMultiplier to 5, so we will not be copying from it in this case
			// Using Style2x2 as a base, we will create a TileObjectData with 5 alternate placements, each anchoring to a different anchor.
			// We also adjust the Origin for the alternates to match vanilla. Style2x2 starts with a origin at 0, 1 and a AnchorBottom, these will both be adjusted in the alternates.
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 5; // Since each style has 5 placement styles, we set this to 5.
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty; // Clear out existing bottom anchor inherited from Style2x2 temporarily so that we don't have to set it to empty in each of the alternates. 

			// To reduce code repetition, we'll use the same AnchorData value multiple times. This works because the tile is as tall as it is wide.
			AnchorData SolidOrSolidSideAnchor2TilesLong = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = Point16.Zero;
			TileObjectData.newAlternate.AnchorTop = SolidOrSolidSideAnchor2TilesLong;
			TileObjectData.addAlternate(1);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = Point16.Zero;
			TileObjectData.newAlternate.AnchorLeft = SolidOrSolidSideAnchor2TilesLong;
			TileObjectData.addAlternate(2);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(1, 0);
			TileObjectData.newAlternate.AnchorRight = SolidOrSolidSideAnchor2TilesLong;
			TileObjectData.addAlternate(3);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = Point16.Zero;
			TileObjectData.newAlternate.AnchorWall = true;
			TileObjectData.addAlternate(4);

			// Finally, we restore the default AnchorBottom, the extra AnchorTypes here allow placing on tables, platforms, and other tiles.
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, 2, 0);
			TileObjectData.addTile(Type);

			// Map entry and extra localization
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(200, 200, 200), name);

			DefaultSignText = this.GetLocalization("DefaultSignText");
		}

		public override void PlaceInWorld(int i, int j, Item item) {
			// This code sets a default value for the sign, this is not typical and can be removed from normal sign tiles.
			int signId = Sign.ReadSign(i, j, true);
			if (signId != -1) {
				Sign.TextSign(signId, DefaultSignText.Value);
			}
		}

		public override bool RightClick(int i, int j) {
			// Normal sign right click behavior happens automatically because of Main.tileSign, this code just shows how to retrieve the text of the sign and should be removed from normal sign tiles.
			int signId = Sign.ReadSign(i, j);
			if (signId != -1) {
				string signText = Main.sign[signId].text;
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(signText), Color.White);
			}
			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// Destroy the associated Sign data.
			Sign.KillSign(i, j);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}
	}
}
