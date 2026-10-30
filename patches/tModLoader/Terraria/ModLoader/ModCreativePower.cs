using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// A button on the Journey Mode power menu, like vanilla's God Mode, Change Time to Dawn or Rain Control. Extend <see cref="ModCreativeToggle"/>, <see cref="ModCreativeButton"/> or <see cref="ModCreativeSlider"/> rather than this class.<para/>
/// <see cref="Category"/> decides where the button appears. Powers in the same place are shown in load order. On the main strip they come after the categories and before Infection Spread and Difficulty. On other strips they come after vanilla's powers.<para/>
/// Modded powers run only on the client using them. Nothing is synced, saved or checked against the server's Journey Mode power permissions, so do that yourself where needed. Use <see cref="CanUse"/> to disable a power, for example with <see cref="Main.countsAsHostForGameplay"/> to allow only the host.
/// </summary>
public abstract class ModCreativePower : ModTexturedType, ILocalizedModType
{
	/// <summary>
	/// The ID of this power, used by <see cref="CreativeMenuLoader.GetPower"/>.
	/// </summary>
	public int Type { get; internal set; }

	public virtual string LocalizationCategory => "CreativePowers";

	/// <summary>
	/// The name of this power, used by its default tooltips.
	/// </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// Added below the tooltip when the player has power descriptions enabled (<see cref="CreativePowerSettings.ShouldPowersBeElaborated"/>), like vanilla powers. Empty by default.
	/// </summary>
	public virtual LocalizedText Description => this.GetLocalization(nameof(Description), () => "");

	/// <summary>
	/// Where this power's button appears. Defaults to <see cref="CreativeCategoryID.None"/>.
	/// <list type="bullet">
	/// <item><see cref="CreativeCategoryID.None"/>: the main strip, like vanilla's Infection Spread toggle.</item>
	/// <item><see cref="CreativeCategoryID.Time"/>, <see cref="CreativeCategoryID.Weather"/> or <see cref="CreativeCategoryID.Personal"/>: that vanilla category's strip.</item>
	/// <item>The <see cref="ModCreativeCategory.Type"/> of a modded category: that category's strip.</item>
	/// </list>
	/// If any mod hides the category with <see cref="ModSystem.CanShowCreativeCategory"/>, this power is hidden with it.
	/// </summary>
	public virtual int Category => CreativeCategoryID.None;

	protected sealed override void Register()
	{
		ModTypeLookup<ModCreativePower>.Register(this);
		Type = CreativeMenuLoader.Add(this);
	}

	public sealed override void SetupContent()
	{
		if (!CreativeMenuLoader.CanHoldPowers(Category))
			throw new Exception($"{FullName}: {nameof(Category)} must be {nameof(CreativeCategoryID)}.{nameof(CreativeCategoryID.None)}, {nameof(CreativeCategoryID.Time)}, {nameof(CreativeCategoryID.Weather)}, {nameof(CreativeCategoryID.Personal)} or the Type of a {nameof(ModCreativeCategory)}, but was {Category}");

		_ = DisplayName;
		_ = Description;
		SetStaticDefaults();
	}

	/// <summary>
	/// Whether the player can use this power right now. Returns true by default.<br/>
	/// While false, the button is shown with a gray border, like vanilla powers the server doesn't allow, and clicking it does nothing.
	/// </summary>
	public virtual bool CanUse() => true;

	/// <summary>
	/// Creates the icon shown on this power's button, which is 40x40. By default this is <see cref="ModTexturedType.Texture"/>, centered.<br/>
	/// Vanilla textures can be used by overriding <see cref="ModTexturedType.Texture"/> with a path such as <c>"Terraria/Images/Item_1"</c>, and vanilla power icons with <see cref="CreativePowersHelper.GetIconImage"/>.<br/>
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

	internal void AddIcon(UIElement button)
	{
		UIElement icon = CreateIcon();
		icon.IgnoresMouseInteraction = true;
		button.Append(icon);
	}

	internal void UpdateButton(IGroupOptionButton button) => button.SetBorderColor(CanUse() ? Color.White : Color.DimGray);

	internal void ShowHoverText(LocalizedText tooltip)
	{
		string text = tooltip.Value;
		string description = Description.Value;
		if (CreativePowerSettings.ShouldPowersBeElaborated && !string.IsNullOrEmpty(description))
			text += "\n" + description;

		Main.instance.MouseTextNoOverride(text);
	}
}
