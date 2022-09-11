using System;

namespace Terraria.GameContent.UI.ResourceSets
{
	partial class PlayerResourceSetsManager
	{
		private static readonly string[] vanillaSets = new string[] { "New", "Default", "HorizontalBars" };
		private string _activeSetConfigKeyOriginal;  // Used to store the original key value, since PlayerResourceSetsManager is loaded way before mods

		// Called by tML after mods have loaded to set the actual display set
		internal void SetActiveFromOriginalConfigKey() {
			SetActive(_activeSetConfigKeyOriginal);
			_activeSetConfigKeyOriginal = _activeSetConfigKey;
		}

		internal void ResetToVanilla() {
			_activeSetConfigKeyOriginal = _activeSetConfigKey;

			string[] keys = new string[_sets.Keys.Count];
			_sets.Keys.CopyTo(keys, 0);

			foreach (string key in keys) {
				if (Array.IndexOf(vanillaSets, key) == -1)
					_sets.Remove(key);
			}

			// Handles resetting the display set to Fancy if the config key isn't present
			SetActive(_activeSetConfigKey);
		}

		public IPlayerResourcesDisplaySet ActiveSet => _activeSet;

		public IPlayerResourcesDisplaySet GetDisplaySet(string nameKey) => _sets.TryGetValue(nameKey, out var set) ? set : null;
	}
}
