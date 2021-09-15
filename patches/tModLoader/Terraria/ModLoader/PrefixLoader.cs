using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Utilities;

namespace Terraria.ModLoader
{
	public static class PrefixLoader
	{
		// TODO storing twice? could see a better implementation
		internal static readonly IList<ModPrefix> prefixes = new List<ModPrefix>();
		internal static readonly IDictionary<PrefixCategory, IList<ModPrefix>> categoryPrefixes;

		public static int PrefixCount { get; private set; } = PrefixID.Count;

		static PrefixLoader() {
			categoryPrefixes = new Dictionary<PrefixCategory, IList<ModPrefix>>();

			foreach (PrefixCategory category in Enum.GetValues(typeof(PrefixCategory))) {
				categoryPrefixes[category] = new List<ModPrefix>();
			}
		}

		internal static void RegisterPrefix(ModPrefix prefix) {
			prefixes.Add(prefix);
			categoryPrefixes[prefix.Category].Add(prefix);
		}

		internal static int ReservePrefixID() {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding items breaks vanilla client compatibility");

			return PrefixCount++;
		}

		/// <summary>
		/// Returns the ModPrefix associated with specified type
		/// If not a ModPrefix, returns null.
		/// </summary>
		public static ModPrefix GetPrefix(int type)
			=> type >= PrefixID.Count && type < PrefixCount ? prefixes[type - PrefixID.Count] : null;

		/// <summary>
		/// Returns a list of all modded prefixes of a certain category.
		/// </summary>
		public static List<ModPrefix> GetPrefixesInCategory(PrefixCategory category)
			=> new List<ModPrefix>(categoryPrefixes[category]);

		internal static void ResizeArrays()
			=> Array.Resize(ref Lang.prefix, PrefixCount);

		internal static void Unload() {
			prefixes.Clear();
			
			PrefixCount = PrefixID.Count;

			foreach (PrefixCategory category in Enum.GetValues(typeof(PrefixCategory))) {
				categoryPrefixes[category].Clear();
			}
		}

		/// <summary>
		/// Performs a mod prefix roll. If the vanillaWeight wins the roll, then prefix is unchanged.
		/// </summary>
		public static void Roll(Item item, ref int prefix, int vanillaWeight, params PrefixCategory[] categories) {
			var wr = new WeightedRandom<int>();

			foreach (PrefixCategory category in categories) {
				foreach (ModPrefix modPrefix in categoryPrefixes[category].Where(x => x.CanRoll(item))) {
					wr.Add(modPrefix.Type, modPrefix.RollChance(item));
				}
			}

			if (vanillaWeight > 0)
				wr.Add((byte)prefix, vanillaWeight);

			prefix = wr.Get();
		}
	}
}
