using ReLogic.Reflection;
using System;
using Terraria.ModLoader;

namespace Terraria.ID
{
	/// <summary>Enumerates the values used with Item.rare</summary>
	public class ItemRarityID
	{
		/// <summary>Minus thirteen (-13)<para/>Master: Fiery Red<para/>Flag: item.master</summary>
		public const int Master = -13;
		/// <summary>Minus twelve (-12)<para/>Expert: Rainbow<para/>Flag: item.expert</summary>
		public const int Expert = -12;
		/// <summary>Minus eleven (-11)<para/>Quest: Amber<para/>Flag: item.quest</summary>
		public const int Quest = -11;
		/// <summary>Minus one (-1)</summary>
		public const int Gray = -1;
		/// <summary>Zero (0)</summary>
		public const int White = 0;
		/// <summary>One (1)</summary>
		public const int Blue = 1;
		/// <summary>Two (2)</summary>
		public const int Green = 2;
		/// <summary>Three (3)</summary>
		public const int Orange = 3;
		/// <summary>Four (4)</summary>
		public const int LightRed = 4;
		/// <summary>Five (5)</summary>
		public const int Pink = 5;
		/// <summary>Six (6)</summary>
		public const int LightPurple = 6;
		/// <summary>Seven (7)</summary>
		public const int Lime = 7;
		/// <summary>Eight (8)</summary>
		public const int Yellow = 8;
		/// <summary>Nine (9)</summary>
		public const int Cyan = 9;
		/// <summary>Ten (10)</summary>
		public const int Red = 10;
		/// <summary>Eleven (11)</summary>
		public const int Purple = 11;
		public const int Count = 12;
		public static readonly IdDictionary Search = IdDictionary.Create<ItemRarityID, int>();

		public static string GetUniqueKey(int type) {
			if (type < -13 || (type > -11 && type < -1) || type > ModRarity.RarityCount)
				throw new ArgumentOutOfRangeException("Invalid type: " + type);

			if (type < Count)
				return "Terraria " + Search.GetName(type);

			var modRarity = ModRarity.GetRarity(type);
			return $"{modRarity.Mod.Name} {modRarity.Name}";
		}

		public static int TypeFromUniqueKey(string key) {
			string[] parts = key.Split(new char[] { ' ' }, 2);
			if (parts.Length != 2)
				return 0;

			return TypeFromUniqueKey(parts[0], parts[1]);
		}
		public static int TypeFromUniqueKey(string mod, string name) {
			if (mod == "Terraria") {
				if (!Search.ContainsName(name))
					return 0;

				return Search.GetId(name);
			}

			return ModLoader.ModLoader.GetMod(mod)?.RarityType(name) ?? 0;
		}
	}
}
