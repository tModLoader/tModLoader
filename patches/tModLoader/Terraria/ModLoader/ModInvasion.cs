using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	public abstract class ModInvasion : ModEvent
	{
		internal class ModInvasionProgressDisplay
		{
			public int DisplayLeft;
			public float Alpha;

			public ModInvasionProgressDisplay(int displayLeft, float alpha) {
				DisplayLeft = displayLeft;
				Alpha = alpha;
			}
		}

		public float Progress { get; set; }

		public abstract string Title { get; }

		public abstract string ProgressText { get; }

		public virtual Color TitleUIColor => new Color(63, 65, 151, 255) * 0.785f;

		public virtual Color ProgressUIColor => new Color(63, 65, 151, 255) * 0.785f;

		public virtual Color ProgressBarColor => new Color(255, 241, 51);
	}
}