using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public static class UICommon
	{
		public static Color DefaultUIBlue = new Color(73, 94, 171);
		public static Color DefaultUIBlueMouseOver = new Color(63, 82, 151) * 0.7f;
		public static Color MainPanelBackground = new Color(33, 43, 79) * 0.8f;

		public static StyleDimension MaxPanelWidth = new StyleDimension(600, 0);

		public static T WithFadedMouseOver<T>(this T elem, Color overColor = default, Color outColor = default) where T : UIPanel {
			if (overColor == default)
				overColor = DefaultUIBlue;

			if (outColor == default)
				outColor = DefaultUIBlueMouseOver;

			elem.OnMouseOver += (evt, _) => {
				SoundEngine.PlaySound(SoundID.MenuTick);
				elem.BackgroundColor = overColor;
			};
			elem.OnMouseOut += (evt, _) => {
				elem.BackgroundColor = outColor;
			};
			return elem;
		}

		public static T WithPadding<T>(this T elem, float pixels) where T : UIElement {
			elem.SetPadding(pixels);
			return elem;
		}

		public static T WithPadding<T>(this T elem, string name, int id, Vector2? anchor = null, Vector2? offset = null) where T : UIElement {
			elem.SetSnapPoint(name, id, anchor, offset);
			return elem;
		}

		public static T WithView<T>(this T elem, float viewSize, float maxViewSize) where T : UIScrollbar {
			elem.SetView(viewSize, maxViewSize);
			return elem;
		}

		public static void AddOrRemoveChild(this UIElement elem, UIElement child, bool add) {
			if (!add) 
				elem.RemoveChild(child);
			else if (!elem.HasChild(child)) 
				elem.Append(child);
		}

		public static void DrawHoverStringInBounds(SpriteBatch spriteBatch, string text, Rectangle? bounds = null) {
			if (bounds == null)
				bounds = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			float x = FontAssets.MouseText.Value.MeasureString(text).X;
			Vector2 vector = Main.MouseScreen + new Vector2(16f);
			vector.X = Math.Min(vector.X, bounds.Value.Right - x - 16);
			vector.Y = Math.Min(vector.Y, bounds.Value.Bottom - 30);
			Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
		}

		public static Asset<Texture2D> ButtonErrorTexture { get; internal set; }
		public static Asset<Texture2D> ButtonConfigTexture { get; internal set; }
		public static Asset<Texture2D> ButtonPlusTexture { get; internal set; }
		public static Asset<Texture2D> ButtonUpDownTexture { get; internal set; }
		public static Asset<Texture2D> ButtonCollapsedTexture { get; internal set; }
		public static Asset<Texture2D> ButtonExpandedTexture { get; internal set; }
		public static Asset<Texture2D> ModBrowserIconsTexture { get; internal set; }
		public static Asset<Texture2D> ButtonExclamationTexture { get; internal set; }
		public static Asset<Texture2D> LoaderTexture { get; internal set; }
		public static Asset<Texture2D> LoaderBgTexture { get; internal set; }
		public static Asset<Texture2D> ButtonDownloadTexture { get; internal set; }
		public static Asset<Texture2D> ButtonDowngradeTexture { get; internal set; }
		public static Asset<Texture2D> ButtonDownloadMultipleTexture { get; internal set; }
		public static Asset<Texture2D> ButtonModInfoTexture { get; internal set; }
		public static Asset<Texture2D> ButtonModConfigTexture { get; internal set; }
		public static Asset<Texture2D> DividerTexture { get; internal set; }
		public static Asset<Texture2D> InnerPanelTexture { get; internal set; }
		public static Asset<Texture2D> InfoDisplayPageArrowTexture { get; internal set; }

		internal static void LoadTextures() {
			Asset<Texture2D> LoadEmbeddedTexture(string name)
				=> ModLoader.ManifestAssets.Request<Texture2D>($"Terraria.ModLoader.{name}");

			ButtonErrorTexture = LoadEmbeddedTexture("UI.ButtonError");
			//ButtonConfigTexture = LoadEmbeddedTexture("Config.UI.ButtonConfig");
			ButtonPlusTexture = LoadEmbeddedTexture("Config.UI.ButtonPlus");
			ButtonUpDownTexture = LoadEmbeddedTexture("Config.UI.ButtonUpDown");
			ButtonCollapsedTexture = LoadEmbeddedTexture("Config.UI.ButtonCollapsed");
			ButtonExpandedTexture = LoadEmbeddedTexture("Config.UI.ButtonExpanded");
			ModBrowserIconsTexture = LoadEmbeddedTexture("UI.UIModBrowserIcons");
			ButtonExclamationTexture = LoadEmbeddedTexture("UI.ButtonExclamation");
			LoaderTexture = LoadEmbeddedTexture("UI.Loader");
			LoaderBgTexture = LoadEmbeddedTexture("UI.LoaderBG");
			ButtonDownloadTexture = LoadEmbeddedTexture("UI.ButtonDownload");
			ButtonDowngradeTexture = LoadEmbeddedTexture("UI.ButtonDowngrade");
			ButtonDownloadMultipleTexture = LoadEmbeddedTexture("UI.ButtonDownloadMultiple");
			ButtonModInfoTexture = LoadEmbeddedTexture("UI.ButtonModInfo");
			ButtonModConfigTexture = LoadEmbeddedTexture("UI.ButtonModConfig");

			DividerTexture = Main.Assets.Request<Texture2D>("Images/UI/Divider");
			InnerPanelTexture = Main.Assets.Request<Texture2D>("Images/UI/InnerPanelBackground");

			InfoDisplayPageArrowTexture = LoadEmbeddedTexture("UI.InfoDisplayPageArrow");
		}
	}
}
