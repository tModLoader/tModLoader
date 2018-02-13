using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public abstract class ModPrefix
	{
		private static byte nextPrefix = PrefixID.Count;
		internal static readonly IList<ModPrefix> prefixes = new List<ModPrefix>();
		internal static readonly IDictionary<PrefixCategory, IList<ModPrefix>> categoryPrefixes;

		static ModPrefix()
		{
			categoryPrefixes = new Dictionary<PrefixCategory, IList<ModPrefix>>();
			foreach (PrefixCategory category in Enum.GetValues(typeof(PrefixCategory)))
			{
				categoryPrefixes[category] = new List<ModPrefix>();
			}
		}

		internal static byte ReservePrefixID()
		{
			if (ModNet.AllowVanillaClients) throw new Exception("Adding items breaks vanilla client compatiblity");
			if (nextPrefix == 0) throw new Exception("Prefix ID limit has been broken");

			byte reserveID = nextPrefix;
			nextPrefix++;
			return reserveID;
		}

		public static ModPrefix GetPrefix(byte type)
		{
			return type >= PrefixID.Count && type < PrefixCount ? prefixes[type - PrefixID.Count] : null;
		}

		/// <summary>
		/// Returns a list of all modded prefixes of a certain category.
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		public static List<ModPrefix> GetPrefixesInCategory(PrefixCategory category)
		{
			return new List<ModPrefix>(categoryPrefixes[category]);
		}

		public static byte PrefixCount => nextPrefix;

		internal static void ResizeArrays()
		{
			Array.Resize(ref Lang.prefix, nextPrefix);
		}

		internal static void Unload()
		{
			prefixes.Clear();
			nextPrefix = PrefixID.Count;
			foreach (PrefixCategory category in Enum.GetValues(typeof(PrefixCategory)))
			{
				categoryPrefixes[category].Clear();
			}
		}

		public Mod mod
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			internal set;
		}

		public byte Type
		{
			get;
			internal set;
		}

		public ModTranslation DisplayName
		{
			get;
			internal set;
		}

		public PrefixCategory Category
		{
			get;
			internal set;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		public virtual void AutoDefaults()
		{
			Category = PrefixCategory.Custom;
			
			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(Name, "([A-Z])", " $1").Trim());
		}

		/// <summary>
		/// Allows you to set the prefix's name/translations and to set its category.
		/// </summary>
		public virtual void SetDefaults()
		{
		}

		/// <summary>
		/// Sets the stat changes for this prefix. If data is not already pre-stored, it is best to store custom data changes to some static variables.
		/// </summary>
		public virtual void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
			ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
		{
		}

		/// <summary>
		/// Validates whether this prefix with the custom data stats set from SetStats is allowed on the given item.
		/// It is not allowed if one of the stat changes do not cause any change (eg. percentage being too small to make a difference).
		/// </summary>
		public virtual void ValidateItem(Item item, ref bool invalid)
		{
		}

		/// <summary>
		/// Applies the custom data stats set in SetStats to the given item.
		/// </summary>
		/// <param name="item"></param>
		public virtual void Apply(Item item)
		{
		}

		/// <summary>
		/// Allows you to modify the sell price of the item based on the prefix or changes in custom data stats. This also influences the item's rarity.
		/// </summary>
		public virtual void ModifyValue(ref float valueMult)
		{
		}
	}

	public enum PrefixCategory
	{
		/// <summary>
		/// Can modify the size of the weapon
		/// </summary>
		Melee,
		/// <summary>
		/// Can modify the shoot speed of the weapon
		/// </summary>
		Ranged,
		/// <summary>
		/// Can modify the mana usage of the weapon
		/// </summary>
		Magic,
		AnyWeapon,
		Accessory,
		/// <summary>
		/// Will not appear by default. Useful as prefixes for your own damage type.
		/// </summary>
		Custom
	}
}
