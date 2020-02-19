using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
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
				Main.PlaySound(SoundID.MenuTick);
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
			float x = Main.fontMouseText.MeasureString(text).X;
			Vector2 vector = Main.MouseScreen + new Vector2(16f);
			vector.X = Math.Min(vector.X, bounds.Value.Right - x - 16);
			vector.Y = Math.Min(vector.Y, bounds.Value.Bottom - 30);
			Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
		}

		internal static Texture2D ButtonErrorTexture;
		internal static Texture2D ButtonConfigTexture;
		internal static Texture2D ButtonPlusTexture;
		internal static Texture2D ButtonUpDownTexture;
		internal static Texture2D ButtonCollapsedTexture;
		internal static Texture2D ButtonExpandedTexture;
		internal static Texture2D ModBrowserIconsTexture;
		internal static Texture2D ButtonExclamationTexture;
		internal static Texture2D LoaderTexture;
		internal static Texture2D LoaderBgTexture;
		internal static Texture2D ButtonDownloadTexture;
		internal static Texture2D ButtonDowngradeTexture;
		internal static Texture2D ButtonDownloadMultipleTexture;
		internal static Texture2D ButtonModInfoTexture;
		internal static Texture2D ButtonModConfigTexture;

		internal static Texture2D DividerTexture;
		internal static Texture2D InnerPanelTexture;

		internal static void LoadTextures() {
			ButtonErrorTexture = LoadEmbeddedTexture("UI.ButtonError.png");
			ButtonConfigTexture = LoadEmbeddedTexture("Config.UI.ButtonConfig.png");
			ButtonPlusTexture = LoadEmbeddedTexture("Config.UI.ButtonPlus.png");
			ButtonUpDownTexture = LoadEmbeddedTexture("Config.UI.ButtonUpDown.png");
			ButtonCollapsedTexture = LoadEmbeddedTexture("Config.UI.ButtonCollapsed.png");
			ButtonExpandedTexture = LoadEmbeddedTexture("Config.UI.ButtonExpanded.png");
			ModBrowserIconsTexture = LoadEmbeddedTexture("UI.UIModBrowserIcons.png");
			ButtonExclamationTexture = LoadEmbeddedTexture("UI.ButtonExclamation.png");
			LoaderTexture = LoadEmbeddedTexture("UI.Loader.png");
			LoaderBgTexture = LoadEmbeddedTexture("UI.LoaderBG.png");
			ButtonDownloadTexture = LoadEmbeddedTexture("UI.ButtonDownload.png");
			ButtonDowngradeTexture = LoadEmbeddedTexture("UI.ButtonDowngrade.png");
			ButtonDownloadMultipleTexture = LoadEmbeddedTexture("UI.ButtonDownloadMultiple.png");
			ButtonModInfoTexture = LoadEmbeddedTexture("UI.ButtonModInfo.png");
			ButtonModConfigTexture = LoadEmbeddedTexture("UI.ButtonModConfig.png");

			DividerTexture = TextureManager.Load("Images/UI/Divider");
			InnerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");

			Texture2D LoadEmbeddedTexture(string name) 
				=> Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream($"Terraria.ModLoader.{name}"));
		}
	}
}
