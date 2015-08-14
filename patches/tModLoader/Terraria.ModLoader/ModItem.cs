using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class ModItem
	{
		//add modItem property to Terraria.Item (internal set)
		//set modItem to null at beginning of Terraria.Item.ResetStats
		public Item item
		{
			get;
			internal set;
		}

		public Mod mod
		{
			get;
			internal set;
		}

		internal string texture;

		public ModItem()
		{
			item = new Item();
			item.modItem = this;
		}

		public void AddTooltip(string tooltip)
		{
			if (item.toolTip == null || item.toolTip.Length == 0)
			{
				item.toolTip = tooltip;
			}
			else
			{
				item.toolTip += Environment.NewLine + tooltip;
			}
		}

		public void AddTooltip2(string tooltip)
		{
			if (item.toolTip2 == null || item.toolTip2.Length == 0)
			{
				item.toolTip2 = tooltip;
			}
			else
			{
				item.toolTip2 += Environment.NewLine + tooltip;
			}
		}

		public virtual bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			return mod.Properties.Autoload;
		}

		public virtual void AutoloadEquip(EquipType equip, ref string texture, ref string armTexture, ref string femaleTexture)
		{
		}

		public virtual DrawAnimation GetAnimation()
		{
			return null;
		}

		public virtual void SetDefaults()
		{
		}

		public virtual bool CanUseItem(Player player)
		{
			return true;
		}

		public virtual void UseStyle(Player player)
		{
		}

		public virtual void HoldStyle(Player player)
		{
		}

		public virtual void HoldItem(Player player)
		{
		}

		public virtual bool ConsumeAmmo(Player player)
		{
			return true;
		}

		public virtual bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			return true;
		}

		public virtual void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
		}

		public virtual void MeleeEffects(Player player, Rectangle hitbox)
		{
		}

		public virtual void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
		{
		}

		public virtual void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
		{
		}

		public virtual void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
		{
		}

		public virtual void OnHitPvp(Player player, Player target, int damage, bool crit)
		{
		}

		public virtual bool UseItem(Player player)
		{
			return false;
		}

		public virtual bool ConsumeItem(Player player)
		{
			return true;
		}

		public virtual bool UseItemFrame(Player player)
		{
			return false;
		}

		public virtual bool HoldItemFrame(Player player)
		{
			return false;
		}

		public virtual void UpdateInventory(Player player)
		{
		}

		public virtual void UpdateEquip(Player player)
		{
		}

		public virtual void UpdateAccessory(Player player)
		{
		}

		public virtual bool IsArmorSet(Item head, Item body, Item legs)
		{
			return false;
		}

		public virtual void UpdateArmorSet(Player player)
		{
		}

		public virtual bool CanRightClick()
		{
			return false;
		}

		public virtual void RightClick(Player player)
		{
		}

		public virtual void DrawHair(ref bool drawHair, ref bool drawAltHair)
		{
		}

		public virtual bool DrawHead()
		{
			return true;
		}

		public virtual void VerticalWingSpeeds(ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
		}

		public virtual void HorizontalWingSpeeds(ref float speed, ref float acceleration)
		{
		}

		public virtual void Update(ref float gravity, ref float maxFallSpeed)
		{
		}

		public virtual void GrabRange(Player player, ref int grabRange)
		{
		}

		public virtual bool GrabStyle(Player player)
		{
			return false;
		}

		public virtual bool OnPickup(Player player)
		{
			return true;
		}

		public virtual Color? GetAlpha(Color lightColor)
		{
			return null;
		}

		public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale)
		{
			return true;
		}

		public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale)
		{
		}

		public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
			Color itemColor, Vector2 origin, float scale)
		{
			return true;
		}

		public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
			Color itemColor, Vector2 origin, float scale)
		{
		}

		public virtual bool CanEquipAccessory(Player player, int slot)
		{
			return true;
		}

		internal void SetupItem(Item item)
		{
			SetupModItem(item);
			EquipLoader.SetSlot(item);
			item.modItem.SetDefaults();
		}
		//change Terraria.Item.Clone
		//  Item newItem = (Item)base.MemberwiseClone();
		//  if (newItem.type >= ItemID.Count)
		//  {
		//      ItemLoader.GetItem(newItem.type).SetupModItem(newItem);
		//  }
		//  return newItem;
		internal void SetupModItem(Item item)
		{
			ModItem newItem = (ModItem)Activator.CreateInstance(GetType());
			newItem.item = item;
			item.modItem = newItem;
			newItem.mod = mod;
		}

		public virtual void SaveCustomData(BinaryWriter writer)
		{
		}

		public virtual void LoadCustomData(BinaryReader reader)
		{
		}

		public virtual void AddRecipes()
		{
		}
	}
}