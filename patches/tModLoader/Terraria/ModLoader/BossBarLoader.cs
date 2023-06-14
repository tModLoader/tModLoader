using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.Localization;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader;

public static class BossBarLoader
{
	/// <summary>
	/// Set to the current info that is being drawn just before any registered bar draws through the vanilla system (modded and vanilla), reset in the method used to draw it.
	/// <para>Allows tML to short-circuit the draw method and make ModBossBar and GlobalBossBar modify the draw parameters. Is null if a ModBossBarStyle skips drawing</para>
	/// </summary>
	internal static BigProgressBarInfo? drawingInfo = null;

	// Cache vanilla boss bar texture as it's being accessed via GetTexture again
	private static Asset<Texture2D> vanillaBossBarTexture;
	public static Asset<Texture2D> VanillaBossBarTexture => vanillaBossBarTexture ??= Main.Assets.Request<Texture2D>("Images/UI/UI_BossBar");

	/// <summary>
	/// Used to prevent switching to the default style while the mods are loading (The code responsible for it runs in the main menu too)
	/// </summary>
	private static bool styleLoading = true;

	internal static readonly ModBossBarStyle vanillaStyle = new DefaultBossBarStyle();

	private static ModBossBarStyle switchToStyle = null;

	public static ModBossBarStyle CurrentStyle { get; private set; } = vanillaStyle;

	/// <summary>
	/// The string that is saved in the config
	/// </summary>
	internal static string lastSelectedStyle = CurrentStyle.FullName;

	internal static readonly IList<ModBossBar> bossBars = new List<ModBossBar>();

	internal static readonly IList<GlobalBossBar> globalBossBars = new List<GlobalBossBar>();

	// This is accessed during the main menu and unloading, hence using locks is necessary in some places
	internal static readonly List<ModBossBarStyle> bossBarStyles = new List<ModBossBarStyle>() {
		vanillaStyle
	};

	// Saves repopulating the list again if we just remove all but the by-default styles on unload
	internal static readonly int defaultStyleCount = bossBarStyles.Count;

	/// <summary>
	/// Only contains textures that exist.
	/// </summary>
	internal static readonly Dictionary<int, Asset<Texture2D>> bossBarTextures = new Dictionary<int, Asset<Texture2D>>();

	internal static void Unload()
	{
		drawingInfo = null;
		vanillaBossBarTexture = null;
		styleLoading = true;
		bossBars.Clear();
		globalBossBars.Clear();
		lock (bossBarStyles) {
			bossBarStyles.RemoveRange(defaultStyleCount, bossBarStyles.Count - defaultStyleCount);
		}
		bossBarTextures.Clear();
	}

	internal static void AddBossBar(ModBossBar bossBar)
	{
		bossBar.index = bossBars.Count;
		bossBars.Add(bossBar);
		ModTypeLookup<ModBossBar>.Register(bossBar);

		//Texture is optional
		if (ModContent.RequestIfExists<Texture2D>(bossBar.Texture, out var bossBarTexture))
			bossBarTextures[bossBar.index] = bossBarTexture;
	}

	internal static void AddGlobalBossBar(GlobalBossBar globalBossBar)
	{
		globalBossBars.Add(globalBossBar);
		ModTypeLookup<GlobalBossBar>.Register(globalBossBar);
	}

	internal static void AddBossBarStyle(ModBossBarStyle bossBarStyle)
	{
		lock (bossBarStyles) {
			bossBarStyles.Add(bossBarStyle);
			ModTypeLookup<ModBossBarStyle>.Register(bossBarStyle);
		}
	}

	/// <summary>
	/// Sets the pending style that should be switched to
	/// </summary>
	/// <param name="bossBarStyle">Pending boss bar style</param>
	internal static void SwitchBossBarStyle(ModBossBarStyle bossBarStyle) => switchToStyle = bossBarStyle;

	/// <summary>
	/// Sets the saved style that should be switched to, handles possibly unloaded/invalid ones and defaults to the vanilla style
	/// </summary>
	internal static void GotoSavedStyle()
	{
		switchToStyle = vanillaStyle;
		if (ModContent.TryFind(lastSelectedStyle, out ModBossBarStyle value))
			switchToStyle = value;

		styleLoading = false;
	}

	/// <summary>
	/// Checks if the style was changed and applies it, saves the config if required
	/// </summary>
	internal static void HandleStyle()
	{
		if (switchToStyle != null && switchToStyle != CurrentStyle) {
			CurrentStyle.OnDeselected();
			CurrentStyle = switchToStyle;
			CurrentStyle.OnSelected();
		}
		switchToStyle = null;

		if (!styleLoading && CurrentStyle.FullName != lastSelectedStyle) {
			lastSelectedStyle = CurrentStyle.FullName;
			Main.SaveSettings();
		}
	}

