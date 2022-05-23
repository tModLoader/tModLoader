using ReLogic.Utilities;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Terraria.Audio
{
	partial class SoundPlayer
	{
		/// <summary>
		/// Safely attempts to get a currently playing <see cref="ActiveSound"/> instance, tied to the provided <see cref="SlotId"/>.
		/// </summary>
		public bool TryGetActiveSound(SlotId id, [NotNullWhen(true)] out ActiveSound? result)
			=> _trackedSounds.TryGetValue(id, out result);
	}
}
