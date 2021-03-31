namespace Terraria
{
	// This partial class, along with the changes to the original class, aims to fix a bug caused by the damage taken by tiles decaying between each tile hit.
	// The bug affects tools that hit multiple tiles per use, though reproduction varies depending on damage dealt per hit.
	// The bug actually doesn't affect the Gravedigger's Shovel in vanilla, as it deals enough damage to tiles for the bug to have no meaningful effect.
	// The way this aims to fix the bug is by adding a way to pause tile damage decay from hitting other tiles while mining is in progress.
	// After damage decay is set to continue, the Prune method is called, once if a tile was hit for more than 0 damage, and again if a tile reached 100 damage total, which mimics how vanilla does it for each individual tile.
	public partial class HitTile
	{
		private bool decayPaused;
		private int neededPrune;

		/// <summary>
		/// Pauses HitObject damage decay when hitting tiles. Useful when hitting multiple tiles at the same time.
		/// </summary>
		internal void PauseDecay() {
			decayPaused = true;
		}

		/// <summary>
		/// Continues HitObject damage decay when hitting tiles.
		/// </summary>
		internal void ContinueDecay() {
			if (!decayPaused)
				return;

			decayPaused = false;
			for (; neededPrune > 0; neededPrune--)
				Prune();
		}
	}
}