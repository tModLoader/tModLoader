using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

public static class UICommon
{
	public static Color DefaultUIBlue = new Color(73, 94, 171);
	public static Color DefaultUIBlueMouseOver = new Color(63, 82, 151) * 0.7f;
	public static Color DefaultUIBorder = Color.Black;
	public static Color DefaultUIBorderMouseOver = Colors.FancyUIFatButtonMouseOver;
	public static Color MainPanelBackground = new Color(33, 43, 79) * 0.8f;

	public static StyleDimension MaxPanelWidth = new StyleDimension(600, 0);

	public static T WithFadedMouseOver<T>(this T elem, Color overColor = default, Color outColor = default, Color overBorderColor = default, Color outBorderColor = default) where T : UIPanel
	{
		if (overColor == default)
			overColor = DefaultUIBlue;

		if (outColor == default)
			outColor = DefaultUIBlueMouseOver;

		if (overBorderColor == default)
			overBorderColor = DefaultUIBorderMouseOver;

		if (outBorderColor == default)
			outBorderColor = DefaultUIBorder;

		elem.OnMouseOver += (evt, _) => {
			SoundEngine.PlaySound(SoundID.MenuTick);
			elem.BackgroundColor = overColor;
			elem.BorderColor = overBorderColor;
		};
		elem.OnMouseOut += (evt, _) => {
			elem.BackgroundColor = outColor;
			elem.BorderColor = outBorderColor;
		};
		return elem;
	}

	public static T WithPadding<T>(this T elem, float pixels) where T : UIElement
	{
		elem.SetPadding(pixels);
		return elem;
	}

	public static T WithPadding<T>(this T elem, string name, int id, Vector2? anchor = null, Vector2? offset = null) where T : UIElement
	{
		elem.SetSnapPoint(name, id, anchor, offset);
		return elem;
	}

	public static T WithView<T>(this T elem, float viewSize, float maxViewSize) where T : UIScrollbar
	{
		elem.SetView(viewSize, maxViewSize);
		return elem;
	}

	public static void AddOrRemoveChild(this UIElement elem, UIElement child, bool add)
	{
		if (!add)
			elem.RemoveChild(child);
		else if (!elem.HasChild(child))
			elem.Append(child);
	}

