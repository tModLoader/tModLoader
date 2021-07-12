using ExampleMod.Content.TileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Container;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	public class AutoClentaminator : ModTile
	{
		private static Asset<Texture2D> mask;

		public override void Load() {
			mask = ModContent.Request<Texture2D>($"{Texture}_Mask");
		}

		public override void SetDefaults() {
			DustType = 11;

			Main.tileFrameImportant[Type] = true;

			TileObjectData newTile = TileObjectData.newTile;
			newTile.CopyFrom(TileObjectData.Style3x3);
			newTile.Width = 3;
			newTile.Height = 3;
			newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, newTile.Width, 0);
			newTile.Origin = new Point16(1, 2);
			newTile.CoordinateHeights = new[] { 16, 16, 18 };
			newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<AutoClentaminatorTE>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Auto Clentaminator");
			AddMapEntry(new Color(190, 230, 190), name);
		}

		public override bool RightClick(int i, int j) {
			if (!TileEntityUtils.TryGetTileEntity(i, j, out AutoClentaminatorTE te)) return false;

			ItemStorage storage = te.GetItemStorage();

			Item item = new Item(ItemID.PurpleSolution) { stack = 5 };
			storage.InsertItem(Main.LocalPlayer, 0, ref item);
			Main.NewText($"Inserted {5 - item.stack} items");
			Item itemInSlot = storage[0];
			Main.NewText($"Currently has {itemInSlot.Name} x{itemInSlot.stack}");

			return true;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			if (mask.Value == null)
				return;
			
			if (!TileEntityUtils.TryGetTileEntity(i, j, out AutoClentaminatorTE te))
				return;

			Vector2 position = new Vector2(i * 16 + 24, j * 16 + 24) - Main.screenPosition + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange));

			// todo: change color based on solution
			spriteBatch.Draw(mask.Value, position, null, Color.Purple, 0f, mask.Value.Size() * 0.5f, te.cleansingProgress * 1.5f, SpriteEffects.None, 0f);
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			var point = TileEntityUtils.TileTopLeft(i, j);

			Main.instance.TilesRenderer.AddSpecialLegacyPoint(point.X, point.Y);
		}

		public override void MouseOver(int i, int j) {
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Items.Placeable.AutoClentaminator>();
			Main.LocalPlayer.cursorItemIconEnabled = true;

			Main.LocalPlayer.noThrow = 2;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 48, 48, ModContent.ItemType<Items.Placeable.AutoClentaminator>());
			ModContent.GetInstance<AutoClentaminatorTE>().Kill(i, j);
		}
	}
}