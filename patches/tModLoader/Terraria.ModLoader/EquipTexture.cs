using System;
using Microsoft.Xna.Framework;
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

		public virtual void ArmorSetShadows(Player player)
		{
			if (item != null)
			{
				item.ArmorSetShadows(player);
			}
		}

		public virtual void SetMatch(bool male, ref int equipSlot, ref bool robes)
		{
			if (item != null)
			{
				item.SetMatch(male, ref equipSlot, ref robes);
			}
		}

		public virtual void DrawHands(ref bool drawHands, ref bool drawArms)
		{
			if (item != null)
			{
				item.DrawHands(ref drawHands, ref drawArms);
			}
		}

		public virtual void DrawHair(ref bool drawHair, ref bool drawAltHair)
		{
			if (item != null)
			{
				item.DrawHair(ref drawHair, ref drawAltHair);
			}
		}

		public virtual bool DrawHead()
		{
			return item == null || item.DrawHead();
		}

		public virtual bool DrawBody()
		{
			return item == null || item.DrawBody();
		}

		public virtual bool DrawLegs()
		{
			return item == null || item.DrawLegs();
		}

		public virtual void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
		{
			if (item != null)
			{
				item.DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
			}
		}

		public virtual void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color)
		{
			if (item != null)
			{
				item.ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);
			}
		}

		[method: Obsolete("WingUpdate will return a bool value later. (Use NewWingUpdate in the meantime.) False will keep everything the same. True, you need to handle all animations in your own code.")]
		public virtual void WingUpdate(Player player, bool inUse)
		{
			if (item != null)
			{
				item.WingUpdate(player, inUse);
			}
		}

		public virtual bool NewWingUpdate(Player player, bool inUse)
		{
			if (item != null)
			{
				if (item.NewWingUpdate(player, inUse))
				{
					return true;
				}
				WingUpdate(player, inUse);
			}
			return false;
		}
	}
}
