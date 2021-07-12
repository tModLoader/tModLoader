using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Utilities;
using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.GlobalItem>;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central class from which item-related functions are carried out. It also stores a list of mod items by ID.
	/// </summary>
	public static class ItemLoader
	{
		internal static readonly IList<ModItem> items = new List<ModItem>();
		internal static readonly IList<GlobalItem> globalItems = new List<GlobalItem>();
		internal static GlobalItem[] NetGlobals;
		internal static readonly int vanillaQuestFishCount = 41;
		internal static readonly int[] vanillaWings = new int[Main.maxWings];

		private static int nextItem = ItemID.Count;
		private static Instanced<GlobalItem>[] globalItemsArray = new Instanced<GlobalItem>[0];

		private static readonly List<HookList> hooks = new List<HookList>();
		private static readonly List<HookList> modHooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<GlobalItem, F>> func) {
			var hook = new HookList(ModLoader.Method(func));

			hooks.Add(hook);

			return hook;
		}

		public static T AddModHook<T>(T hook) where T : HookList {
			hook.Update(globalItems);

			modHooks.Add(hook);

			return hook;
		}

		private static void FindVanillaWings() {
			if (vanillaWings[1] != 0)
				return;

			Item item = new Item();
			for (int k = 0; k < ItemID.Count; k++) {
				item.SetDefaults(k);
				if (item.wingSlot > 0) {
					vanillaWings[item.wingSlot] = k;
				}
			}
		}

		internal static int ReserveItemID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding items breaks vanilla client compatibility");

			int reserveID = nextItem;
			nextItem++;
			return reserveID;
		}

		/// <summary>
		/// Gets the ModItem instance corresponding to the specified type. Returns null if no modded item has the given type.
		/// </summary>
		public static ModItem GetItem(int type) {
			return type >= ItemID.Count && type < ItemCount ? items[type - ItemID.Count] : null;
		}

		public static int ItemCount => nextItem;

		internal static void ResizeArrays(bool unloading) {
			//Textures
			Array.Resize(ref TextureAssets.Item, nextItem);
			Array.Resize(ref TextureAssets.ItemFlame, nextItem);

			//Sets
			LoaderUtils.ResetStaticMembers(typeof(ItemID), true);

			//Etc
			Array.Resize(ref Item.cachedItemSpawnsByType, nextItem);
			Array.Resize(ref Item.staff, nextItem);
			Array.Resize(ref Item.claw, nextItem);
			Array.Resize(ref Lang._itemNameCache, nextItem);
			Array.Resize(ref Lang._itemTooltipCache, nextItem);

			for (int k = ItemID.Count; k < nextItem; k++) {
				Lang._itemNameCache[k] = LocalizedText.Empty;
				Lang._itemTooltipCache[k] = ItemTooltip.None;
				Item.cachedItemSpawnsByType[k] = -1;
			}

			//Animation collections can be accessed during an ongoing (un)loading process.
			//Which is why the following 2 lines have to run without any interruptions.
			lock (Main.itemAnimationsRegistered) {
				Array.Resize(ref Main.itemAnimations, nextItem);

				Main.InitializeItemAnimations();
			}

			if (unloading)
				Array.Resize(ref Main.anglerQuestItemNetIDs, vanillaQuestFishCount);
			else
				Main.anglerQuestItemNetIDs = Main.anglerQuestItemNetIDs
					.Concat(items.Where(modItem => modItem.IsQuestFish()).Select(modItem => modItem.Type))
					.ToArray();

			FindVanillaWings();

			globalItemsArray = globalItems
				.Select(g => new Instanced<GlobalItem>(g.index, g))
				.ToArray();

			NetGlobals = ModLoader.BuildGlobalHook<GlobalItem, Action<Item, BinaryWriter>>(globalItems, g => g.NetSend);

			foreach (var hook in hooks.Union(modHooks)) {
				hook.Update(globalItems);
			}
		}

		internal static void Unload() {
			items.Clear();
			nextItem = ItemID.Count;
			globalItems.Clear();
			modHooks.Clear();
		}

		internal static bool IsModItem(int index) => index >= ItemID.Count;

		private static bool GeneralPrefix(Item item) => item.maxStack == 1 && item.damage > 0 && item.ammo == 0 && !item.accessory;

		//Add all these to Terraria.Item.Prefix
		internal static bool MeleePrefix(Item item) => item.ModItem != null && GeneralPrefix(item) && item.melee && !item.noUseGraphic;
		internal static bool WeaponPrefix(Item item) => item.ModItem != null && GeneralPrefix(item) && item.melee && item.noUseGraphic;
		internal static bool RangedPrefix(Item item) => item.ModItem != null && GeneralPrefix(item) && item.ranged; //(item.ranged || item.thrown);
		internal static bool MagicPrefix(Item item) => item.ModItem != null && GeneralPrefix(item) && (item.magic || item.summon);

		private static HookList HookSetDefaults = AddHook<Action<Item>>(g => g.SetDefaults);

		internal static void SetDefaults(Item item, bool createModItem = true) {
			if (IsModItem(item.type) && createModItem)
				item.ModItem = GetItem(item.type).Clone(item);

			GlobalItem Instantiate(GlobalItem g)
				=> g.InstancePerEntity ? g.Clone(item, item) : g;

			LoaderUtils.InstantiateGlobals(item, globalItems, ref item.globalItems, Instantiate, () => {
				item.ModItem?.AutoDefaults();
				item.ModItem?.SetDefaults();
			});

			foreach (var g in HookSetDefaults.Enumerate(item.globalItems)) {
				g.SetDefaults(item);
			}
		}

		private static HookList HookOnCreate = AddHook<Action<Item, ItemCreationContext>>(g => g.OnCreate);
		public static void OnCreate(Item item, ItemCreationContext context) {
			foreach (var g in HookOnCreate.Enumerate(item.globalItems)) {
				g.OnCreate(item, context);
			}

			item.ModItem?.OnCreate(context);
		}

		private static HookList HookChoosePrefix = AddHook<Func<Item, UnifiedRandom, int>>(g => g.ChoosePrefix);

		public static int ChoosePrefix(Item item, UnifiedRandom rand) {
			foreach (var g in HookChoosePrefix.Enumerate(item.globalItems)) {
				int pre = g.ChoosePrefix(item, rand);
				if (pre > 0) {
					return pre;
				}
			}
			if (item.ModItem != null) {
				int pre = item.ModItem.ChoosePrefix(rand);
				if (pre > 0) {
					return pre;
				}
			}
			return -1;
		}

		private static HookList HookPrefixChance = AddHook<Func<Item, int, UnifiedRandom, bool?>>(g => g.PrefixChance);

		/// <summary>
		/// Allows for blocking, forcing and altering chance of prefix rolling.
		/// False (block) takes precedence over True (force).
		/// Null gives vanilla behaviour
		/// </summary>
		public static bool? PrefixChance(Item item, int pre, UnifiedRandom rand) {
			bool? result = null;
			foreach (var g in HookPrefixChance.Enumerate(item.globalItems)) {
				bool? r = g.PrefixChance(item, pre, rand);
				if (r.HasValue)
					result = r.Value && (result ?? true);
			}
			if (item.ModItem != null) {
				bool? r = item.ModItem.PrefixChance(pre, rand);
				if (r.HasValue)
					result = r.Value && (result ?? true);
			}
			return result;
		}

		private static HookList HookAllowPrefix = AddHook<Func<Item, int, bool>>(g => g.AllowPrefix);
		public static bool AllowPrefix(Item item, int pre) {
			bool result = true;
			foreach (var g in HookAllowPrefix.Enumerate(item.globalItems)) {
				result &= g.AllowPrefix(item, pre);
			}
			if (item.ModItem != null) {
				result &= item.ModItem.AllowPrefix(pre);
			}
			return result;
		}

		private static HookList HookCanUseItem = AddHook<Func<Item, Player, bool>>(g => g.CanUseItem);
		//in Terraria.Player.ItemCheck
		//  inside block if (this.controlUseItem && this.itemAnimation == 0 && this.releaseUseItem && item.useStyle > 0)
		//  set initial flag2 to ItemLoader.CanUseItem(item, this)
		/// <summary>
		/// Returns the "and" operation on the results of ModItem.CanUseItem and all GlobalItem.CanUseItem hooks.
		/// Does not fail fast (every hook is called).
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="player">The player holding the item.</param>
		public static bool CanUseItem(Item item, Player player) {
			bool flag = true;
			if (item.ModItem != null)
				flag &= item.ModItem.CanUseItem(player);

			foreach (var g in HookCanUseItem.Enumerate(item.globalItems)) {
				flag &= g.CanUseItem(item, player);
			}

			return flag;
		}

		private static HookList HookUseStyle = AddHook<Action<Item, Player, Rectangle>>(g => g.UseStyle);
		//in Terraria.Player.ItemCheck after useStyle if/else chain call ItemLoader.UseStyle(item, this)
		/// <summary>
		/// Calls ModItem.UseStyle and all GlobalItem.UseStyle hooks.
		/// </summary>
		public static void UseStyle(Item item, Player player, Rectangle heldItemFrame) {
			if (item.IsAir)
				return;

			item.ModItem?.UseStyle(player, heldItemFrame);

			foreach (var g in HookUseStyle.Enumerate(item.globalItems)) {
				g.UseStyle(item, player, heldItemFrame);
			}
		}

		private static HookList HookHoldStyle = AddHook<Action<Item, Player, Rectangle>>(g => g.HoldStyle);
		//in Terraria.Player.ItemCheck after holdStyle if/else chain call ItemLoader.HoldStyle(item, this)
		/// <summary>
		/// If the player is not holding onto a rope and is not in the middle of using an item, calls ModItem.HoldStyle and all GlobalItem.HoldStyle hooks.
		/// <br/> Returns whether or not the vanilla logic should be skipped.
		/// </summary>
		public static void HoldStyle(Item item, Player player, Rectangle heldItemFrame) {
			if (item.IsAir || player.pulley || player.itemAnimation > 0)
				return;

			item.ModItem?.HoldStyle(player, heldItemFrame);

			foreach (var g in HookHoldStyle.Enumerate(item.globalItems)) {
				g.HoldStyle(item, player, heldItemFrame);
			}
		}

		private static HookList HookHoldItem = AddHook<Action<Item, Player>>(g => g.HoldItem);
		//in Terraria.Player.ItemCheck before this.controlUseItem setting this.releaseUseItem call ItemLoader.HoldItem(item, this)
		/// <summary>
		/// Calls ModItem.HoldItem and all GlobalItem.HoldItem hooks.
		/// </summary>
		public static void HoldItem(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.HoldItem(player);

			foreach (var g in HookHoldItem.Enumerate(item.globalItems)) {
				g.HoldItem(item, player);
			}
		}

		private static HookList HookUseTimeMultiplier = AddHook<Func<Item, Player, float>>(g => g.UseTimeMultiplier);

		public static float UseTimeMultiplier(Item item, Player player) {
			if (item.IsAir)
				return 1f;

			float multiplier = item.ModItem?.UseTimeMultiplier(player) ?? 1f;

			foreach (var g in HookUseTimeMultiplier.Enumerate(item.globalItems)) {
				multiplier *= g.UseTimeMultiplier(item, player);
			}

			return multiplier;
		}

		private static HookList HookUseAnimationMultiplier = AddHook<Func<Item, Player, float>>(g => g.UseAnimationMultiplier);

		public static float UseAnimationMultiplier(Item item, Player player) {
			if (item.IsAir)
				return 1f;

			float multiplier = item.ModItem?.UseAnimationMultiplier(player) ?? 1f;

			foreach (var g in HookUseAnimationMultiplier.Enumerate(item.globalItems)) {
				multiplier *= g.UseAnimationMultiplier(item, player);
			}

			return multiplier;
		}

		private static HookList HookUseSpeedMultiplier = AddHook<Func<Item, Player, float>>(g => g.UseSpeedMultiplier);

		public static float UseSpeedMultiplier(Item item, Player player) {
			if (item.IsAir)
				return 1f;

			float multiplier = item.ModItem?.UseSpeedMultiplier(player) ?? 1f;

			foreach (var g in HookUseSpeedMultiplier.Enumerate(item.globalItems)) {
				multiplier *= g.UseSpeedMultiplier(item, player);
			}

			return multiplier;
		}

		private delegate void DelegateGetHealLife(Item item, Player player, bool quickHeal, ref int healValue);
		private static HookList HookGetHealLife = AddHook<DelegateGetHealLife>(g => g.GetHealLife);
		/// <summary>
		/// Calls ModItem.GetHealLife, then all GlobalItem.GetHealLife hooks.
		/// </summary>
		public static void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			item.ModItem?.GetHealLife(player, quickHeal, ref healValue);

			foreach (var g in HookGetHealLife.Enumerate(item.globalItems)) {
				g.GetHealLife(item, player, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateGetHealMana(Item item, Player player, bool quickHeal, ref int healValue);
		private static HookList HookGetHealMana = AddHook<DelegateGetHealMana>(g => g.GetHealMana);
		/// <summary>
		/// Calls ModItem.GetHealMana, then all GlobalItem.GetHealMana hooks.
		/// </summary>
		public static void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			item.ModItem?.GetHealMana(player, quickHeal, ref healValue);

			foreach (var g in HookGetHealMana.Enumerate(item.globalItems)) {
				g.GetHealMana(item, player, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateModifyManaCost(Item item, Player player, ref float reduce, ref float mult);
		private static HookList HookModifyManaCost = AddHook<DelegateModifyManaCost>(g => g.ModifyManaCost);
		/// <summary>
		/// Calls ModItem.ModifyManaCost, then all GlobalItem.ModifyManaCost hooks.
		/// </summary>
		public static void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult) {
			if (item.IsAir)
				return;
			
			item.ModItem?.ModifyManaCost(player, ref reduce, ref mult);

			foreach (var g in HookModifyManaCost.Enumerate(item.globalItems)) {
				g.ModifyManaCost(item, player, ref reduce, ref mult);
			}
		}

		private static HookList HookOnMissingMana = AddHook<Action<Item, Player, int>>(g => g.OnMissingMana);
		/// <summary>
		/// Calls ModItem.OnMissingMana, then all GlobalItem.OnMissingMana hooks.
		/// </summary>
		public static void OnMissingMana(Item item, Player player, int neededMana) {
			if (item.IsAir)
				return;
			
			item.ModItem?.OnMissingMana(player, neededMana);

			foreach (var g in HookOnMissingMana.Enumerate(item.globalItems)) {
				g.OnMissingMana(item, player, neededMana);
			}
		}

		private static HookList HookOnConsumeMana = AddHook<Action<Item, Player, int>>(g => g.OnConsumeMana);
		/// <summary>
		/// Calls ModItem.OnConsumeMana, then all GlobalItem.OnConsumeMana hooks.
		/// </summary>
		public static void OnConsumeMana(Item item, Player player, int manaConsumed) {
			if (item.IsAir)
				return;
			
			item.ModItem?.OnConsumeMana(player, manaConsumed);

			foreach (var g in HookOnConsumeMana.Enumerate(item.globalItems)) {
				g.OnConsumeMana(item, player, manaConsumed);
			}
		}

		private delegate void DelegateModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup);
		private static HookList HookModifyResearchSorting = AddHook<DelegateModifyResearchSorting>(g => g.ModifyResearchSorting);
		public static void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			if (item.IsAir)
				return;

			item.ModItem?.ModifyResearchSorting(ref itemGroup);

			foreach (var g in HookModifyResearchSorting.Enumerate(item.globalItems)) {
				g.ModifyResearchSorting(item, ref itemGroup);
			}
		}

		private delegate void DelegateModifyWeaponDamage(Item item, Player player, ref StatModifier damage, ref float flat);
		private static HookList HookModifyWeaponDamage = AddHook<DelegateModifyWeaponDamage>(g => g.ModifyWeaponDamage);
		/// <summary>
		/// Calls ModItem.HookModifyWeaponDamage, then all GlobalItem.HookModifyWeaponDamage hooks.
		/// </summary>
		public static void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage, ref float flat) {
			if (item.IsAir)
				return;

			item.ModItem?.ModifyWeaponDamage(player, ref damage, ref flat);

			foreach (var g in HookModifyWeaponDamage.Enumerate(item.globalItems)) {
				g.ModifyWeaponDamage(item, player, ref damage, ref flat);
			}
		}

		private delegate void DelegateModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback, ref float flat);
		private static HookList HookModifyWeaponKnockback = AddHook<DelegateModifyWeaponKnockback>(g => g.ModifyWeaponKnockback);
		/// <summary>
		/// Calls ModItem.ModifyWeaponKnockback, then all GlobalItem.ModifyWeaponKnockback hooks.
		/// </summary>
		public static void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback, ref float flat) {
			if (item.IsAir)
				return;

			item.ModItem?.ModifyWeaponKnockback(player, ref knockback, ref flat);

			foreach (var g in HookModifyWeaponKnockback.Enumerate(item.globalItems)) {
				g.ModifyWeaponKnockback(item, player, ref knockback, ref flat);
			}
		}


		private delegate void DelegateModifyWeaponCrit(Item item, Player player, ref int crit);
		private static HookList HookModifyWeaponCrit = AddHook<DelegateModifyWeaponCrit>(g => g.ModifyWeaponCrit);
		/// <summary>
		/// Calls ModItem.ModifyWeaponCrit, then all GlobalItem.ModifyWeaponCrit hooks.
		/// </summary>
		public static void ModifyWeaponCrit(Item item, Player player, ref int crit) {
			if (item.IsAir)
				return;

			item.ModItem?.ModifyWeaponCrit(player, ref crit);

			foreach (var g in HookModifyWeaponCrit.Enumerate(item.globalItems)) {
				g.ModifyWeaponCrit(item, player, ref crit);
			}
		}

		/// <summary>
		/// If the item is a modded item, ModItem.checkProjOnSwing is true, and the player is not at the beginning of the item's use animation, sets canShoot to false.
		/// </summary>
		public static bool CheckProjOnSwing(Player player, Item item) {
			return item.ModItem == null || !item.ModItem.OnlyShootOnSwing || player.itemAnimation == player.itemAnimationMax - 1;
		}

		private delegate void DelegatePickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback);
		private static HookList HookPickAmmo = AddHook<DelegatePickAmmo>(g => g.PickAmmo);
		/// <summary>
		/// Calls ModItem.PickAmmo, then all GlobalItem.PickAmmo hooks.
		/// </summary>
		public static void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
			ammo.ModItem?.PickAmmo(weapon, player, ref type, ref speed, ref damage, ref knockback);

			foreach (var g in HookPickAmmo.Enumerate(ammo.globalItems)) {
				g.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
			}
		}

		private static HookList HookConsumeAmmo = AddHook<Func<Item, Player, bool>>(g => g.ConsumeAmmo);
		//near end of Terraria.Player.PickAmmo before flag2 is checked add
		//  if(!ItemLoader.ConsumeAmmo(sItem, item, this)) { flag2 = true; }
		/// <summary>
		/// Calls ModItem.ConsumeAmmo for the weapon, ModItem.ConsumeAmmo for the ammo, then each GlobalItem.ConsumeAmmo hook for the weapon and ammo, until one of them returns false. If all of them return true, returns true.
		/// </summary>
		public static bool ConsumeAmmo(Item item, Item ammo, Player player) {
			if (item.ModItem != null && !item.ModItem.ConsumeAmmo(player) ||
					ammo.ModItem != null && !ammo.ModItem.ConsumeAmmo(player))
				return false;

			foreach (var g in HookConsumeAmmo.Enumerate(ammo.globalItems)) {
				if (!g.ConsumeAmmo(item, player) ||
					!g.ConsumeAmmo(ammo, player))
					return false;
			}

			return true;
		}

		private static HookList HookOnConsumeAmmo = AddHook<Action<Item, Player>>(g => g.OnConsumeAmmo);
		/// <summary>
		/// Calls ModItem.OnConsumeAmmo for the weapon, ModItem.OnConsumeAmmo for the ammo, then each GlobalItem.OnConsumeAmmo hook for the weapon and ammo.
		/// </summary>
		public static void OnConsumeAmmo(Item item, Item ammo, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.OnConsumeAmmo(player);
			ammo.ModItem?.OnConsumeAmmo(player);

			foreach (var g in HookOnConsumeAmmo.Enumerate(item.globalItems)) {
				g.OnConsumeAmmo(item, player);
			}

			foreach (var g in HookOnConsumeAmmo.Enumerate(ammo.globalItems)) {
				g.OnConsumeAmmo(item, player);
			}
		}

		private static HookList HookCanShoot = AddHook<Func<Item, Player, bool>>(g => g.CanShoot);
		/// <summary>
		/// Calls each GlobalItem.CanShoot hook, then ModItem.CanShoot, until one of them returns false. If all of them return true, returns true.
		/// </summary>
		public static bool CanShoot(Item item, Player player) {
			foreach (var g in HookCanShoot.Enumerate(item.globalItems)) {
				if (!g.CanShoot(item, player))
					return false;
			}

			return item.ModItem?.CanShoot(player) ?? true;
		}

		private delegate void DelegateModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockBack);
		private static HookList HookModifyShootStats = AddHook<DelegateModifyShootStats>(g => g.ModifyShootStats);
		/// <summary>
		/// Calls ModItem.ModifyShootStats, then each GlobalItem.ModifyShootStats hook.
		/// </summary>
		public static void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			item.ModItem?.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);

			foreach (var g in HookModifyShootStats.Enumerate(item.globalItems)) {
				g.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
			}
		}

		private static HookList HookShoot = AddHook<Func<Item, Player, ProjectileSource_Item_WithAmmo, Vector2, Vector2, int, int, float, bool>>(g => g.Shoot);
		/// <summary>
		/// Calls each GlobalItem.Shoot hook then, if none of them returns false, calls the ModItem.Shoot hook and returns its value.
		/// </summary>
		public static bool Shoot(Item item, Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, bool defaultResult = true) {
			foreach (var g in HookShoot.Enumerate(item.globalItems)) {
				defaultResult &= g.Shoot(item, player, source, position, velocity, type, damage, knockback);
			}

			return defaultResult && (item.ModItem?.Shoot(player, source, position, velocity, type, damage, knockback) ?? true);
		}

		private delegate void DelegateUseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox);
		private static HookList HookUseItemHitbox = AddHook<DelegateUseItemHitbox>(g => g.UseItemHitbox);
		//in Terraria.Player.ItemCheck after end of useStyle if/else chain for melee hitbox
		//  call ItemLoader.UseItemHitbox(item, this, ref r2, ref flag17)
		/// <summary>
		/// Calls ModItem.UseItemHitbox, then all GlobalItem.UseItemHitbox hooks.
		/// </summary>
		public static void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
			item.ModItem?.UseItemHitbox(player, ref hitbox, ref noHitbox);

			foreach (var g in HookUseItemHitbox.Enumerate(item.globalItems)) {
				g.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
			}
		}

		private static HookList HookMeleeEffects = AddHook<Action<Item, Player, Rectangle>>(g => g.MeleeEffects);
		//in Terraria.Player.ItemCheck after magma stone dust effect for melee weapons
		//  call ItemLoader.MeleeEffects(item, this, r2)
		/// <summary>
		/// Calls ModItem.MeleeEffects and all GlobalItem.MeleeEffects hooks.
		/// </summary>
		public static void MeleeEffects(Item item, Player player, Rectangle hitbox) {
			item.ModItem?.MeleeEffects(player, hitbox);

			foreach (var g in HookMeleeEffects.Enumerate(item.globalItems)) {
				g.MeleeEffects(item, player, hitbox);
			}
		}

		private static HookList HookCanHitNPC = AddHook<Func<Item, Player, NPC, bool?>>(g => g.CanHitNPC);
		//in Terraria.Player.ItemCheck before checking whether npc type can be hit add
		//  bool? modCanHit = ItemLoader.CanHitNPC(item, this, Main.npc[num292]);
		//  if(modCanHit.HasValue && !modCanHit.Value) { continue; }
		//in if statement afterwards add || (modCanHit.HasValue && modCanHit.Value)
		/// <summary>
		/// Gathers the results of ModItem.CanHitNPC and all GlobalItem.CanHitNPC hooks. 
		/// If any of them returns false, this returns false. 
		/// Otherwise, if any of them returns true then this returns true. 
		/// If all of them return null, this returns null.
		/// </summary>
		public static bool? CanHitNPC(Item item, Player player, NPC target) {
			bool? flag = null;

			foreach (GlobalItem g in HookCanHitNPC.Enumerate(item.globalItems)) {
				bool? canHit = g.CanHitNPC(item, player, target);

				if (canHit.HasValue) {
					if (!canHit.Value) {
						return false;
					}

					flag = true;
				}
			}

			if (item.ModItem != null) {
				bool? canHit = item.ModItem.CanHitNPC(player, target);

				if (canHit.HasValue) {
					if (!canHit.Value) {
						return false;
					}

					flag = true;
				}
			}

			return flag;
		}

		private delegate void DelegateModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit);
		private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(g => g.ModifyHitNPC);
		//in Terraria.Player.ItemCheck for melee attacks after damage variation
		//  call ItemLoader.ModifyHitNPC(item, this, Main.npc[num292], ref num282, ref num283, ref flag18)
		/// <summary>
		/// Calls ModItem.ModifyHitNPC, then all GlobalItem.ModifyHitNPC hooks.
		/// </summary>
		public static void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
			item.ModItem?.ModifyHitNPC(player, target, ref damage, ref knockBack, ref crit);

			foreach (var g in HookModifyHitNPC.Enumerate(item.globalItems)) {
				g.ModifyHitNPC(item, player, target, ref damage, ref knockBack, ref crit);
			}
		}

		private static HookList HookOnHitNPC = AddHook<Action<Item, Player, NPC, int, float, bool>>(g => g.OnHitNPC);
		//in Terraria.Player.ItemCheck for melee attacks before updating informational accessories
		//  call ItemLoader.OnHitNPC(item, this, Main.npc[num292], num295, num283, flag18)
		/// <summary>
		/// Calls ModItem.OnHitNPC and all GlobalItem.OnHitNPC hooks.
		/// </summary>
		public static void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
			item.ModItem?.OnHitNPC(player, target, damage, knockBack, crit);

			foreach (var g in HookOnHitNPC.Enumerate(item.globalItems)) {
				g.OnHitNPC(item, player, target, damage, knockBack, crit);
			}
		}

		private static HookList HookCanHitPvp = AddHook<Func<Item, Player, Player, bool>>(g => g.CanHitPvp);
		//in Terraria.Player.ItemCheck add to beginning of pvp collision check
		/// <summary>
		/// Calls all GlobalItem.CanHitPvp hooks, then ModItem.CanHitPvp, until one of them returns false. 
		/// If all of them return true, this returns true.
		/// </summary>
		public static bool CanHitPvp(Item item, Player player, Player target) {
			foreach (var g in HookCanHitPvp.Enumerate(item.globalItems)) {
				if (!g.CanHitPvp(item, player, target))
					return false;
			}

			return item.ModItem == null || item.ModItem.CanHitPvp(player, target);
		}

		private delegate void DelegateModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvp = AddHook<DelegateModifyHitPvp>(g => g.ModifyHitPvp);
		//in Terraria.Player.ItemCheck for pvp melee attacks after damage variation
		//  call ItemLoader.ModifyHitPvp(item, this, Main.player[num302], ref num282, ref flag20)
		/// <summary>
		/// Calls ModItem.ModifyHitPvp, then all GlobalItem.ModifyHitPvp hooks.
		/// </summary>
		public static void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit) {
			item.ModItem?.ModifyHitPvp(player, target, ref damage, ref crit);

			foreach (var g in HookModifyHitPvp.Enumerate(item.globalItems)) {
				g.ModifyHitPvp(item, player, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvp = AddHook<Action<Item, Player, Player, int, bool>>(g => g.OnHitPvp);
		//in Terraria.Player.ItemCheck for pvp melee attacks before NetMessage stuff
		//  call ItemLoader.OnHitPvp(item, this, Main.player[num302], num304, flag20)
		/// <summary>
		/// Calls ModItem.OnHitPvp and all GlobalItem.OnHitPvp hooks.
		/// </summary>
		public static void OnHitPvp(Item item, Player player, Player target, int damage, bool crit) {
			item.ModItem?.OnHitPvp(player, target, damage, crit);

			foreach (var g in HookOnHitPvp.Enumerate(item.globalItems)) {
				g.OnHitPvp(item, player, target, damage, crit);
			}
		}

		private static HookList HookUseItem = AddHook<Func<Item, Player, bool?>>(g => g.UseItem);
		/// <summary>
		/// Returns false if any of ModItem.UseItem or GlobalItem.UseItem return false.
		/// Returns true if anything returns true without returning false.
		/// Returns null by default.
		/// Does not fail fast (calls every hook)
		/// </summary>
		public static bool? UseItem(Item item, Player player) {
			if (item.IsAir)
				return false;

			bool? result = null;

			foreach (var g in HookUseItem.Enumerate(item.globalItems)) {
				bool? useItem = g.UseItem(item, player);

				if (useItem.HasValue && result != false) {
					result = useItem.Value;
				}
			}

			bool? modItemResult = item.ModItem?.UseItem(player);

			return result ?? modItemResult;
		}

		private static HookList HookUseAnimation = AddHook<Action<Item, Player>>(g => g.UseAnimation);
		
		public static void UseAnimation(Item item, Player player) {
			foreach (var g in HookUseAnimation.Enumerate(item.globalItems)) {
				g.Instance(item).UseAnimation(item, player);
			}

			item.ModItem?.UseAnimation(player);
		}

		private static HookList HookConsumeItem = AddHook<Func<Item, Player, bool>>(g => g.ConsumeItem);
		//near end of Terraria.Player.ItemCheck
		//  if (flag22 && ItemLoader.ConsumeItem(item, this))
		/// <summary>
		/// If ModItem.ConsumeItem or any of the GlobalItem.ConsumeItem hooks returns false, sets consume to false.
		/// </summary>
		public static bool ConsumeItem(Item item, Player player) {
			if (item.IsAir) return true;
			if (item.ModItem != null && !item.ModItem.ConsumeItem(player))
				return false;

			foreach (var g in HookConsumeItem.Enumerate(item.globalItems)) {
				if (!g.ConsumeItem(item, player))
					return false;
			}

			OnConsumeItem(item, player);
			return true;
		}

		private static HookList HookOnConsumeItem = AddHook<Action<Item, Player>>(g => g.OnConsumeItem);
		/// <summary>
		/// Calls ModItem.OnConsumeItem and all GlobalItem.OnConsumeItem hooks.
		/// </summary>
		public static void OnConsumeItem(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.OnConsumeItem(player);

			foreach (var g in HookOnConsumeItem.Enumerate(item.globalItems)) {
				g.OnConsumeItem(item, player);
			}
		}

		private static HookList HookUseItemFrame = AddHook<Action<Item, Player>>(g => g.UseItemFrame);
		//in Terraria.Player.PlayerFrame at end of useStyle if/else chain
		//  call if(ItemLoader.UseItemFrame(this.inventory[this.selectedItem], this)) { return; }
		/// <summary>
		/// Calls ModItem.UseItemFrame, then all GlobalItem.UseItemFrame hooks, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static void UseItemFrame(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.UseItemFrame(player);

			foreach (var g in HookUseItemFrame.Enumerate(item.globalItems)) {
				g.UseItemFrame(item, player);
			}
		}

		private static HookList HookHoldItemFrame = AddHook<Action<Item, Player>>(g => g.HoldItemFrame);
		//in Terraria.Player.PlayerFrame at end of holdStyle if statements
		//  call if(ItemLoader.HoldItemFrame(this.inventory[this.selectedItem], this)) { return; }
		/// <summary>
		/// Calls ModItem.HoldItemFrame, then all GlobalItem.HoldItemFrame hooks, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static void HoldItemFrame(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.HoldItemFrame(player);

			foreach (var g in HookHoldItemFrame.Enumerate(item.globalItems)) {
				g.HoldItemFrame(item, player);
			}
		}

		private static HookList HookAltFunctionUse = AddHook<Func<Item, Player, bool>>(g => g.AltFunctionUse);
		/// <summary>
		/// Calls ModItem.AltFunctionUse, then all GlobalItem.AltFunctionUse hooks, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static bool AltFunctionUse(Item item, Player player) {
			if (item.IsAir)
				return false;

			if (item.ModItem != null && item.ModItem.AltFunctionUse(player))
				return true;

			foreach (var g in HookAltFunctionUse.Enumerate(item.globalItems)) {
				if (g.AltFunctionUse(item, player))
					return true;
			}

			return false;
		}

		private static HookList HookUpdateInventory = AddHook<Action<Item, Player>>(g => g.UpdateInventory);
		//place at end of first for loop in Terraria.Player.UpdateEquips
		//  call ItemLoader.UpdateInventory(this.inventory[j], this)
		/// <summary>
		/// Calls ModItem.UpdateInventory and all GlobalItem.UpdateInventory hooks.
		/// </summary>
		public static void UpdateInventory(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.UpdateInventory(player);

			foreach (var g in HookUpdateInventory.Enumerate(item.globalItems)) {
				g.UpdateInventory(item, player);
			}
		}

		private static HookList HookUpdateEquip = AddHook<Action<Item, Player>>(g => g.UpdateEquip);
		/// <summary>
		/// Hook at the end of Player.VanillaUpdateEquip can be called from modded slots for modded equipments
		/// </summary>
		public static void UpdateEquip(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.UpdateEquip(player);

			foreach (var g in HookUpdateEquip.Enumerate(item.globalItems)) {
				g.UpdateEquip(item, player);
			}
		}

		private static HookList HookUpdateAccessory = AddHook<Action<Item, Player, bool>>(g => g.UpdateAccessory);
		/// <summary>
		/// Hook at the end of Player.ApplyEquipFunctional can be called from modded slots for modded equipments
		/// </summary>
		public static void UpdateAccessory(Item item, Player player, bool hideVisual) {
			if (item.IsAir)
				return;

			item.ModItem?.UpdateAccessory(player, hideVisual);

			foreach (var g in HookUpdateAccessory.Enumerate(item.globalItems)) {
				g.UpdateAccessory(item, player, hideVisual);
			}
		}

		private static HookList HookUpdateVanity = AddHook<Action<Item, Player>>(g => g.UpdateVanity);
		/// <summary>
		/// Hook at the end of Player.ApplyEquipVanity can be called from modded slots for modded equipments
		/// </summary>
		public static void UpdateVanity(Item item, Player player) {
			if (item.IsAir)
				return;

			item.ModItem?.UpdateVanity(player);

			foreach (var g in HookUpdateVanity.Enumerate(item.globalItems)) {
				g.UpdateVanity(item, player);
			}
		}

		private static HookList HookUpdateArmorSet = AddHook<Action<Player, string>>(g => g.UpdateArmorSet);
		//at end of Terraria.Player.UpdateArmorSets call ItemLoader.UpdateArmorSet(this, this.armor[0], this.armor[1], this.armor[2])
		/// <summary>
		/// If the head's ModItem.IsArmorSet returns true, calls the head's ModItem.UpdateArmorSet. This is then repeated for the body, then the legs. Then for each GlobalItem, if GlobalItem.IsArmorSet returns a non-empty string, calls GlobalItem.UpdateArmorSet with that string.
		/// </summary>
		public static void UpdateArmorSet(Player player, Item head, Item body, Item legs) {
			if (head.ModItem != null && head.ModItem.IsArmorSet(head, body, legs))
				head.ModItem.UpdateArmorSet(player);

			if (body.ModItem != null && body.ModItem.IsArmorSet(head, body, legs))
				body.ModItem.UpdateArmorSet(player);

			if (legs.ModItem != null && legs.ModItem.IsArmorSet(head, body, legs))
				legs.ModItem.UpdateArmorSet(player);

			foreach (GlobalItem globalItem in HookUpdateArmorSet.Enumerate(globalItemsArray)) {
				string set = globalItem.IsArmorSet(head, body, legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.UpdateArmorSet(player, set);
			}
		}

		private static HookList HookPreUpdateVanitySet = AddHook<Action<Player, string>>(g => g.PreUpdateVanitySet);
		//in Terraria.Player.PlayerFrame after setting armor effects fields call this
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's PreUpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.PreUpdateVanitySet, using player.head, player.body, and player.legs.
		/// </summary>
		public static void PreUpdateVanitySet(Player player) {
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body) ?? EquipLoader.GetEquipTexture(EquipType.BodyLegacy, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
				headTexture.PreUpdateVanitySet(player);

			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
				bodyTexture.PreUpdateVanitySet(player);

			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
				legTexture.PreUpdateVanitySet(player);

			foreach (GlobalItem globalItem in HookPreUpdateVanitySet.Enumerate(globalItemsArray)) {
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.PreUpdateVanitySet(player, set);
			}
		}

		private static HookList HookUpdateVanitySet = AddHook<Action<Player, string>>(g => g.UpdateVanitySet);
		//in Terraria.Player.PlayerFrame after armor sets creating dust call this
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's UpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.UpdateVanitySet, using player.head, player.body, and player.legs.
		/// </summary>
		public static void UpdateVanitySet(Player player) {
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body) ?? EquipLoader.GetEquipTexture(EquipType.BodyLegacy, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
				headTexture.UpdateVanitySet(player);

			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
				bodyTexture.UpdateVanitySet(player);

			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
				legTexture.UpdateVanitySet(player);

			foreach (GlobalItem globalItem in HookUpdateVanitySet.Enumerate(globalItemsArray)) {
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.UpdateVanitySet(player, set);
			}
		}

		private static HookList HookArmorSetShadows = AddHook<Action<Player, string>>(g => g.ArmorSetShadows);
		//in Terraria.Main.DrawPlayers after armor combinations setting flags call
		//  ItemLoader.ArmorSetShadows(player);
		/// <summary>
		/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's ArmorSetShadows. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.ArmorSetShadows, using player.head, player.body, and player.legs.
		/// </summary>
		public static void ArmorSetShadows(Player player) {
			EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body) ?? EquipLoader.GetEquipTexture(EquipType.BodyLegacy, player.body);
			EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

			if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
				headTexture.ArmorSetShadows(player);

			if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
				bodyTexture.ArmorSetShadows(player);

			if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
				legTexture.ArmorSetShadows(player);

			foreach (GlobalItem globalItem in HookArmorSetShadows.Enumerate(globalItemsArray)) {
				string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
				if (!string.IsNullOrEmpty(set))
					globalItem.ArmorSetShadows(player, set);
			}
		}

		private delegate void DelegateSetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes);
		private static HookList HookSetMatch = AddHook<DelegateSetMatch>(g => g.SetMatch);
		/// <summary>
		/// Calls EquipTexture.SetMatch, then all GlobalItem.SetMatch hooks.
		/// </summary>   
		public static void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) {
			EquipTexture texture = EquipLoader.GetEquipTexture((EquipType)armorSlot, type);

			texture?.SetMatch(male, ref equipSlot, ref robes);

			foreach (var g in HookSetMatch.Enumerate(globalItemsArray)) {
				g.SetMatch(armorSlot, type, male, ref equipSlot, ref robes);
			}
		}

		private static HookList HookCanRightClick = AddHook<Func<Item, bool>>(g => g.CanRightClick);
		//in Terraria.UI.ItemSlot.RightClick in end of item-opening if/else chain before final else
		//  make else if(ItemLoader.CanRightClick(inv[slot]))
		/// <summary>
		/// Calls ModItem.CanRightClick, then all GlobalItem.CanRightClick hooks, until one of the returns true. If one of the returns true, returns Main.mouseRight. Otherwise, returns false.
		/// </summary>
		public static bool CanRightClick(Item item) {
			if (item.IsAir || !Main.mouseRight)
				return false;

			if (item.ModItem != null && item.ModItem.CanRightClick())
				return true;

			foreach (var g in HookCanRightClick.Enumerate(item.globalItems)) {
				if (g.CanRightClick(item))
					return true;
			}

			return false;
		}

		private static HookList HookRightClick = AddHook<Action<Item, Player>>(g => g.RightClick);
		//in Terraria.UI.ItemSlot in block from CanRightClick call ItemLoader.RightClick(inv[slot], player)
		/// <summary>
		/// If Main.mouseRightRelease is true, the following steps are taken:
		/// 1. Call ModItem.RightClick
		/// 2. Calls all GlobalItem.RightClick hooks
		/// 3. Call ItemLoader.ConsumeItem, and if it returns true, decrements the item's stack
		/// 4. Sets the item's type to 0 if the item's stack is 0
		/// 5. Plays the item-grabbing sound
		/// 6. Sets Main.stackSplit to 30
		/// 7. Sets Main.mouseRightRelease to false
		/// 8. Calls Recipe.FindRecipes.
		/// </summary>
		public static void RightClick(Item item, Player player) {
			if (!Main.mouseRightRelease)
				return;

			item.ModItem?.RightClick(player);

			foreach (var g in HookRightClick.Enumerate(item.globalItems)) {
				g.RightClick(item, player);
			}

			if (ConsumeItem(item, player) && --item.stack == 0)
				item.SetDefaults();

			SoundEngine.PlaySound(7);
			Main.stackSplit = 30;
			Main.mouseRightRelease = false;
			Recipe.FindRecipes();
		}

		//in Terraria.UI.ItemSlot add this to boss bag check
		/// <summary>
		/// Returns whether ModItem.bossBagNPC is greater than 0. Returns false if item is not a modded item.
		/// </summary>
		public static bool IsModBossBag(Item item) {
			return item.ModItem != null && item.ModItem.BossBagNPC > 0;
		}

		//in Terraria.Player.OpenBossBag after setting num14 call
		//  ItemLoader.OpenBossBag(type, this, ref num14);
		/// <summary>
		/// If the item is a modded item and ModItem.bossBagNPC is greater than 0, calls ModItem.OpenBossBag and sets npc to ModItem.bossBagNPC.
		/// </summary>
		public static void OpenBossBag(int type, Player player, ref int npc) {
			ModItem modItem = GetItem(type);
			if (modItem != null && modItem.BossBagNPC > 0) {
				modItem.OpenBossBag(player);
				npc = modItem.BossBagNPC;
			}
		}

		private static HookList HookPreOpenVanillaBag = AddHook<Func<string, Player, int, bool>>(g => g.PreOpenVanillaBag);
		//in beginning of Terraria.Player.openBag methods add
		//  if(!ItemLoader.PreOpenVanillaBag("bagName", this, arg)) { return; }
		//at the end of the following methods in Player.cs, add: NPCLoader.blockLoot.Clear(); // clear blockloot
		//methods: OpenBossBag, openCrate, openGoodieBag, openHerbBag, openLockbox, openPresent
		/// <summary>
		/// Calls each GlobalItem.PreOpenVanillaBag hook until one of them returns false. Returns true if all of them returned true.
		/// </summary>
		public static bool PreOpenVanillaBag(string context, Player player, int arg) {
			bool result = true;
			foreach (var g in HookPreOpenVanillaBag.Enumerate(globalItemsArray)) {
				result &= g.PreOpenVanillaBag(context, player, arg);
			}

			if (!result) {
				NPCLoader.blockLoot.Clear(); // clear blockloot
				return false;
			}

			return true;
		}

		private static HookList HookOpenVanillaBag = AddHook<Action<string, Player, int>>(g => g.OpenVanillaBag);
		//in Terraria.Player.openBag methods after PreOpenVanillaBag if statements
		//  add ItemLoader.OpenVanillaBag("bagname", this, arg);
		/// <summary>
		/// Calls all GlobalItem.OpenVanillaBag hooks.
		/// </summary>
		public static void OpenVanillaBag(string context, Player player, int arg) {
			foreach (var g in HookOpenVanillaBag.Enumerate(globalItemsArray)) {
				g.OpenVanillaBag(context, player, arg);
			}
		}

		private static HookList HookCanStackInWorld = AddHook<Func<Item, Item, bool>>(g => g.CanStackInWorld);
		//in Terraria.Item.CombineWithNearbyItems after num comparison
		// if(!ItemLoader.CanStackInWorld(this, item)) { continue; }
		/// <summary>
		/// Calls all GlobalItem.CanStackInWorld hooks until one returns false then ModItem.CanStackInWorld. Returns whether any of the hooks returned false.
		/// </summary>
		public static bool CanStackInWorld(Item item1, Item item2) {
			foreach (var g in HookCanStackInWorld.Enumerate(globalItemsArray)) {
				if (!g.CanStackInWorld(item1, item2))
					return false;
			}

			return item1.ModItem?.CanStackInWorld(item2) ?? true;
		}

		private delegate bool DelegateReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount);
		private static HookList HookReforgePrice = AddHook<DelegateReforgePrice>(g => g.ReforgePrice);
		/// <summary>
		/// Call all ModItem.ReforgePrice, then GlobalItem.ReforgePrice hooks.
		/// </summary>
		/// <param name="canApplyDiscount"></param>
		/// <returns></returns>
		public static bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
			bool b = item.ModItem?.ReforgePrice(ref reforgePrice, ref canApplyDiscount) ?? true;

			foreach (var g in HookReforgePrice.Enumerate(item.globalItems)) {
				b &= g.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
			}

			return b;
		}

		// @todo: PreReforge marked obsolete until v0.11
		private static HookList HookPreReforge = AddHook<Func<Item, bool>>(g => g.PreReforge);
		/// <summary>
		/// Calls ModItem.PreReforge, then all GlobalItem.PreReforge hooks.
		/// </summary>
		public static bool PreReforge(Item item) {
			bool b = item.ModItem?.PreReforge() ?? true;

			foreach (var g in HookPreReforge.Enumerate(item.globalItems)) {
				b &= g.PreReforge(item);
			}

			return b;
		}

		private static HookList HookPostReforge = AddHook<Action<Item>>(g => g.PostReforge);
		/// <summary>
		/// Calls ModItem.PostReforge, then all GlobalItem.PostReforge hooks.
		/// </summary>
		public static void PostReforge(Item item) {
			item.ModItem?.PostReforge();

			foreach (var g in HookPostReforge.Enumerate(item.globalItems)) {
				g.PostReforge(item);
			}
		}

		private delegate void DelegateDrawHands(int body, ref bool drawHands, ref bool drawArms);
		private static HookList HookDrawHands = AddHook<DelegateDrawHands>(g => g.DrawHands);
		/// <summary>
		/// Calls the item's body equipment texture's DrawHands hook, then all GlobalItem.DrawHands hooks.
		/// "body" is the player's associated body equipment texture.
		/// </summary>
		public static void DrawHands(Player player, ref bool drawHands, ref bool drawArms) {
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, player.body) ?? EquipLoader.GetEquipTexture(EquipType.BodyLegacy, player.body);

			texture?.DrawHands(ref drawHands, ref drawArms);

			foreach (var g in HookDrawHands.Enumerate(globalItemsArray)) {
				g.DrawHands(player.body, ref drawHands, ref drawArms);
			}
		}

		private delegate void DelegateDrawHair(int body, ref bool drawHair, ref bool drawAltHair);
		private static HookList HookDrawHair = AddHook<DelegateDrawHair>(g => g.DrawHair);
		//in Terraria.Main.DrawPlayerHead after if statement that sets flag2 to true
		//  call ItemLoader.DrawHair(drawPlayer, ref flag, ref flag2)
		//in Terraria.Main.DrawPlayer after if statement that sets flag5 to true
		//  call ItemLoader.DrawHair(drawPlayer, ref flag4, ref flag5)
		/// <summary>
		/// Calls the item's head equipment texture's DrawHair hook, then all GlobalItem.DrawHair hooks.
		/// "head" is the player's associated head equipment texture.
		/// </summary>
		public static void DrawHair(Player player, ref bool drawHair, ref bool drawAltHair) {
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			texture?.DrawHair(ref drawHair, ref drawAltHair);

			foreach (var g in HookDrawHair.Enumerate(globalItemsArray)) {
				g.DrawHair(player.head, ref drawHair, ref drawAltHair);
			}
		}

		private static HookList HookDrawHead = AddHook<Func<int, bool>>(g => g.DrawHead);
		//in Terraria.Main.DrawPlayerHead in if statement after ItemLoader.DrawHair
		//and in Terraria.Main.DrawPlayer in if (!drawPlayer.invis && drawPlayer.head != 38 && drawPlayer.head != 135)
		//  use && with ItemLoader.DrawHead(drawPlayer)
		/// <summary>
		/// Calls the item's head equipment texture's DrawHead hook, then all GlobalItem.DrawHead hooks, until one of them returns false. Returns true if none of them return false.
		/// "head" is the player's associated head equipment texture.
		/// </summary>
		public static bool DrawHead(Player player) {
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
			if (texture != null && !texture.DrawHead())
				return false;

			foreach (var g in HookDrawHead.Enumerate(globalItemsArray)) {
				if (!g.DrawHead(player.head))
					return false;
			}

			return true;
		}

		private static HookList HookDrawBody = AddHook<Func<int, bool>>(g => g.DrawBody);
		/// <summary>
		/// Calls the item's body equipment texture's DrawBody hook, then all GlobalItem.DrawBody hooks, until one of them returns false. Returns true if none of them return false.
		/// "body" is the player's associated body equipment texture.
		/// </summary>
		public static bool DrawBody(Player player) {
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, player.body) ?? EquipLoader.GetEquipTexture(EquipType.BodyLegacy, player.body);

			if (texture != null && !texture.DrawBody())
				return false;

			foreach (var g in HookDrawBody.Enumerate(globalItemsArray)) {
				if (!g.DrawBody(player.body))
					return false;
			}

			return true;
		}

		private static HookList HookDrawLegs = AddHook<Func<int, int, bool>>(g => g.DrawLegs);
		/// <summary>
		/// Calls the item's leg equipment texture's DrawLegs hook, then the item's shoe equipment texture's DrawLegs hook, then all GlobalItem.DrawLegs hooks, until one of them returns false. Returns true if none of them return false.
		/// "legs" and "shoes" are the player's associated legs and shoes equipment textures.
		/// </summary>
		public static bool DrawLegs(Player player) {
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);
			if (texture != null && !texture.DrawLegs())
				return false;

			texture = EquipLoader.GetEquipTexture(EquipType.Shoes, player.shoe);
			if (texture != null && !texture.DrawLegs())
				return false;

			foreach (var g in HookDrawLegs.Enumerate(globalItemsArray)) {
				if (!g.DrawLegs(player.legs, player.shoe))
					return false;
			}

			return true;
		}

		private delegate void DelegateDrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor);
		private static HookList HookDrawArmorColor = AddHook<DelegateDrawArmorColor>(g => g.DrawArmorColor);
		/// <summary>
		/// Calls the item's equipment texture's DrawArmorColor hook, then all GlobalItem.DrawArmorColor hooks.
		/// </summary>
		public static void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color,
			ref int glowMask, ref Color glowMaskColor) {
			EquipTexture texture = EquipLoader.GetEquipTexture(type, slot);
			texture?.DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);

			foreach (var g in HookDrawArmorColor.Enumerate(globalItemsArray)) {
				g.DrawArmorColor(type, slot, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
			}
		}

		private delegate void DelegateArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color);
		private static HookList HookArmorArmGlowMask = AddHook<DelegateArmorArmGlowMask>(g => g.ArmorArmGlowMask);
		/// <summary>
		/// Calls the item's body equipment texture's ArmorArmGlowMask hook, then all GlobalItem.ArmorArmGlowMask hooks.
		/// </summary>
		public static void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, slot) ?? EquipLoader.GetEquipTexture(EquipType.BodyLegacy, slot);

			texture?.ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);

			foreach (var g in HookArmorArmGlowMask.Enumerate(globalItemsArray)) {
				g.ArmorArmGlowMask(slot, drawPlayer, shadow, ref glowMask, ref color);
			}
		}

		/// <summary>s
		/// Returns the wing item that the player is functionally using. If player.wingsLogic has been modified, so no equipped wing can be found to match what the player is using, this creates a new Item object to return.
		/// </summary>
		public static Item GetWing(Player player) {
			//TODO: this doesn't work with wings in modded accessory slots
			Item item = null;
			for (int k = 3; k < 10; k++) {
				if (player.armor[k].wingSlot == player.wingsLogic) {
					item = player.armor[k];
				}
			}
			if (item != null) {
				return item;
			}
			if (player.wingsLogic > 0 && player.wingsLogic < Main.maxWings) {
				item = new Item();
				item.SetDefaults(vanillaWings[player.wingsLogic]);
				return item;
			}
			if (player.wingsLogic >= Main.maxWings) {
				EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic);
				if (texture?.Item != null)
					return texture.Item.Item;
			}
			return null;
		}

		private delegate void DelegateVerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend);
		private static HookList HookVerticalWingSpeeds = AddHook<DelegateVerticalWingSpeeds>(g => g.VerticalWingSpeeds);
		//in Terraria.Player.WingMovement after if statements that set num1-5
		//  call ItemLoader.VerticalWingSpeeds(this, ref num2, ref num5, ref num4, ref num3, ref num)
		/// <summary>
		/// If the player is using wings, this uses the result of GetWing, and calls ModItem.VerticalWingSpeeds then all GlobalItem.VerticalWingSpeeds hooks.
		/// </summary>
		public static void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			Item item = GetWing(player);
			if (item == null) {
				EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic);
				texture?.VerticalWingSpeeds(
					player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
					ref maxAscentMultiplier, ref constantAscend);
				return;
			}

			item.ModItem?.VerticalWingSpeeds(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
				ref maxAscentMultiplier, ref constantAscend);

			foreach (var g in HookVerticalWingSpeeds.Enumerate(item.globalItems)) {
				g.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising,
					ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
			}
		}

		private delegate void DelegateHorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration);
		private static HookList HookHorizontalWingSpeeds = AddHook<DelegateHorizontalWingSpeeds>(g => g.HorizontalWingSpeeds);
		//in Terraria.Player.Update after wingsLogic if statements modifying accRunSpeed and runAcceleration
		//  call ItemLoader.HorizontalWingSpeeds(this)
		/// <summary>
		/// If the player is using wings, this uses the result of GetWing, and calls ModItem.HorizontalWingSpeeds then all GlobalItem.HorizontalWingSpeeds hooks.
		/// </summary>
		public static void HorizontalWingSpeeds(Player player) {
			Item item = GetWing(player);
			if (item == null) {
				EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic);
				texture?.HorizontalWingSpeeds(player, ref player.accRunSpeed, ref player.runAcceleration);
				return;
			}
			
			item.ModItem?.HorizontalWingSpeeds(player, ref player.accRunSpeed, ref player.runAcceleration);

			foreach (var g in HookHorizontalWingSpeeds.Enumerate(item.globalItems)) {
				g.HorizontalWingSpeeds(item, player, ref player.accRunSpeed, ref player.runAcceleration);
			}
		}

		private static HookList HookWingUpdate = AddHook<Func<int, Player, bool, bool>>(g => g.WingUpdate);
		/// <summary>
		/// If wings can be seen on the player, calls the player's wing's equipment texture's WingUpdate and all GlobalItem.WingUpdate hooks.
		/// </summary>
		public static bool WingUpdate(Player player, bool inUse) {
			if (player.wings <= 0)
				return false;

			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wings);
			bool? retVal = texture?.WingUpdate(player, inUse);

			foreach (var g in HookWingUpdate.Enumerate(globalItemsArray)) {
				retVal |= g.WingUpdate(player.wings, player, inUse);
			}

			return retVal ?? false;
		}

		private delegate void DelegateUpdate(Item item, ref float gravity, ref float maxFallSpeed);
		private static HookList HookUpdate = AddHook<DelegateUpdate>(g => g.Update);
		//in Terraria.Item.UpdateItem before item movement (denoted by ItemID.Sets.ItemNoGravity)
		//  call ItemLoader.Update(this, ref num, ref num2)
		/// <summary>
		/// Calls ModItem.Update, then all GlobalItem.Update hooks.
		/// </summary>
		public static void Update(Item item, ref float gravity, ref float maxFallSpeed) {
			item.ModItem?.Update(ref gravity, ref maxFallSpeed);

			foreach (var g in HookUpdate.Enumerate(item.globalItems)) {
				g.Update(item, ref gravity, ref maxFallSpeed);
			}
		}

		private static HookList HookCanBurnInLava = AddHook<Func<Item, bool?>>(g => g.CanBurnInLava);
		/// <summary>
		/// Calls ModItem.CanBurnInLava.
		/// </summary>
		public static bool? CanBurnInLava(Item item)
		{
			bool? canBurnInLava = null;
			foreach (var g in HookCanBurnInLava.Enumerate(item.globalItems)) {
				switch (g.CanBurnInLava(item)) {
					case null:
						continue;
					case false:
						canBurnInLava = false;
						continue;
					case true:
						return true;
				}
			}

			return canBurnInLava ?? item.ModItem?.CanBurnInLava();
		}
		
		private static HookList HookPostUpdate = AddHook<Action<Item>>(g => g.PostUpdate);
		/// <summary>
		/// Calls ModItem.PostUpdate and all GlobalItem.PostUpdate hooks.
		/// </summary>
		public static void PostUpdate(Item item) {
			item.ModItem?.PostUpdate();

			foreach (var g in HookPostUpdate.Enumerate(item.globalItems)) {
				g.PostUpdate(item);
			}
		}

		private delegate void DelegateGrabRange(Item item, Player player, ref int grabRange);
		private static HookList HookGrabRange = AddHook<DelegateGrabRange>(g => g.GrabRange);
		//in Terraria.Player.GrabItems after increasing grab range add
		//  ItemLoader.GrabRange(Main.item[j], this, ref num);
		/// <summary>
		/// Calls ModItem.GrabRange, then all GlobalItem.GrabRange hooks.
		/// </summary>
		public static void GrabRange(Item item, Player player, ref int grabRange) {
			item.ModItem?.GrabRange(player, ref grabRange);

			foreach (var g in HookGrabRange.Enumerate(item.globalItems)) {
				g.GrabRange(item, player, ref grabRange);
			}
		}

		private static HookList HookGrabStyle = AddHook<Func<Item, Player, bool>>(g => g.GrabStyle);
		//in Terraria.Player.GrabItems between setting beingGrabbed to true and grab styles add
		//  if(ItemLoader.GrabStyle(Main.item[j], this)) { } else
		/// <summary>
		/// Calls all GlobalItem.GrabStyle hooks then ModItem.GrabStyle, until one of them returns true. Returns whether any of the hooks returned true.
		/// </summary>
		public static bool GrabStyle(Item item, Player player) {
			foreach (var g in HookGrabStyle.Enumerate(item.globalItems)) {
				if (g.GrabStyle(item, player))
					return true;
			}

			return item.ModItem != null && item.ModItem.GrabStyle(player);
		}

		private static HookList HookCanPickup = AddHook<Func<Item, Player, bool>>(g => g.CanPickup);
		//in Terraria.Player.GrabItems first per item if statement add
		//  && ItemLoader.CanPickup(Main.item[j], this)
		public static bool CanPickup(Item item, Player player) {
			foreach (var g in HookCanPickup.Enumerate(item.globalItems)) {
				if (!g.CanPickup(item, player))
					return false;
			}

			return item.ModItem?.CanPickup(player) ?? true;
		}

		private static HookList HookOnPickup = AddHook<Func<Item, Player, bool>>(g => g.OnPickup);
		//in Terraria.Player.GrabItems before special pickup effects add
		//  if(!ItemLoader.OnPickup(Main.item[j], this)) { Main.item[j] = new Item(); continue; }
		/// <summary>
		/// Calls all GlobalItem.OnPickup hooks then ModItem.OnPickup, until one of the returns false. Returns true if all of the hooks return true.
		/// </summary>
		public static bool OnPickup(Item item, Player player) {
			foreach (var g in HookOnPickup.Enumerate(item.globalItems)) {
				if (!g.OnPickup(item, player))
					return false;
			}

			return item.ModItem?.OnPickup(player) ?? true;
		}

		private static HookList HookItemSpace = AddHook<Func<Item, Player, bool>>(g => g.ItemSpace);
		//in Terraria.Player.GrabItems before grab effect
		//  (this.ItemSpace(Main.item[j]) || ItemLoader.ExtraPickupSpace(Main.item[j], this)
		public static bool ItemSpace(Item item, Player player) {
			foreach (var g in HookItemSpace.Enumerate(item.globalItems)) {
				if (g.ItemSpace(item, player))
					return true;
			}

			return item.ModItem?.ItemSpace(player) ?? false;
		}

		private static HookList HookGetAlpha = AddHook<Func<Item, Color, Color?>>(g => g.GetAlpha);
		//in Terraria.UI.ItemSlot.GetItemLight remove type too high check
		//in beginning of Terraria.Item.GetAlpha call
		//  Color? modColor = ItemLoader.GetAlpha(this, newColor);
		//  if(modColor.HasValue) { return modColor.Value; }
		/// <summary>
		/// Calls all GlobalItem.GetAlpha hooks then ModItem.GetAlpha, until one of them returns a color, and returns that color. Returns null if all of the hooks return null.
		/// </summary>
		public static Color? GetAlpha(Item item, Color lightColor) {
			if (item.IsAir)
				return null;

			foreach (var g in HookGetAlpha.Enumerate(item.globalItems)) {
				Color? color = g.GetAlpha(item, lightColor);
				if (color.HasValue)
					return color;
			}

			return item.ModItem?.GetAlpha(lightColor);
		}

		private delegate bool DelegatePreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI);
		private static HookList HookPreDrawInWorld = AddHook<DelegatePreDrawInWorld>(g => g.PreDrawInWorld);

		/// <summary>
		/// Returns the "and" operator on the results of ModItem.PreDrawInWorld and all GlobalItem.PreDrawInWorld hooks.
		/// </summary>
		public static bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			bool flag = true;
			if (item.ModItem != null)
				flag &= item.ModItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

			foreach (var g in HookPreDrawInWorld.Enumerate(item.globalItems)) {
				flag &= g.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
			}

			return flag;
		}

		private static HookList HookPostDrawInWorld = AddHook<Action<Item, SpriteBatch, Color, Color, float, float, int>>(g => g.PostDrawInWorld);
		//in Terraria.Main.DrawItem before every return (including for PreDrawInWorld) and at end of method call
		//  ItemLoader.PostDrawInWorld(item, Main.spriteBatch, color, alpha, rotation, scale)
		/// <summary>
		/// Calls ModItem.PostDrawInWorld, then all GlobalItem.PostDrawInWorld hooks.
		/// </summary>
		public static void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			item.ModItem?.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);

			foreach (var g in HookPostDrawInWorld.Enumerate(item.globalItems)) {
				g.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
			}
		}

		private static HookList HookPreDrawInInventory = AddHook<Func<Item, SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float, bool>>(g => g.PreDrawInInventory);
		//in Terraria.UI.ItemSlot.Draw place item-drawing code inside if statement
		//  if(ItemLoader.PreDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
		//    item.GetColor(color), origin, num4 * num3))
		/// <summary>
		/// Returns the "and" operator on the results of all GlobalItem.PreDrawInInventory hooks and ModItem.PreDrawInInventory.
		/// </summary>
		public static bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) {
			bool flag = true;
			foreach (var g in HookPreDrawInInventory.Enumerate(item.globalItems)) {
				flag &= g.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}

			if (item.ModItem != null)
				flag &= item.ModItem.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);

			return flag;
		}

		private static HookList HookPostDrawInInventory = AddHook<Action<Item, SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float>>(g => g.PostDrawInInventory);
		//in Terraria.UI.ItemSlot.Draw after if statement for PreDrawInInventory call
		//  ItemLoader.PostDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
		//    item.GetColor(color), origin, num4 * num3);
		/// <summary>
		/// Calls ModItem.PostDrawInInventory, then all GlobalItem.PostDrawInInventory hooks.
		/// </summary>
		public static void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) {
			item.ModItem?.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);

			foreach (var g in HookPostDrawInInventory.Enumerate(item.globalItems)) {
				g.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}
		}

		private static HookList HookHoldoutOffset = AddHook<Func<int, Vector2?>>(g => g.HoldoutOffset);
		public static void HoldoutOffset(float gravDir, int type, ref Vector2 offset) {
			ModItem modItem = GetItem(type);

			if (modItem != null) {
				Vector2? modOffset = modItem.HoldoutOffset();

				if (modOffset.HasValue) {
					offset.X = modOffset.Value.X;
					offset.Y += gravDir * modOffset.Value.Y;
				}
			}

			foreach (var g in HookHoldoutOffset.Enumerate(globalItemsArray)) {
				Vector2? modOffset = g.HoldoutOffset(type);

				if (modOffset.HasValue) {
					offset.X = modOffset.Value.X;
					offset.Y = TextureAssets.Item[type].Value.Height / 2f + gravDir * modOffset.Value.Y;
				}
			}
		}

		private static HookList HookHoldoutOrigin = AddHook<Func<int, Vector2?>>(g => g.HoldoutOrigin);
		public static void HoldoutOrigin(Player player, ref Vector2 origin) {
			Item item = player.inventory[player.selectedItem];
			Vector2 modOrigin = Vector2.Zero;
			if (item.ModItem != null) {
				Vector2? modOrigin2 = item.ModItem.HoldoutOrigin();
				if (modOrigin2.HasValue) {
					modOrigin = modOrigin2.Value;
				}
			}
			foreach (var g in HookHoldoutOrigin.Enumerate(item.globalItems)) {
				Vector2? modOrigin2 = g.HoldoutOrigin(item.type);
				if (modOrigin2.HasValue) {
					modOrigin = modOrigin2.Value;
				}
			}
			modOrigin.X *= player.direction;
			modOrigin.Y *= -player.gravDir;
			origin += modOrigin;
		}

		private static HookList HookCanEquipAccessory = AddHook<Func<Item, Player, int, bool>>(g => g.CanEquipAccessory);
		//in Terraria.UI.ItemSlot.AccCheck replace 2nd and 3rd return false with
		//  return !ItemLoader.CanEquipAccessory(item, slot)
		public static bool CanEquipAccessory(Item item, int slot) {
			Player player = Main.player[Main.myPlayer];
			if (item.ModItem != null && !item.ModItem.CanEquipAccessory(player, slot))
				return false;

			foreach (var g in HookCanEquipAccessory.Enumerate(item.globalItems)) {
				if (!g.CanEquipAccessory(item, player, slot))
					return false;
			}

			return true;
		}

		private delegate void DelegateExtractinatorUse(int extractType, ref int resultType, ref int resultStack);
		private static HookList HookExtractinatorUse = AddHook<DelegateExtractinatorUse>(g => g.ExtractinatorUse);
		public static void ExtractinatorUse(ref int resultType, ref int resultStack, int extractType) {
			GetItem(extractType)?.ExtractinatorUse(ref resultType, ref resultStack);

			foreach (var g in HookExtractinatorUse.Enumerate(globalItemsArray)) {
				g.ExtractinatorUse(extractType, ref resultType, ref resultStack);
			}
		}

		private delegate void DelegateCaughtFishStack(int type, ref int stack);
		private static HookList HookCaughtFishStack = AddHook<DelegateCaughtFishStack>(g => g.CaughtFishStack);
		public static void CaughtFishStack(Item item) {
			item.ModItem?.CaughtFishStack(ref item.stack);

			foreach (var g in HookCaughtFishStack.Enumerate(item.globalItems)) {
				g.CaughtFishStack(item.type, ref item.stack);
			}
		}

		private static HookList HookIsAnglerQuestAvailable = AddHook<Func<int, bool>>(g => g.IsAnglerQuestAvailable);
		public static void IsAnglerQuestAvailable(int itemID, ref bool notAvailable) {
			ModItem modItem = GetItem(itemID);
			if (modItem != null)
				notAvailable |= !modItem.IsAnglerQuestAvailable();

			foreach (var g in HookIsAnglerQuestAvailable.Enumerate(globalItemsArray)) {
				notAvailable |= !g.IsAnglerQuestAvailable(itemID);
			}
		}

		private delegate void DelegateAnglerChat(int type, ref string chat, ref string catchLocation);
		private static HookList HookAnglerChat = AddHook<DelegateAnglerChat>(g => g.AnglerChat);
		public static string AnglerChat(int type) {
			string chat = "";
			string catchLocation = "";
			GetItem(type)?.AnglerQuestChat(ref chat, ref catchLocation);

			foreach (var g in HookAnglerChat.Enumerate(globalItemsArray)) {
				g.AnglerChat(type, ref chat, ref catchLocation);
			}

			if (string.IsNullOrEmpty(chat) || string.IsNullOrEmpty(catchLocation))
				return null;

			return chat + "\n\n(" + catchLocation + ")";
		}

		private delegate bool DelegatePreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y);
		private static HookList HookPreDrawTooltip = AddHook<DelegatePreDrawTooltip>(g => g.PreDrawTooltip);
		public static bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) {
			bool modItemPreDraw = item.ModItem?.PreDrawTooltip(lines, ref x, ref y) ?? true;
			List<bool> globalItemPreDraw = new List<bool>();
			
			foreach (var g in HookPreDrawTooltip.Enumerate(item.globalItems)) {
				globalItemPreDraw.Add(g.PreDrawTooltip(item, lines, ref x, ref y));
			}

			return modItemPreDraw && globalItemPreDraw.All(z => z);
		}

		private delegate void DelegatePostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines);
		private static HookList HookPostDrawTooltip = AddHook<DelegatePostDrawTooltip>(g => g.PostDrawTooltip);
		public static void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines) {
			item.ModItem?.PostDrawTooltip(lines);

			foreach (var g in HookPostDrawTooltip.Enumerate(item.globalItems)) {
				g.PostDrawTooltip(item, lines);
			}
		}

		private delegate bool DelegatePreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset);
		private static HookList HookPreDrawTooltipLine = AddHook<DelegatePreDrawTooltipLine>(g => g.PreDrawTooltipLine);
		public static bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
			bool modItemPreDrawLine = item.ModItem?.PreDrawTooltipLine(line, ref yOffset) ?? true;
			List<bool> globalItemPreDrawLine = new List<bool>();

			foreach (var g in HookPreDrawTooltipLine.Enumerate(item.globalItems)) {
				globalItemPreDrawLine.Add(g.PreDrawTooltipLine(item, line, ref yOffset));
			}
			
			return modItemPreDrawLine && globalItemPreDrawLine.All(x => x);
		}

		private delegate void DelegatePostDrawTooltipLine(Item item, DrawableTooltipLine line);
		private static HookList HookPostDrawTooltipLine = AddHook<DelegatePostDrawTooltipLine>(g => g.PostDrawTooltipLine);
		public static void PostDrawTooltipLine(Item item, DrawableTooltipLine line) {
			item.ModItem?.PostDrawTooltipLine(line);

			foreach (var g in HookPostDrawTooltipLine.Enumerate(item.globalItems)) {
				g.PostDrawTooltipLine(item, line);
			}
		}

		private static HookList HookModifyTooltips = AddHook<Action<Item, List<TooltipLine>>>(g => g.ModifyTooltips);
		public static List<TooltipLine> ModifyTooltips(Item item, ref int numTooltips, string[] names, ref string[] text,
			ref bool[] modifier, ref bool[] badModifier, ref int oneDropLogo, out Color?[] overrideColor) {
			List<TooltipLine> tooltips = new List<TooltipLine>();
			for (int k = 0; k < numTooltips; k++) {
				TooltipLine tooltip = new TooltipLine(names[k], text[k]);
				tooltip.isModifier = modifier[k];
				tooltip.isModifierBad = badModifier[k];
				if (k == oneDropLogo) {
					tooltip.oneDropLogo = true;
				}
				tooltips.Add(tooltip);
			}
			
			item.ModItem?.ModifyTooltips(tooltips);

			foreach (var g in HookModifyTooltips.Enumerate(item.globalItems)) {
				g.ModifyTooltips(item, tooltips);
			}

			numTooltips = tooltips.Count;
			text = new string[numTooltips];
			modifier = new bool[numTooltips];
			badModifier = new bool[numTooltips];
			oneDropLogo = -1;
			overrideColor = new Color?[numTooltips];
			for (int k = 0; k < numTooltips; k++) {
				text[k] = tooltips[k].text;
				modifier[k] = tooltips[k].isModifier;
				badModifier[k] = tooltips[k].isModifierBad;
				if (tooltips[k].oneDropLogo) {
					oneDropLogo = k;
				}
				overrideColor[k] = tooltips[k].overrideColor;
			}

			return tooltips;
		}

		private static HookList HookNeedsSaving = AddHook<Func<Item, bool>>(g => g.NeedsSaving);
		public static bool NeedsModSaving(Item item) {
			if (item.type <= ItemID.None)
				return false;

			if (item.ModItem != null || item.prefix >= PrefixID.Count)
				return true;

			foreach (var g in HookNeedsSaving.Enumerate(item.globalItems)) {
				if (g.NeedsSaving(item))
					return true;
			}

			return false;
		}

		internal static void WriteNetGlobalOrder(BinaryWriter w) {
			w.Write((short)NetGlobals.Length);
			foreach (var globalItem in NetGlobals) {
				w.Write(globalItem.Mod.netID);
				w.Write(globalItem.Name);
			}
		}

		internal static void ReadNetGlobalOrder(BinaryReader r) {
			short n = r.ReadInt16();
			NetGlobals = new GlobalItem[n];
			for (short i = 0; i < n; i++)
				NetGlobals[i] = ModContent.Find<GlobalItem>(ModNet.GetMod(r.ReadInt16()).Name, r.ReadString());
		}

		private static bool HasMethod(Type t, string method, params Type[] args) {
			return t.GetMethod(method, args).DeclaringType != typeof(GlobalItem);
		}

		internal static void VerifyGlobalItem(GlobalItem item) {
			var type = item.GetType();
			int saveMethods = 0;
			if (HasMethod(type, "NeedsSaving", typeof(Item))) saveMethods++;
			if (HasMethod(type, "Save", typeof(Item))) saveMethods++;
			if (HasMethod(type, "Load", typeof(Item), typeof(TagCompound))) saveMethods++;
			if (saveMethods > 0 && saveMethods < 3)
				throw new Exception(type + " must override all of (NeedsSaving/Save/Load) or none");

			int netMethods = 0;
			if (HasMethod(type, "NetSend", typeof(Item), typeof(BinaryWriter))) netMethods++;
			if (HasMethod(type, "NetReceive", typeof(Item), typeof(BinaryReader))) netMethods++;
			if (netMethods == 1)
				throw new Exception(type + " must override both of (NetSend/NetReceive) or none");

			bool hasInstanceFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Any(f => f.DeclaringType.IsSubclassOf(typeof(GlobalItem)));

			if (hasInstanceFields) {
				if (!item.InstancePerEntity)
					throw new Exception(type + " has instance fields but does not set InstancePerEntity to true. Either use static fields, or per instance globals");

				if (!HasMethod(type, "Clone", typeof(Item), typeof(Item)))
					throw new Exception(type + " has InstancePerEntity but does not override Clone(Item, Item)");
			}
		}
	}
}
