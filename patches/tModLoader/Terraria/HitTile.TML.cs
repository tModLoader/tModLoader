namespace Terraria
{
	public partial class HitTile
	{
		private bool decayPaused;

		/// <summary>
		/// Pauses HitObject damage decay when hitting tiles. Useful when hitting multiple tiles at the same time.
		/// </summary>
		public void PauseDecay() {
			decayPaused = true;
		}

		/// <summary>
		/// Continues HitObject damage decay when hitting tiles.
		/// </summary>
		public void ContinueDecay() {
			if (!decayPaused)
				return;

			decayPaused = false;
			Prune();
		}
	}
}