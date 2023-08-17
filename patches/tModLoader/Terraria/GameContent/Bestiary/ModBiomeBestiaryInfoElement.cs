using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace Terraria.GameContent.Bestiary;

public class ModBiomeBestiaryInfoElement : ModBestiaryInfoElement, IBestiaryBackgroundImagePathAndColorProvider
{
	public ModBiomeBestiaryInfoElement(ModLoader.Mod mod, string displayName, string iconPath, string backgroundPath, Color? backgroundColor)
	{
		_mod = mod;
		_displayName = displayName;
		_iconPath = iconPath;
		_backgroundPath = backgroundPath;
		_backgroundColor = backgroundColor;
	}

	public override UIElement GetFilterImage()
	{
		Asset<Texture2D> asset;
		if (_iconPath != null && ModContent.RequestIfExists<Texture2D>(_iconPath, out asset, AssetRequestMode.ImmediateLoad)) {
			if (asset.Size() == new Vector2(30)) {
				return new UIImage(asset) {
					HAlign = 0.5f,
					VAlign = 0.5f
				};
			}
			_mod.Logger.Info(_iconPath + " needs to be 30x30 pixels.");
		}
		asset = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Tags_Shadow");
		return new UIImageFramed(asset, asset.Frame(16, 5, 0, 4)) {
			HAlign = 0.5f,
			VAlign = 0.5f
		};
	}

	public Asset<Texture2D> GetBackgroundImage()
	{
		if (_backgroundPath == null || !ModContent.RequestIfExists<Texture2D>(_backgroundPath, out Asset<Texture2D> asset, AssetRequestMode.ImmediateLoad))
			return null;

		if (asset.Size() == new Vector2(115, 65)) {
			return asset;
		}

		_mod.Logger.Info(_backgroundPath + " needs to be 115x65 pixels.");
		return null;
	}

	public Color? GetBackgroundColor() => _backgroundColor;
}

