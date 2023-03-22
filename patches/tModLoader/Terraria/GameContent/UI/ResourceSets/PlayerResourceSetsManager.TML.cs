using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.GameContent.UI.ResourceSets;

partial class PlayerResourceSetsManager
{
	private static readonly string[] vanillaSets = new string[] { "New", "NewWithText", "HorizontalBars", "HorizontalBarsWithText", "HorizontalBarsWithFullText", "Default" };

	private readonly List<string> accessKeys = new(vanillaSets);
	private int selectedSet = 0;

	private string _activeSetConfigKeyOriginal;  // Used to store the original key value, since PlayerResourceSetsManager is loaded way before mods

	internal void AddModdedDisplaySets()
	{
		if (Main.dedServ)
			return;

		foreach (ModResourceDisplaySet display in ResourceDisplaySetLoader.moddedDisplaySets) {
			string key = display.ConfigKey;
			_sets[key] = display;
			accessKeys.Add(key);
		}
	}

	// Called by tML after mods have loaded to set the actual display set
	internal void SetActiveFromOriginalConfigKey()
	{
		if (Main.dedServ)
			return;

		SetActive(_activeSetConfigKeyOriginal);
		// In case the display set didn't exist, force the original key back to Fancy
		_activeSetConfigKeyOriginal = _activeSetConfigKey;
	}

	private void SetActiveFrameFromIndex(int index)
	{
		selectedSet = index;
		SetActiveFrame(_sets[accessKeys[selectedSet]]);
	}

	internal void ResetToVanilla()
	{
		if (Main.dedServ)
			return;

		_activeSetConfigKey = _activeSetConfigKeyOriginal;

		foreach (string key in accessKeys.Skip(vanillaSets.Length))
			_sets.Remove(key);

		accessKeys.Clear();
		accessKeys.AddRange(vanillaSets);

		// Handles resetting the display set to Fancy if the config key isn't present
		SetActive(_activeSetConfigKey);
	}

	public IPlayerResourcesDisplaySet ActiveSet => _activeSet;

	public IPlayerResourcesDisplaySet GetDisplaySet(string nameKey) => _sets.TryGetValue(nameKey, out var set) ? set : null;

	public ModResourceDisplaySet GetDisplaySet<T>() where T : ModResourceDisplaySet => GetDisplaySet(ModContent.GetInstance<T>().ConfigKey) as T;
}
