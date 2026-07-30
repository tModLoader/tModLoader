using System;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// Describes an option shown in the world creation menu, such as a world difficulty or world evil option.
/// </summary>
public sealed class WorldCreationMenuOption
{
	/// <summary>
	/// The stable identifier for this option. Vanilla difficulties use <c>VanillaDifficulty:0</c> through <c>VanillaDifficulty:3</c>; vanilla evils use <c>VanillaEvil:-1</c> through <c>VanillaEvil:1</c>.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// An ID assigned after all mods have modified the option list. Vanilla difficulties use <c>GameModeID</c> values, vanilla evils use <c>-1</c> through <c>1</c>, and modded options are automatically assigned afterward in final list order.
	/// </summary>
	public int Type { get; internal set; }

	/// <summary>
	/// The text shown on the option button.
	/// </summary>
	public LocalizedText Title { get; }

	/// <summary>
	/// The text shown in the description panel while hovering this option.
	/// </summary>
	public LocalizedText Description { get; }

	/// <summary>
	/// The color used to draw the option title.
	/// </summary>
	public Color TextColor { get; }

	/// <summary>
	/// The asset path for the option icon.
	/// </summary>
	public string IconTexturePath { get; }

	/// <summary>
	/// Optional asset path for a custom image drawn on the world creation preview plate when this option is selected.
	/// <br/>For custom difficulty options, this is drawn after the vanilla/custom difficulty background and before the evil overlay.
	/// <br/>For custom evil options, this is drawn after the difficulty and world size layers.
	/// </summary>
	public string PreviewTexturePath { get; }

	/// <summary>
	/// Optional second asset path for custom difficulty options. This is drawn after <see cref="PreviewTexturePath"/> and after the evil overlay, matching the second vanilla difficulty layer used for the bunny.
	/// </summary>
	public string PreviewTexturePath2 { get; }

	/// <summary>
	/// Called when this option is selected.
	/// </summary>
	public Action OnSelected { get; }

	/// <summary>
	/// Called when another option in the same group is selected.
	/// </summary>
	public Action OnDeselected { get; }

	/// <summary>
	/// Called to determine whether this option is currently selected.
	/// </summary>
	public Func<bool> IsSelected { get; }

	/// <summary>
	/// The vanilla preview value used by the world creation preview plate. Use <c>0</c> for normal/random fallback, <c>1</c> for expert/corruption, <c>2</c> for master/crimson, and <c>3</c> for journey difficulty.
	/// <br/>Use <see cref="byte.MaxValue"/> for fully custom options that should not draw any vanilla difficulty or evil preview layer.
	/// </summary>
	public byte PreviewValue { get; }

	public WorldCreationMenuOption(string id, LocalizedText title, LocalizedText description, Color textColor, string iconTexturePath, Action onSelected, Func<bool> isSelected, byte previewValue = 0, Action onDeselected = null, string previewTexturePath = null, string previewTexturePath2 = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		ArgumentNullException.ThrowIfNull(title);
		ArgumentNullException.ThrowIfNull(description);
		ArgumentNullException.ThrowIfNull(onSelected);
		ArgumentNullException.ThrowIfNull(isSelected);

		Id = id;
		Title = title;
		Description = description;
		TextColor = textColor;
		IconTexturePath = iconTexturePath;
		PreviewTexturePath = previewTexturePath;
		PreviewTexturePath2 = previewTexturePath2;
		OnSelected = onSelected;
		OnDeselected = onDeselected ?? (() => { });
		IsSelected = isSelected;
		PreviewValue = previewValue;
	}
}

