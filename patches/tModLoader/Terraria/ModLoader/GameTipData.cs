using Terraria.Localization;

namespace Terraria.ModLoader
{
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
		/// The name of the mod this GameTip belongs to.
		/// For vanilla tips, this value is "Terraria"
		/// </summary>
		public string Mod {
			get;
			internal set;
		}

		/// <summary>
		/// Retrieves the "short" key of this GameTip, excluding the beginning Mods.ModName.GameTips portion.
		/// For example, if the key was "Mods.ExampleMod.GameTips.ExampleTip", this would return
		/// "ExampleTip".
		/// </summary>
		public string ShortKey {
			get;
			internal set;
		}

		internal bool isVisible = true;

		public GameTipData(LocalizedText text, Mod mod) : this(text, mod.Name) { }

		internal GameTipData(LocalizedText text, string mod) {
			TipText = text;
			Mod = mod;
			ShortKey = text.Key.Replace($"Mods.{mod}.GameTips.", "");
		}

		internal GameTipData(LocalizedText text) {
			TipText = text;
			Mod = "Terraria";
			ShortKey = text.Key;
		}

		/// <summary>
		/// Until reload, prevents this tip from ever appearing during loading screens.
		/// </summary>
		public void Hide() {
			isVisible = false;
		}

		/// <summary>
		/// Returns a new object which is a clone of this object.
		/// </summary>
		public GameTipData Clone() {
			return new GameTipData(new LocalizedText(TipText.Key, TipText.Value), Mod);
		}
	}
}
