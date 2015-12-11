using System;
using Terraria;

namespace Terraria.ModLoader
{
	public class EquipTexture
	{
		public string Texture
		{
			get;
			internal set;
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

		public EquipType Type
		{
			get;
			internal set;
		}

		public int Slot
		{
			get;
			internal set;
		}

		public ModItem item
		{
			get;
			internal set;
		}

		public virtual void UpdateVanity(Player player, EquipType type)
		{
			if (item != null)
			{
				item.UpdateVanity(player, type);
			}
		}

		public virtual bool IsVanitySet(int head, int body, int legs)
		{
			if (item == null)
			{
				return false;
			}
			return item.IsVanitySet(head, body, legs);
		}

		public virtual void PreUpdateVanitySet(Player player)
		{
			if (item != null)
			{
				item.PreUpdateVanitySet(player);
			}
		}

		public virtual void UpdateVanitySet(Player player)
		{
			if (item != null)
			{
				item.UpdateVanitySet(player);
			}
		}

		public virtual void ArmorSetShadows(Player player, ref bool longTrail, ref bool smallPulse, ref bool largePulse, ref bool shortTrail)
		{
			if (item != null)
			{
				item.ArmorSetShadows(player, ref longTrail, ref smallPulse, ref largePulse, ref shortTrail);
			}
		}
	}
}
