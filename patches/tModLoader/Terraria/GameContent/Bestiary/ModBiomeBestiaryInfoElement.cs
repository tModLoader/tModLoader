using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.GameContent.Bestiary
{
	public class ModBiomeBestiaryInfoElement : IFilterInfoProvider, IProvideSearchFilterString, IBestiaryInfoElement, IBestiaryBackgroundImagePathAndColorProvider
	{
		private ModLoader.Mod _mod;
		private string _displayName;
		private string _iconPath;
		private string _backgroundPath;
		private Color? _backgroundColor;

		public ModBiomeBestiaryInfoElement(ModLoader.Mod mod, string displayName, string iconPath, string backgroundPath, Color? backgroundColor) {
			_mod = mod;
			_displayName = displayName;
			_iconPath = iconPath;
			_backgroundPath = backgroundPath;
			_backgroundColor = backgroundColor;
		}

		public UIElement GetFilterImage() {
			Asset<Texture2D> asset;
			if (_iconPath != null && _mod.Assets.HasAsset<Texture2D>(_iconPath)) {
				asset = _mod.Assets.Request<Texture2D>(_iconPath);
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

		public UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
			if (info.UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0)
				return null;

			UIElement uIElement = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, 12, 7) {
				Width = new StyleDimension(-14f, 1f),
				Height = new StyleDimension(34f, 0f),
				BackgroundColor = new Color(43, 56, 101),
				BorderColor = Color.Transparent,
				Left = new StyleDimension(5f, 0f)
			};

			uIElement.SetPadding(0f);
			uIElement.PaddingRight = 5f;
			UIElement filterImage = GetFilterImage();
			filterImage.HAlign = 0f;
			filterImage.Left = new StyleDimension(5f, 0f);
			UIText element = new UIText(Language.GetText(GetDisplayNameKey()), 0.8f) {
				HAlign = 0f,
				Left = new StyleDimension(38f, 0f),
				TextOriginX = 0f,
				VAlign = 0.5f,
				DynamicallyScaleDownToWidth = true
			};

			if (filterImage != null)
				uIElement.Append(filterImage);

			uIElement.Append(element);
			AddOnHover(uIElement);
			return uIElement;
		}

		private void AddOnHover(UIElement button) {
			button.OnUpdate += delegate (UIElement e) {
				ShowButtonName(e);
			};
		}

		private void ShowButtonName(UIElement element) {
			if (element.IsMouseHovering) {
				string textValue = Language.GetTextValue(GetDisplayNameKey());
				Main.instance.MouseText(textValue, 0, 0);
			}
		}

		public string GetDisplayNameKey() => _displayName;

		public string GetSearchString(ref BestiaryUICollectionInfo info) {
			if (info.UnlockState == BestiaryEntryUnlockState.NotKnownAtAll_0)
				return null;

			return _displayName;
		}

		public Asset<Texture2D> GetBackgroundImage() {
			if (_backgroundPath == null)
				return null;

			Asset<Texture2D> asset = _mod.Assets.Request<Texture2D>(_backgroundPath);
			if (asset.Size() == new Vector2(115, 65)) {
				return asset;
			}

			_mod.Logger.Info(_backgroundPath + " needs to be 115x65 pixels.");
			return null;
		}

		public Color? GetBackgroundColor() => _backgroundColor;
	}
}

