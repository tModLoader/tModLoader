using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ID;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Chat;

namespace ExampleMod.Content.Tiles
{
	public class ExampleSign : ModTile
	{
		public override void SetStaticDefaults()
		{
			// Note: The max amount of signs in a world is 1000 (the size of the tileSign array).
			Main.tileSign[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			// Use the vanilla sign style as our foundation.
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Signs, 0));

            // Implement fixes for two of the five styles from the Vanilla Sign TileObjectData.
            // Fix Vanilla TileObjectData: Allow attaching sign to the ground by changing origin.
            TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
            TileObjectData.newSubTile.Origin = new Point16(0, 0);
            TileObjectData.addSubTile(0);

            // Fix Vanilla TileObjectData: Allow attaching to a solid object that is to the right of the sign.
            TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.Origin = new Point16(0, 0);
            TileObjectData.newSubTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newSubTile.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree, TileObjectData.newSubTile.Width, 0);
            TileObjectData.addSubTile(3);
            TileObjectData.addTile(Type);

			RegisterItemDrop(ModContent.ItemType<ExampleSignItem>());

            AddMapEntry(new Color(200, 200, 200));
			TileID.Sets.DisableSmartCursor[Type] = true;
			AdjTiles = new int[] { Type };
		}

		public override void PlaceInWorld(int i, int j, Item item)
        {
            // When the third param is true, ReadSign() initializes a new Sign object and returns an ID.
            // The ID is the array index for our new Sign in the Main.sign[] array.
            int signId = Sign.ReadSign(i, j, true);

            Main.sign[signId].text = "I am a sign, right-click me!";
		}

		public override bool RightClick(int i, int j)
		{
			int signId = Sign.ReadSign(i, j);
			string signText = Main.sign[signId].text;
            
			ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(signText), Color.White);
			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			// Use internal method to destroy associated Sign data.
            Sign.KillSign(i, j);
		}

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }
    }	
}