using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria.UI;

public partial class ChestUI
{
	// TML: #ModdedChestSizes. The world chest's coordinates and tile type, or false for banks.
	private static bool TryGetOpenChestTile(out int i, out int j, out int type)
	{
		i = j = type = 0;
		int chestIndex = Main.LocalPlayer.chest;
		if (chestIndex < 0 || Main.chest[chestIndex] is not Chest chest)
			return false;

		i = chest.x;
		j = chest.y;
		type = Main.tile[i, j].TileType;
		return true;
	}

	internal static bool CanScrollWithMouseWheel() => !TryGetOpenChestTile(out int i, out int j, out int type) || TileLoader.CanScrollChest(i, j, type);

	// TML: #ModdedChestSizes. Up and down buttons for chests with more rows than fit on screen, placed like the crafting grid's buttons.
	private static void DrawScrollButtons(SpriteBatch spriteBatch)
	{
		if (LastHighestChestRow <= 0 || TryGetOpenChestTile(out _, out _, out int type) && TileID.Sets.HideChestScrollButtons[type])
			return;

		int x = 73 - 20;
		int y = Main.instance.invBottom + 2;
		if (StartingRowForDrawing > 0)
			DrawScrollButton(spriteBatch, TextureAssets.CraftUpButton.Value, x, y, 1);

		if (StartingRowForDrawing < LastHighestChestRow)
			DrawScrollButton(spriteBatch, TextureAssets.CraftDownButton.Value, x, y + 20, -1);
	}

	private static void DrawScrollButton(SpriteBatch spriteBatch, Texture2D texture, int x, int y, int direction)
	{
		if (Main.mouseX >= x && Main.mouseX <= x + texture.Width && Main.mouseY >= y && Main.mouseY <= y + texture.Height && !PlayerInput.IgnoreMouseInterface) {
			Main.LocalPlayer.mouseInterface = true;
			if (Main.mouseLeftRelease && Main.mouseLeft) {
				Scroll(direction);
				Main.mouseLeftRelease = false;
			}
		}

		spriteBatch.Draw(texture, new Vector2(x, y), null, new Color(200, 200, 200, 200), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
	}
}
