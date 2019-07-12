using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public static class UICommon
	{
		public static Color defaultUIBlue = new Color(73, 94, 171);
		public static Color defaultUIBlueMouseOver = new Color(63, 82, 151) * 0.7f;
		public static Color mainPanelBackground = new Color(33, 43, 79) * 0.8f;

		public static StyleDimension MaxPanelWidth = new StyleDimension(600, 0);

		public static T WithFadedMouseOver<T>(this T elem, Color overColor = default(Color), Color outColor = default(Color)) where T : UIPanel {
			if (overColor == default(Color))
				overColor = defaultUIBlue;

			if (outColor == default(Color))
				outColor = defaultUIBlueMouseOver;

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

		internal static Texture2D buttonErrorTexture;
		internal static Texture2D buttonConfigTexture;
		internal static Texture2D buttonPlusTexture;
		internal static Texture2D buttonUpDownTexture;
		internal static Texture2D buttonCollapsedTexture;
		internal static Texture2D buttonExpandedTexture;
		internal static Texture2D modBrowserIconsTexture;
		internal static Texture2D buttonExclamationTexture;
		internal static Texture2D loaderTexture;
		internal static Texture2D loaderBGTexture;
		internal static Texture2D buttonDownloadTexture;
		internal static Texture2D buttonDownloadMultipleTexture;
		internal static void LoadTextures() {
			buttonErrorTexture = LoadEmbeddedTexture("Terraria.ModLoader.UI.ButtonError.png");
			buttonConfigTexture = LoadEmbeddedTexture("Terraria.ModLoader.Config.UI.ButtonConfig.png");
			buttonPlusTexture = LoadEmbeddedTexture("Terraria.ModLoader.Config.UI.ButtonPlus.png");
			buttonUpDownTexture = LoadEmbeddedTexture("Terraria.ModLoader.Config.UI.ButtonUpDown.png");
			buttonCollapsedTexture = LoadEmbeddedTexture("Terraria.ModLoader.Config.UI.ButtonCollapsed.png");
			buttonExpandedTexture = LoadEmbeddedTexture("Terraria.ModLoader.Config.UI.ButtonExpanded.png");
			modBrowserIconsTexture = LoadEmbeddedTexture("Terraria.ModLoader.UI.UIModBrowserIcons.png");
			buttonExclamationTexture = LoadEmbeddedTexture("Terraria.ModLoader.UI.ButtonExclamation.png");
			loaderTexture = LoadEmbeddedTexture("Terraria.ModLoader.UI.Loader.png");
			loaderBGTexture = LoadEmbeddedTexture("Terraria.ModLoader.UI.LoaderBG.png");
			buttonDownloadTexture = LoadEmbeddedTexture("Terraria.ModLoader.UI.ButtonDownload.png");
			buttonDownloadMultipleTexture = LoadEmbeddedTexture("Terraria.ModLoader.UI.ButtonDownloadMultiple.png");

			Texture2D LoadEmbeddedTexture(string name) => Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream(name));
		}
	}
}
