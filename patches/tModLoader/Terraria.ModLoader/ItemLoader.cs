using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central class from which item-related functions are carried out. It also stores a list of mod items by ID.
	/// </summary>
	public static class ItemLoader
	{
		private static int nextItem = ItemID.Count;
		internal static readonly IList<ModItem> items = new List<ModItem>();
		internal static readonly IList<GlobalItem> globalItems = new List<GlobalItem>();
		internal static GlobalItem[] NetGlobals;
		internal static readonly IList<ItemInfo> infoList = new List<ItemInfo>();
		internal static readonly IDictionary<string, int> infoIndexes = new Dictionary<string, int>();
		internal static readonly IList<int> animations = new List<int>();
		internal static readonly int vanillaQuestFishCount = Main.anglerQuestItemNetIDs.Length;
		internal static readonly IList<int> questFish = new List<int>();
		internal static readonly int[] vanillaWings = new int[Main.maxWings];

		private static Action<Item>[] HookSetDefaults = new Action<Item>[0];
		private static Func<Item, Player, bool>[] HookCanUseItem;
		private static Action<Item, Player>[] HookUseStyle;
		private static Action<Item, Player>[] HookHoldStyle;
		private static Action<Item, Player>[] HookHoldItem;
		private static Func<Item, Player, float>[] HookUseTimeMultiplier;
		private static Func<Item, Player, float>[] HookMeleeSpeedMultiplier;
		private delegate void DelegateGetWeaponDamage(Item item, Player player, ref int damage);
		private static DelegateGetWeaponDamage[] HookGetWeaponDamage;
		private delegate void DelegateGetWeaponKnockback(Item item, Player player, ref float knockback);
		private static DelegateGetWeaponKnockback[] HookGetWeaponKnockback;
		private delegate void DelegatePickAmmo(Item item, Player player, ref int type, ref float speed, ref int damage, ref float knockback);
		private static DelegatePickAmmo[] HookPickAmmo;
		private static Func<Item, Player, bool>[] HookConsumeAmmo;
		private delegate bool DelegateShoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack);
		private static DelegateShoot[] HookShoot;
		private delegate void DelegateUseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox);
		private static DelegateUseItemHitbox[] HookUseItemHitbox;
		private static Action<Item, Player, Rectangle>[] HookMeleeEffects;
		private static Func<Item, Player, NPC, bool?>[] HookCanHitNPC;
		private delegate void DelegateModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit);
		private static DelegateModifyHitNPC[] HookModifyHitNPC;
		private static Action<Item, Player, NPC, int, float, bool>[] HookOnHitNPC;
		private static Func<Item, Player, Player, bool>[] HookCanHitPvp;
		private delegate void DelegateModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit);
		private static DelegateModifyHitPvp[] HookModifyHitPvp;
		private static Action<Item, Player, Player, int, bool>[] HookOnHitPvp;
		private static Func<Item, Player, bool>[] HookUseItem;
		private static Func<Item, Player, bool>[] HookConsumeItem;
		private static Func<Item, Player, bool>[] HookUseItemFrame;
		private static Func<Item, Player, bool>[] HookHoldItemFrame;
		private static Func<Item, Player, bool>[] HookAltFunctionUse;
		private static Action<Item, Player>[] HookUpdateInventory;
		private static Action<Item, Player>[] HookUpdateEquip;
		private static Action<Item, Player, bool>[] HookUpdateAccessory;
		private static GlobalItem[] HookUpdateArmorSet;
		private static GlobalItem[] HookPreUpdateVanitySet;
		private static GlobalItem[] HookUpdateVanitySet;
		private static GlobalItem[] HookArmorSetShadows;
		private delegate void DelegateSetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes);
		private static DelegateSetMatch[] HookSetMatch;
		private static Func<Item, bool>[] HookCanRightClick;
		private static Action<Item, Player>[] HookRightClick;
		private static Func<string, Player, int, bool>[] HookPreOpenVanillaBag;
		private static Action<string, Player, int>[] HookOpenVanillaBag;
		private static Action<Item>[] HookPreReforge;
		private static Action<Item>[] HookPostReforge;
		private delegate void DelegateDrawHands(int body, ref bool drawHands, ref bool drawArms);
		private static DelegateDrawHands[] HookDrawHands;
		private delegate void DelegateDrawHair(int body, ref bool drawHair, ref bool drawAltHair);
		private static DelegateDrawHair[] HookDrawHair;
		private static Func<int, bool>[] HookDrawHead;
		private static Func<int, bool>[] HookDrawBody;
		private static Func<int, int, bool>[] HookDrawLegs;
		private delegate void DelegateDrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor);
		private static DelegateDrawArmorColor[] HookDrawArmorColor;
		private delegate void DelegateArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color);
		private static DelegateArmorArmGlowMask[] HookArmorArmGlowMask;
		private delegate void DelegateVerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend);
		private static DelegateVerticalWingSpeeds[] HookVerticalWingSpeeds;
		private delegate void DelegateHorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration);
		private static DelegateHorizontalWingSpeeds[] HookHorizontalWingSpeeds;
		private static Func<int, Player, bool, bool>[] HookWingUpdate;
		private delegate void DelegateUpdate(Item item, ref float gravity, ref float maxFallSpeed);
		private static DelegateUpdate[] HookUpdate;
		private static Action<Item>[] HookPostUpdate;
		private delegate void DelegateGrabRange(Item item, Player player, ref int grabRange);
		private static DelegateGrabRange[] HookGrabRange;
		private static Func<Item, Player, bool>[] HookGrabStyle;
		private static Func<Item, Player, bool>[] HookCanPickup;
		private static Func<Item, Player, bool>[] HookOnPickup;
		private static Func<Item, Player, bool>[] HookExtraPickupSpace;
		private static Func<Item, Color, Color?>[] HookGetAlpha;
		private delegate bool DelegatePreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI);
		private static DelegatePreDrawInWorld[] HookPreDrawInWorld;
		private static Action<Item, SpriteBatch, Color, Color, float, float, int>[] HookPostDrawInWorld;
		private static Func<Item, SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float, bool>[] HookPreDrawInInventory;
		private static Action<Item, SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float>[] HookPostDrawInInventory;
		private static Func<int, Vector2?>[] HookHoldoutOffset;
		private static Func<int, Vector2?>[] HookHoldoutOrigin;
		private static Func<Item, Player, int, bool>[] HookCanEquipAccessory;
		private delegate void DelegateExtractinatorUse(int extractType, ref int resultType, ref int resultStack);
		private static DelegateExtractinatorUse[] HookExtractinatorUse;
		private delegate void DelegateCaughtFishStack(int type, ref int stack);
		private static DelegateCaughtFishStack[] HookCaughtFishStack;
		private static Func<int, bool>[] HookIsAnglerQuestAvailable;
		private delegate void DelegateAnglerChat(int type, ref string chat, ref string catchLocation);
		private static DelegateAnglerChat[] HookAnglerChat;
		private static Action<Item, Recipe>[] HookOnCraft;
		private static Action<Item, List<TooltipLine>>[] HookModifyTooltips;
		private static Func<Item, bool>[] HookNeedsSaving;
		private static Action<Item>[] HookPreSaveCustomData;

		static ItemLoader()
		{
			for (int k = 0; k < ItemID.Count; k++)
			{
				Item item = new Item();
				item.SetDefaults(k);
				if (item.wingSlot > 0)
				{
					vanillaWings[item.wingSlot] = k;
				}
			}
		}

		internal static int ReserveItemID()
		{
			if (ModNet.AllowVanillaClients) throw new Exception("Adding items breaks vanilla client compatiblity");

			int reserveID = nextItem;
			nextItem++;
			return reserveID;
		}

		/// <summary>
		/// Gets the ModItem instance corresponding to the specified type. Returns null if no modded item has the given type.
		/// </summary>
		public static ModItem GetItem(int type)
		{
			return type >= ItemID.Count && type < ItemCount ? items[type - ItemID.Count] : null;
		}

		internal static int ItemCount => nextItem;

		internal static void ResizeArrays()
		{
			Array.Resize(ref Main.itemTexture, nextItem);
			Array.Resize(ref Main.itemFlameLoaded, nextItem);
			Array.Resize(ref Main.itemFlameTexture, nextItem);
			Array.Resize(ref Main.itemAnimations, nextItem);
			Array.Resize(ref Item.itemCaches, nextItem);
			Array.Resize(ref Item.staff, nextItem);
			Array.Resize(ref Item.claw, nextItem);
			Array.Resize(ref ItemID.Sets.BannerStrength, nextItem);
			Array.Resize(ref ItemID.Sets.KillsToBanner, nextItem);
			Array.Resize(ref ItemID.Sets.CanFishInLava, nextItem);
			//Array.Resize(ref ItemID.Sets.TextureCopyLoad, nextItem); //not needed?
			Array.Resize(ref ItemID.Sets.TrapSigned, nextItem);
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
			Array.Resize(ref ItemID.Sets.SortingPriorityBossSpawns, nextItem);
			Array.Resize(ref ItemID.Sets.SortingPriorityWiring, nextItem);
			Array.Resize(ref ItemID.Sets.SortingPriorityMaterials, nextItem);
			Array.Resize(ref ItemID.Sets.SortingPriorityExtractibles, nextItem);
			Array.Resize(ref ItemID.Sets.SortingPriorityRopes, nextItem);
			Array.Resize(ref ItemID.Sets.SortingPriorityPainting, nextItem);
			Array.Resize(ref ItemID.Sets.SortingPriorityTerraforming, nextItem);
			Array.Resize(ref ItemID.Sets.GamepadExtraRange, nextItem);
			Array.Resize(ref ItemID.Sets.GamepadWholeScreenUseRange, nextItem);
			Array.Resize(ref ItemID.Sets.GamepadSmartQuickReach, nextItem);
			Array.Resize(ref ItemID.Sets.Yoyo, nextItem);
			Array.Resize(ref ItemID.Sets.AlsoABuildingItem, nextItem);
			Array.Resize(ref ItemID.Sets.LockOnIgnoresCollision, nextItem);
			Array.Resize(ref ItemID.Sets.LockOnAimAbove, nextItem);
			Array.Resize(ref ItemID.Sets.LockOnAimCompensation, nextItem);
			Array.Resize(ref ItemID.Sets.SingleUseInGamepad, nextItem);
			for (int k = ItemID.Count; k < nextItem; k++)
			{
				ItemID.Sets.BannerStrength[k] = new ItemID.BannerEffect(1f);
				ItemID.Sets.KillsToBanner[k] = 50;
				Item.itemCaches[k] = -1;
				//ItemID.Sets.TextureCopyLoad[k] = -1;
				ItemID.Sets.ExtractinatorMode[k] = -1;
				ItemID.Sets.StaffMinionSlotsRequired[k] = 1;
				ItemID.Sets.SortingPriorityBossSpawns[k] = -1;
				ItemID.Sets.SortingPriorityWiring[k] = -1;
				ItemID.Sets.SortingPriorityMaterials[k] = -1;
				ItemID.Sets.SortingPriorityExtractibles[k] = -1;
				ItemID.Sets.SortingPriorityRopes[k] = -1;
				ItemID.Sets.SortingPriorityPainting[k] = -1;
				ItemID.Sets.SortingPriorityTerraforming[k] = -1;
			}
			Array.Resize(ref Main.anglerQuestItemNetIDs, vanillaQuestFishCount + questFish.Count);
			for (int k = 0; k < questFish.Count; k++)
			{
				Main.anglerQuestItemNetIDs[vanillaQuestFishCount + k] = questFish[k];
			}
			
			NetGlobals = ModLoader.BuildGlobalHook<GlobalItem, Action<Item, BinaryWriter>>(globalItems, g => g.NetSend);

			ModLoader.BuildGlobalHook(ref HookSetDefaults, globalItems, g => g.SetDefaults);
			ModLoader.BuildGlobalHook(ref HookCanUseItem, globalItems, g => g.CanUseItem);
			ModLoader.BuildGlobalHook(ref HookCanUseItem, globalItems, g => g.CanUseItem);
			ModLoader.BuildGlobalHook(ref HookUseStyle, globalItems, g => g.UseStyle);
			ModLoader.BuildGlobalHook(ref HookHoldStyle, globalItems, g => g.HoldStyle);
			ModLoader.BuildGlobalHook(ref HookHoldItem, globalItems, g => g.HoldItem);
			ModLoader.BuildGlobalHook(ref HookUseTimeMultiplier, globalItems, g => g.UseTimeMultiplier);
			ModLoader.BuildGlobalHook(ref HookMeleeSpeedMultiplier, globalItems, g => g.MeleeSpeedMultiplier);
			ModLoader.BuildGlobalHook(ref HookGetWeaponDamage, globalItems, g => g.GetWeaponDamage);
			ModLoader.BuildGlobalHook(ref HookGetWeaponKnockback, globalItems, g => g.GetWeaponKnockback);
			ModLoader.BuildGlobalHook(ref HookPickAmmo, globalItems, g => g.PickAmmo);
			ModLoader.BuildGlobalHook(ref HookConsumeAmmo, globalItems, g => g.ConsumeAmmo);
			ModLoader.BuildGlobalHook(ref HookShoot, globalItems, g => g.Shoot);
			ModLoader.BuildGlobalHook(ref HookUseItemHitbox, globalItems, g => g.UseItemHitbox);
			ModLoader.BuildGlobalHook(ref HookMeleeEffects, globalItems, g => g.MeleeEffects);
			ModLoader.BuildGlobalHook(ref HookCanHitNPC, globalItems, g => g.CanHitNPC);
			ModLoader.BuildGlobalHook(ref HookModifyHitNPC, globalItems, g => g.ModifyHitNPC);
			ModLoader.BuildGlobalHook(ref HookOnHitNPC, globalItems, g => g.OnHitNPC);
			ModLoader.BuildGlobalHook(ref HookCanHitPvp, globalItems, g => g.CanHitPvp);
			ModLoader.BuildGlobalHook(ref HookModifyHitPvp, globalItems, g => g.ModifyHitPvp);
			ModLoader.BuildGlobalHook(ref HookOnHitPvp, globalItems, g => g.OnHitPvp);
			ModLoader.BuildGlobalHook(ref HookUseItem, globalItems, g => g.UseItem);
			ModLoader.BuildGlobalHook(ref HookConsumeItem, globalItems, g => g.ConsumeItem);
			ModLoader.BuildGlobalHook(ref HookUseItemFrame, globalItems, g => g.UseItemFrame);
			ModLoader.BuildGlobalHook(ref HookHoldItemFrame, globalItems, g => g.HoldItemFrame);
			ModLoader.BuildGlobalHook(ref HookAltFunctionUse, globalItems, g => g.AltFunctionUse);
			ModLoader.BuildGlobalHook(ref HookUpdateInventory, globalItems, g => g.UpdateInventory);
			ModLoader.BuildGlobalHook(ref HookUpdateEquip, globalItems, g => g.UpdateEquip);
			ModLoader.BuildGlobalHook(ref HookUpdateAccessory, globalItems, g => g.UpdateAccessory);
			HookUpdateArmorSet = ModLoader.BuildGlobalHook<GlobalItem, Action<Player, string>>(globalItems, g => g.UpdateArmorSet);
			HookPreUpdateVanitySet = ModLoader.BuildGlobalHook<GlobalItem, Action<Player, string>>(globalItems, g => g.UpdateArmorSet);
			HookUpdateVanitySet = ModLoader.BuildGlobalHook<GlobalItem, Action<Player, string>>(globalItems, g => g.UpdateArmorSet);
			HookArmorSetShadows = ModLoader.BuildGlobalHook<GlobalItem, Action<Player, string>>(globalItems, g => g.UpdateArmorSet);
			ModLoader.BuildGlobalHook(ref HookSetMatch, globalItems, g => g.SetMatch);
			ModLoader.BuildGlobalHook(ref HookCanRightClick, globalItems, g => g.CanRightClick);
			ModLoader.BuildGlobalHook(ref HookRightClick, globalItems, g => g.RightClick);
			ModLoader.BuildGlobalHook(ref HookPreOpenVanillaBag, globalItems, g => g.PreOpenVanillaBag);
			ModLoader.BuildGlobalHook(ref HookOpenVanillaBag, globalItems, g => g.OpenVanillaBag);
			ModLoader.BuildGlobalHook(ref HookPreReforge, globalItems, g => g.PreReforge);
			ModLoader.BuildGlobalHook(ref HookPostReforge, globalItems, g => g.PostReforge);
			ModLoader.BuildGlobalHook(ref HookDrawHands, globalItems, g => g.DrawHands);
			ModLoader.BuildGlobalHook(ref HookDrawHair, globalItems, g => g.DrawHair);
			ModLoader.BuildGlobalHook(ref HookDrawHead, globalItems, g => g.DrawHead);
			ModLoader.BuildGlobalHook(ref HookDrawBody, globalItems, g => g.DrawBody);
			ModLoader.BuildGlobalHook(ref HookDrawLegs, globalItems, g => g.DrawLegs);
			ModLoader.BuildGlobalHook(ref HookDrawArmorColor, globalItems, g => g.DrawArmorColor);
			ModLoader.BuildGlobalHook(ref HookArmorArmGlowMask, globalItems, g => g.ArmorArmGlowMask);
			ModLoader.BuildGlobalHook(ref HookVerticalWingSpeeds, globalItems, g => g.VerticalWingSpeeds);
			ModLoader.BuildGlobalHook(ref HookHorizontalWingSpeeds, globalItems, g => g.HorizontalWingSpeeds);
			ModLoader.BuildGlobalHook(ref HookWingUpdate, globalItems, g => g.WingUpdate);
			ModLoader.BuildGlobalHook(ref HookUpdate, globalItems, g => g.Update);
			ModLoader.BuildGlobalHook(ref HookPostUpdate, globalItems, g => g.PostUpdate);
			ModLoader.BuildGlobalHook(ref HookGrabRange, globalItems, g => g.GrabRange);
			ModLoader.BuildGlobalHook(ref HookGrabStyle, globalItems, g => g.GrabStyle);
			ModLoader.BuildGlobalHook(ref HookCanPickup, globalItems, g => g.CanPickup);
			ModLoader.BuildGlobalHook(ref HookOnPickup, globalItems, g => g.OnPickup);
			ModLoader.BuildGlobalHook(ref HookExtraPickupSpace, globalItems, g => g.ExtraPickupSpace);
			ModLoader.BuildGlobalHook(ref HookGetAlpha, globalItems, g => g.GetAlpha);
			ModLoader.BuildGlobalHook(ref HookPreDrawInWorld, globalItems, g => g.PreDrawInWorld);
			ModLoader.BuildGlobalHook(ref HookPostDrawInWorld, globalItems, g => g.PostDrawInWorld);
			ModLoader.BuildGlobalHook(ref HookPreDrawInInventory, globalItems, g => g.PreDrawInInventory);
			ModLoader.BuildGlobalHook(ref HookPostDrawInInventory, globalItems, g => g.PostDrawInInventory);
			ModLoader.BuildGlobalHook(ref HookHoldoutOffset, globalItems, g => g.HoldoutOffset);
			ModLoader.BuildGlobalHook(ref HookHoldoutOrigin, globalItems, g => g.HoldoutOrigin);
			ModLoader.BuildGlobalHook(ref HookCanEquipAccessory, globalItems, g => g.CanEquipAccessory);
			ModLoader.BuildGlobalHook(ref HookExtractinatorUse, globalItems, g => g.ExtractinatorUse);
			ModLoader.BuildGlobalHook(ref HookCaughtFishStack, globalItems, g => g.CaughtFishStack);
			ModLoader.BuildGlobalHook(ref HookIsAnglerQuestAvailable, globalItems, g => g.IsAnglerQuestAvailable);
			ModLoader.BuildGlobalHook(ref HookAnglerChat, globalItems, g => g.AnglerChat);
			ModLoader.BuildGlobalHook(ref HookOnCraft, globalItems, g => g.OnCraft);
			ModLoader.BuildGlobalHook(ref HookModifyTooltips, globalItems, g => g.ModifyTooltips);
			ModLoader.BuildGlobalHook(ref HookNeedsSaving, globalItems, g => g.NeedsSaving);
		}

		internal static void Unload()
		{
			items.Clear();
			nextItem = ItemID.Count;
			globalItems.Clear();
			infoList.Clear();
			infoIndexes.Clear();
			animations.Clear();
			questFish.Clear();
		}

		internal static bool IsModItem(Item item)
		{
			return item.type >= ItemID.Count;
		}

		private static bool GeneralPrefix(Item item)
		{
			return item.maxStack == 1 && item.damage > 0 && item.ammo == 0 && !item.accessory;
		}
		//add to Terraria.Item.Prefix
		internal static bool MeleePrefix(Item item)
		{
			return item.modItem != null && GeneralPrefix(item) && item.melee && !item.noUseGraphic;
		}
		//add to Terraria.Item.Prefix
		internal static bool WeaponPrefix(Item item)
		{
			return item.modItem != null && GeneralPrefix(item) && item.melee && item.noUseGraphic;
		}
		//add to Terraria.Item.Prefix
		internal static bool RangedPrefix(Item item)
		{
			return item.modItem != null && GeneralPrefix(item) && item.ranged;
		}
		//add to Terraria.Item.Prefix
		internal static bool MagicPrefix(Item item)
		{
			return item.modItem != null && GeneralPrefix(item) && (item.magic || item.summon);
		}
		//in Terraria.Item.SetDefaults get rid of type-too-high check
		//add near end of Terraria.Item.SetDefaults after setting netID
		//in Terraria.Item.SetDefaults move Lang stuff before SetupItem
		internal static void SetupItem(Item item)
		{
			SetupItemInfo(item);
			if (IsModItem(item))
			{
				GetItem(item.type).SetupItem(item);
			}
			foreach (var hook in HookSetDefaults)
			{
				hook(item);
			}
		}

		internal static void SetupItemInfo(Item item)
		{
			item.itemInfo = infoList.Select(info => info.Clone()).ToArray();
		}

		internal static ItemInfo GetItemInfo(Item item, Mod mod, string name)
		{
			int index;
			return infoIndexes.TryGetValue(mod.Name + ':' + name, out index) ? item.itemInfo[index] : null;
		}
		//near end of Terraria.Main.DrawItem before default drawing call
		//  if(ItemLoader.animations.Contains(item.type))
		//  { ItemLoader.DrawAnimatedItem(item, whoAmI, color, alpha, rotation, scale); return; }
		internal static void DrawAnimatedItem(Item item, int whoAmI, Color color, Color alpha, float rotation, float scale)
		{
			int frameCount = Main.itemAnimations[item.type].FrameCount;
			int frameDuration = Main.itemAnimations[item.type].TicksPerFrame;
			Main.itemFrameCounter[whoAmI]++;
			if (Main.itemFrameCounter[whoAmI] >= frameDuration)
			{
				Main.itemFrameCounter[whoAmI] = 0;
				Main.itemFrame[whoAmI]++;
			}
			if (Main.itemFrame[whoAmI] >= frameCount)
			{
				Main.itemFrame[whoAmI] = 0;
			}
			Rectangle frame = Main.itemTexture[item.type].Frame(1, frameCount, 0, Main.itemFrame[whoAmI]);
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
		/// <summary>
		/// Returns the "and" operation on the results of ModItem.CanUseItem and all GlobalItem.CanUseItem hooks.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="player">The player holding the item.</param>
		public static bool CanUseItem(Item item, Player player)
		{
			bool flag = true;
			if (item.modItem != null)
			{
				flag &= item.modItem.CanUseItem(player);
			}
			foreach (var hook in HookCanUseItem)
			{
				flag &= hook(item, player);
			}

			return flag;
		}

		//in Terraria.Player.ItemCheck after useStyle if/else chain call ItemLoader.UseStyle(item, this)
		/// <summary>
		/// Calls ModItem.UseStyle and all GlobalItem.UseStyle hooks.
		/// </summary>
		public static void UseStyle(Item item, Player player)
		{
			if (item.IsAir) return;
			item.modItem?.UseStyle(player);

			foreach (var hook in HookUseStyle)
			{
				hook(item, player);
			}
		}

		//in Terraria.Player.ItemCheck after holdStyle if/else chain call ItemLoader.HoldStyle(item, this)
		/// <summary>
		/// If the player is not holding onto a rope and is not in the middle of using an item, calls ModItem.HoldStyle and all GlobalItem.HoldStyle hooks.
		/// </summary>
		public static void HoldStyle(Item item, Player player)
		{
			if (item.IsAir) return;
			if (!player.pulley && player.itemAnimation <= 0)
			{
				item.modItem?.HoldStyle(player);

				foreach (var hook in HookHoldStyle)
				{
					hook(item, player);
				}
			}
		}

		//in Terraria.Player.ItemCheck before this.controlUseItem setting this.releaseUseItem call ItemLoader.HoldItem(item, this)
		/// <summary>
		/// Calls ModItem.HoldItem and all GlobalItem.HoldItem hooks.
		/// </summary>
		public static void HoldItem(Item item, Player player)
		{
			if (item.IsAir) return;
			item.modItem?.HoldItem(player);

			foreach (var hook in HookHoldItem)
			{
				hook(item, player);
			}
		}

		public static float UseTimeMultiplier(Item item, Player player)
		{
			if (item.IsAir) return 1f;
			float multiplier = item.modItem?.UseTimeMultiplier(player) ?? 1f;
			foreach (var hook in HookUseTimeMultiplier)
			{
				multiplier *= hook(item, player);
			}
			return multiplier;
		}

		public static float MeleeSpeedMultiplier(Item item, Player player)
		{
			if (item.IsAir) return 1f;
			float multiplier = item.modItem?.MeleeSpeedMultiplier(player) ?? 1f;
			foreach (var hook in HookMeleeSpeedMultiplier)
			{
				multiplier *= hook(item, player);
			}
			return multiplier;
		}

		/// <summary>
		/// Calls ModItem.GetWeaponDamage, then all GlobalItem.GetWeaponDamage hooks.
		/// </summary>
		public static void GetWeaponDamage(Item item, Player player, ref int damage)
		{
			if (item.IsAir) return;
			item.modItem?.GetWeaponDamage(player, ref damage);

			foreach (var hook in HookGetWeaponDamage)
			{
				hook(item, player, ref damage);
			}
		}

		/// <summary>
		/// Calls ModItem.GetWeaponKnockback, then all GlobalItem.GetWeaponKnockback hooks.
		/// </summary>
		public static void GetWeaponKnockback(Item item, Player player, ref float knockback)
		{
			item.modItem?.GetWeaponKnockback(player, ref knockback);

			foreach (var hook in HookGetWeaponKnockback)
			{
				hook(item, player, ref knockback);
			}
		}

		/// <summary>
		/// If the item is a modded item, ModItem.checkProjOnSwing is true, and the player is not at the beginning of the item's use animation, sets canShoot to false.
		/// </summary>
		public static bool CheckProjOnSwing(Player player, Item item)
		{
			return item.modItem == null || !item.modItem.projOnSwing || player.itemAnimation == player.itemAnimationMax - 1;
		}

		/// <summary>
		/// Calls ModItem.PickAmmo, then all GlobalItem.PickAmmo hooks.
		/// </summary>
		public static void PickAmmo(Item item, Player player, ref int type, ref float speed, ref int damage, ref float knockback)
		{
			item.modItem?.PickAmmo(player, ref type, ref speed, ref damage, ref knockback);

			foreach (var hook in HookPickAmmo)
			{
				hook(item, player, ref type, ref speed, ref damage, ref knockback);
			}
		}

		//near end of Terraria.Player.PickAmmo before flag2 is checked add
		//  if(!ItemLoader.ConsumeAmmo(sItem, item, this)) { flag2 = true; }
		/// <summary>
		/// Calls ModItem.ConsumeAmmo for the weapon, ModItem.ConsumeAmmo for the ammo, then each GlobalItem.ConsumeAmmo hook for the weapon and ammo, until one of them returns false. If all of them return true, returns true.
		/// </summary>
		public static bool ConsumeAmmo(Item item, Item ammo, Player player)
		{
			if (item.modItem != null && !item.modItem.ConsumeAmmo(player) ||
					ammo.modItem != null && !ammo.modItem.ConsumeAmmo(player))
			{
				return false;
			}
			foreach (var hook in HookConsumeAmmo)
			{
				if (!hook(item, player) || !hook(ammo, player))
				{
					return false;
				}
			}
			return true;
		}

		//in Terraria.Player.ItemCheck at end of if/else chain for shooting place if on last else
		//  if(ItemLoader.Shoot(item, this, ref vector2, ref num78, ref num79, ref num71, ref num73, ref num74))
		/// <summary>
		/// Calls each GlobalItem.Shoot hook, then ModItem.Shoot, until one of them returns false. If all of them return true, returns true.
		/// </summary>
		/// <param name="item">The weapon item.</param>
		/// <param name="player">The player.</param>
		/// <param name="position">The shoot spawn position.</param>
		/// <param name="speedX">The speed x calculated from shootSpeed and mouse position.</param>
		/// <param name="speedY">The speed y calculated from shootSpeed and mouse position.</param>
		/// <param name="type">The projectile type choosen by ammo and weapon.</param>
		/// <param name="damage">The projectile damage.</param>
		/// <param name="knockBack">The projectile knock back.</param>
		/// <returns></returns>
		public static bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			foreach (var hook in HookShoot)
			{
				if (!hook(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack))
				{
					return false;
				}
			}
			if (item.modItem != null && !item.modItem.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack))
			{
				return false;
			}
			return true;
		}

		//in Terraria.Player.ItemCheck after end of useStyle if/else chain for melee hitbox
		//  call ItemLoader.UseItemHitbox(item, this, ref r2, ref flag17)
		/// <summary>
		/// Calls ModItem.UseItemHitbox, then all GlobalItem.UseItemHitbox hooks.
		/// </summary>
		public static void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
			item.modItem?.UseItemHitbox(player, ref hitbox, ref noHitbox);

			foreach (var hook in HookUseItemHitbox)
			{
				hook(item, player, ref hitbox, ref noHitbox);
			}
		}

		//in Terraria.Player.ItemCheck after magma stone dust effect for melee weapons
		//  call ItemLoader.MeleeEffects(item, this, r2)
		/// <summary>
		/// Calls ModItem.MeleeEffects and all GlobalItem.MeleeEffects hooks.
		/// </summary>
		public static void MeleeEffects(Item item, Player player, Rectangle hitbox)
		{
			item.modItem?.MeleeEffects(player, hitbox);

			foreach (var hook in HookMeleeEffects)
			{
				hook(item, player, hitbox);
			}
		}

		//in Terraria.Player.ItemCheck before checking whether npc type can be hit add
		//  bool? modCanHit = ItemLoader.CanHitNPC(item, this, Main.npc[num292]);
		//  if(modCanHit.HasValue && !modCanHit.Value) { continue; }
		//in if statement afterwards add || (modCanHit.HasValue && modCanHit.Value)
		/// <summary>
		/// Gathers the results of ModItem.CanHitNPC and all GlobalItem.CanHitNPC hooks. If any of them returns false, this returns false. Otherwise, if any of them returns true then this returns true. If all of the returns null, this returns null.
		/// </summary>
		public static bool? CanHitNPC(Item item, Player player, NPC target)
		{
			bool? canHit = item.modItem?.CanHitNPC(player, target);
			if (canHit.HasValue && !canHit.Value)
			{
				return false;
			}
			foreach (var hook in HookCanHitNPC)
			{
				bool? globalCanHit = hook(item, player, target);
				if (globalCanHit.HasValue)
				{
					if (globalCanHit.Value)
					{
						canHit = true;
					}
					else
					{
						return false;
					}
				}
			}
			return canHit;
		}

		//in Terraria.Player.ItemCheck for melee attacks after damage variation
		//  call ItemLoader.ModifyHitNPC(item, this, Main.npc[num292], ref num282, ref num283, ref flag18)
		/// <summary>
		/// Calls ModItem.ModifyHitNPC, then all GlobalItem.ModifyHitNPC hooks.
		/// </summary>
		public static void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
		{
			item.modItem?.ModifyHitNPC(player, target, ref damage, ref knockBack, ref crit);

			foreach (var hook in HookModifyHitNPC)
			{
				hook(item, player, target, ref damage, ref knockBack, ref crit);
			}
		}

		//in Terraria.Player.ItemCheck for melee attacks before updating informational accessories
		//  call ItemLoader.OnHitNPC(item, this, Main.npc[num292], num295, num283, flag18)
		/// <summary>
		/// Calls ModItem.OnHitNPC and all GlobalItem.OnHitNPC hooks.
		/// </summary>
		public static void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit)
		{
			item.modItem?.OnHitNPC(player, target, damage, knockBack, crit);

			foreach (var hook in HookOnHitNPC)
			{
				hook(item, player, target, damage, knockBack, crit);
			}
		}

		//in Terraria.Player.ItemCheck add to beginning of pvp collision check
		/// <summary>
		/// Calls all GlobalItem.CanHitPvp hooks, then ModItem.CanHitPvp, until one of them returns false. If all of them return true, this returns true.
		/// </summary>
		public static bool CanHitPvp(Item item, Player player, Player target)
		{
			foreach (var hook in HookCanHitPvp)
			{
				if (!hook(item, player, target))
				{
					return false;
				}
			}
			if (item.modItem != null)
			{
				return item.modItem.CanHitPvp(player, target);
			}
			return true;
		}

		//in Terraria.Player.ItemCheck for pvp melee attacks after damage variation
		//  call ItemLoader.ModifyHitPvp(item, this, Main.player[num302], ref num282, ref flag20)
		/// <summary>
		/// Calls ModItem.ModifyHitPvp, then all GlobalItem.ModifyHitPvp hooks.
		/// </summary>
		public static void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit)
		{
			item.modItem?.ModifyHitPvp(player, target, ref damage, ref crit);

			foreach (var hook in HookModifyHitPvp)
			{
				hook(item, player, target, ref damage, ref crit);
			}
		}

		//in Terraria.Player.ItemCheck for pvp melee attacks before NetMessage stuff
		//  call ItemLoader.OnHitPvp(item, this, Main.player[num302], num304, flag20)
		/// <summary>
		/// Calls ModItem.OnHitPvp and all GlobalItem.OnHitPvp hooks.
		/// </summary>
		public static void OnHitPvp(Item item, Player player, Player target, int damage, bool crit)
		{
			item.modItem?.OnHitPvp(player, target, damage, crit);

			foreach (var hook in HookOnHitPvp)
			{
				hook(item, player, target, damage, crit);
			}
		}

		/// <summary>
		/// Returns the "or" operation on the results of ModItem.UseItem and all GlobalItem.UseItem hooks.
		/// </summary>
		public static bool UseItem(Item item, Player player)
		{
			if (item.IsAir) return false;
			bool flag = false;
			if (item.modItem != null)
			{
				flag |= item.modItem.UseItem(player);
			}
			foreach (var hook in HookUseItem)
			{
				flag |= hook(item, player);
			}
			return flag;
		}

		//near end of Terraria.Player.ItemCheck
		//  if (flag22 && ItemLoader.ConsumeItem(item, this))
		/// <summary>
		/// If ModItem.ConsumeItem or any of the GlobalItem.ConsumeItem hooks returns false, sets consume to false.
		/// </summary>
		public static bool ConsumeItem(Item item, Player player)
		{
			//if (item.IsAir) return true;
			if (item.modItem != null && !item.modItem.ConsumeItem(player))
			{
				return false;
			}
			foreach (var hook in HookConsumeItem)
			{
				if (!hook(item, player))
				{
					return false;
				}
			}
			return true;
		}

		//in Terraria.Player.PlayerFrame at end of useStyle if/else chain
		//  call if(ItemLoader.UseItemFrame(this.inventory[this.selectedItem], this)) { return; }
		/// <summary>
		/// Calls ModItem.UseItemFrame, then all GlobalItem.UseItemFrame hooks, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static bool UseItemFrame(Item item, Player player)
		{
			if (item.modItem != null && item.modItem.UseItemFrame(player))
			{
				return true;
			}
			foreach (var hook in HookUseItemFrame)
			{
				if (hook(item, player))
				{
					return true;
				}
			}
			return false;
		}

		//in Terraria.Player.PlayerFrame at end of holdStyle if statements
		//  call if(ItemLoader.HoldItemFrame(this.inventory[this.selectedItem], this)) { return; }
		/// <summary>
		/// Calls ModItem.HoldItemFrame, then all GlobalItem.HoldItemFrame hooks, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static bool HoldItemFrame(Item item, Player player)
		{
			if (item.IsAir) return false;
			if (item.modItem != null && item.modItem.HoldItemFrame(player))
			{
				return true;
			}
			foreach (var hook in HookHoldItemFrame)
			{
				if (hook(item, player))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Calls ModItem.AltFunctionUse, then all GlobalItem.AltFunctionUse hooks, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static bool AltFunctionUse(Item item, Player player)
		{
			if (item.IsAir) return false;
			if (item.modItem != null && item.modItem.AltFunctionUse(player))
			{
				return true;
			}
			foreach (var hook in HookAltFunctionUse)
			{
				if (hook(item, player))
				{
					return true;
				}
			}
			return false;
		}

		//place at end of first for loop in Terraria.Player.UpdateEquips
		//  call ItemLoader.UpdateInventory(this.inventory[j], this)
		/// <summary>
		/// Calls ModItem.UpdateInventory and all GlobalItem.UpdateInventory hooks.
		/// </summary>
		public static void UpdateInventory(Item item, Player player)
		{
			if (item.IsAir) return;
			item.modItem?.UpdateInventory(player);

			foreach (var hook in HookUpdateInventory)
			{
				hook(item, player);
			}
		}

		//place in second for loop of Terraria.Player.UpdateEquips before prefix checking
		//  call ItemLoader.UpdateEquip(this.armor[k], this)
		/// <summary>
		/// Calls ModItem.UpdateEquip and all GlobalItem.UpdateEquip hooks.
		/// </summary>
		public static void UpdateEquip(Item item, Player player)
		{
			if (item.IsAir) return;
			item.modItem?.UpdateEquip(player);

			foreach (var hook in HookUpdateEquip)
			{
				hook(item, player);
			}
		}

		//place at end of third for loop of Terraria.Player.UpdateEquips
		//  call ItemLoader.UpdateAccessory(this.armor[l], this, this.hideVisual[l])
		/// <summary>
		/// Calls ModItem.UpdateAccessory and all GlobalItem.UpdateAccessory hooks.
		/// </summary>
		public static void UpdateAccessory(Item item, Player player, bool hideVisual)
		{
			if (item.IsAir) return;
			item.modItem?.UpdateAccessory(player, hideVisual);

			foreach (var hook in HookUpdateAccessory)
			{
				hook(item, player, hideVisual);
			}
		}

		/// <summary>
		/// Calls each of the item's equipment texture's UpdateVanity hook.
		/// </summary>
		public static void UpdateVanity(Player player)
		{
			foreach (EquipType type in EquipLoader.EquipTypes)
			{
				int slot = EquipLoader.GetPlayerEquip(player, type);
				EquipTexture texture = EquipLoader.GetEquipTexture(type, slot);
				texture?.UpdateVanity(player, type);
			}
		}

		//at end of Terraria.Player.UpdateArmorSets call ItemLoader.UpdateArmorSet(this, this.armor[0], this.armor[1], this.armor[2])
		/// <summary>
		/// If the head's ModItem.IsArmorSet returns true, calls the head's ModItem.UpdateArmorSet. This is then repeated for the body, then the legs. Then for each GlobalItem, if GlobalItem.IsArmorSet returns a non-empty string, calls GlobalItem.UpdateArmorSet with that string.
		/// </summary>
		public static void UpdateArmorSet(Player player, Item head, Item body, Item legs)
		{
			if (head.modItem != null && head.modItem.IsArmorSet(head, body, legs))
			{
				head.modItem.UpdateArmorSet(player);
			}
			if (body.modItem != null && body.modItem.IsArmorSet(head, body, legs))
			{
				body.modItem.UpdateArmorSet(player);
			}
			if (legs.modItem != null && legs.modItem.IsArmorSet(head, body, legs))
			{
				legs.modItem.UpdateArmorSet(player);
			}
			foreach (GlobalItem globalItem in HookUpdateArmorSet)
			{
				string set = globalItem.IsArmorSet(head, body, legs);
				if (!string.IsNullOrEmpty(set))
				{
					globalItem.UpdateArmorSet(player, set);
				}
			}
		}

		//in Terraria.Player.PlayerFrame after setting armor effects fields call this
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's PreUpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.PreUpdateVanitySet, using player.head, player.body, and player.legs.
		/// </summary>
		public static void PreUpdateVanitySet(Player player)
		{
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);
			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				headTexture.PreUpdateVanitySet(player);
			}
			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				bodyTexture.PreUpdateVanitySet(player);
			}
			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				legTexture.PreUpdateVanitySet(player);
			}
			foreach (GlobalItem globalItem in HookPreUpdateVanitySet)
			{
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
				{
					globalItem.PreUpdateVanitySet(player, set);
				}
			}
		}

		//in Terraria.Player.PlayerFrame after armor sets creating dust call this
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's UpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.UpdateVanitySet, using player.head, player.body, and player.legs.
		/// </summary>
		public static void UpdateVanitySet(Player player)
		{
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);
			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				headTexture.UpdateVanitySet(player);
			}
			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				bodyTexture.UpdateVanitySet(player);
			}
			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				legTexture.UpdateVanitySet(player);
			}
			foreach (GlobalItem globalItem in HookUpdateVanitySet)
			{
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
				{
					globalItem.UpdateVanitySet(player, set);
				}
			}
		}

		//in Terraria.Main.DrawPlayers after armor combinations setting flags call
		//  ItemLoader.ArmorSetShadows(player);
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's ArmorSetShadows. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.ArmorSetShadows, using player.head, player.body, and player.legs.
		/// </summary>
		public static void ArmorSetShadows(Player player)
		{
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);
			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				headTexture.ArmorSetShadows(player);
			}
			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				bodyTexture.ArmorSetShadows(player);
			}
			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			{
				legTexture.ArmorSetShadows(player);
			}
			foreach (GlobalItem globalItem in HookArmorSetShadows)
			{
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
				{
					globalItem.ArmorSetShadows(player, set);
				}
			}
		}

		/// <summary>
		/// Calls ModItem.SetMatch, then all GlobalItem.SetMatch hooks.
		/// </summary>
		public static void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes)
		{
			EquipTexture texture = null;
			if (armorSlot == 1)
			{
				texture = EquipLoader.GetEquipTexture(EquipType.Body, type);
			}
			else if (armorSlot == 2)
			{
				texture = EquipLoader.GetEquipTexture(EquipType.Legs, type);
			}
			texture?.SetMatch(male, ref equipSlot, ref robes);

			foreach (var hook in HookSetMatch)
			{
				hook(armorSlot, type, male, ref equipSlot, ref robes);
			}
		}

		//in Terraria.UI.ItemSlot.RightClick in end of item-opening if/else chain before final else
		//  make else if(ItemLoader.CanRightClick(inv[slot]))
		/// <summary>
		/// Calls ModItem.CanRightClick, then all GlobalItem.CanRightClick hooks, until one of the returns true. If one of the returns true, returns Main.mouseRight. Otherwise, returns false.
		/// </summary>
		public static bool CanRightClick(Item item)
		{
			if (item.IsAir) return false;
			if (!Main.mouseRight)
			{
				return false;
			}
			if (item.modItem != null && item.modItem.CanRightClick())
			{
				return true;
			}
			foreach (var hook in HookCanRightClick)
			{
				if (hook(item))
				{
					return true;
				}
			}
			return false;
		}

		//in Terraria.UI.ItemSlot in block from CanRightClick call ItemLoader.RightClick(inv[slot], player)
		/// <summary>
		/// If Main.mouseRightRelease is true, the following steps are taken:
		/// 1. Call ModItem.RightClick
		/// 2. Calls all GlobalItem.RightClick hooks
		/// 3. Decrements the item's stack
		/// 4. Sets the item's type to 0 if the item's stack is 0
		/// 5. Plays the item-grabbing sound
		/// 6. Sets Main.stackSplit to 30
		/// 7. Sets Main.mouseRightRelease to false
		/// 8. Calls Recipe.FindRecipes.
		/// </summary>
		public static void RightClick(Item item, Player player)
		{
			if (Main.mouseRightRelease)
			{
				item.modItem?.RightClick(player);

				foreach (var hook in HookRightClick)
				{
					hook(item, player);
				}
				item.stack--;
				if (item.stack == 0)
				{
					item.SetDefaults();
				}
				Main.PlaySound(7);
				Main.stackSplit = 30;
				Main.mouseRightRelease = false;
				Recipe.FindRecipes();
			}
		}

		//in Terraria.UI.ItemSlot add this to boss bag check
		/// <summary>
		/// Returns whether ModItem.bossBagNPC is greater than 0. Returns false if item is not a modded item.
		/// </summary>
		public static bool IsModBossBag(Item item)
		{
			return item.modItem != null && item.modItem.bossBagNPC > 0;
		}

		//in Terraria.Player.OpenBossBag after setting num14 call
		//  ItemLoader.OpenBossBag(type, this, ref num14);
		/// <summary>
		/// If the item is a modded item and ModItem.bossBagNPC is greater than 0, calls ModItem.OpenBossBag and sets npc to ModItem.bossBagNPC.
		/// </summary>
		public static void OpenBossBag(int type, Player player, ref int npc)
		{
			ModItem modItem = GetItem(type);
			if (modItem != null && modItem.bossBagNPC > 0)
			{
				modItem.OpenBossBag(player);
				npc = modItem.bossBagNPC;
			}
		}

		//in beginning of Terraria.Player.openBag methods add
		//  if(!ItemLoader.PreOpenVanillaBag("bagName", this, arg)) { return; }
		/// <summary>
		/// Calls each GlobalItem.PreOpenVanillaBag hook until one of them returns false. Returns true if all of them returned true.
		/// </summary>
		public static bool PreOpenVanillaBag(string context, Player player, int arg)
		{
			foreach (var hook in HookPreOpenVanillaBag)
			{
				if (!hook(context, player, arg))
				{
					return false;
				}
			}
			return true;
		}

		//in Terraria.Player.openBag methods after PreOpenVanillaBag if statements
		//  add ItemLoader.OpenVanillaBag("bagname", this, arg);
		/// <summary>
		/// Calls all GlobalItem.OpenVanillaBag hooks.
		/// </summary>
		public static void OpenVanillaBag(string context, Player player, int arg)
		{
			foreach (var hook in HookOpenVanillaBag)
			{
				hook(context, player, arg);
			}
		}

		/// <summary>
		/// Calls ModItem.PreReforge, then all GlobalItem.PreReforge hooks.
		/// </summary>
		public static void PreReforge(Item item)
		{
			if (IsModItem(item))
			{
				item.modItem.PreReforge();
			}
			foreach (var hook in HookPreReforge)
			{
				hook(item);
			}
		}

		/// <summary>
		/// Calls ModItem.PostReforge, then all GlobalItem.PostReforge hooks.
		/// </summary>
		public static void PostReforge(Item item)
		{
			if (IsModItem(item))
			{
				item.modItem.PostReforge();
			}
			foreach (var hook in HookPostReforge)
			{
				hook(item);
			}
		}

		/// <summary>
		/// Calls the item's body equipment texture's DrawHands hook, then all GlobalItem.DrawHands hooks.
		/// </summary>
		public static void DrawHands(Player player, ref bool drawHands, ref bool drawArms)
		{
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			texture?.DrawHands(ref drawHands, ref drawArms);

			foreach (var hook in HookDrawHands)
			{
				hook(player.body, ref drawHands, ref drawArms);
			}
		}

		//in Terraria.Main.DrawPlayerHead after if statement that sets flag2 to true
		//  call ItemLoader.DrawHair(drawPlayer, ref flag, ref flag2)
		//in Terraria.Main.DrawPlayer after if statement that sets flag5 to true
		//  call ItemLoader.DrawHair(drawPlayer, ref flag4, ref flag5)
		/// <summary>
		/// Calls the item's head equipment texture's DrawHair hook, then all GlobalItem.DrawHair hooks.
		/// </summary>
		public static void DrawHair(Player player, ref bool drawHair, ref bool drawAltHair)
		{
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			texture?.DrawHair(ref drawHair, ref drawAltHair);

			foreach (var hook in HookDrawHair)
			{
				hook(player.body, ref drawHair, ref drawAltHair);
			}
		}

		//in Terraria.Main.DrawPlayerHead in if statement after ItemLoader.DrawHair
		//and in Terraria.Main.DrawPlayer in if (!drawPlayer.invis && drawPlayer.head != 38 && drawPlayer.head != 135)
		//  use && with ItemLoader.DrawHead(drawPlayer)
		/// <summary>
		/// Calls the item's head equipment texture's DrawHead hook, then all GlobalItem.DrawHead hooks, until one of them returns false. Returns true if none of them return false.
		/// </summary>
		public static bool DrawHead(Player player)
		{
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			if (texture != null && !texture.DrawHead())
			{
				return false;
			}
			foreach (var hook in HookDrawHead)
			{
				if (!hook(player.head))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Calls the item's body equipment texture's DrawBody hook, then all GlobalItem.DrawBody hooks, until one of them returns false. Returns true if none of them return false.
		/// </summary>
		public static bool DrawBody(Player player)
		{
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
			if (texture != null && !texture.DrawBody())
			{
				return false;
			}
			foreach (var hook in HookDrawBody)
			{
				if (!hook(player.head))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Calls the item's leg equipment texture's DrawLegs hook, then the item's shoe equipment texture's DrawLegs hook, then all GlobalItem.DrawLegs hooks, until one of them returns false. Returns true if none of them return false.
		/// </summary>
		public static bool DrawLegs(Player player)
		{
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);
			if (texture != null && !texture.DrawLegs())
			{
				return false;
			}
			texture = EquipLoader.GetEquipTexture(EquipType.Shoes, player.shoe);
			if (texture != null && !texture.DrawLegs())
			{
				return false;
			}
			foreach (var hook in HookDrawLegs)
			{
				if (!hook(player.legs, player.shoe))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Calls the item's equipment texture's DrawArmorColor hook, then all GlobalItem.DrawArmorColor hooks.
		/// </summary>
		public static void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color,
			ref int glowMask, ref Color glowMaskColor)
		{
			EquipTexture texture = EquipLoader.GetEquipTexture(type, slot);
			texture?.DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);

			foreach (var hook in HookDrawArmorColor)
			{
				hook(type, slot, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
			}
		}

		/// <summary>
		/// Calls the item's body equipment texture's ArmorArmGlowMask hook, then all GlobalItem.ArmorArmGlowMask hooks.
		/// </summary>
		public static void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color)
		{
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, slot);
			texture?.ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);

			foreach (var hook in HookArmorArmGlowMask)
			{
				hook(slot, drawPlayer, shadow, ref glowMask, ref color);
			}
		}

		/// <summary>
		/// Returns the wing item that the player is functionally using. If player.wingsLogic has been modified, so no equipped wing can be found to match what the player is using, this creates a new Item object to return.
		/// </summary>
		public static Item GetWing(Player player)
		{
			Item item = null;
			for (int k = 3; k < 8 + player.extraAccessorySlots; k++)
			{
				if (player.armor[k].wingSlot == player.wingsLogic)
				{
					item = player.armor[k];
				}
			}
			if (item != null)
			{
				return item;
			}
			if (player.wingsLogic > 0 && player.wingsLogic < Main.maxWings)
			{
				item = new Item();
				item.SetDefaults(vanillaWings[player.wingsLogic]);
				return item;
			}
			if (player.wingsLogic >= Main.maxWings)
			{
				EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic);
				if (texture != null && texture.item != null)
				{
					return texture.item.item;
				}
			}
			return null;
		}

		//in Terraria.Player.WingMovement after if statements that set num1-5
		//  call ItemLoader.VerticalWingSpeeds(this, ref num2, ref num5, ref num4, ref num3, ref num)
		/// <summary>
		/// If the player is using wings, this uses the result of GetWing, and calls ModItem.VerticalWingSpeeds then all GlobalItem.VerticalWingSpeeds hooks.
		/// </summary>
		public static void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			Item item = GetWing(player);
			if (item == null)
			{
				EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic)?.VerticalWingSpeeds(
					player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
					ref maxAscentMultiplier, ref constantAscend);
				return;
			}
			item.modItem?.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
				ref maxAscentMultiplier, ref constantAscend);

			foreach (var hook in HookVerticalWingSpeeds)
			{
				hook(item, player, ref ascentWhenFalling, ref ascentWhenRising,
					ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
			}
		}

		//in Terraria.Player.Update after wingsLogic if statements modifying accRunSpeed and runAcceleration
		//  call ItemLoader.HorizontalWingSpeeds(this)
		/// <summary>
		/// If the player is using wings, this uses the result of GetWing, and calls ModItem.HorizontalWingSpeeds then all GlobalItem.HorizontalWingSpeeds hooks.
		/// </summary>
		public static void HorizontalWingSpeeds(Player player)
		{
			Item item = GetWing(player);
			if (item == null)
			{
				EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic)?.HorizontalWingSpeeds(
					player, ref player.accRunSpeed, ref player.runAcceleration);
				return;
			}
			item.modItem?.HorizontalWingSpeeds(player, ref player.accRunSpeed, ref player.runAcceleration);

			foreach (var hook in HookHorizontalWingSpeeds)
			{
				hook(item, player, ref player.accRunSpeed, ref player.runAcceleration);
			}
		}

		/// <summary>
		/// If wings can be seen on the player, calls the player's wing's equipment texture's WingUpdate and all GlobalItem.WingUpdate hooks.
		/// </summary>
		public static bool WingUpdate(Player player, bool inUse)
		{
			if (player.wings <= 0)
			{
				return false;
			}
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wings);
			bool? retVal = texture?.WingUpdate(player, inUse);

			foreach (var hook in HookWingUpdate)
			{
				retVal |= hook(player.wings, player, inUse);
			}
			return retVal ?? false;
		}

		//in Terraria.Item.UpdateItem before item movement (denoted by ItemID.Sets.ItemNoGravity)
		//  call ItemLoader.Update(this, ref num, ref num2)
		/// <summary>
		/// Calls ModItem.Update, then all GlobalItem.Update hooks.
		/// </summary>
		public static void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			item.modItem?.Update(ref gravity, ref maxFallSpeed);

			foreach (var hook in HookUpdate)
			{
				hook(item, ref gravity, ref maxFallSpeed);
			}
		}

		/// <summary>
		/// Calls ModItem.PostUpdate and all GlobalItem.PostUpdate hooks.
		/// </summary>
		public static void PostUpdate(Item item)
		{
			item.modItem?.PostUpdate();

			foreach (var hook in HookPostUpdate)
			{
				hook(item);
			}
		}

		//in Terraria.Player.GrabItems after increasing grab range add
		//  ItemLoader.GrabRange(Main.item[j], this, ref num);
		/// <summary>
		/// Calls ModItem.GrabRange, then all GlobalItem.GrabRange hooks.
		/// </summary>
		public static void GrabRange(Item item, Player player, ref int grabRange)
		{
			item.modItem?.GrabRange(player, ref grabRange);

			foreach (var hook in HookGrabRange)
			{
				hook(item, player, ref grabRange);
			}
		}

		//in Terraria.Player.GrabItems between setting beingGrabbed to true and grab styles add
		//  if(ItemLoader.GrabStyle(Main.item[j], this)) { } else
		/// <summary>
		/// Calls all GlobalItem.GrabStyle hooks then ModItem.GrabStyle, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static bool GrabStyle(Item item, Player player)
		{
			foreach (var hook in HookGrabStyle)
			{
				if (hook(item, player))
				{
					return true;
				}
			}
			if (item.modItem != null)
			{
				return item.modItem.GrabStyle(player);
			}
			return false;
		}

		//in Terraria.Player.GrabItems first per item if statement add
		//  && ItemLoader.CanPickup(Main.item[j], this)
		public static bool CanPickup(Item item, Player player)
		{
			foreach (var hook in HookCanPickup)
			{
				if (!hook(item, player))
				{
					return false;
				}
			}
			return item.modItem?.CanPickup(player) ?? true;
		}

		//in Terraria.Player.GrabItems before special pickup effects add
		//  if(!ItemLoader.OnPickup(Main.item[j], this)) { Main.item[j] = new Item(); continue; }
		/// <summary>
		/// Calls all GlobalItem.OnPickup hooks then ModItem.OnPickup, until one of the returns false. Returns true if all of the hooks return true.
		/// </summary>
		public static bool OnPickup(Item item, Player player)
		{
			foreach (var hook in HookOnPickup)
			{
				if (!hook(item, player))
				{
					return false;
				}
			}
			return item.modItem?.OnPickup(player) ?? true;
		}

		//in Terraria.Player.GrabItems before grab effect
		//  (this.ItemSpace(Main.item[j]) || ItemLoader.ExtraPickupSpace(Main.item[j], this)
		public static bool ExtraPickupSpace(Item item, Player player)
		{
			foreach (var hook in HookExtraPickupSpace)
			{
				if (hook(item, player))
				{
					return true;
				}
			}
			return item.modItem?.ExtraPickupSpace(player) ?? false;
		}

		//in Terraria.UI.ItemSlot.GetItemLight remove type too high check
		//in beginning of Terraria.Item.GetAlpha call
		//  Color? modColor = ItemLoader.GetAlpha(this, newColor);
		//  if(modColor.HasValue) { return modColor.Value; }
		/// <summary>
		/// Calls all GlobalItem.GetAlpha hooks then ModItem.GetAlpha, until one of them returns a color, and returns that color. Returns null if all of the hooks return null.
		/// </summary>
		public static Color? GetAlpha(Item item, Color lightColor)
		{
			if (item.IsAir) return null;
			foreach (var hook in HookGetAlpha)
			{
				Color? color = hook(item, lightColor);
				if (color.HasValue)
				{
					return color;
				}
			}

			return item.modItem?.GetAlpha(lightColor);
		}

		//in Terraria.Main.DrawItem after ItemSlot.GetItemLight call
		//  if(!ItemLoader.PreDrawInWorld(item, Main.spriteBatch, color, alpha, ref rotation, ref scale)) { return; }
		/// <summary>
		/// Returns the "and" operator on the results of ModItem.PreDrawInWorld and all GlobalItem.PreDrawInWorld hooks.
		/// </summary>
		public static bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			bool flag = true;
			if (item.modItem != null && !item.modItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI))
			{
				flag = false;
			}
			foreach (var hook in HookPreDrawInWorld)
			{
				if (!hook(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI))
				{
					flag = false;
				}
			}
			return flag;
		}

		//in Terraria.Main.DrawItem before every return (including for PreDrawInWorld) and at end of method call
		//  ItemLoader.PostDrawInWorld(item, Main.spriteBatch, color, alpha, rotation, scale)
		/// <summary>
		/// Calls ModItem.PostDrawInWorld, then all GlobalItem.PostDrawInWorld hooks.
		/// </summary>
		public static void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			item.modItem?.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);

			foreach (var hook in HookPostDrawInWorld)
			{
				hook(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
			}
		}

		//in Terraria.UI.ItemSlot.Draw place item-drawing code inside if statement
		//  if(ItemLoader.PreDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
		//    item.GetColor(color), origin, num4 * num3))
		/// <summary>
		/// Returns the "and" operator on the results of all GlobalItem.PreDrawInInventory hooks and ModItem.PreDrawInInventory.
		/// </summary>
		public static bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			bool flag = true;
			foreach (var hook in HookPreDrawInInventory)
			{
				if (!hook(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale))
				{
					flag = false;
				}
			}
			if (item.modItem != null && !item.modItem.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
			{
				flag = false;
			}
			return flag;
		}

		//in Terraria.UI.ItemSlot.Draw after if statement for PreDrawInInventory call
		//  ItemLoader.PostDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
		//    item.GetColor(color), origin, num4 * num3);
		/// <summary>
		/// Calls ModItem.PostDrawInInventory, then all GlobalItem.PostDrawInInventory hooks.
		/// </summary>
		public static void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			item.modItem?.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);

			foreach (var hook in HookPostDrawInInventory)
			{
				hook(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}
		}

		public static void HoldoutOffset(float gravDir, int type, ref Vector2 offset)
		{
			ModItem modItem = GetItem(type);
			if (modItem != null)
			{
				Vector2? modOffset = modItem.HoldoutOffset();
				if (modOffset.HasValue)
				{
					offset.X = modOffset.Value.X;
					offset.Y += gravDir * modOffset.Value.Y;
				}
			}
			foreach (var hook in HookHoldoutOffset)
			{
				Vector2? modOffset = hook(type);
				if (modOffset.HasValue)
				{
					offset.X = modOffset.Value.X;
					offset.Y = Main.itemTexture[type].Height / 2f + gravDir * modOffset.Value.Y;
				}
			}
		}

		public static void HoldoutOrigin(Player player, ref Vector2 origin)
		{
			Item item = player.inventory[player.selectedItem];
			Vector2 modOrigin = Vector2.Zero;
			if (item.modItem != null)
			{
				Vector2? modOrigin2 = item.modItem.HoldoutOrigin();
				if (modOrigin2.HasValue)
				{
					modOrigin = modOrigin2.Value;
				}
			}
			foreach (var hook in HookHoldoutOrigin)
			{
				Vector2? modOrigin2 = hook(item.type);
				if (modOrigin2.HasValue)
				{
					modOrigin = modOrigin2.Value;
				}
			}
			modOrigin.X *= player.direction;
			modOrigin.Y *= -player.gravDir;
			origin += modOrigin;
		}

		//in Terraria.UI.ItemSlot.AccCheck replace 2nd and 3rd return false with
		//  return !ItemLoader.CanEquipAccessory(item, slot)
		public static bool CanEquipAccessory(Item item, int slot)
		{
			Player player = Main.player[Main.myPlayer];
			if (item.modItem != null && !item.modItem.CanEquipAccessory(player, slot))
			{
				return false;
			}
			foreach (var hook in HookCanEquipAccessory)
			{
				if (!hook(item, player, slot))
				{
					return false;
				}
			}
			return true;
		}

		public static void ExtractinatorUse(ref int resultType, ref int resultStack, int extractType)
		{
			GetItem(extractType)?.ExtractinatorUse(ref resultType, ref resultStack);

			foreach (var hook in HookExtractinatorUse)
			{
				hook(extractType, ref resultType, ref resultStack);
			}
		}

		public static void AutoLightSelect(Item item, ref bool dryTorch, ref bool wetTorch, ref bool glowstick)
		{
			if (item.modItem != null)
			{
				item.modItem.AutoLightSelect(ref dryTorch, ref wetTorch, ref glowstick);
				if (wetTorch)
				{
					dryTorch = false;
					glowstick = false;
				}
				if (dryTorch)
				{
					glowstick = false;
				}
			}
		}

		public static void CaughtFishStack(Item item)
		{
			item.modItem?.CaughtFishStack(ref item.stack);

			foreach (var hook in HookCaughtFishStack)
			{
				hook(item.type, ref item.stack);
			}
		}

		public static void IsAnglerQuestAvailable(int itemID, ref bool notAvailable)
		{
			ModItem modItem = GetItem(itemID);
			if (modItem != null)
			{
				notAvailable &= !modItem.IsAnglerQuestAvailable();
			}
			foreach (var hook in HookIsAnglerQuestAvailable)
			{
				notAvailable &= !hook(itemID);
			}
		}

		public static string AnglerChat(int type)
		{
			string chat = "";
			string catchLocation = "";
			ModItem modItem = GetItem(type);
			if (modItem != null)
			{
				modItem.AnglerQuestChat(ref chat, ref catchLocation);
			}
			foreach (var hook in HookAnglerChat)
			{
				hook(type, ref chat, ref catchLocation);
			}
			if (string.IsNullOrEmpty(chat) || string.IsNullOrEmpty(chat))
			{
				return null;
			}
			return chat + "\n\n(" + catchLocation + ")";
		}

		public static void OnCraft(Item item, Recipe recipe)
		{
			if (IsModItem(item))
			{
				item.modItem.OnCraft(recipe);
			}
			foreach (var hook in HookOnCraft)
			{
				hook(item, recipe);
			}
		}

		public static void ModifyTooltips(Item item, ref int numTooltips, string[] names, ref string[] text,
			ref bool[] modifier, ref bool[] badModifier, ref int oneDropLogo, out Color?[] overrideColor)
		{
			List<TooltipLine> tooltips = new List<TooltipLine>();
			for (int k = 0; k < numTooltips; k++)
			{
				TooltipLine tooltip = new TooltipLine(names[k], text[k]);
				tooltip.isModifier = modifier[k];
				tooltip.isModifierBad = badModifier[k];
				if (k == oneDropLogo)
				{
					tooltip.oneDropLogo = true;
				}
				tooltips.Add(tooltip);
			}
			if (IsModItem(item))
			{
				item.modItem.ModifyTooltips(tooltips);
			}
			foreach (var hook in HookModifyTooltips)
			{
				hook(item, tooltips);
			}
			numTooltips = tooltips.Count;
			text = new string[numTooltips];
			modifier = new bool[numTooltips];
			badModifier = new bool[numTooltips];
			oneDropLogo = -1;
			overrideColor = new Color?[numTooltips];
			for (int k = 0; k < numTooltips; k++)
			{
				text[k] = tooltips[k].text;
				modifier[k] = tooltips[k].isModifier;
				badModifier[k] = tooltips[k].isModifierBad;
				if (tooltips[k].oneDropLogo)
				{
					oneDropLogo = k;
				}
				overrideColor[k] = tooltips[k].overrideColor;
			}
		}

		public static bool NeedsModSaving(Item item)
		{
			return item.type != 0 && (IsModItem(item) || HookNeedsSaving.Count(hook => hook(item)) > 0);
		}

		internal static void WriteNetGlobalOrder(BinaryWriter w)
		{
			w.Write((short)NetGlobals.Length);
			foreach (var globalItem in NetGlobals)
			{
				w.Write(globalItem.mod.netID);
				w.Write(globalItem.Name);
			}
		}

		internal static void ReadNetGlobalOrder(BinaryReader r)
		{
			short n = r.ReadInt16();
			NetGlobals = new GlobalItem[n];
			for (short i = 0; i < n; i++)
				NetGlobals[i] = ModNet.GetMod(r.ReadInt16()).GetGlobalItem(r.ReadString());
		}
	}
}
