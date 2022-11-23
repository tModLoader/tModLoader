using ReLogic.Utilities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Terraria.Audio;

partial class SoundPlayer
{
	/// <summary>
	/// Safely attempts to get a currently playing <see cref="ActiveSound"/> instance, tied to the provided <see cref="SlotId"/>.
	/// </summary>
	public bool TryGetActiveSound(SlotId id, [NotNullWhen(true)] out ActiveSound? result)
		=> _trackedSounds.TryGetValue(id, out result);

	public void StopAll(in SoundStyle style)
	{
		List<SlotVector<ActiveSound>.ItemPair> stopped = new();

		foreach (SlotVector<ActiveSound>.ItemPair item in (IEnumerable<SlotVector<ActiveSound>.ItemPair>)_trackedSounds) {
			if (style.IsTheSameAs(item.Value.Style)) {
				item.Value.Stop();
				stopped.Add(item);
			}
		}

		foreach (var item in stopped) {
			_trackedSounds.Remove(item.Id);
		}
	}
}