	/// <summary>
	/// Returns the texture that the given bar is using. If it does not have a custom one, it returns the vanilla texture
	/// </summary>
	/// <param name="bossBar">The ModBossBar</param>
	/// <returns>Its texture, or the vanilla texture</returns>
	public static Asset<Texture2D> GetTexture(ModBossBar bossBar)
	{
		if (!bossBarTextures.TryGetValue(bossBar.index, out Asset<Texture2D> texture))
			texture = VanillaBossBarTexture;
		return texture;
	}

	/// <summary>
	/// Gets the ModBossBar associated with this NPC
	/// </summary>
	/// <param name="npc">The NPC</param>
	/// <param name="value">When this method returns, contains the ModBossBar associated with the specified NPC</param>
	/// <returns><see langword="true"/> if a ModBossBar is assigned to it; otherwise, <see langword="false"/>.</returns>
	public static bool NpcToBossBar(NPC npc, out ModBossBar value)
	{
		value = null;
		if (npc.BossBar is ModBossBar bossBar)
			value = bossBar;
		return value != null;
	}

	/// <summary>
	/// Inserts the boss bar style select option into the main and in-game menu under the "Interface" category
	/// </summary>
	internal static string InsertMenu(out Action onClick)
	{
		string styleText = null;
		ModBossBarStyle pendingBossBarStyle = null;
		foreach (ModBossBarStyle bossBarStyle in bossBarStyles) {
			if (bossBarStyle == CurrentStyle) {
				styleText = bossBarStyle.DisplayName;
				break;
			}

			pendingBossBarStyle = bossBarStyle;
		}

		if (pendingBossBarStyle == null)
			pendingBossBarStyle = bossBarStyles.Last();

		if (styleText == null || bossBarStyles.Count == 1)
			styleText = Language.GetTextValue("tModLoader.BossBarStyleNoOptions");

		onClick = () => SwitchBossBarStyle(pendingBossBarStyle);

		return Language.GetTextValue("tModLoader.BossBarStyle", styleText);
	}

	public static bool PreDraw(SpriteBatch spriteBatch, BigProgressBarInfo info, ref BossBarDrawParams drawParams)
	{
		int index = info.npcIndexToAimAt;
		if (index < 0 || index > Main.maxNPCs)
			return false; // Invalid data, abort

		NPC npc = Main.npc[index];

		bool isModded = NpcToBossBar(npc, out ModBossBar bossBar);

		if (isModded)
			drawParams.BarTexture = GetTexture(bossBar).Value;

		bool modify = true;

		foreach (GlobalBossBar globalBossBar in globalBossBars) {
			modify &= globalBossBar.PreDraw(spriteBatch, npc, ref drawParams);
		}

		if (modify && isModded)
			modify = bossBar.PreDraw(spriteBatch, npc, ref drawParams);

		return modify;
	}

	public static void PostDraw(SpriteBatch spriteBatch, BigProgressBarInfo info, BossBarDrawParams drawParams)
	{
		int index = info.npcIndexToAimAt;
		if (index < 0 || index > Main.maxNPCs)
			return; // Invalid data, abort

		NPC npc = Main.npc[index];

		if (NpcToBossBar(npc, out ModBossBar bossBar))
			bossBar.PostDraw(spriteBatch, npc, drawParams);

		foreach (GlobalBossBar globalBossBar in globalBossBars) {
			globalBossBar.PostDraw(spriteBatch, npc, drawParams);
		}
	}

