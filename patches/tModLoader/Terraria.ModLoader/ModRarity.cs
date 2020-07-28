using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public abstract class ModRarity
	{
		private static int nextRarity = ItemRarityID.Count;

		internal static readonly IList<ModRarity> rarities = new List<ModRarity>();

		internal static int ReserveRarityID()
		{
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding items breaks vanilla client compatbility");
			if (nextRarity == 0)
				throw new Exception("ItemRarity ID limit hasas been broken");

			int reserveID = nextRarity;
			nextRarity++;
			return reserveID;
		}

		/// <summary>
		/// Returns the ModRarity associated with the specified type.
		/// If not a ModRarity, returns null.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ModRarity GetRarity(int type)
		{
			return type >= ItemRarityID.Count && type < RarityCount ? rarities[type - ItemRarityID.Count] : null;
		}

		public static int RarityCount => nextRarity;

		internal static void Unload()
		{
			rarities.Clear();
			nextRarity = ItemRarityID.Count;
		}

		public Mod mod {
			get;
			internal set;
		}

		public string Name {
			get;
			internal set;
		}

		public int Type {
			get;
			internal set;
		}

		public virtual bool Autoload(ref string name)
		{
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Your ModRarity's color.
		/// Returns White by default.
		/// </summary>
		/// <returns></returns>
		public virtual Color RarityColor() => Color.White;

		/// <summary>
		/// Your modded rarity plus one. Used for prefixes. Returns your modded rarity by default.
		/// </summary>
		/// <returns></returns>
		public virtual int RarityPlusOne() => GetRarity(Type).Type;

		/// <summary>
		/// Your modded rarity plus two. Used for prefixes. Returns your modded rarity by default.
		/// </summary>
		/// <returns></returns>
		public virtual int RarityPlusTwo() => GetRarity(Type).Type;

		/// <summary>
		/// Your modded rarity minus one. Used for prefixes. Returns your modded rarity by default.
		/// </summary>
		/// <returns></returns>
		public virtual int RarityMinusOne() => GetRarity(Type).Type;

		/// <summary>
		/// Your modded rarity minus two. Used for prefixes. Returns your modded rarity by default.
		/// </summary>
		/// <returns></returns>
		public virtual int RarityMinusTwo() => GetRarity(Type).Type;
	}
}
