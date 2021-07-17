using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.GameContent.Bestiary
{
	public class ModSourceBestiaryInfoElement : ModBestiaryInfoElement
	{
		private ModLoader.Assets.ModAssetRepository _assets;

		public ModSourceBestiaryInfoElement(ModLoader.Mod mod, string displayName, ModLoader.Assets.ModAssetRepository assets) {
			_mod = mod;
			_displayName = displayName;
			_assets = assets;
		}

		public override UIElement GetFilterImage() {
			Asset<Texture2D> asset;
			if (_assets.HasAsset<Texture2D>("icon_small")) {
				asset = _assets.Request<Texture2D>("icon_small");
				if (asset.Size() == new Vector2(30)) {
					return new UIImage(asset) {
						HAlign = 0.5f,
						VAlign = 0.5f
					};
				}
				_mod.Logger.Info("icon_small needs to be 30x30 pixels.");
			}
			asset = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Tags_Shadow");
			return new UIImageFramed(asset, asset.Frame(16, 5, 0, 4)) {
				HAlign = 0.5f,
				VAlign = 0.5f
			};
		}
	}
}

