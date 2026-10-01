using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.GameContent.Bestiary;

public class ModSourceBestiaryInfoElement : ModBestiaryInfoElement
{
	public ModSourceBestiaryInfoElement(ModLoader.Mod mod, string displayName)
	{
		_mod = mod;
		_displayName = displayName;
	}

	public override UIElement GetFilterImage()
	{
		Asset<Texture2D> asset = _mod.SmallModIcon;
		if (asset != null) {
			return new UIImage(asset) {
				HAlign = 0.5f,
				VAlign = 0.5f
			};
		}

		asset = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Tags_Shadow", AssetRequestMode.ImmediateLoad);
		return new UIImageFramed(asset, asset.Frame(16, 5, 0, 4)) {
			HAlign = 0.5f,
			VAlign = 0.5f
		};
	}
}

