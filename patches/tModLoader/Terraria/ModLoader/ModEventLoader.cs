using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class ModEventLoader
	{
		internal static readonly IList<ModEvent> ModEvents = new List<ModEvent>();

		internal static int Register(ModEvent modEvent) {
			ModEvents.Add(modEvent);
			return ModEvents.Count - 1;
		}

		internal static void UpdateActiveEvents() {
			foreach (ModEvent modEvent in ModEvents) {
				if (modEvent.Active)
					modEvent.Update();
			}
		}

		internal static void Unload() {
			ModEvents.Clear();
		}
	}
}