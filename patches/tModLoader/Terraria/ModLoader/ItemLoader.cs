using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.Utilities;
using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.GlobalItem>;

namespace Terraria.ModLoader;

/// <summary>
/// This serves as the central class from which item-related functions are carried out. It also stores a list of mod items by ID.
/// </summary>
public static class ItemLoader
{
	internal static readonly IList<ModItem> items = new List<ModItem>();
	internal static readonly List<GlobalItem> globalItems = new();
	internal static GlobalItem[] NetGlobals;
	internal static readonly int vanillaQuestFishCount = 41;
	internal static readonly int[] vanillaWings = new int[Main.maxWings];

	private static int nextItem = ItemID.Count;

	private static readonly List<HookList> hooks = new List<HookList>();
	private static readonly List<HookList> modHooks = new List<HookList>();

	private static HookList AddHook<F>(Expression<Func<GlobalItem, F>> func) where F : Delegate {
		var hook = HookList.Create(func);

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
		LoaderUtils.ResetStaticMembers(typeof(AmmoID), true);
		LoaderUtils.ResetStaticMembers(typeof(PrefixLegacy.ItemSets), true);

		//Etc
		Array.Resize(ref Item.cachedItemSpawnsByType, nextItem);
		Array.Resize(ref Item.staff, nextItem);
		Array.Resize(ref Item.claw, nextItem);
		Array.Resize(ref Lang._itemNameCache, nextItem);
		Array.Resize(ref Lang._itemTooltipCache, nextItem);

		Array.Resize(ref RecipeLoader.FirstRecipeForItem, nextItem);

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

		NetGlobals = globalItems.WhereMethodIsOverridden<GlobalItem, Action<Item, BinaryWriter>>(g => g.NetSend).ToArray();

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

	internal static bool IsModItem(int index)
		=> index >= ItemID.Count;

	internal static bool MeleePrefix(Item item)
		=> item.ModItem != null && item.ModItem.MeleePrefix();

	internal static bool WeaponPrefix(Item item)
		=> item.ModItem != null && item.ModItem.WeaponPrefix();

	internal static bool RangedPrefix(Item item)
		=> item.ModItem != null && item.ModItem.RangedPrefix();

	internal static bool MagicPrefix(Item item)
		=> item.ModItem != null && item.ModItem.MagicPrefix();

	private static HookList HookSetDefaults = AddHook<Action<Item>>(g => g.SetDefaults);

	internal static void SetDefaults(Item item, bool createModItem = true) {
		if (IsModItem(item.type) && createModItem)
			item.ModItem = GetItem(item.type).NewInstance(item);

		LoaderUtils.InstantiateGlobals(item, globalItems, ref item.globalItems, () => {
			item.ModItem?.AutoDefaults();
			item.ModItem?.SetDefaults();
		});

		foreach (var g in HookSetDefaults.Enumerate(item.globalItems)) {
			g.SetDefaults(item);
		}
	}

	private static HookList HookOnSpawn = AddHook<Action<Item, IEntitySource>>(g => g.OnSpawn);

	internal static void OnSpawn(Item item, IEntitySource source) {
		item.ModItem?.OnSpawn(source);

		foreach (GlobalItem g in HookOnSpawn.Enumerate(item.globalItems)) {
			g.OnSpawn(item, source);
		}
	}

	private static HookList HookOnCreate = AddHook<Action<Item, ItemCreationContext>>(g => g.OnCreated);

	public static void OnCreated(Item item, ItemCreationContext context) {
		foreach (var g in HookOnCreate.Enumerate(item.globalItems)) {
			g.OnCreated(item, context);
		}

		item.ModItem?.OnCreated(context);
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

	public static bool CanUseItem(Item item, Player player) {
		if (item.ModItem != null && !item.ModItem.CanUseItem(player))
			return false;

		foreach (var g in HookCanUseItem.Enumerate(item.globalItems)) {
			if (!g.CanUseItem(item, player))
				return false;
		}

		return true;
	}

	private static HookList HookCanAutoReuseItem = AddHook<Func<Item, Player, bool?>>(g => g.CanAutoReuseItem);

	public static bool? CanAutoReuseItem(Item item, Player player) {
		bool? flag = null;

		foreach (var g in HookCanAutoReuseItem.Enumerate(item.globalItems)) {
			bool? allow = g.CanAutoReuseItem(item, player);

			if (allow.HasValue) {
				if (!allow.Value) {
					return false;
				}

				flag = true;
			}
		}

		if (item.ModItem != null) {
			bool? allow = item.ModItem.CanAutoReuseItem(player);

			if (allow.HasValue) {
				if (!allow.Value) {
					return false;
				}

				flag = true;
			}
		}

		return flag;
	}

	private static HookList HookUseStyle = AddHook<Action<Item, Player, Rectangle>>(g => g.UseStyle);

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

	/// <summary>
	/// If the player is not holding onto a rope and is not in the middle of using an item, calls ModItem.HoldStyle and all GlobalItem.HoldStyle hooks.
	/// <br/> Returns whether or not the vanilla logic should be skipped.
	/// </summary>
	public static void HoldStyle(Item item, Player player, Rectangle heldItemFrame) {
		if (item.IsAir || player.pulley || player.ItemAnimationActive)
			return;

		item.ModItem?.HoldStyle(player, heldItemFrame);

		foreach (var g in HookHoldStyle.Enumerate(item.globalItems)) {
			g.HoldStyle(item, player, heldItemFrame);
		}
	}

	private static HookList HookHoldItem = AddHook<Action<Item, Player>>(g => g.HoldItem);

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

	private delegate bool? DelegateCanConsumeBait(Player baiter, Item bait);
	private static HookList HookCanConsumeBait = AddHook<DelegateCanConsumeBait>(g => g.CanConsumeBait);

	public static bool? CanConsumeBait(Player player, Item bait) {
		bool? ret = bait.ModItem?.CanConsumeBait(player);

		foreach (GlobalItem g in HookCanConsumeBait.Enumerate(bait)) {
			if (g.CanConsumeBait(player, bait) is bool b)
				ret = (ret ?? true) && b;
		}
		
		return ret;
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

	private delegate bool DelegateCanResearch(Item item);
	private static HookList HookCanResearch = AddHook<DelegateCanResearch>(g => g.CanResearch);

	/// <summary>
	/// Hook that determines if an item will be prevented from being consumed by the research function. 
	/// </summary>
	/// <param name="item">The item to be consumed or not</param>
	public static bool CanResearch(Item item) {
		if (item.ModItem != null && !item.ModItem.CanResearch())
			return false;
		foreach (var g in HookCanResearch.Enumerate(item.globalItems)) {
			if (!g.Instance(item).CanResearch(item))
				return false;
		}
		return true;
	}

	private delegate void DelegateOnResearched(Item item, bool fullyResearched);
	private static HookList HookOnResearched = AddHook<DelegateOnResearched>(g => g.OnResearched);

	public static void OnResearched(Item item, bool fullyResearched) {
		if (item.IsAir)
			return;

		item.ModItem?.OnResearched(fullyResearched);

		foreach (var g in HookOnResearched.Enumerate(item.globalItems))
			g.Instance(item).OnResearched(item, fullyResearched);
	}
    
	private delegate void DelegateModifyWeaponDamage(Item item, Player player, ref StatModifier damage);
	private static HookList HookModifyWeaponDamage = AddHook<DelegateModifyWeaponDamage>(g => g.ModifyWeaponDamage);

	/// <summary>
	/// Calls ModItem.HookModifyWeaponDamage, then all GlobalItem.HookModifyWeaponDamage hooks.
	/// </summary>
	public static void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
		if (item.IsAir)
			return;

		item.ModItem?.ModifyWeaponDamage(player, ref damage);

		foreach (var g in HookModifyWeaponDamage.Enumerate(item.globalItems)) {
			g.ModifyWeaponDamage(item, player, ref damage);
		}
	}

	private delegate void DelegateModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback);
	private static HookList HookModifyWeaponKnockback = AddHook<DelegateModifyWeaponKnockback>(g => g.ModifyWeaponKnockback);

	/// <summary>
	/// Calls ModItem.ModifyWeaponKnockback, then all GlobalItem.ModifyWeaponKnockback hooks.
	/// </summary>
	public static void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) {
		if (item.IsAir)
			return;

		item.ModItem?.ModifyWeaponKnockback(player, ref knockback);

		foreach (var g in HookModifyWeaponKnockback.Enumerate(item.globalItems)) {
			g.ModifyWeaponKnockback(item, player, ref knockback);
		}
	}


