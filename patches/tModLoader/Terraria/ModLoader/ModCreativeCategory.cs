using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// A category on the main Journey Mode power strip, next to vanilla's Duplication, Research, Time, Weather and Personal categories.<para/>
/// Clicking its button opens a strip of the <see cref="ModCreativePower"/>s whose <see cref="ModCreativePower.Category"/> is this category's <see cref="Type"/>, laid out like vanilla's Time and Personal strips.
/// For anything else, such as a window like vanilla's duplication window, override <see cref="CreateContent"/>. Opening any other category closes it, the same as the vanilla categories.<para/>
/// The server's Journey Mode power permissions only apply to vanilla powers. If something in your category should be restricted in multiplayer, check that yourself, for example with <see cref="Main.countsAsHostForGameplay"/>.
/// </summary>
public abstract class ModCreativeCategory : ModTexturedType, ILocalizedModType
{
	/// <summary>
	/// The distance from the left of the power menu at which vanilla places windows and strips opened from the main strip, such as the duplication window.<br/>
	/// Use it as the <see cref="UIElement.Left"/> of the element returned by <see cref="CreateContent"/> to line it up the same way.
	/// </summary>
	public const float ContentLeft = 80f;

	/// <summary>
	/// The ID of this category, after the vanilla ones in <see cref="ID.CreativeCategoryID"/>.
	/// </summary>
	public int Type { get; internal set; }

	public virtual string LocalizationCategory => "CreativeCategories";

	/// <summary>
	/// The name of this category, used by the default <see cref="OpenTooltip"/> and <see cref="CloseTooltip"/>.
	/// </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// Shown when hovering the button while this category is closed. Defaults to "Open <see cref="DisplayName"/>".
	/// </summary>
	public virtual LocalizedText OpenTooltip => Language.GetText("tModLoader.CreativeMenuOpen").WithFormatArgs(DisplayName);

	/// <summary>
	/// Shown when hovering the button while this category is open. Defaults to "Close <see cref="DisplayName"/>".
	/// </summary>
	public virtual LocalizedText CloseTooltip => Language.GetText("tModLoader.CreativeMenuClose").WithFormatArgs(DisplayName);

	/// <summary>
	/// Added below the tooltip when the player has power descriptions enabled (<see cref="CreativePowerSettings.ShouldPowersBeElaborated"/>), like vanilla categories. Empty by default.
	/// </summary>
	public virtual LocalizedText Description => this.GetLocalization(nameof(Description), () => "");

	/// <summary>
	/// Whether this category is the one selected on the main power strip.<br/>
	/// Vanilla keeps the selected category while the power menu is hidden, so this can be true while the menu isn't visible. It's false again after entering another world, since vanilla builds a new menu then.
	/// </summary>
	public bool IsSelected => Main.CreativeMenu.CurrentMainCategory == Type;

	protected sealed override void Register()
	{
		ModTypeLookup<ModCreativeCategory>.Register(this);
		Type = CreativeMenuLoader.Add(this);
	}

	public sealed override void SetupContent()
	{
		_ = DisplayName;
		_ = Description;
		SetStaticDefaults();
	}

	/// <summary>
	/// Creates the icon shown on this category's button, which is 40x40. By default this is <see cref="ModTexturedType.Texture"/>, centered.<br/>
	/// Vanilla textures can be used by overriding <see cref="ModTexturedType.Texture"/> with a path such as <c>"Terraria/Images/Buff_2"</c>, and vanilla power icons with <see cref="CreativePowersHelper.GetIconImage"/>.<br/>
	/// When making your own <see cref="UIImage"/>, request its texture with <see cref="AssetRequestMode.ImmediateLoad"/>. <see cref="UIImage"/> takes its size from the texture when it's created, so a texture that hasn't loaded yet is drawn off-center.
	/// </summary>
	public virtual UIElement CreateIcon()
	{
		// Loaded immediately, since UIImage takes its size from the texture when it's created.
		return new UIImage(ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad)) {
			HAlign = 0.5f,
			VAlign = 0.5f
		};
	}

	/// <summary>
	/// Creates an extra element shown while this category is open, such as a window like vanilla's duplication window. Returns null by default.<br/>
	/// Not needed for powers, which are shown on a strip automatically. If this category has powers too, place the element so it doesn't cover their strip.<br/>
	/// Called the first time the category is opened after the power menu is built, which happens each time a world is entered. The element is kept and reused after that.<br/>
	/// The element is placed inside the power menu, below the top bar. Set its <see cref="UIElement.Left"/> to <see cref="ContentLeft"/> to place it beside the main strip.<br/>
	/// Remember to set <see cref="Player.mouseInterface"/> while the mouse is over the element, so clicks don't use the held item.
	/// </summary>
	public virtual UIElement CreateContent() => null;
}
