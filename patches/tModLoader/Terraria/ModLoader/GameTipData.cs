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

		internal bool isVisible = true;

		public GameTipData(LocalizedText text, Mod mod) {
			TipText = text;
			Mod = mod.Name;
		}

		internal GameTipData(LocalizedText text) {
			TipText = text;
			Mod = "Terraria";
		}

		/// <summary>
		/// Until reload, prevents this tip from ever appearing during loading screens.
		/// </summary>
		public void DisableVisibility() {
			isVisible = false;
		}

		/// <summary>
		/// Returns a new copy which is a clone of this object.
		/// </summary>
		public GameTipData Clone() {
			return new GameTipData(new LocalizedText(TipText.Key, TipText.Value));
		}
	}
}
