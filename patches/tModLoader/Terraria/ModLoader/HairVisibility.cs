using System;

namespace Terraria.ModLoader;

/// <summary>
/// Determines in what UIs a hairstyle is visible.
/// </summary>
[Flags]
public enum HairVisibility
{
	CharacterCreation = 0b01,
	Stylist = 0b10,

	All = CharacterCreation | Stylist
}