using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.Config.UI.Elements;
public class UIHeaderElement : UIConfigElement
{
	public string Header;

	private UIText _header;

	public override bool FitsType(Type type) => false;

	public override void CreateUI()
	{
		DrawLabel = false;
		DrawPanel = false;
		DrawTooltip = false;

		_header = new UIText(Header) {
			VAlign = 0.5f,
			HAlign = 0f,
		};
		Append(_header);

		var underline = new UIImage(TextureAssets.MagicPixel) {
			Width = { Percent = 1f },
			Height = { Pixels = 2 },
			VAlign = 1f,
			HAlign = 0f,
			ScaleToFit = true,
		};
		Append(underline);
	}

	public override void RefreshUI()
	{
		_header?.SetText(Header);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		return;
		CalculatedStyle dimensions = GetDimensions();
		float settingsWidth = dimensions.Width + 1f;
		Vector2 position = new Vector2(dimensions.X, dimensions.Y) + new Vector2(8, 4);

		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dimensions.X + 10, (int)dimensions.Y + (int)dimensions.Height - 2, (int)dimensions.Width - 20, 1), Color.LightGray);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dimensions.X + 10, (int)dimensions.Y + (int)dimensions.Height - 1, (int)dimensions.Width - 20, 1), Color.DarkGray);

		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Header, position, Color.White, 0f, Vector2.Zero, new Vector2(1f), settingsWidth - 20, 2f);
	}
}
