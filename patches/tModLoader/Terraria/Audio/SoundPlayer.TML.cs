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

	/// <summary>
	/// Stops all sounds matching the provided <see cref="SoundStyle"/>. Use <see cref="StopAll(in SoundStyle, int)"/> instead if stopping just a specific variant is desired.
	/// </summary>
	public void StopAll(in SoundStyle style)
	{
		List<SlotVector<ActiveSound>.ItemPair> stopped = new();

		foreach (SlotVector<ActiveSound>.ItemPair item in (IEnumerable<SlotVector<ActiveSound>.ItemPair>)_trackedSounds) {
			if (style.IsVariantOf(item.Value.Style)) {
				item.Value.Stop();
				stopped.Add(item);
			}
		}

		foreach (var item in stopped) {
			_trackedSounds.Remove(item.Id);
		}
	}

	/// <summary>
	/// Stops all sounds matching the provided <see cref="SoundStyle"/> and variant choice. Use <see cref="StopAll(in SoundStyle)"/> instead if stopping all variants is desired.
	/// </summary>
	public void StopAll(in SoundStyle style, int variant)
	{
		var checkStyle = style with { SelectedVariant = variant };

		List<SlotVector<ActiveSound>.ItemPair> stopped = new();

		foreach (SlotVector<ActiveSound>.ItemPair item in (IEnumerable<SlotVector<ActiveSound>.ItemPair>)_trackedSounds) {
			if (checkStyle.IsTheSameAs(item.Value.Style)) {
				item.Value.Stop();
				stopped.Add(item);
			}
		}

		foreach (var item in stopped) {
			_trackedSounds.Remove(item.Id);
		}
	}
}
