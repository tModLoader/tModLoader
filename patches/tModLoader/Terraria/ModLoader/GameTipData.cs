using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// Wrapper class for a LocalizedText and visibility field that has intended use with modification
/// of Game Tips.
/// </summary>
public sealed class GameTipData
{
	public LocalizedText TipText {
		get;
		internal set;
	}

	/// <summary>
	/// The mod instance this tip belongs to. This value is null
	/// for vanilla tips.
	/// </summary>
	public Mod Mod {
		get;
		internal set;
	}

	/// <summary>
	/// Retrieves the "name" of this GameTip, which is the Key excluding the beginning Mods.ModName.GameTips portion.
	/// For example, if the key was "Mods.ExampleMod.GameTips.ExampleTip", this would return "ExampleTip".
	/// </summary>
	public string Name {
		get;
		internal set;
	}

	/// <summary>
	/// Retrieves the FULL "name" of this GameTip, which includes the Mod and this tip's Name.
	/// For example, if this tip was from ExampleMod and was named "ExampleTip", this would
	/// return "ExampleMod/ExampleTip"
	/// </summary>
	public string FullName {
		get;
		internal set;
	}

	internal bool isVisible = true;

	public GameTipData(LocalizedText text, Mod mod)
	{
		TipText = text;
		Mod = mod;
		Name = text.Key.Replace($"Mods.{mod.Name}.GameTips.", "");
		FullName = $"{Mod.Name}/{Name}";
	}

	internal GameTipData(LocalizedText text)
	{
		TipText = text;
		Mod = null;
		Name = text.Key;
		FullName = $"Terraria/{Name}";
	}

	/// <summary>
	/// Until reload, prevents this tip from ever appearing during loading screens.
	/// </summary>
	public void Hide()
	{
		isVisible = false;
	}
}
