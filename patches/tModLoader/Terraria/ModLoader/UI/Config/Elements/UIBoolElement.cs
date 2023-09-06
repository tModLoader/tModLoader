using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.UI.Config.Elements;
public class UIBoolElement : UIConfigElement<bool>
{
	private static readonly Asset<Texture2D> _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

	private UIImageFramed _toggleImage;
	private UIText _toggleLabel;

	public override void OnInitialize()
	{
		base.OnInitialize();

		OnLeftClick += (_, _) => {
			SoundEngine.PlaySound(SoundID.MenuTick);
			Value = !Value;
		};

		_toggleImage = new UIImageFramed(_toggleTexture, GetTextureFrame()) {
			Left = { Pixels = -4 },
			HAlign = 1f,
			VAlign = 0.5f,
		};
		Append(_toggleImage);

		_toggleLabel = new UIText(GetToggleLabel(), TextScale) {
			Left = { Pixels = -16 - PaddingLeft },
			HAlign = 1f,
			VAlign = 0.5f,
		};
		Append(_toggleLabel);
	}

	public override void Recalculate()
	{
		base.Recalculate();

		_toggleImage.SetFrame(GetTextureFrame());
		_toggleLabel.SetText(GetToggleLabel());
	}

	private LocalizedText GetToggleLabel()
		=> Value ? Language.GetText("tModLoader.ModConfigTrue") : Language.GetText("tModLoader.ModConfigFalse");

	private Rectangle GetTextureFrame()
		=> new(!Value ? (_toggleTexture.Width() - 2) / 2 + 2 : 0, 0, (_toggleTexture.Width() - 2) / 2, _toggleTexture.Height());// TODO: use non vanilla texture?
}
