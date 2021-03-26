namespace Terraria
{
	public partial class HitTile
	{
		private int pauseDuration;

		/// <summary>
		/// Pauses HitObject damage decay when hitting tiles. Useful when hitting multiple tiles at the same time.
		/// </summary>
		/// <param name="hits"> The amount of times `HitObject` can be called before damage decay is automatically continued. </param>
		public void PauseDecay(int hits) {
			pauseDuration = hits + 1;
		}

		/// <summary>
		/// Continues HitObject damage decay when hitting tiles.
		/// </summary>
		public void ContinueDecay() {
			pauseDuration = 0;
			Prune();
		}

		private void OnHitObject() {
			if (pauseDuration > 0)
				pauseDuration--;
		}
	}
}