	private delegate void DelegateModifyWeaponCrit(Item item, Player player, ref float crit);
	private static HookList HookModifyWeaponCrit = AddHook<DelegateModifyWeaponCrit>(g => g.ModifyWeaponCrit);

	/// <summary>
	/// Calls ModItem.ModifyWeaponCrit, then all GlobalItem.ModifyWeaponCrit hooks.
	/// </summary>
	public static void ModifyWeaponCrit(Item item, Player player, ref float crit) {
		if (item.IsAir)
			return;

		item.ModItem?.ModifyWeaponCrit(player, ref crit);

		foreach (var g in HookModifyWeaponCrit.Enumerate(item.globalItems)) {
			g.ModifyWeaponCrit(item, player, ref crit);
		}
	}

	private static HookList HookNeedsAmmo = AddHook<Func<Item, Player, bool>>(g => g.NeedsAmmo);
	/// <summary>
	/// Calls ModItem.NeedsAmmo, then all GlobalItem.NeedsAmmo hooks, until any of them returns false.
	/// </summary>
	public static bool NeedsAmmo(Item weapon, Player player) {
		if (!weapon.ModItem?.NeedsAmmo(player) ?? false)
			return false;

		foreach (var g in HookNeedsAmmo.Enumerate(weapon.globalItems)) {
			if (!g.NeedsAmmo(weapon, player))
				return false;
		}

		return true;
	}

