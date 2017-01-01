using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

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
		internal string flameTexture = "";
		public bool projOnSwing = false;
		public int bossBagNPC;

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

		public virtual void AutoloadFlame(ref string texture)
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

		public virtual void GetWeaponDamage(Player player, ref int damage)
		{
		}

		public virtual void GetWeaponKnockback(Player player, ref float knockback)
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

		public virtual bool? CanHitNPC(Player player, NPC target)
		{
			return null;
		}

		public virtual void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
		{
		}

		public virtual void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
		{
		}

		public virtual bool CanHitPvp(Player player, Player target)
		{
			return true;
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

		public virtual bool AltFunctionUse(Player player)
		{
			return false;
		}

		public virtual void UpdateInventory(Player player)
		{
		}

		public virtual void UpdateEquip(Player player)
		{
		}

		public virtual void UpdateAccessory(Player player, bool hideVisual)
		{
		}

		public virtual void UpdateVanity(Player player, EquipType type)
		{
		}

		public virtual bool IsArmorSet(Item head, Item body, Item legs)
		{
			return false;
		}

		public virtual void UpdateArmorSet(Player player)
		{
		}

		public virtual bool IsVanitySet(int head, int body, int legs)
		{
			Item headItem = new Item();
			if (head >= 0)
			{
				headItem.SetDefaults(Item.headType[head], true);
			}
			Item bodyItem = new Item();
			if (body >= 0)
			{
				bodyItem.SetDefaults(Item.bodyType[body], true);
			}
			Item legItem = new Item();
			if (legs >= 0)
			{
				legItem.SetDefaults(Item.legType[legs], true);
			}
			return IsArmorSet(headItem, bodyItem, legItem);
		}

		public virtual void PreUpdateVanitySet(Player player)
		{
		}

		public virtual void UpdateVanitySet(Player player)
		{
		}

		public virtual void ArmorSetShadows(Player player)
		{
		}

		public virtual void SetMatch(bool male, ref int equipSlot, ref bool robes)
		{
		}

		public virtual bool CanRightClick()
		{
			return false;
		}

		public virtual void RightClick(Player player)
		{
		}

		public virtual void OpenBossBag(Player player)
		{
		}

		public virtual void PreReforge()
		{
		}

		public virtual void PostReforge()
		{
		}

		public virtual void DrawHands(ref bool drawHands, ref bool drawArms)
		{
		}

		public virtual void DrawHair(ref bool drawHair, ref bool drawAltHair)
		{
		}

		public virtual bool DrawHead()
		{
			return true;
		}

		public virtual bool DrawBody()
		{
			return true;
		}

		public virtual bool DrawLegs()
		{
			return true;
		}

		public virtual void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor)
		{
		}

		public virtual void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color)
		{
		}

		[method: Obsolete("Use the overloaded method with the player parameter.")]
		public virtual void VerticalWingSpeeds(ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
		}

		public virtual void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
	ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			VerticalWingSpeeds(ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
		}

		[method: Obsolete("Use the overloaded method with the player parameter.")]
		public virtual void HorizontalWingSpeeds(ref float speed, ref float acceleration)
		{
		}

		public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
		{
			HorizontalWingSpeeds(ref speed, ref acceleration);
		}

		[method: Obsolete("WingUpdate will return a bool value later. (Use NewWingUpdate in the meantime.) False will keep everything the same. True, you need to handle all animations in your own code.")]
		public virtual void WingUpdate(Player player, bool inUse)
		{
		}

		public virtual bool NewWingUpdate(Player player, bool inUse)
		{
			return false;
		}

		public virtual void Update(ref float gravity, ref float maxFallSpeed)
		{
		}

		public virtual void PostUpdate()
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

		public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			return true;
		}

		public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
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

		public virtual Vector2? HoldoutOffset()
		{
			return null;
		}

		public virtual Vector2? HoldoutOrigin()
		{
			return null;
		}

		public virtual bool CanEquipAccessory(Player player, int slot)
		{
			return true;
		}

		public virtual void ExtractinatorUse(ref int resultType, ref int resultStack)
		{
		}

		public virtual void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick)
		{
		}

		public virtual void CaughtFishStack(ref int stack)
		{
		}

		public virtual bool IsQuestFish()
		{
			return false;
		}

		public virtual bool IsAnglerQuestAvailable()
		{
			return true;
		}

		public virtual void AnglerQuestChat(ref string description, ref string catchLocation)
		{
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
			ModItem newItem = Clone(item);
			newItem.item = item;
			item.modItem = newItem;
			newItem.mod = mod;
		}

		internal void SetupClone(Item clone)
		{
			ModItem newItem = CloneNewInstances ? Clone(clone) : (ModItem)Activator.CreateInstance(GetType());
			newItem.item = clone;
			newItem.mod = mod;
			newItem.texture = texture;
			newItem.flameTexture = flameTexture;
			newItem.projOnSwing = projOnSwing;
			newItem.bossBagNPC = bossBagNPC;
			clone.modItem = newItem;
		}

		public virtual ModItem Clone(Item item)
		{
			return Clone();
		}

		public virtual ModItem Clone()
		{
			return (ModItem)MemberwiseClone();
		}

		public virtual bool CloneNewInstances => false;

		public virtual TagCompound Save()
		{
			return null;
		}

		public virtual void Load(TagCompound tag)
		{
		}

		public virtual void LoadLegacy(BinaryReader reader)
		{
		}

		public virtual void NetSend(BinaryWriter writer)
		{
		}

		public virtual void NetRecieve(BinaryReader reader)
		{
		}

		public virtual void AddRecipes()
		{
		}

		public virtual void OnCraft(Recipe recipe)
		{
		}

		public virtual void ModifyTooltips(List<TooltipLine> tooltips)
		{
		}
	}
}