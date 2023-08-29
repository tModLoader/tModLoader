using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.UI;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.UI;
// Not UIHeader because that already exists in vanilla
public class UIHeading : UIElement
{
	public object Text { get; private set; }

	private UIText _header;

	public UIHeading(object text)
	{
		Text = text;
	}

	public override void OnInitialize()
	{
		Width.Set(0, 1f);
		Height.Set(30, 0f);
		SetPadding(4);

		float lineHeight = 1;
		float lineOffset = 2;
		var underline = new UIImage(TextureAssets.MagicPixel) {
			Top = { Pixels = lineHeight + lineOffset },
			Width = { Percent = 1f },
			Height = { Pixels = lineHeight },
			VAlign = 1f,
			HAlign = 0f,
			ScaleToFit = true,
			Color = Color.LightGray,
		};
		Append(underline);

		var underlineShadow = new UIImage(TextureAssets.MagicPixel) {
			Top = { Pixels = lineHeight + lineHeight + lineOffset },
			Width = { Percent = 1f },
			Height = { Pixels = lineHeight },
			VAlign = 1f,
			HAlign = 0f,
			ScaleToFit = true,
			Color = Color.DarkGray,
		};
		Append(underlineShadow);

		_header = new UIText(Text.ToString()) {
			VAlign = 0.5f,
			HAlign = 0f,
		};
		Append(_header);
	}

	public void SetText(object text)
	{
		Text = text;
		_header?.SetText(Text.ToString());
	}
}
