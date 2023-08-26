using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Config.UI.Elements;
public class UIBoolElement : UIConfigElement<bool>
{
	readonly Asset<Texture2D> _texture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

	UIImageFramed _toggleImage;
	UIText _toggleLabel;

	public override void CreateUI()
	{
		OnLeftClick += (_, _) => {
			SoundEngine.PlaySound(SoundID.MenuTick);
			Value = !Value;
			RefreshUI();
		};

		_toggleImage = new UIImageFramed(_texture, GetTextureFrame()) {
			HAlign = 1f,
			VAlign = 0.5f,
		};
		Append(_toggleImage);

		_toggleLabel = new UIText(GetToggleLabel()) {
			Left = { Pixels = -20 },
			HAlign = 1f,
			VAlign = 0.5f,
		};
		Append(_toggleLabel);
	}

	public override void RefreshUI()
	{
		_toggleImage.SetFrame(GetTextureFrame());
		_toggleLabel.SetText(GetToggleLabel());
	}

	private LocalizedText GetToggleLabel()
		=> Value ? Language.GetText("tModLoader.ModConfigTrue") : Language.GetText("tModLoader.ModConfigFalse");

	private Rectangle GetTextureFrame()
		=> Value ? _texture.Frame(2, 1, 0, 0, sizeOffsetX: -1) : _texture.Frame(2, 1, 1, 0);
}
