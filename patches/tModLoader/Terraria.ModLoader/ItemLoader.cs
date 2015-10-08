using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class ItemLoader
	{
		private static int nextItem = ItemID.Count;
		internal static readonly IDictionary<int, ModItem> items = new Dictionary<int, ModItem>();
		internal static readonly IList<GlobalItem> globalItems = new List<GlobalItem>();
		internal static readonly IList<int> animations = new List<int>();

		internal static int ReserveItemID()
		{
			int reserveID = nextItem;
			nextItem++;
			return reserveID;
		}

		public static ModItem GetItem(int type)
		{
			if (items.ContainsKey(type))
			{
				return items[type];
			}
			else
			{
				return null;
			}
		}

		internal static void ResizeArrays()
		{
			Array.Resize(ref Main.itemTexture, nextItem);
			Array.Resize(ref Main.itemName, nextItem);
			Array.Resize(ref Main.itemFlameLoaded, nextItem);
			Array.Resize(ref Main.itemFlameTexture, nextItem);
			Array.Resize(ref Main.itemAnimations, nextItem);
			Array.Resize(ref Item.staff, nextItem);
			Array.Resize(ref Item.claw, nextItem);
			Array.Resize(ref ItemID.Sets.Deprecated, nextItem);
			Array.Resize(ref ItemID.Sets.NeverShiny, nextItem);
			Array.Resize(ref ItemID.Sets.ItemIconPulse, nextItem);
			Array.Resize(ref ItemID.Sets.ItemNoGravity, nextItem);
			Array.Resize(ref ItemID.Sets.ExtractinatorMode, nextItem);
			Array.Resize(ref ItemID.Sets.StaffMinionSlotsRequired, nextItem);
			Array.Resize(ref ItemID.Sets.ExoticPlantsForDyeTrade, nextItem);
			Array.Resize(ref ItemID.Sets.NebulaPickup, nextItem);
			Array.Resize(ref ItemID.Sets.AnimatesAsSoul, nextItem);
			Array.Resize(ref ItemID.Sets.gunProj, nextItem);
		}

		internal static void Unload()
		{
			items.Clear();
			nextItem = ItemID.Count;
			globalItems.Clear();
			animations.Clear();
		}

		internal static bool IsModItem(Item item)
		{
			return item.type >= ItemID.Count;
		}
		//add to Terraria.Item.Prefix
		internal static bool MeleePrefix(Item item)
		{
			if (item.modItem == null)
			{
				return false;
			}
			return item.maxStack == 1 && item.damage > 0 && item.melee && !item.noUseGraphic && !item.accessory;
		}
		//add to Terraria.Item.Prefix
		internal static bool WeaponPrefix(Item item)
		{
			if (item.modItem == null)
			{
				return false;
			}
			return item.maxStack == 1 && item.damage > 0 && item.melee && item.noUseGraphic && !item.accessory;
		}
		//add to Terraria.Item.Prefix
		internal static bool RangedPrefix(Item item)
		{
			if (item.modItem == null)
			{
				return false;
			}
			return item.maxStack == 1 && item.damage > 0 && item.ranged && !item.accessory;
		}
		//add to Terraria.Item.Prefix
		internal static bool MagicPrefix(Item item)
		{
			if (item.modItem == null)
			{
				return false;
			}
			return item.maxStack == 1 && item.damage > 0 && (item.magic || item.summon) && !item.accessory;
		}
		//in Terraria.Item.SetDefaults get rid of type-too-high check
		//add near end of Terraria.Item.SetDefaults after setting netID
		//in Terraria.Item.SetDefaults move Lang stuff before SetupItem
		internal static void SetupItem(Item item)
		{
			if (IsModItem(item))
			{
				GetItem(item.type).SetupItem(item);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				try
				{
					globalItem.SetDefaults(item);
				}
				catch
				{
					ModLoader.DisableMod(globalItem.mod.file);
					throw;
				}
			}
		}
		//near end of Terraria.Main.DrawItem before default drawing call
		//  if(ItemLoader.animations.Contains(item.type))
		//  { ItemLoader.DrawAnimatedItem(item, color, alpha, rotation, scale); return; }
		internal static void DrawAnimatedItem(Item item, Color color, Color alpha, float rotation, float scale)
		{
			int frameCount = Main.itemAnimations[item.type].FrameCount;
			int frameDuration = Main.itemAnimations[item.type].TicksPerFrame;
			Main.itemFrameCounter[item.whoAmI]++;
			if (Main.itemFrameCounter[item.whoAmI] >= frameDuration)
			{
				Main.itemFrameCounter[item.whoAmI] = 0;
				Main.itemFrame[item.whoAmI]++;
			}
			if (Main.itemFrame[item.whoAmI] >= frameCount)
			{
				Main.itemFrame[item.whoAmI] = 0;
			}
			Rectangle frame = Main.itemTexture[item.type].Frame(1, frameCount, 0, Main.itemFrame[item.whoAmI]);
			float offX = (float)(item.width / 2 - frame.Width / 2);
			float offY = (float)(item.height - frame.Height);
			Main.spriteBatch.Draw(Main.itemTexture[item.type], new Vector2(item.position.X - Main.screenPosition.X + (float)(frame.Width / 2) + offX, item.position.Y - Main.screenPosition.Y + (float)(frame.Height / 2) + offY), new Rectangle?(frame), alpha, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
			if (item.color != default(Color))
			{
				Main.spriteBatch.Draw(Main.itemTexture[item.type], new Vector2(item.position.X - Main.screenPosition.X + (float)(frame.Width / 2) + offX, item.position.Y - Main.screenPosition.Y + (float)(frame.Height / 2) + offY), new Rectangle?(frame), item.GetColor(color), rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
			}
		}

		private static Rectangle AnimatedItemFrame(Item item)
		{
			int frameCount = Main.itemAnimations[item.type].FrameCount;
			int frameDuration = Main.itemAnimations[item.type].TicksPerFrame;
			return Main.itemAnimations[item.type].GetFrame(Main.itemTexture[item.type]);
		}
		//in Terraria.Player.ItemCheck
		//  inside block if (this.controlUseItem && this.itemAnimation == 0 && this.releaseUseItem && item.useStyle > 0)
		//  set initial flag2 to ItemLoader.CanUseItem(item, this)
		internal static bool CanUseItem(Item item, Player player)
		{
			bool flag = true;
			if (IsModItem(item))
			{
				flag = flag && item.modItem.CanUseItem(player);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				flag = flag && globalItem.CanUseItem(item, player);
			}
			return flag;
		}
		//in Terraria.Player.ItemCheck after useStyle if/else chain call ItemLoader.UseStyle(item, this)
		internal static void UseStyle(Item item, Player player)
		{
			if (IsModItem(item))
			{
				item.modItem.UseStyle(player);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.UseStyle(item, player);
			}
		}
		//in Terraria.Player.ItemCheck after holdStyle if/else chain call ItemLoader.HoldStyle(item, this)
		internal static void HoldStyle(Item item, Player player)
		{
			if (!player.pulley && player.itemAnimation <= 0)
			{
				if (IsModItem(item))
				{
					item.modItem.HoldStyle(player);
				}
				foreach (GlobalItem globalItem in globalItems)
				{
					globalItem.HoldStyle(item, player);
				}
			}
		}
		//in Terraria.Player.ItemCheck before this.controlUseItem setting this.releaseUseItem call ItemLoader.HoldItem(item, this)
		internal static void HoldItem(Item item, Player player)
		{
			if (IsModItem(item))
			{
				item.modItem.HoldItem(player);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.HoldItem(item, player);
			}
		}
		//near end of Terraria.Player.PickAmmo before flag2 is checked add
		//  if(!ItemLoader.ConsumeAmmo(sItem, item, this)) { flag2 = true; }
		internal static bool ConsumeAmmo(Item item, Item ammo, Player player)
		{
			if (IsModItem(item) && !item.modItem.ConsumeAmmo(player))
			{
				return false;
			}
			if (IsModItem(ammo) && !ammo.modItem.ConsumeAmmo(player))
			{
				return false;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.ConsumeAmmo(item, player) || !globalItem.ConsumeAmmo(ammo, player))
				{
					return false;
				}
			}
			return true;
		}
		//in Terraria.Player.ItemCheck at end of if/else chain for shooting place if on last else
		//  if(ItemLoader.Shoot(item, this, ref vector2, ref num78, ref num79, ref num71, ref num73, ref num74))
		internal static bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack))
				{
					return false;
				}
			}
			if (IsModItem(item))
			{
				if (!item.modItem.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack))
				{
					return false;
				}
			}
			return true;
		}
		//in Terraria.Player.ItemCheck after end of useStyle if/else chain for melee hitbox
		//  call ItemLoader.UseItemHitbox(item, this, ref r2, ref flag17)
		internal static void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
			if (IsModItem(item))
			{
				item.modItem.UseItemHitbox(player, ref hitbox, ref noHitbox);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
			}
		}
		//in Terraria.Player.ItemCheck after magma stone dust effect for melee weapons
		//  call ItemLoader.MeleeEffects(item, this, r2)
		internal static void MeleeEffects(Item item, Player player, Rectangle hitbox)
		{
			if (IsModItem(item))
			{
				item.modItem.MeleeEffects(player, hitbox);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.MeleeEffects(item, player, hitbox);
			}
		}
		//in Terraria.Player.ItemCheck before checking whether npc type can be hit add
		//  bool? modCanHit = ItemLoader.CanHitNPC(item, this, Main.npc[num292]);
		//  if(modCanHit.HasValue && !modCanHit.Value) { continue; }
		//in if statement afterwards add || (modCanHit.HasValue && modCanHit.Value)
		internal static bool? CanHitNPC(Item item, Player player, NPC target)
		{
			bool? flag = null;
			foreach (GlobalItem globalItem in globalItems)
			{
				bool? canHit = globalItem.CanHitNPC(item, player, target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			if (IsModItem(item))
			{
				bool? canHit = item.modItem.CanHitNPC(player, target);
				if (canHit.HasValue && !canHit.Value)
				{
					return false;
				}
				if (canHit.HasValue)
				{
					flag = canHit.Value;
				}
			}
			return flag;
		}
		//in Terraria.Player.ItemCheck for melee attacks after damage variation
		//  call ItemLoader.ModifyHitNPC(item, this, Main.npc[num292], ref num282, ref num283, ref flag18)
		internal static void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
		{
			if (IsModItem(item))
			{
				item.modItem.ModifyHitNPC(player, target, ref damage, ref knockBack, ref crit);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.ModifyHitNPC(item, player, target, ref damage, ref knockBack, ref crit);
			}
		}
		//in Terraria.Player.ItemCheck for melee attacks before updating informational accessories
		//  call ItemLoader.OnHitNPC(item, this, Main.npc[num292], num295, num283, flag18)
		internal static void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit)
		{
			if (IsModItem(item))
			{
				item.modItem.OnHitNPC(player, target, damage, knockBack, crit);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.OnHitNPC(item, player, target, damage, knockBack, crit);
			}
		}
		//in Terraria.Player.ItemCheck add to beginning of pvp collision check
		internal static bool CanHitPvp(Item item, Player player, Player target)
		{
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.CanHitPvp(item, player, target))
				{
					return false;
				}
			}
			if (IsModItem(item))
			{
				return item.modItem.CanHitPvp(player, target);
			}
			return true;
		}
		//in Terraria.Player.ItemCheck for pvp melee attacks after damage variation
		//  call ItemLoader.ModifyHitPvp(item, this, Main.player[num302], ref num282, ref flag20)
		internal static void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit)
		{
			if (IsModItem(item))
			{
				item.modItem.ModifyHitPvp(player, target, ref damage, ref crit);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.ModifyHitPvp(item, player, target, ref damage, ref crit);
			}
		}
		//in Terraria.Player.ItemCheck for pvp melee attacks before NetMessage stuff
		//  call ItemLoader.OnHitPvp(item, this, Main.player[num302], num304, flag20)
		internal static void OnHitPvp(Item item, Player player, Player target, int damage, bool crit)
		{
			if (IsModItem(item))
			{
				item.modItem.OnHitPvp(player, target, damage, crit);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.OnHitPvp(item, player, target, damage, crit);
			}
		}
		//in Terraria.Player.ItemCheck inside block if (this.itemTime == 0 && this.itemAnimation > 0) before hairDye
		//  call ItemLoader.UseItem(item, this)
		internal static void UseItem(Item item, Player player)
		{
			if (IsModItem(item) && item.modItem.UseItem(player))
			{
				player.itemTime = item.useTime;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (globalItem.UseItem(item, player))
				{
					player.itemTime = item.useTime;
				}
			}
		}
		//near end of Terraria.Player.ItemCheck before flag22 is checked
		//  call ItemLoader.ConsumeItem(item, this, ref flag22)
		internal static void ConsumeItem(Item item, Player player, ref bool consume)
		{
			if (IsModItem(item) && !item.modItem.ConsumeItem(player))
			{
				consume = false;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.ConsumeItem(item, player))
				{
					consume = false;
				}
			}
		}
		//in Terraria.Player.PlayerFrame at end of useStyle if/else chain
		//  call if(ItemLoader.UseItemFrame(this.inventory[this.selectedItem], this)) { return; }
		internal static bool UseItemFrame(Item item, Player player)
		{
			if (IsModItem(item) && item.modItem.UseItemFrame(player))
			{
				return true;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (globalItem.UseItemFrame(item, player))
				{
					return true;
				}
			}
			return false;
		}
		//in Terraria.Player.PlayerFrame at end of holdStyle if statements
		//  call if(ItemLoader.HoldItemFrame(this.inventory[this.selectedItem], this)) { return; }
		internal static bool HoldItemFrame(Item item, Player player)
		{
			if (IsModItem(item) && item.modItem.HoldItemFrame(player))
			{
				return true;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (globalItem.HoldItemFrame(item, player))
				{
					return true;
				}
			}
			return false;
		}
		//place at end of first for loop in Terraria.Player.UpdateEquips
		//  call ItemLoader.UpdateInventory(this.inventory[j], this)
		internal static void UpdateInventory(Item item, Player player)
		{
			if (IsModItem(item))
			{
				item.modItem.UpdateInventory(player);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.UpdateInventory(item, player);
			}
		}
		//place in second for loop of Terraria.Player.UpdateEquips before prefix checking
		//  call ItemLoader.UpdateEquip(this.armor[k], this)
		internal static void UpdateEquip(Item item, Player player)
		{
			if (IsModItem(item))
			{
				item.modItem.UpdateEquip(player);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.UpdateEquip(item, player);
			}
		}
		//place at end of third for loop of Terraria.Player.UpdateEquips
		//  call ItemLoader.UpdateAccessory(this.armor[l], this)
		internal static void UpdateAccessory(Item item, Player player)
		{
			if (IsModItem(item))
			{
				item.modItem.UpdateAccessory(player);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.UpdateAccessory(item, player);
			}
		}
		//at end of Terraria.Player.UpdateArmorSets call ItemLoader.UpdateArmorSet(this, this.armor[0], this.armor[1], this.armor[2])
		internal static void UpdateArmorSet(Player player, Item head, Item body, Item legs)
		{
			if (IsModItem(head) && head.modItem.IsArmorSet(head, body, legs))
			{
				head.modItem.UpdateArmorSet(player);
			}
			if (IsModItem(body) && body.modItem.IsArmorSet(head, body, legs))
			{
				body.modItem.UpdateArmorSet(player);
			}
			if (IsModItem(legs) && legs.modItem.IsArmorSet(head, body, legs))
			{
				legs.modItem.UpdateArmorSet(player);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				string set = globalItem.IsArmorSet(head, body, legs);
				if (set.Length > 0)
				{
					globalItem.UpdateArmorSet(player, set);
				}
			}
		}

		internal static void UpdateArmorSetShadows(Player player, Item head, Item body, Item legs, ref bool longTrail, ref bool smallPulse, ref bool largePulse, ref bool shortTrail)
		{
			if (IsModItem(head) && head.modItem.IsArmorSet(head, body, legs))
			{
				head.modItem.UpdateArmorSetShadows(player, ref longTrail, ref smallPulse, ref largePulse, ref shortTrail);
			}
			if (IsModItem(body) && body.modItem.IsArmorSet(head, body, legs))
			{
				body.modItem.UpdateArmorSetShadows(player, ref longTrail, ref smallPulse, ref largePulse, ref shortTrail);
			}
			if (IsModItem(legs) && legs.modItem.IsArmorSet(head, body, legs))
			{
				legs.modItem.UpdateArmorSetShadows(player, ref longTrail, ref smallPulse, ref largePulse, ref shortTrail);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				string set = globalItem.IsArmorSet(head, body, legs);
				if (set.Length > 0)
				{
					globalItem.UpdateArmorSetShadows(player, set, ref longTrail, ref smallPulse, ref largePulse, ref shortTrail);
				}
			}
		}
		//in Terraria.UI.ItemSlot.RightClick in end of item-opening if/else chain before final else
		//  make else if(ItemLoader.CanRightClick(inv[slot]))
		internal static bool CanRightClick(Item item)
		{
			if (IsModItem(item) && item.modItem.CanRightClick())
			{
				return Main.mouseRight;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (globalItem.CanRightClick(item))
				{
					return Main.mouseRight;
				}
			}
			return false;
		}
		//in Terraria.UI.ItemSlot in block from CanRightClick call ItemLoader.RightClick(inv[slot], player)
		internal static void RightClick(Item item, Player player)
		{
			if (Main.mouseRightRelease)
			{
				if (IsModItem(item))
				{
					item.modItem.RightClick(player);
				}
				foreach (GlobalItem globalItem in globalItems)
				{
					globalItem.RightClick(item, player);
				}
				item.stack--;
				if (item.stack == 0)
				{
					item.SetDefaults(0, false);
				}
				Main.PlaySound(7, -1, -1, 1);
				Main.stackSplit = 30;
				Main.mouseRightRelease = false;
				Recipe.FindRecipes();
			}
		}
		//in Terraria.UI.ItemSlot add this to boss bag check
		internal static bool IsModBossBag(Item item)
		{
			if (IsModItem(item))
			{
				return item.modItem.bossBagNPC > 0;
			}
			return false;
		}
		//in Terraria.Player.OpenBossBag after setting num14 call
		//  ItemLoader.OpenBossBag(type, this, ref num14);
		internal static void OpenBossBag(int type, Player player, ref int npc)
		{
			if (type >= ItemID.Count && items[type].bossBagNPC > 0)
			{
				items[type].OpenBossBag(player);
				npc = items[type].bossBagNPC;
			}
		}
		//in beginning of Terraria.Player.openBag methods add
		//  if(!ItemLoader.PreOpenVanillaBag("bagName", this, arg)) { return; }
		internal static bool PreOpenVanillaBag(string context, Player player, int arg)
		{
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.PreOpenVanillaBag(context, player, arg))
				{
					return false;
				}
			}
			return true;
		}
		//in Terraria.Player.openBag methods after PreOpenVanillaBag if statements
		//  add ItemLoader.OpenVanillaBag("bagname", this, arg);
		internal static void OpenVanillaBag(string context, Player player, int arg)
		{
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.OpenVanillaBag(context, player, arg);
			}
		}
		//in Terraria.Main.DrawPlayerHead after if statement that sets flag2 to true
		//  call ItemLoader.DrawHair(drawPlayer, ref flag, ref flag2)
		//in Terraria.Main.DrawPlayer after if statement that sets flag5 to true
		//  call ItemLoader.DrawHair(drawPlayer, ref flag4, ref flag5)
		internal static void DrawHair(Player player, ref bool drawHair, ref bool drawAltHair)
		{
			Item item = player.armor[10].headSlot >= 0 ? player.armor[10] : player.armor[0];
			if (IsModItem(item))
			{
				item.modItem.DrawHair(ref drawHair, ref drawAltHair);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.DrawHair(item, ref drawHair, ref drawAltHair);
			}
		}
		//in Terraria.Main.DrawPlayerHead in if statement after ItemLoader.DrawHair
		//and in Terraria.Main.DrawPlayer in if (!drawPlayer.invis && drawPlayer.head != 38 && drawPlayer.head != 135)
		//  use && with ItemLoader.DrawHead(drawPlayer)
		internal static bool DrawHead(Player player)
		{
			Item item = player.armor[10].headSlot >= 0 ? player.armor[10] : player.armor[0];
			if (IsModItem(item) && !item.modItem.DrawHead())
			{
				return false;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.DrawHead(item))
				{
					return false;
				}
			}
			return true;
		}

		private static Item GetWing(Player player, bool social = false)
		{
			Item item = null;
			for (int k = 3; k < 8 + player.extraAccessorySlots; k++)
			{
				if (player.armor[k].wingSlot > 0)
				{
					item = player.armor[k];
				}
			}
			if (social)
			{
				for (int k = 13; k < 18 + player.extraAccessorySlots; k++)
				{
					if (player.armor[k].wingSlot > 0)
					{
						item = player.armor[k];
					}
				}
			}
			return item;
		}
		//in Terraria.Player.WingMovement after if statements that set num1-5
		//  call ItemLoader.VerticalWingSpeeds(this, ref num2, ref num5, ref num4, ref num3, ref num)
		internal static void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			Item item = GetWing(player);
			if (item == null)
			{
				return;
			}
			if (IsModItem(item))
			{
				item.modItem.VerticalWingSpeeds(ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
					ref maxAscentMultiplier, ref constantAscend);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.VerticalWingSpeeds(item, ref ascentWhenFalling, ref ascentWhenRising,
					ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
			}
		}
		//in Terraria.Player.Update after wingsLogic if statements modifying accRunSpeed and runAcceleration
		//  call ItemLoader.HorizontalWingSpeeds(this)
		internal static void HorizontalWingSpeeds(Player player)
		{
			Item item = GetWing(player);
			if (item == null)
			{
				return;
			}
			if (IsModItem(item))
			{
				item.modItem.HorizontalWingSpeeds(ref player.accRunSpeed, ref player.runAcceleration);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.HorizontalWingSpeeds(item, ref player.accRunSpeed, ref player.runAcceleration);
			}
		}

		internal static void WingUpdate(Player player, bool inUse)
		{
			Item item = GetWing(player, true);
			if (item == null)
			{
				return;
			}
			if (IsModItem(item))
			{
				item.modItem.WingUpdate(player, inUse);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.WingUpdate(item, player, inUse);
			}
		}
		//in Terraria.Item.UpdateItem before item movement (denoted by ItemID.Sets.ItemNoGravity)
		//  call ItemLoader.Update(this, ref num, ref num2)
		internal static void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			if (IsModItem(item))
			{
				item.modItem.Update(ref gravity, ref maxFallSpeed);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.Update(item, ref gravity, ref maxFallSpeed);
			}
		}
		//in Terraria.Player.GrabItems after increasing grab range add
		//  ItemLoader.GrabRange(Main.item[j], this, ref num);
		internal static void GrabRange(Item item, Player player, ref int grabRange)
		{
			if (IsModItem(item))
			{
				item.modItem.GrabRange(player, ref grabRange);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.GrabRange(item, player, ref grabRange);
			}
		}
		//in Terraria.Player.GrabItems between setting beingGrabbed to true and grab styles add
		//  if(ItemLoader.GrabStyle(Main.item[j], this)) { } else
		internal static bool GrabStyle(Item item, Player player)
		{
			foreach (GlobalItem globalItem in globalItems)
			{
				if (globalItem.GrabStyle(item, player))
				{
					return true;
				}
			}
			if (IsModItem(item))
			{
				return item.modItem.GrabStyle(player);
			}
			return false;
		}
		//in Terraria.Player.GrabItems before special pickup effects add
		//  if(!ItemLoader.OnPickup(Main.item[j], this)) { Main.item[j] = new Item(); continue; }
		internal static bool OnPickup(Item item, Player player)
		{
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.OnPickup(item, player))
				{
					return false;
				}
			}
			if (IsModItem(item))
			{
				return item.modItem.OnPickup(player);
			}
			return true;
		}
		//in Terraria.UI.ItemSlot.GetItemLight remove type too high check
		//in beginning of Terraria.Item.GetAlpha call
		//  Color? modColor = ItemLoader.GetAlpha(this, newColor);
		//  if(modColor.HasValue) { return modColor.Value; }
		internal static Color? GetAlpha(Item item, Color lightColor)
		{
			foreach (GlobalItem globalItem in globalItems)
			{
				Color? color = globalItem.GetAlpha(item, lightColor);
				if (color.HasValue)
				{
					return color;
				}
			}
			if (IsModItem(item))
			{
				return item.modItem.GetAlpha(lightColor);
			}
			return null;
		}
		//in Terraria.Main.DrawItem after ItemSlot.GetItemLight call
		//  if(!ItemLoader.PreDrawInWorld(item, Main.spriteBatch, color, alpha, ref rotation, ref scale)) { return; }
		internal static bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale)
		{
			bool flag = true;
			if (IsModItem(item) && !item.modItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale))
			{
				flag = false;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale))
				{
					flag = false;
				}
			}
			return flag;
		}
		//in Terraria.Main.DrawItem before every return (including for PreDrawInWorld) and at end of method call
		//  ItemLoader.PostDrawInWorld(item, Main.spriteBatch, color, alpha, rotation, scale)
		internal static void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale)
		{
			if (IsModItem(item))
			{
				item.modItem.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale);
			}
		}
		//in Terraria.UI.ItemSlot.Draw place item-drawing code inside if statement
		//  if(ItemLoader.PreDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
		//    item.GetColor(color), origin, num4 * num3))
		internal static bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			bool flag = true;
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale))
				{
					flag = false;
				}
			}
			if (IsModItem(item) && !item.modItem.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
			{
				flag = false;
			}
			return flag;
		}
		//in Terraria.UI.ItemSlot.Draw after if statement for PreDrawInInventory call
		//  ItemLoader.PostDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
		//    item.GetColor(color), origin, num4 * num3);
		internal static void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (IsModItem(item))
			{
				item.modItem.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				globalItem.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}
		}
		//in Terraria.UI.ItemSlot.AccCheck replace 2nd and 3rd return false with
		//  return !ItemLoader.CanEquipAccessory(item, slot)
		internal static bool CanEquipAccessory(Item item, int slot)
		{
			Player player = Main.player[Main.myPlayer];
			if (IsModItem(item) && !item.modItem.CanEquipAccessory(player, slot))
			{
				return false;
			}
			foreach (GlobalItem globalItem in globalItems)
			{
				if (!globalItem.CanEquipAccessory(item, player, slot))
				{
					return false;
				}
			}
			return true;
		}
	}
}