	/// <summary>
	/// Draws a healthbar with fixed barTexture dimensions (516x348) where the effective bar top left starts at 32x24, and is 456x22 big
	/// <para>The icon top left starts at 4x20, and is 26x28 big</para>
	/// <para>Frame 0 contains the frame (outline)</para>
	/// <para>Frame 1 contains the 2 pixel wide strip for the tip of the bar itself</para>
	/// <para>Frame 2 contains the 2 pixel wide strip for the bar itself, stretches out</para>
	/// <para>Frame 3 contains the background</para>
	/// <para>Frame 4 contains the 2 pixel wide strip for the tip of the bar itself (optional shield)</para>
	/// <para>Frame 5 contains the 2 pixel wide strip for the bar itself, stretches out (optional shield)</para>
	/// <para>Supply your own textures if you need a different shape/color, otherwise you can make your own method to draw it</para>
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="drawParams">The draw parameters for the boss bar</param>
	public static void DrawFancyBar_TML(SpriteBatch spriteBatch, BossBarDrawParams drawParams)
	{
		// DrawFancyBar without shieldCurrent gets redirected to DrawFancyBar with shieldCurrent as 0f
		// DrawFancyBar with shieldCurrent gets redirected to this

		(Texture2D barTexture, Vector2 barCenter, Texture2D iconTexture, Rectangle iconFrame, Color iconColor, float life, float lifeMax, float shield, float shieldMax, float iconScale, bool showText, Vector2 textOffset) = drawParams;

		Point barSize = new Point(456, 22); //Size of the bar
		Point topLeftOffset = new Point(32, 24); //Where the top left of the bar starts
		int frameCount = 6;

		Rectangle bgFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 3);
		Color bgColor = Color.White * 0.2f;

		int scale = (int)(barSize.X * life / lifeMax);
		scale -= scale % 2;
		Rectangle barFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 2);
		barFrame.X += topLeftOffset.X;
		barFrame.Y += topLeftOffset.Y;
		barFrame.Width = 2;
		barFrame.Height = barSize.Y;

		Rectangle tipFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 1);
		tipFrame.X += topLeftOffset.X;
		tipFrame.Y += topLeftOffset.Y;
		tipFrame.Width = 2;
		tipFrame.Height = barSize.Y;

		int shieldScale = (int)(barSize.X * shield / shieldMax);
		shieldScale -= shieldScale % 2;

		Rectangle barShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 5);
		barShieldFrame.X += topLeftOffset.X;
		barShieldFrame.Y += topLeftOffset.Y;
		barShieldFrame.Width = 2;
		barShieldFrame.Height = barSize.Y;

		Rectangle tipShieldFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 4);
		tipShieldFrame.X += topLeftOffset.X;
		tipShieldFrame.Y += topLeftOffset.Y;
		tipShieldFrame.Width = 2;
		tipShieldFrame.Height = barSize.Y;

		Rectangle barPosition = Utils.CenteredRectangle(barCenter, barSize.ToVector2());
		Vector2 barTopLeft = barPosition.TopLeft();
		Vector2 topLeft = barTopLeft - topLeftOffset.ToVector2();

		// Background
		spriteBatch.Draw(barTexture, topLeft, bgFrame, bgColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

		// Bar itself
		Vector2 stretchScale = new Vector2(scale / barFrame.Width, 1f);
		Color barColor = Color.White;
		spriteBatch.Draw(barTexture, barTopLeft, barFrame, barColor, 0f, Vector2.Zero, stretchScale, SpriteEffects.None, 0f);

		// Tip
		spriteBatch.Draw(barTexture, barTopLeft + new Vector2(scale - 2, 0f), tipFrame, barColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

		// Bar itself (shield)
		if (shield > 0f) {
			stretchScale = new Vector2(shieldScale / barFrame.Width, 1f);
			spriteBatch.Draw(barTexture, barTopLeft, barShieldFrame, barColor, 0f, Vector2.Zero, stretchScale, SpriteEffects.None, 0f);

			// Tip
			spriteBatch.Draw(barTexture, barTopLeft + new Vector2(shieldScale - 2, 0f), tipShieldFrame, barColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		// Frame
		Rectangle frameFrame = barTexture.Frame(verticalFrames: frameCount, frameY: 0);
		spriteBatch.Draw(barTexture, topLeft, frameFrame, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

		// Icon
		Vector2 iconOffset = new Vector2(4f, 20f);
		Vector2 iconSize = new Vector2(26f, 28f);
		// The vanilla method with the shieldCurrent parameter, which is used only by the lunar pillars, uses iconSize = iconFrame.Size() instead, which have a size of 26x30,
		// causing a slight vertical offset that is barely noticeable. Considering that the non-shieldCurrent method is the more general one, let's keep it like this
		// (changing that using the lunar pillar code will cause many other icons to be offset instead) --direwolf420
		Vector2 iconPos = iconOffset + iconSize / 2f;
		// iconFrame Centered around iconPos
		spriteBatch.Draw(iconTexture, topLeft + iconPos, iconFrame, iconColor, 0f, iconFrame.Size() / 2f, iconScale, SpriteEffects.None, 0f);

		if (BigProgressBarSystem.ShowText && showText) {
			if (shield > 0f)
				BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, shield, shieldMax);
			else
				BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, life, lifeMax);
		}
	}
}