	public static void DrawHoverStringInBounds(SpriteBatch spriteBatch, string text, Rectangle? bounds = null)
	{
		if (bounds == null)
			bounds = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

		Vector2 stringSize = Terraria.UI.Chat.ChatManager.GetStringSize(FontAssets.MouseText.Value, text, Vector2.One);
		Vector2 vector = Main.MouseScreen + new Vector2(16f);
		vector.X = Math.Min(vector.X, bounds.Value.Right - stringSize.X - 16);
		vector.Y = Math.Min(vector.Y, bounds.Value.Bottom - stringSize.Y - 16);
		Color color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255); // 255 needed for black check in item tags

		Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, vector, color, 0f, Vector2.Zero, Vector2.One);
	}

	/// <summary>
	/// Draws a tooltip on the mouse cursor.
	/// <br/><br/> Functions like <see cref="Main.MouseText(string, int, byte, int, int, int, int, int)"/> and <see cref="Main.hoverItemName"/>, but adds the same background seen in item tooltips behind the text.
	/// <br/><br/> Only works during rendering, calling this during Update will not do anything.
	/// </summary>
	/// <param name="text"></param>
	public static void TooltipMouseText(string text)
	{
		if (Main.SettingsEnabled_OpaqueBoxBehindTooltips) {
			Item fakeItem = new Item();
			fakeItem.SetDefaults(0, noMatCheck: true);
			fakeItem.SetNameOverride(text);
			fakeItem.type = 1;
			fakeItem.scale = 0f;
			fakeItem.rare = 0;
			fakeItem.value = -1;
			Main.HoverItem = fakeItem;
			Main.instance.MouseText("");
			Main.mouseText = true;
		}
		else {
			Main.instance.MouseText(text);
		}
	}

	public static Asset<Texture2D> ButtonErrorTexture { get; internal set; }
	public static Asset<Texture2D> ButtonConfigTexture { get; internal set; }
	public static Asset<Texture2D> ButtonPlusTexture { get; internal set; }
	public static Asset<Texture2D> ButtonUpDownTexture { get; internal set; }
	public static Asset<Texture2D> ButtonCollapsedTexture { get; internal set; }
	public static Asset<Texture2D> ButtonExpandedTexture { get; internal set; }
	public static Asset<Texture2D> ModBrowserIconsTexture { get; internal set; }
	public static Asset<Texture2D> UIAchievementsMenuIconsTexture { get; internal set; }
	public static Asset<Texture2D> ConfigSideIndicatorTexture { get; internal set; }
	public static Asset<Texture2D> ButtonExclamationTexture { get; internal set; }
	public static Asset<Texture2D> ButtonDepsTexture { get; internal set; }
	public static Asset<Texture2D> ButtonUpgradeCsproj { get; internal set; }
	public static Asset<Texture2D> ButtonUpgradeLang { get; internal set; }
	public static Asset<Texture2D> ButtonRunTModPorter { get; internal set; }
	public static Asset<Texture2D> ButtonOpenFolder { get; internal set; }
	public static Asset<Texture2D> ButtonOpenFolderCustom { get; internal set; }
	public static Asset<Texture2D> ButtonTranslationModTexture { get; internal set; }
	public static Asset<Texture2D> LoaderTexture { get; internal set; }
	public static Asset<Texture2D> LoaderBgTexture { get; internal set; }
	public static Asset<Texture2D> ButtonDownloadTexture { get; internal set; }
	public static Asset<Texture2D> ButtonDowngradeTexture { get; internal set; }
	public static Asset<Texture2D> ButtonDownloadMultipleTexture { get; internal set; }
	public static Asset<Texture2D> ButtonModInfoTexture { get; internal set; }
	public static Asset<Texture2D> ButtonModConfigTexture { get; internal set; }
	public static Asset<Texture2D> ModLocationModPackIcon { get; internal set; }
	public static Asset<Texture2D> ModLocationLocalIcon { get; internal set; }
	public static Asset<Texture2D> DividerTexture { get; internal set; }
	public static Asset<Texture2D> InnerPanelTexture { get; internal set; }
	public static Asset<Texture2D> InfoDisplayPageArrowTexture { get; internal set; }
	public static Asset<Texture2D> tModLoaderTitleLinkButtonsTexture { get; internal set; }
	public static Asset<Texture2D> CopyCodeButtonTexture { get; internal set; }
	public static Asset<Texture2D> DropdownIconTexture { get; internal set; }

	internal static void LoadTextures()
	{
		Asset<Texture2D> LoadEmbeddedTexture(string name)
			=> ModLoader.ManifestAssets.Request<Texture2D>($"Terraria.ModLoader.{name}");

		ButtonErrorTexture = LoadEmbeddedTexture("UI.ButtonError");
		//ButtonConfigTexture = LoadEmbeddedTexture("Config.UI.ButtonConfig");
		ButtonPlusTexture = LoadEmbeddedTexture("Config.UI.ButtonPlus");
		ButtonUpDownTexture = LoadEmbeddedTexture("Config.UI.ButtonUpDown");
		ButtonCollapsedTexture = LoadEmbeddedTexture("Config.UI.ButtonCollapsed");
		ButtonExpandedTexture = LoadEmbeddedTexture("Config.UI.ButtonExpanded");
		ModBrowserIconsTexture = LoadEmbeddedTexture("UI.UIModBrowserIcons");
		UIAchievementsMenuIconsTexture = LoadEmbeddedTexture("UI.UIAchievementsMenuIcons");
		ConfigSideIndicatorTexture = LoadEmbeddedTexture("UI.ConfigSideIndicator");
		ButtonExclamationTexture = LoadEmbeddedTexture("UI.ButtonExclamation");
		ButtonDepsTexture = LoadEmbeddedTexture("UI.ButtonDeps");
		ButtonUpgradeCsproj = LoadEmbeddedTexture("UI.ButtonUpgradeCsproj");
		ButtonUpgradeLang = LoadEmbeddedTexture("UI.ButtonUpgradeLang");
		ButtonRunTModPorter = LoadEmbeddedTexture("UI.ButtonRunTModPorter");
		ButtonOpenFolder = LoadEmbeddedTexture("UI.ButtonOpenFolder");
		ButtonOpenFolderCustom = LoadEmbeddedTexture("UI.ButtonOpenFolderCustom");
		ButtonTranslationModTexture = LoadEmbeddedTexture("UI.ButtonTranslationMod");
		LoaderTexture = LoadEmbeddedTexture("UI.Loader");
		LoaderBgTexture = LoadEmbeddedTexture("UI.LoaderBG");
		ButtonDownloadTexture = LoadEmbeddedTexture("UI.ButtonDownload");
		ButtonDowngradeTexture = LoadEmbeddedTexture("UI.ButtonDowngrade");
		ButtonDownloadMultipleTexture = LoadEmbeddedTexture("UI.ButtonDownloadMultiple");
		ButtonModInfoTexture = LoadEmbeddedTexture("UI.ButtonModInfo");
		ButtonModConfigTexture = LoadEmbeddedTexture("UI.ButtonModConfig");
		ModLocationModPackIcon = LoadEmbeddedTexture("UI.ModLocationModPackIcon");
		ModLocationLocalIcon = LoadEmbeddedTexture("UI.ModLocationLocalIcon");

		DividerTexture = Main.Assets.Request<Texture2D>("Images/UI/Divider");
		InnerPanelTexture = Main.Assets.Request<Texture2D>("Images/UI/InnerPanelBackground");

		InfoDisplayPageArrowTexture = LoadEmbeddedTexture("UI.InfoDisplayPageArrow");
		tModLoaderTitleLinkButtonsTexture = LoadEmbeddedTexture("UI.tModLoaderTitleLinkButtons");
		CopyCodeButtonTexture = LoadEmbeddedTexture("UI.CopyCodeButton");
		DropdownIconTexture = LoadEmbeddedTexture("UI.DropdownIcon");
	}
}