	private delegate void DelegatePickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback);
	private static HookList HookPickAmmo = AddHook<DelegatePickAmmo>(g => g.PickAmmo);

	/// <summary>
	/// Calls ModItem.PickAmmo, then all GlobalItem.PickAmmo hooks.
	/// </summary>
	public static void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
		ammo.ModItem?.PickAmmo(weapon, player, ref type, ref speed, ref damage, ref knockback);

		foreach (var g in HookPickAmmo.Enumerate(ammo.globalItems)) {
			g.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
		}
	}

	private static HookList HookCanChooseAmmo = AddHook<Func<Item, Item, Player, bool?>>(g => g.CanChooseAmmo);
	private static HookList HookCanBeChosenAsAmmo = AddHook<Func<Item, Item, Player, bool?>>(g => g.CanBeChosenAsAmmo);

	/// <summary>
	/// Calls each <see cref="GlobalItem.CanChooseAmmo"/> hook for the weapon, and each <see cref="GlobalItem.CanBeChosenAsAmmo"/> hook for the ammo,<br></br>
	/// then each corresponding hook in <see cref="ModItem"/> if applicable for the weapon and/or ammo, until one of them returns a concrete false value.<br></br>
	/// If all of them fail to do this, returns either true (if one returned true prior) or <c>ammo.ammo == weapon.useAmmo</c>.
	/// </summary>
	public static bool CanChooseAmmo(Item weapon, Item ammo, Player player) {
		bool? result = null;
		foreach (var g in HookCanChooseAmmo.Enumerate(weapon.globalItems)) {
			bool? r = g.CanChooseAmmo(weapon, ammo, player);
			if (r is false)
				return false;

			result ??= r;
		}

		foreach (var g in HookCanBeChosenAsAmmo.Enumerate(ammo.globalItems)) {
			bool? r = g.CanBeChosenAsAmmo(ammo, weapon, player);
			if (r is false)
				return false;

			result ??= r;
		}

		if (weapon.ModItem != null) {
			bool? r = weapon.ModItem.CanChooseAmmo(ammo, player);
			if (r is false)
				return false;

			result ??= r;
		}

		if (ammo.ModItem != null) {
			bool? r = ammo.ModItem.CanBeChosenAsAmmo(weapon, player);
			if (r is false)
				return false;

			result ??= r;
		}
		return result ?? ammo.ammo == weapon.useAmmo;
	}

	private static HookList HookCanConsumeAmmo = AddHook<Func<Item, Item, Player, bool>>(g => g.CanConsumeAmmo);
	private static HookList HookCanBeConsumedAsAmmo = AddHook<Func<Item, Item, Player, bool>>(g => g.CanBeConsumedAsAmmo);

	/// <summary>
	/// Calls each <see cref="GlobalItem.CanConsumeAmmo"/> hook for the weapon, and each <see cref="GlobalItem.CanBeConsumedAsAmmo"/> hook for the ammo,<br></br>
	/// then each corresponding hook in <see cref="ModItem"/> if applicable for the weapon and/or ammo, until one of them returns a concrete false value.<br></br>
	/// If all of them fail to do this, returns true.
	/// </summary>
	public static bool CanConsumeAmmo(Item weapon, Item ammo, Player player) {
		foreach (var g in HookCanConsumeAmmo.Enumerate(weapon.globalItems)) {
			if (!g.CanConsumeAmmo(weapon, ammo, player))
				return false;
		}

		foreach (var g in HookCanBeConsumedAsAmmo.Enumerate(ammo.globalItems)) {
			if (!g.CanBeConsumedAsAmmo(ammo, weapon, player))
				return false;
		}
		if (weapon.ModItem != null && !weapon.ModItem.CanConsumeAmmo(ammo, player) ||
			ammo.ModItem != null && !ammo.ModItem.CanBeConsumedAsAmmo(weapon, player))
			return false;
		return true;
	}

	private static HookList HookOnConsumeAmmo = AddHook<Action<Item, Item, Player>>(g => g.OnConsumeAmmo);
	private static HookList HookOnConsumedAsAmmo = AddHook<Action<Item, Item, Player>>(g => g.OnConsumedAsAmmo);

	/// <summary>
	/// Calls <see cref="ModItem.OnConsumeAmmo"/> for the weapon, <see cref="ModItem.OnConsumedAsAmmo"/> for the ammo,
	/// then each corresponding hook for the weapon and ammo.
	/// </summary>
	public static void OnConsumeAmmo(Item weapon, Item ammo, Player player) {
		if (weapon.IsAir)
			return;

		weapon.ModItem?.OnConsumeAmmo(ammo, player);
		ammo.ModItem?.OnConsumedAsAmmo(weapon, player);

		foreach (var g in HookOnConsumeAmmo.Enumerate(weapon.globalItems)) {
			g.OnConsumeAmmo(weapon, ammo, player);
		}

		foreach (var g in HookOnConsumedAsAmmo.Enumerate(ammo.globalItems)) {
			g.OnConsumedAsAmmo(ammo, weapon, player);
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

	private static HookList HookShoot = AddHook<Func<Item, Player, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float, bool>>(g => g.Shoot);

	/// <summary>
	/// Calls each GlobalItem.Shoot hook then, if none of them returns false, calls the ModItem.Shoot hook and returns its value.
	/// </summary>
	public static bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, bool defaultResult = true) {
		foreach (var g in HookShoot.Enumerate(item.globalItems)) {
			defaultResult &= g.Shoot(item, player, source, position, velocity, type, damage, knockback);
		}

		return defaultResult && (item.ModItem?.Shoot(player, source, position, velocity, type, damage, knockback) ?? true);
	}

	private delegate void DelegateUseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox);
	private static HookList HookUseItemHitbox = AddHook<DelegateUseItemHitbox>(g => g.UseItemHitbox);

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

	/// <summary>
	/// Calls ModItem.MeleeEffects and all GlobalItem.MeleeEffects hooks.
	/// </summary>
	public static void MeleeEffects(Item item, Player player, Rectangle hitbox) {
		item.ModItem?.MeleeEffects(player, hitbox);

		foreach (var g in HookMeleeEffects.Enumerate(item.globalItems)) {
			g.MeleeEffects(item, player, hitbox);
		}
	}

	private static HookList HookCanCatchNPC = AddHook<Func<Item, NPC, Player, bool?>>(g => g.CanCatchNPC);

	/// <summary>
	/// Gathers the results of all <see cref="GlobalItem.CanCatchNPC"/> hooks, then the <see cref="ModItem.CanCatchNPC"/> hook if applicable.<br></br>
	/// If any of them returns false, this returns false.<br></br>
	/// Otherwise, if any of them returns true, then this returns true.<br></br>
	/// If all of them return null, this returns null.<br></br>
	/// </summary>
	public static bool? CanCatchNPC(Item item, NPC target, Player player) {
		bool? canCatchOverall = null;
		foreach (GlobalItem g in HookCanCatchNPC.Enumerate(item.globalItems)) {
			bool? canCatchFromGlobalItem = g.CanCatchNPC(item, target, player);
			if (canCatchFromGlobalItem.HasValue) {
				if (!canCatchFromGlobalItem.Value)
					return false;

				canCatchOverall = true;
			}
		}
		if (item.ModItem != null) {
			bool? canCatchAsModItem = item.ModItem.CanCatchNPC(target, player);
			if (canCatchAsModItem.HasValue) {
				if (!canCatchAsModItem.Value)
					return false;

				canCatchOverall = true;
			}
		}
		return canCatchOverall;
	}

	private static HookList HookOnCatchNPC = AddHook<Action<Item, NPC, Player, bool>>(g => g.OnCatchNPC);

	public static void OnCatchNPC(Item item, NPC npc, Player player, bool failed) {
		item.ModItem?.OnCatchNPC(npc, player, failed);

		foreach (GlobalItem g in HookOnCatchNPC.Enumerate(item.globalItems)) {
			g.OnCatchNPC(item, npc, player, failed);
		}
	}


	private delegate void DelegateModifyItemScale(Item item, Player player, ref float scale);
	private static HookList HookModifyItemScale = AddHook<DelegateModifyItemScale>(g => g.ModifyItemScale);

	/// <summary>
	/// Calls <see cref="ModItem.ModifyItemScale"/> if applicable, then all applicable <see cref="GlobalItem.ModifyItemScale"/> instances.
	/// </summary>
	public static void ModifyItemScale(Item item, Player player, ref float scale) {
		item.ModItem?.ModifyItemScale(player, ref scale);

		foreach (var g in HookModifyItemScale.Enumerate(item.globalItems)) {
			g.ModifyItemScale(item, player, ref scale);
		}
	}

	private static HookList HookCanHitNPC = AddHook<Func<Item, Player, NPC, bool?>>(g => g.CanHitNPC);

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
			return null;

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
	/// Hook at the end of Player.VanillaUpdateEquip can be called to apply additional code related to accessory slots for a particular item
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
	/// Hook at the end of Player.ApplyEquipFunctional can be called to apply additional code related to accessory slots for a particular item.
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
	/// Hook at the end of Player.ApplyEquipVanity can be called to apply additional code related to accessory slots for a particular item
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

		foreach (GlobalItem globalItem in HookUpdateArmorSet.Enumerate(globalItems)) {
			string set = globalItem.IsArmorSet(head, body, legs);
			if (!string.IsNullOrEmpty(set))
				globalItem.UpdateArmorSet(player, set);
		}
	}

	private static HookList HookPreUpdateVanitySet = AddHook<Action<Player, string>>(g => g.PreUpdateVanitySet);

	/// <summary>
	/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's PreUpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.PreUpdateVanitySet, using player.head, player.body, and player.legs.
	/// </summary>
	public static void PreUpdateVanitySet(Player player) {
		EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
		EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
		EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

		if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			headTexture.PreUpdateVanitySet(player);

		if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			bodyTexture.PreUpdateVanitySet(player);

		if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			legTexture.PreUpdateVanitySet(player);

		foreach (GlobalItem globalItem in HookPreUpdateVanitySet.Enumerate(globalItems)) {
			string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
			if (!string.IsNullOrEmpty(set))
				globalItem.PreUpdateVanitySet(player, set);
		}
	}

	private static HookList HookUpdateVanitySet = AddHook<Action<Player, string>>(g => g.UpdateVanitySet);

	/// <summary>
	/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's UpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.UpdateVanitySet, using player.head, player.body, and player.legs.
	/// </summary>
	public static void UpdateVanitySet(Player player) {
		EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
		EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
		EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

		if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			headTexture.UpdateVanitySet(player);

		if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			bodyTexture.UpdateVanitySet(player);

		if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			legTexture.UpdateVanitySet(player);

		foreach (GlobalItem globalItem in HookUpdateVanitySet.Enumerate(globalItems)) {
			string set = globalItem.IsVanitySet(player.head, player.body, player.legs);
			if (!string.IsNullOrEmpty(set))
				globalItem.UpdateVanitySet(player, set);
		}
	}

	private static HookList HookArmorSetShadows = AddHook<Action<Player, string>>(g => g.ArmorSetShadows);

	/// <summary>
	/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's ArmorSetShadows. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.ArmorSetShadows, using player.head, player.body, and player.legs.
	/// </summary>
	public static void ArmorSetShadows(Player player) {
		EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
		EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
		EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

		if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			headTexture.ArmorSetShadows(player);

		if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			bodyTexture.ArmorSetShadows(player);

		if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			legTexture.ArmorSetShadows(player);

		foreach (GlobalItem globalItem in HookArmorSetShadows.Enumerate(globalItems)) {
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

		foreach (var g in HookSetMatch.Enumerate(globalItems)) {
			g.SetMatch(armorSlot, type, male, ref equipSlot, ref robes);
		}
	}

	private static HookList HookCanRightClick = AddHook<Func<Item, bool>>(g => g.CanRightClick);

	/// <summary>
	/// Calls ModItem.CanRightClick, then all GlobalItem.CanRightClick hooks, until one of the returns true.
	/// Also returns true if ItemID.Sets.OpenableBag
	/// </summary>
	public static bool CanRightClick(Item item) {
		if (item.IsAir)
			return false;

		if (ItemID.Sets.OpenableBag[item.type])
			return true;

		if (item.ModItem != null && item.ModItem.CanRightClick())
			return true;

		foreach (var g in HookCanRightClick.Enumerate(item.globalItems)) {
			if (g.CanRightClick(item))
				return true;
		}

		return false;
	}

	private static HookList HookRightClick = AddHook<Action<Item, Player>>(g => g.RightClick);

	/// <summary>
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
		RightClickCallHooks(item, player);

		if (ConsumeItem(item, player) && --item.stack == 0)
			item.SetDefaults();

		SoundEngine.PlaySound(7);
		Main.stackSplit = 30;
		Main.mouseRightRelease = false;
		Recipe.FindRecipes();
	}

	internal static void RightClickCallHooks(Item item, Player player) {
		item.ModItem?.RightClick(player);

		foreach (var g in HookRightClick.Enumerate(item.globalItems)) {
			g.RightClick(item, player);
		}
	}


	private static HookList HookModifyItemLoot = AddHook<Action<Item, ItemLoot>>(g => g.ModifyItemLoot);
	
	/// <summary>
	/// Calls each GlobalItem.ModifyItemLoot hooks.
	/// </summary>
	public static void ModifyItemLoot(Item item, ItemLoot itemLoot) {
		item.ModItem?.ModifyItemLoot(itemLoot);

		foreach (var g in HookModifyItemLoot.Enumerate(item.globalItems)) {
			g.ModifyItemLoot(item, itemLoot);
		}
	}

	private static HookList HookCanStack = AddHook<Func<Item, Item, bool>>(g => g.CanStack);

	/// <summary>
	/// Returns false if item prefixes don't match. Then calls all GlobalItem.CanStack hooks until one returns false then ModItem.CanStack. Returns whether any of the hooks returned false.
	/// </summary>
	public static bool CanStack(Item increase, Item decrease) {
		if (increase.prefix != decrease.prefix) // TML: #StackablePrefixWeapons
			return false;

		foreach (var g in HookCanStack.Enumerate(increase.globalItems)) {
			if (!g.CanStack(increase, decrease))
				return false;
		}

		return increase.ModItem?.CanStack(decrease) ?? true;
	}

	private static HookList HookCanStackInWorld = AddHook<Func<Item, Item, bool>>(g => g.CanStackInWorld);
	
	/// <summary>
	/// Calls all GlobalItem.CanStackInWorld hooks until one returns false then ModItem.CanStackInWorld. Returns whether any of the hooks returned false.
	/// </summary>
	public static bool CanStackInWorld(Item increase, Item decrease) {
		foreach (var g in HookCanStackInWorld.Enumerate(increase.globalItems)) {
			if (!g.CanStackInWorld(increase, decrease))
				return false;
		}

		return increase.ModItem?.CanStackInWorld(decrease) ?? true;
	}
	
	private static HookList HookOnStack = AddHook<Action<Item, Item, int>>(g => g.OnStack);

	/// <summary>
	/// Calls CanStack.  Returns false if CanStack is false.  Calls StackItems if CanStack is true<br/>
	/// Stacks item1 and item2.  Calls all GlobalItem.OnStack and ModItem.OnStack hooks if item1.stack < item1.maxStack.<br/>
	/// </summary>
	/// <param name="increase">Item where the stack is being increased.</param>
	/// <param name="decrease">Item where the stack is being reduced.</param>
	/// <param name="numTransfered">Amount to be transfered </param>
	/// <param name="infiniteSource">The final stack of item2</param>
	public static bool TryStackItems(Item increase, Item decrease, out int numTransfered, bool infiniteSource = false) {
		numTransfered = 0;
		if (!CanStack(increase, decrease))
			return false;

		StackItems(increase, decrease, out numTransfered, infiniteSource);

		return true;
	}

	/// <summary>
	/// Stacks item1 and item2.  Calls all GlobalItem.OnStack and ModItem.OnStack hooks if item1.stack < item1.maxStack.
	/// </summary>
	/// <param name="increase">Item where the stack is being increased.</param>
	/// <param name="decrease">Item where the stack is being reduced.</param>
	/// <param name="numTransfered">Amount to be transfered </param>
	/// <param name="infiniteSource"></param>
	/// <param name="numToTransfer">Used to only transfer a specidied amount instead of all.</param>
	public static void StackItems(Item increase, Item decrease, out int numTransfered, bool infiniteSource = false, int? numToTransfer = null) {
		numTransfered = numToTransfer ?? Math.Min(decrease.stack, increase.maxStack - increase.stack);

		foreach (var g in HookOnStack.Enumerate(increase.globalItems)) {
			g.OnStack(increase, decrease, numTransfered);
		}

		increase.ModItem?.OnStack(decrease, numTransfered);

		if (decrease.favorited) {
			increase.favorited = true;
			decrease.favorited = false;
		}

		increase.stack += numTransfered;
		if (!infiniteSource)
			decrease.stack -= numTransfered;
	}

	private static HookList HookSplitStack = AddHook<Action<Item, Item, int>>(g => g.SplitStack);

	public static Item TransferWithLimit(Item decrease, int limit) {
		Item increase = decrease.Clone();
		if (decrease.stack <= limit) {
			decrease.TurnToAir();
		}
		else {
			SplitStack(increase, decrease, limit);
		}
		return increase;
	}

	/// <summary>
	/// Called when splitting a stack of items.
	/// </summary>
	/// <param name="increase">A clone of decrease.  Stack is set to zero then incremented in SplitStack or after SplitStack is called.</param>
	/// <param name="decrease">The original item with stack being reduced.</param>
	/// <param name="numToTransfer">Usually 1, but possible to be higher.</param>
	public static void SplitStack(Item increase, Item decrease, int numToTransfer) {
		increase.stack = 0;
		increase.favorited = false;

		foreach (var g in HookSplitStack.Enumerate(increase.globalItems)) {
			g.SplitStack(increase, decrease, numToTransfer);
		}

		increase.ModItem?.SplitStack(decrease, numToTransfer);

		increase.stack += numToTransfer;
		decrease.stack -= numToTransfer;
	}

	private delegate bool DelegateReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount);
	private static HookList HookReforgePrice = AddHook<DelegateReforgePrice>(g => g.ReforgePrice);

	/// <summary>
	/// Call all ModItem.ReforgePrice, then GlobalItem.ReforgePrice hooks.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="reforgePrice"></param>
	/// <param name="canApplyDiscount"></param>
	/// <returns></returns>
	public static bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
		bool b = item.ModItem?.ReforgePrice(ref reforgePrice, ref canApplyDiscount) ?? true;

		foreach (var g in HookReforgePrice.Enumerate(item.globalItems)) {
			b &= g.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
		}

		return b;
	}

	// TODO: PreReforge marked obsolete until v0.11
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

	private delegate void DelegateDrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor);
	private static HookList HookDrawArmorColor = AddHook<DelegateDrawArmorColor>(g => g.DrawArmorColor);

	/// <summary>
	/// Calls the item's equipment texture's DrawArmorColor hook, then all GlobalItem.DrawArmorColor hooks.
	/// </summary>
	public static void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color,
		ref int glowMask, ref Color glowMaskColor) {
		EquipTexture texture = EquipLoader.GetEquipTexture(type, slot);
		texture?.DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);

		foreach (var g in HookDrawArmorColor.Enumerate(globalItems)) {
			g.DrawArmorColor(type, slot, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
		}
	}

	private delegate void DelegateArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color);
	private static HookList HookArmorArmGlowMask = AddHook<DelegateArmorArmGlowMask>(g => g.ArmorArmGlowMask);

	/// <summary>
	/// Calls the item's body equipment texture's ArmorArmGlowMask hook, then all GlobalItem.ArmorArmGlowMask hooks.
	/// </summary>
	public static void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
		EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, slot);

		texture?.ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);

		foreach (var g in HookArmorArmGlowMask.Enumerate(globalItems)) {
			g.ArmorArmGlowMask(slot, drawPlayer, shadow, ref glowMask, ref color);
		}
	}

	/*
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
	*/

	private delegate void DelegateVerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend);
	private static HookList HookVerticalWingSpeeds = AddHook<DelegateVerticalWingSpeeds>(g => g.VerticalWingSpeeds);
	
	/// <summary>
	/// If the player is using wings, this uses the result of GetWing, and calls ModItem.VerticalWingSpeeds then all GlobalItem.VerticalWingSpeeds hooks.
	/// </summary>
	public static void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
		ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		Item item = player.equippedWings;
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
	
	/// <summary>
	/// If the player is using wings, this uses the result of GetWing, and calls ModItem.HorizontalWingSpeeds then all GlobalItem.HorizontalWingSpeeds hooks.
	/// </summary>
	public static void HorizontalWingSpeeds(Player player) {
		Item item = player.equippedWings;
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

		foreach (var g in HookWingUpdate.Enumerate(globalItems)) {
			retVal |= g.WingUpdate(player.wings, player, inUse);
		}

		return retVal ?? false;
	}

	private delegate void DelegateUpdate(Item item, ref float gravity, ref float maxFallSpeed);
	private static HookList HookUpdate = AddHook<DelegateUpdate>(g => g.Update);
	
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
	
	public static bool CanPickup(Item item, Player player) {
		foreach (var g in HookCanPickup.Enumerate(item.globalItems)) {
			if (!g.CanPickup(item, player))
				return false;
		}

		return item.ModItem?.CanPickup(player) ?? true;
	}

	private static HookList HookOnPickup = AddHook<Func<Item, Player, bool>>(g => g.OnPickup);
	
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
	
	public static bool ItemSpace(Item item, Player player) {
		foreach (var g in HookItemSpace.Enumerate(item.globalItems)) {
			if (g.ItemSpace(item, player))
				return true;
		}

		return item.ModItem?.ItemSpace(player) ?? false;
	}

	private static HookList HookGetAlpha = AddHook<Func<Item, Color, Color?>>(g => g.GetAlpha);
	
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

		foreach (var g in HookHoldoutOffset.Enumerate(globalItems)) {
			Vector2? modOffset = g.HoldoutOffset(type);

			if (modOffset.HasValue) {
				offset.X = modOffset.Value.X;
				offset.Y = TextureAssets.Item[type].Value.Height / 2f + gravDir * modOffset.Value.Y;
			}
		}
	}

	private static HookList HookHoldoutOrigin = AddHook<Func<int, Vector2?>>(g => g.HoldoutOrigin);

	public static void HoldoutOrigin(Player player, ref Vector2 origin) {
		Item item = player.HeldItem;
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

	private static HookList HookCanEquipAccessory = AddHook<Func<Item, Player, int, bool, bool>>(g => g.CanEquipAccessory);
	
	public static bool CanEquipAccessory(Item item, int slot, bool modded) {
		Player player = Main.player[Main.myPlayer];
		if (item.ModItem != null && !item.ModItem.CanEquipAccessory(player, slot, modded))
			return false;

		foreach (var g in HookCanEquipAccessory.Enumerate(item.globalItems)) {
			if (!g.CanEquipAccessory(item, player, slot, modded))
				return false;
		}

		return true;
	}

	private static HookList HookCanAccessoryBeEquippedWith = AddHook<Func<Item, Item, Player, bool>>(g => g.CanAccessoryBeEquippedWith);

	public static bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem) {
		Player player = Main.player[Main.myPlayer];
		return CanAccessoryBeEquippedWith(equippedItem, incomingItem, player) && CanAccessoryBeEquippedWith(incomingItem, equippedItem, player);
	}

	private static bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
		if (equippedItem.ModItem != null && !equippedItem.ModItem.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player))
			return false;

		if (incomingItem.ModItem != null && !incomingItem.ModItem.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player))
			return false;

		foreach (var g in HookCanAccessoryBeEquippedWith.Enumerate(incomingItem.globalItems)) {
			if (!g.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player))
				return false;
		}

		return true;
	}

	private delegate void DelegateExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack);
	private static HookList HookExtractinatorUse = AddHook<DelegateExtractinatorUse>(g => g.ExtractinatorUse);

	public static void ExtractinatorUse(ref int resultType, ref int resultStack, int extractType, int extractinatorBlockType) {
		GetItem(extractType)?.ExtractinatorUse(extractinatorBlockType, ref resultType, ref resultStack);

		foreach (var g in HookExtractinatorUse.Enumerate(globalItems)) {
			g.ExtractinatorUse(extractType, extractinatorBlockType, ref resultType, ref resultStack);
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

		foreach (var g in HookIsAnglerQuestAvailable.Enumerate(globalItems)) {
			notAvailable |= !g.IsAnglerQuestAvailable(itemID);
		}
	}

	private delegate void DelegateAnglerChat(int type, ref string chat, ref string catchLocation);
	private static HookList HookAnglerChat = AddHook<DelegateAnglerChat>(g => g.AnglerChat);

	public static string AnglerChat(int type) {
		string chat = "";
		string catchLocation = "";
		GetItem(type)?.AnglerQuestChat(ref chat, ref catchLocation);

		foreach (var g in HookAnglerChat.Enumerate(globalItems)) {
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

	public static List<TooltipLine> ModifyTooltips(Item item, ref int numTooltips, string[] names, ref string[] text, ref bool[] modifier, ref bool[] badModifier, ref int oneDropLogo, out Color?[] overrideColor) {
		var tooltips = new List<TooltipLine>();

		for (int k = 0; k < numTooltips; k++) {
			TooltipLine tooltip = new TooltipLine(names[k], text[k]);
			tooltip.IsModifier = modifier[k];
			tooltip.IsModifierBad = badModifier[k];

			if (k == oneDropLogo) {
				tooltip.OneDropLogo = true;
			}

			tooltips.Add(tooltip);
		}

		item.ModItem?.ModifyTooltips(tooltips);

		if (!item.IsAir) { // Prevents dummy items used in Main.HoverItem from getting unrelated tooltips
			foreach (var g in HookModifyTooltips.Enumerate(item.globalItems)) {
				g.ModifyTooltips(item, tooltips);
			}
		}

		numTooltips = tooltips.Count;
		text = new string[numTooltips];
		modifier = new bool[numTooltips];
		badModifier = new bool[numTooltips];
		oneDropLogo = -1;
		overrideColor = new Color?[numTooltips];

		for (int k = 0; k < numTooltips; k++) {
			text[k] = tooltips[k].Text;
			modifier[k] = tooltips[k].IsModifier;
			badModifier[k] = tooltips[k].IsModifierBad;

			if (tooltips[k].OneDropLogo) {
				oneDropLogo = k;
			}

			overrideColor[k] = tooltips[k].OverrideColor;
		}

		return tooltips;
	}

	internal static bool NeedsModSaving(Item item) {
		if (item.type <= ItemID.None)
			return false;

		if (item.ModItem != null || item.prefix >= PrefixID.Count)
			return true;

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
}
