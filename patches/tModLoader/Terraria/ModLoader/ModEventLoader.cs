using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class ModEventLoader
	{
		internal static readonly IList<ModEvent> ModEvents = new List<ModEvent>();
		internal static readonly IList<ModInvasion> ModInvasions = new List<ModInvasion>();

		internal static int Register(ModEvent modEvent) {
			ModEvents.Add(modEvent);

			if (modEvent is ModInvasion modInvasion)
				ModInvasions.Add(modInvasion);

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
			ModInvasions.Clear();
		}
	}
}