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
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Utilities;
using HookList = Terraria.ModLoader.Core.GlobalHookList<Terraria.ModLoader.GlobalItem>;

namespace Terraria.ModLoader;

/// <summary>
/// This serves as the central class from which item-related functions are carried out. It also stores a list of mod items by ID.
/// </summary>
public static class ItemLoader
{
	public static int ItemCount { get; private set; } = ItemID.Count;
	public static int UseStyleCount { get; private set; } = ItemUseStyleID.Count;
	private static readonly IList<ModItem> items = new List<ModItem>();

	private static readonly List<HookList> hooks = new List<HookList>();
	private static readonly List<HookList> modHooks = new List<HookList>();

	internal static readonly int vanillaQuestFishCount = 41;

	private static HookList AddHook<F>(Expression<Func<GlobalItem, F>> func) where F : Delegate
	{
		var hook = HookList.Create(func);
		hooks.Add(hook);
		return hook;
	}

	public static T AddModHook<T>(T hook) where T : HookList
	{
		modHooks.Add(hook);
		return hook;
	}

	internal static int Register(ModItem item)
	{
		items.Add(item);
		return ItemCount++;
	}

	/// <summary>
	/// Registers a new item use style (<see cref="ItemUseStyleID"/>). The return value is its unique ID suitable for <see cref="Item.useStyle"/>.
	/// </summary>
	public static int RegisterUseStyle(Mod mod, string useStyleName)
	{
		int useStyle = UseStyleCount++;
		ItemUseStyleID.Search.Add($"{mod?.Name ?? "Terraria"}/{useStyleName}", useStyle);
		return useStyle;
	}

	/// <summary>
	/// Gets the ModItem template instance corresponding to the specified type (not the clone/new instance which gets added to Items as the game is played). Returns null if no modded item has the given type.
	/// </summary>
	public static ModItem GetItem(int type)
	{
		return type >= ItemID.Count && type < ItemCount ? items[type - ItemID.Count] : null;
	}

	internal static void ResizeArrays(bool unloading)
	{
		if (!unloading)
			GlobalList<GlobalItem>.FinishLoading(ItemCount);

		//Textures
		Array.Resize(ref TextureAssets.Item, ItemCount);
		Array.Resize(ref TextureAssets.ItemFlame, ItemCount);

		//Sets
		LoaderUtils.ResetStaticMembers(typeof(ItemID));
		LoaderUtils.ResetStaticMembers(typeof(AmmoID));
		LoaderUtils.ResetStaticMembers(typeof(PrefixLegacy.ItemSets));
		if (unloading)
			LoaderUtils.ResetStaticMembers(typeof(ItemUseStyleID));

		//Etc
		Array.Resize(ref Item.cachedItemSpawnsByType, ItemCount);
		Array.Resize(ref Item.staff, ItemCount);
		Array.Resize(ref Item.claw, ItemCount);
		Array.Resize(ref Lang._itemNameCache, ItemCount);
		Array.Resize(ref Lang._itemTooltipCache, ItemCount);

		Array.Resize(ref RecipeLoader.FirstRecipeForItem, ItemCount);

		for (int k = ItemID.Count; k < ItemCount; k++) {
			Lang._itemNameCache[k] = LocalizedText.Empty;
			Lang._itemTooltipCache[k] = ItemTooltip.None;
			Item.cachedItemSpawnsByType[k] = -1;
		}

		//Animation collections can be accessed during an ongoing (un)loading process.
		//Which is why the following 2 lines have to run without any interruptions.
		lock (Main.itemAnimationsRegistered) {
			Array.Resize(ref Main.itemAnimations, ItemCount);
			Main.InitializeItemAnimations();
		}

		if (unloading)
			Array.Resize(ref Main.anglerQuestItemNetIDs, vanillaQuestFishCount);
		else
			Main.anglerQuestItemNetIDs = Main.anglerQuestItemNetIDs
				.Concat(items.Where(modItem => modItem.IsQuestFish()).Select(modItem => modItem.Type))
				.ToArray();
	}

	internal static void FinishSetup()
	{
		GlobalLoaderUtils<GlobalItem, Item>.BuildTypeLookups(new Item().SetDefaults);
		UpdateHookLists();
		GlobalTypeLookups<GlobalItem>.LogStats();

		foreach (ModItem item in items) {
			Lang._itemNameCache[item.Type] = item.DisplayName;
			Lang._itemTooltipCache[item.Type] = ItemTooltip.FromLocalization(item.Tooltip);
			ContentSamples.ItemsByType[item.Type].RebuildTooltip();
		}

		ValidateDropsSet();
	}

	private static void UpdateHookLists()
	{
		foreach (var hook in hooks.Union(modHooks)) {
			hook.Update();
		}
	}

	internal static void ValidateDropsSet()
	{
		foreach (var pair in ItemID.Sets.GeodeDrops) {
			string exceptionCommon = $"{Lang.GetItemNameValue(pair.Key)} registered in 'ItemID.Sets.{nameof(ItemID.Sets.GeodeDrops)}'";
			if (pair.Value.minStack < 1)
				throw new Exception($"{exceptionCommon} must have minStack bigger than 0");

			if (pair.Value.maxStack <= pair.Value.minStack)
				throw new Exception($"{exceptionCommon} must have maxStack bigger than minStack");
		}

		foreach (var pair in ItemID.Sets.OreDropsFromSlime) {
			string exceptionCommon = $"{Lang.GetItemNameValue(pair.Key)} registered in 'ItemID.Sets.{nameof(ItemID.Sets.OreDropsFromSlime)}'";
			if (pair.Value.minStack < 1)
				throw new Exception($"{exceptionCommon} must have minStack bigger than 0");

			if (pair.Value.maxStack < pair.Value.minStack)
				throw new Exception($"{exceptionCommon} must have maxStack bigger than or equal to minStack");
		}
	}

	internal static void Unload()
	{
		ItemCount = ItemID.Count;
		UseStyleCount = ItemUseStyleID.Count;
		items.Clear();
		FlexibleTileWand.Reload();
		GlobalList<GlobalItem>.Reset();
		modHooks.Clear();
		UpdateHookLists();
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

	internal static void SetDefaults(Item item, bool createModItem = true)
	{
		if (IsModItem(item.type) && createModItem)
			item.ModItem = GetItem(item.type).NewInstance(item);

		GlobalLoaderUtils<GlobalItem, Item>.SetDefaults(item, ref item._globals, static i => {
			i.ModItem?.AutoDefaults();
			i.ModItem?.SetDefaults();
		});
	}

	private static HookList HookOnSpawn = AddHook<Action<Item, IEntitySource>>(g => g.OnSpawn);

	internal static void OnSpawn(Item item, IEntitySource source)
	{
		item.ModItem?.OnSpawn(source);

		foreach (GlobalItem g in HookOnSpawn.Enumerate(item)) {
			g.OnSpawn(item, source);
		}
	}

	private static HookList HookOnCreate = AddHook<Action<Item, ItemCreationContext>>(g => g.OnCreated);

	public static void OnCreated(Item item, ItemCreationContext context)
	{
		foreach (var g in HookOnCreate.Enumerate(item)) {
			g.OnCreated(item, context);
		}

		item.ModItem?.OnCreated(context);
	}

	private static HookList HookChoosePrefix = AddHook<Func<Item, UnifiedRandom, int>>(g => g.ChoosePrefix);

	public static int ChoosePrefix(Item item, UnifiedRandom rand)
	{
		foreach (var g in HookChoosePrefix.Enumerate(item)) {
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
	/// Null gives vanilla behavior
	/// </summary>
	public static bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
	{
		bool? result = null;
		foreach (var g in HookPrefixChance.Enumerate(item)) {
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

	public static bool AllowPrefix(Item item, int pre)
	{
		bool result = true;
		foreach (var g in HookAllowPrefix.Enumerate(item)) {
			result &= g.AllowPrefix(item, pre);
		}
		if (item.ModItem != null) {
			result &= item.ModItem.AllowPrefix(pre);
		}
		return result;
	}

	private static HookList HookCanUseItem = AddHook<Func<Item, Player, bool>>(g => g.CanUseItem);

	public static bool CanUseItem(Item item, Player player)
	{
		if (item.ModItem != null && !item.ModItem.CanUseItem(player))
			return false;

		foreach (var g in HookCanUseItem.Enumerate(item)) {
			if (!g.CanUseItem(item, player))
				return false;
		}

		return true;
	}

	private static HookList HookCanAutoReuseItem = AddHook<Func<Item, Player, bool?>>(g => g.CanAutoReuseItem);

	public static bool? CanAutoReuseItem(Item item, Player player)
	{
		bool? flag = null;

		foreach (var g in HookCanAutoReuseItem.Enumerate(item)) {
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
	public static void UseStyle(Item item, Player player, Rectangle heldItemFrame)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UseStyle(player, heldItemFrame);

		foreach (var g in HookUseStyle.Enumerate(item)) {
			g.UseStyle(item, player, heldItemFrame);
		}
	}

	private static HookList HookHoldStyle = AddHook<Action<Item, Player, Rectangle>>(g => g.HoldStyle);

	/// <summary>
	/// If the player is not holding onto a rope and is not in the middle of using an item, calls ModItem.HoldStyle and all GlobalItem.HoldStyle hooks.
	/// <br/> Returns whether or not the vanilla logic should be skipped.
	/// </summary>
	public static void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
	{
		if (item.IsAir || player.pulley || player.ItemAnimationActive)
			return;

		item.ModItem?.HoldStyle(player, heldItemFrame);

		foreach (var g in HookHoldStyle.Enumerate(item)) {
			g.HoldStyle(item, player, heldItemFrame);
		}
	}

	private static HookList HookHoldItem = AddHook<Action<Item, Player>>(g => g.HoldItem);

	/// <summary>
	/// Calls ModItem.HoldItem and all GlobalItem.HoldItem hooks.
	/// </summary>
	public static void HoldItem(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.HoldItem(player);

		foreach (var g in HookHoldItem.Enumerate(item)) {
			g.HoldItem(item, player);
		}
	}

	private static HookList HookUseTimeMultiplier = AddHook<Func<Item, Player, float>>(g => g.UseTimeMultiplier);

	public static float UseTimeMultiplier(Item item, Player player)
	{
		if (item.IsAir)
			return 1f;

		float multiplier = item.ModItem?.UseTimeMultiplier(player) ?? 1f;

		foreach (var g in HookUseTimeMultiplier.Enumerate(item)) {
			multiplier *= g.UseTimeMultiplier(item, player);
		}

		return multiplier;
	}

	private static HookList HookUseAnimationMultiplier = AddHook<Func<Item, Player, float>>(g => g.UseAnimationMultiplier);

	public static float UseAnimationMultiplier(Item item, Player player)
	{
		if (item.IsAir)
			return 1f;

		float multiplier = item.ModItem?.UseAnimationMultiplier(player) ?? 1f;

		foreach (var g in HookUseAnimationMultiplier.Enumerate(item)) {
			multiplier *= g.UseAnimationMultiplier(item, player);
		}

		return multiplier;
	}

	private static HookList HookUseSpeedMultiplier = AddHook<Func<Item, Player, float>>(g => g.UseSpeedMultiplier);

	public static float UseSpeedMultiplier(Item item, Player player)
	{
		if (item.IsAir)
			return 1f;

		float multiplier = item.ModItem?.UseSpeedMultiplier(player) ?? 1f;

		foreach (var g in HookUseSpeedMultiplier.Enumerate(item)) {
			multiplier *= g.UseSpeedMultiplier(item, player);
		}

		return multiplier;
	}

	private delegate void DelegateGetHealLife(Item item, Player player, bool quickHeal, ref int healValue);
	private static HookList HookGetHealLife = AddHook<DelegateGetHealLife>(g => g.GetHealLife);

	/// <summary>
	/// Calls ModItem.GetHealLife, then all GlobalItem.GetHealLife hooks.
	/// </summary>
	public static void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
	{
		if (item.IsAir)
			return;

		item.ModItem?.GetHealLife(player, quickHeal, ref healValue);

		foreach (var g in HookGetHealLife.Enumerate(item)) {
			g.GetHealLife(item, player, quickHeal, ref healValue);
		}
	}

	private delegate void DelegateGetHealMana(Item item, Player player, bool quickHeal, ref int healValue);
	private static HookList HookGetHealMana = AddHook<DelegateGetHealMana>(g => g.GetHealMana);

	/// <summary>
	/// Calls ModItem.GetHealMana, then all GlobalItem.GetHealMana hooks.
	/// </summary>
	public static void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
	{
		if (item.IsAir)
			return;

		item.ModItem?.GetHealMana(player, quickHeal, ref healValue);

		foreach (var g in HookGetHealMana.Enumerate(item)) {
			g.GetHealMana(item, player, quickHeal, ref healValue);
		}
	}

	private delegate void DelegateModifyManaCost(Item item, Player player, ref float reduce, ref float mult);
	private static HookList HookModifyManaCost = AddHook<DelegateModifyManaCost>(g => g.ModifyManaCost);

	/// <summary>
	/// Calls ModItem.ModifyManaCost, then all GlobalItem.ModifyManaCost hooks.
	/// </summary>
	public static void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
	{
		if (item.IsAir)
			return;

		item.ModItem?.ModifyManaCost(player, ref reduce, ref mult);

		foreach (var g in HookModifyManaCost.Enumerate(item)) {
			g.ModifyManaCost(item, player, ref reduce, ref mult);
		}
	}

	private static HookList HookOnMissingMana = AddHook<Action<Item, Player, int>>(g => g.OnMissingMana);

	/// <summary>
	/// Calls ModItem.OnMissingMana, then all GlobalItem.OnMissingMana hooks.
	/// </summary>
	public static void OnMissingMana(Item item, Player player, int neededMana)
	{
		if (item.IsAir)
			return;

		item.ModItem?.OnMissingMana(player, neededMana);

		foreach (var g in HookOnMissingMana.Enumerate(item)) {
			g.OnMissingMana(item, player, neededMana);
		}
	}

	private static HookList HookOnConsumeMana = AddHook<Action<Item, Player, int>>(g => g.OnConsumeMana);

	/// <summary>
	/// Calls ModItem.OnConsumeMana, then all GlobalItem.OnConsumeMana hooks.
	/// </summary>
	public static void OnConsumeMana(Item item, Player player, int manaConsumed)
	{
		if (item.IsAir)
			return;

		item.ModItem?.OnConsumeMana(player, manaConsumed);

		foreach (var g in HookOnConsumeMana.Enumerate(item)) {
			g.OnConsumeMana(item, player, manaConsumed);
		}
	}

	private delegate void DelegateModifyPotionDelay(Item item, Player player, ref int baseDelay);
	private static HookList HookModifyPotionDelay = AddHook<DelegateModifyPotionDelay>(g => g.ModifyPotionDelay);

	public static void ModifyPotionDelay(Item item, Player player, ref int baseDelay)
	{
		item.ModItem?.ModifyPotionDelay(player, ref baseDelay);

		foreach (var g in HookModifyPotionDelay.Enumerate(item)) {
			g.ModifyPotionDelay(item, player, ref baseDelay);
		}
	}

	private delegate bool DelegateApplyPotionDelay(Item item, Player player, int potionDelay);
	private static HookList HookApplyPotionDelay = AddHook<DelegateApplyPotionDelay>(g => g.ApplyPotionDelay);

	public static bool ApplyPotionDelay(Item item, Player player, int potionDelay)
	{
		foreach (var g in HookApplyPotionDelay.Enumerate(item)) {
			if (!g.ApplyPotionDelay(item, player, potionDelay))
				return false;
		}

		return item.ModItem?.ApplyPotionDelay(player, potionDelay) ?? true;
	}

	private delegate bool? DelegateCanConsumeBait(Player baiter, Item bait);
	private static HookList HookCanConsumeBait = AddHook<DelegateCanConsumeBait>(g => g.CanConsumeBait);

	public static bool? CanConsumeBait(Player player, Item bait)
	{
		bool? ret = bait.ModItem?.CanConsumeBait(player);

		foreach (var g in HookCanConsumeBait.Enumerate(bait)) {
			if (g.CanConsumeBait(player, bait) is bool b)
				ret = (ret ?? true) && b;
		}

		return ret;
	}

	private delegate void DelegateModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup);
	private static HookList HookModifyResearchSorting = AddHook<DelegateModifyResearchSorting>(g => g.ModifyResearchSorting);

	public static void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
	{
		if (item.IsAir)
			return;

		item.ModItem?.ModifyResearchSorting(ref itemGroup);

		foreach (var g in HookModifyResearchSorting.Enumerate(item)) {
			g.ModifyResearchSorting(item, ref itemGroup);
		}
	}

	private delegate bool DelegateCanResearch(Item item);
	private static HookList HookCanResearch = AddHook<DelegateCanResearch>(g => g.CanResearch);

	/// <summary>
	/// Hook that determines if an item will be prevented from being consumed by the research function.
	/// </summary>
	/// <param name="item">The item to be consumed or not</param>
	public static bool CanResearch(Item item)
	{
		if (item.ModItem != null && !item.ModItem.CanResearch())
			return false;
		foreach (var g in HookCanResearch.Enumerate(item)) {
			if (!g.Instance(item).CanResearch(item))
				return false;
		}
		return true;
	}

	private delegate void DelegateOnResearched(Item item, bool fullyResearched);
	private static HookList HookOnResearched = AddHook<DelegateOnResearched>(g => g.OnResearched);

	public static void OnResearched(Item item, bool fullyResearched)
	{
		if (item.IsAir)
			return;

		item.ModItem?.OnResearched(fullyResearched);

		foreach (var g in HookOnResearched.Enumerate(item))
			g.Instance(item).OnResearched(item, fullyResearched);
	}

	private delegate void DelegateModifyWeaponDamage(Item item, Player player, ref StatModifier damage);
	private static HookList HookModifyWeaponDamage = AddHook<DelegateModifyWeaponDamage>(g => g.ModifyWeaponDamage);

	/// <summary>
	/// Calls ModItem.HookModifyWeaponDamage, then all GlobalItem.HookModifyWeaponDamage hooks.
	/// </summary>
	public static void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
	{
		if (item.IsAir)
			return;

		item.ModItem?.ModifyWeaponDamage(player, ref damage);

		foreach (var g in HookModifyWeaponDamage.Enumerate(item)) {
			g.ModifyWeaponDamage(item, player, ref damage);
		}
	}

	private delegate void DelegateModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback);
	private static HookList HookModifyWeaponKnockback = AddHook<DelegateModifyWeaponKnockback>(g => g.ModifyWeaponKnockback);

	/// <summary>
	/// Calls ModItem.ModifyWeaponKnockback, then all GlobalItem.ModifyWeaponKnockback hooks.
	/// </summary>
	public static void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
	{
		if (item.IsAir)
			return;

		item.ModItem?.ModifyWeaponKnockback(player, ref knockback);

		foreach (var g in HookModifyWeaponKnockback.Enumerate(item)) {
			g.ModifyWeaponKnockback(item, player, ref knockback);
		}
	}


	private delegate void DelegateModifyWeaponCrit(Item item, Player player, ref float crit);
	private static HookList HookModifyWeaponCrit = AddHook<DelegateModifyWeaponCrit>(g => g.ModifyWeaponCrit);

	/// <summary>
	/// Calls ModItem.ModifyWeaponCrit, then all GlobalItem.ModifyWeaponCrit hooks.
	/// </summary>
	public static void ModifyWeaponCrit(Item item, Player player, ref float crit)
	{
		if (item.IsAir)
			return;

		item.ModItem?.ModifyWeaponCrit(player, ref crit);

		foreach (var g in HookModifyWeaponCrit.Enumerate(item)) {
			g.ModifyWeaponCrit(item, player, ref crit);
		}
	}

	private static HookList HookNeedsAmmo = AddHook<Func<Item, Player, bool>>(g => g.NeedsAmmo);
	/// <summary>
	/// Calls ModItem.NeedsAmmo, then all GlobalItem.NeedsAmmo hooks, until any of them returns false.
	/// </summary>
	public static bool NeedsAmmo(Item weapon, Player player)
	{
		if (!weapon.ModItem?.NeedsAmmo(player) ?? false)
			return false;

		foreach (var g in HookNeedsAmmo.Enumerate(weapon)) {
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
	public static void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
	{
		ammo.ModItem?.PickAmmo(weapon, player, ref type, ref speed, ref damage, ref knockback);

		foreach (var g in HookPickAmmo.Enumerate(ammo)) {
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
	public static bool CanChooseAmmo(Item weapon, Item ammo, Player player)
	{
		bool? result = null;
		foreach (var g in HookCanChooseAmmo.Enumerate(weapon)) {
			bool? r = g.CanChooseAmmo(weapon, ammo, player);
			if (r is false)
				return false;

			result ??= r;
		}

		foreach (var g in HookCanBeChosenAsAmmo.Enumerate(ammo)) {
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
	public static bool CanConsumeAmmo(Item weapon, Item ammo, Player player)
	{
		foreach (var g in HookCanConsumeAmmo.Enumerate(weapon)) {
			if (!g.CanConsumeAmmo(weapon, ammo, player))
				return false;
		}

		foreach (var g in HookCanBeConsumedAsAmmo.Enumerate(ammo)) {
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
	public static void OnConsumeAmmo(Item weapon, Item ammo, Player player)
	{
		if (weapon.IsAir)
			return;

		weapon.ModItem?.OnConsumeAmmo(ammo, player);
		ammo.ModItem?.OnConsumedAsAmmo(weapon, player);

		foreach (var g in HookOnConsumeAmmo.Enumerate(weapon)) {
			g.OnConsumeAmmo(weapon, ammo, player);
		}

		foreach (var g in HookOnConsumedAsAmmo.Enumerate(ammo)) {
			g.OnConsumedAsAmmo(ammo, weapon, player);
		}
	}

	private static HookList HookCanShoot = AddHook<Func<Item, Player, bool>>(g => g.CanShoot);

	/// <summary>
	/// Calls each GlobalItem.CanShoot hook, then ModItem.CanShoot, until one of them returns false. If all of them return true, returns true.
	/// </summary>
	public static bool CanShoot(Item item, Player player)
	{
		foreach (var g in HookCanShoot.Enumerate(item)) {
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
	public static void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		item.ModItem?.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);

		foreach (var g in HookModifyShootStats.Enumerate(item)) {
			g.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
		}
	}

	private static HookList HookShoot = AddHook<Func<Item, Player, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float, bool>>(g => g.Shoot);

	/// <summary>
	/// Calls each GlobalItem.Shoot hook then, if none of them returns false, calls the ModItem.Shoot hook and returns its value.
	/// </summary>
	public static bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, bool defaultResult = true)
	{
		foreach (var g in HookShoot.Enumerate(item)) {
			defaultResult &= g.Shoot(item, player, source, position, velocity, type, damage, knockback);
		}

		return defaultResult && (item.ModItem?.Shoot(player, source, position, velocity, type, damage, knockback) ?? true);
	}

	private delegate void DelegateUseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox);
	private static HookList HookUseItemHitbox = AddHook<DelegateUseItemHitbox>(g => g.UseItemHitbox);

	/// <summary>
	/// Calls ModItem.UseItemHitbox, then all GlobalItem.UseItemHitbox hooks.
	/// </summary>
	public static void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
	{
		item.ModItem?.UseItemHitbox(player, ref hitbox, ref noHitbox);

		foreach (var g in HookUseItemHitbox.Enumerate(item)) {
			g.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
		}
	}

	private static HookList HookMeleeEffects = AddHook<Action<Item, Player, Rectangle>>(g => g.MeleeEffects);

	/// <summary>
	/// Calls ModItem.MeleeEffects and all GlobalItem.MeleeEffects hooks.
	/// </summary>
	public static void MeleeEffects(Item item, Player player, Rectangle hitbox)
	{
		item.ModItem?.MeleeEffects(player, hitbox);

		foreach (var g in HookMeleeEffects.Enumerate(item)) {
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
	public static bool? CanCatchNPC(Item item, NPC target, Player player)
	{
		bool? canCatchOverall = null;
		foreach (var g in HookCanCatchNPC.Enumerate(item)) {
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

	public static void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
	{
		item.ModItem?.OnCatchNPC(npc, player, failed);

		foreach (var g in HookOnCatchNPC.Enumerate(item)) {
			g.OnCatchNPC(item, npc, player, failed);
		}
	}


	private delegate void DelegateModifyItemScale(Item item, Player player, ref float scale);
	private static HookList HookModifyItemScale = AddHook<DelegateModifyItemScale>(g => g.ModifyItemScale);

	/// <summary>
	/// Calls <see cref="ModItem.ModifyItemScale"/> if applicable, then all applicable <see cref="GlobalItem.ModifyItemScale"/> instances.
	/// </summary>
	public static void ModifyItemScale(Item item, Player player, ref float scale)
	{
		item.ModItem?.ModifyItemScale(player, ref scale);

		foreach (var g in HookModifyItemScale.Enumerate(item)) {
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
	public static bool? CanHitNPC(Item item, Player player, NPC target)
	{
		bool? flag = null;

		foreach (var g in HookCanHitNPC.Enumerate(item)) {
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

	private static HookList HookCanCollideNPC = AddHook<Func<Item, Rectangle, Player, NPC, bool?>>(g => g.CanMeleeAttackCollideWithNPC);

	public static bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
	{
		bool? flag = null;

		foreach (var g in HookCanCollideNPC.Enumerate(item)) {
			bool? canCollide = g.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, player, target);

			if (canCollide.HasValue) {
				if (!canCollide.Value) {
					return false;
				}

				flag = true;
			}
		}

		if (item.ModItem != null) {
			bool? canHit = item.ModItem.CanMeleeAttackCollideWithNPC(meleeAttackHitbox, player, target);

			if (canHit.HasValue) {
				if (!canHit.Value) {
					return false;
				}

				flag = true;
			}
		}

		return flag;
	}

	private delegate void DelegateModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(g => g.ModifyHitNPC);

	/// <summary>
	/// Calls ModItem.ModifyHitNPC, then all GlobalItem.ModifyHitNPC hooks.
	/// </summary>
	public static void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		item.ModItem?.ModifyHitNPC(player, target, ref modifiers);

		foreach (var g in HookModifyHitNPC.Enumerate(item)) {
			g.ModifyHitNPC(item, player, target, ref modifiers);
		}
	}

	private static HookList HookOnHitNPC = AddHook<Action<Item, Player, NPC, NPC.HitInfo, int>>(g => g.OnHitNPC);

	/// <summary>
	/// Calls ModItem.OnHitNPC and all GlobalItem.OnHitNPC hooks.
	/// </summary>
	public static void OnHitNPC(Item item, Player player, NPC target, in NPC.HitInfo hit, int damageDone)
	{
		item.ModItem?.OnHitNPC(player, target, hit, damageDone);

		foreach (var g in HookOnHitNPC.Enumerate(item)) {
			g.OnHitNPC(item, player, target, hit, damageDone);
		}
	}

	private static HookList HookCanHitPvp = AddHook<Func<Item, Player, Player, bool>>(g => g.CanHitPvp);

	/// <summary>
	/// Calls all GlobalItem.CanHitPvp hooks, then ModItem.CanHitPvp, until one of them returns false.
	/// If all of them return true, this returns true.
	/// </summary>
	public static bool CanHitPvp(Item item, Player player, Player target)
	{
		foreach (var g in HookCanHitPvp.Enumerate(item)) {
			if (!g.CanHitPvp(item, player, target))
				return false;
		}

		return item.ModItem == null || item.ModItem.CanHitPvp(player, target);
	}

	private delegate void DelegateModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers);
	private static HookList HookModifyHitPvp = AddHook<DelegateModifyHitPvp>(g => g.ModifyHitPvp);

	/// <summary>
	/// Calls ModItem.ModifyHitPvp, then all GlobalItem.ModifyHitPvp hooks.
	/// </summary>
	public static void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
	{
		item.ModItem?.ModifyHitPvp(player, target, ref modifiers);

		foreach (var g in HookModifyHitPvp.Enumerate(item)) {
			g.ModifyHitPvp(item, player, target, ref modifiers);
		}
	}

	private static HookList HookOnHitPvp = AddHook<Action<Item, Player, Player, Player.HurtInfo>>(g => g.OnHitPvp);

	/// <summary>
	/// Calls ModItem.OnHitPvp and all GlobalItem.OnHitPvp hooks. <br/>
	/// Called on local, server and remote clients. <br/>
	/// </summary>
	public static void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
	{
		item.ModItem?.OnHitPvp(player, target, hurtInfo);

		foreach (var g in HookOnHitPvp.Enumerate(item)) {
			g.OnHitPvp(item, player, target, hurtInfo);
		}
	}

	private static HookList HookUseItem = AddHook<Func<Item, Player, bool?>>(g => g.UseItem);

	/// <summary>
	/// Returns false if any of ModItem.UseItem or GlobalItem.UseItem return false.
	/// Returns true if anything returns true without returning false.
	/// Returns null by default.
	/// Does not fail fast (calls every hook)
	/// </summary>
	public static bool? UseItem(Item item, Player player)
	{
		if (item.IsAir)
			return null;

		bool? result = null;

		foreach (var g in HookUseItem.Enumerate(item)) {
			bool? useItem = g.UseItem(item, player);

			if (useItem.HasValue && result != false) {
				result = useItem.Value;
			}
		}

		bool? modItemResult = item.ModItem?.UseItem(player);

		return result ?? modItemResult;
	}

	private static HookList HookUseAnimation = AddHook<Action<Item, Player>>(g => g.UseAnimation);

	public static void UseAnimation(Item item, Player player)
	{
		foreach (var g in HookUseAnimation.Enumerate(item)) {
			g.Instance(item).UseAnimation(item, player);
		}

		item.ModItem?.UseAnimation(player);
	}

	private static HookList HookConsumeItem = AddHook<Func<Item, Player, bool>>(g => g.ConsumeItem);

	/// <summary>
	/// If ModItem.ConsumeItem or any of the GlobalItem.ConsumeItem hooks returns false, sets consume to false.
	/// </summary>
	public static bool ConsumeItem(Item item, Player player)
	{
		if (item.IsAir) return true;
		if (item.ModItem != null && !item.ModItem.ConsumeItem(player))
			return false;

		foreach (var g in HookConsumeItem.Enumerate(item)) {
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
	public static void OnConsumeItem(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.OnConsumeItem(player);

		foreach (var g in HookOnConsumeItem.Enumerate(item)) {
			g.OnConsumeItem(item, player);
		}
	}

	private static HookList HookUseItemFrame = AddHook<Action<Item, Player>>(g => g.UseItemFrame);

	/// <summary>
	/// Calls ModItem.UseItemFrame, then all GlobalItem.UseItemFrame hooks, until one of them returns true. Returns whether any of the hooks returned true.
	/// </summary>
	public static void UseItemFrame(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UseItemFrame(player);

		foreach (var g in HookUseItemFrame.Enumerate(item)) {
			g.UseItemFrame(item, player);
		}
	}

	private static HookList HookHoldItemFrame = AddHook<Action<Item, Player>>(g => g.HoldItemFrame);

	/// <summary>
	/// Calls ModItem.HoldItemFrame, then all GlobalItem.HoldItemFrame hooks, until one of them returns true. Returns whether any of the hooks returned true.
	/// </summary>
	public static void HoldItemFrame(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.HoldItemFrame(player);

		foreach (var g in HookHoldItemFrame.Enumerate(item)) {
			g.HoldItemFrame(item, player);
		}
	}

	private static HookList HookAltFunctionUse = AddHook<Func<Item, Player, bool>>(g => g.AltFunctionUse);

	/// <summary>
	/// Calls ModItem.AltFunctionUse, then all GlobalItem.AltFunctionUse hooks, until one of them returns true. Returns whether any of the hooks returned true.
	/// </summary>
	public static bool AltFunctionUse(Item item, Player player)
	{
		if (item.IsAir)
			return false;

		if (item.ModItem != null && item.ModItem.AltFunctionUse(player))
			return true;

		foreach (var g in HookAltFunctionUse.Enumerate(item)) {
			if (g.AltFunctionUse(item, player))
				return true;
		}

		return false;
	}

	private static HookList HookUpdateInventory = AddHook<Action<Item, Player>>(g => g.UpdateInventory);

	/// <summary>
	/// Calls ModItem.UpdateInventory and all GlobalItem.UpdateInventory hooks.
	/// </summary>
	public static void UpdateInventory(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UpdateInventory(player);

		foreach (var g in HookUpdateInventory.Enumerate(item)) {
			g.UpdateInventory(item, player);
		}
	}

	private static HookList HookUpdateInfoAccessory = AddHook<Action<Item, Player>>(g => g.UpdateInfoAccessory);

	/// <summary>
	/// Calls ModItem.UpdateInfoAccessory and all GlobalItem.UpdateInfoAccessory hooks.
	/// </summary>
	public static void UpdateInfoAccessory(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UpdateInfoAccessory(player);

		foreach (var g in HookUpdateInfoAccessory.Enumerate(item)) {
			g.UpdateInfoAccessory(item, player);
		}
	}

	private static HookList HookUpdateEquip = AddHook<Action<Item, Player>>(g => g.UpdateEquip);

	/// <summary>
	/// Hook at the end of Player.VanillaUpdateEquip can be called to apply additional code related to accessory slots for a particular item
	/// </summary>
	public static void UpdateEquip(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UpdateEquip(player);

		foreach (var g in HookUpdateEquip.Enumerate(item)) {
			g.UpdateEquip(item, player);
		}
	}

	private static HookList HookUpdateAccessory = AddHook<Action<Item, Player, bool>>(g => g.UpdateAccessory);

	/// <summary>
	/// Hook at the end of Player.ApplyEquipFunctional can be called to apply additional code related to accessory slots for a particular item.
	/// </summary>
	public static void UpdateAccessory(Item item, Player player, bool hideVisual)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UpdateAccessory(player, hideVisual);

		foreach (var g in HookUpdateAccessory.Enumerate(item)) {
			g.UpdateAccessory(item, player, hideVisual);
		}
	}

	private static HookList HookUpdateVanity = AddHook<Action<Item, Player>>(g => g.UpdateVanity);

	/// <summary>
	/// Hook at the end of Player.ApplyEquipVanity can be called to apply additional code related to accessory slots for a particular item
	/// </summary>
	public static void UpdateVanity(Item item, Player player)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UpdateVanity(player);

		foreach (var g in HookUpdateVanity.Enumerate(item)) {
			g.UpdateVanity(item, player);
		}
	}

	private static HookList HookUpdateVisibleAccessory = AddHook<Action<Item, Player, bool>>(g => g.UpdateVisibleAccessory);

	/// <summary>
	/// Hook at the end of Player.UpdateVisibleAccessory that can be called to set flags related to player drawing.
	/// </summary>
	public static void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
	{
		if (item.IsAir)
			return;

		item.ModItem?.UpdateVisibleAccessory(player, hideVisual);

		foreach (var g in HookUpdateVisibleAccessory.Enumerate(item)) {
			g.UpdateVisibleAccessory(item, player, hideVisual);
		}
	}

	private static HookList HookUpdateItemDye = AddHook<Action<Item, Player, int, bool>>(g => g.UpdateItemDye);

	public static void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
	{
		item.ModItem?.UpdateItemDye(player, dye, hideVisual);

		foreach (var g in HookUpdateItemDye.Enumerate(item)) {
			g.UpdateItemDye(item, player, dye, hideVisual);
		}
	}

	private static HookList HookUpdateArmorSet = AddHook<Action<Player, string>>(g => g.UpdateArmorSet);

	/// <summary>
	/// If the head's <see cref="ModItem.IsArmorSet(Item, Item, Item)"/> returns true, calls the head's <see cref="ModItem.UpdateArmorSet(Player)"/>. This is then repeated for the body, then the legs. Then for each GlobalItem, if <see cref="GlobalItem.IsArmorSet(Item, Item, Item)"/> returns a non-empty string, calls <see cref="GlobalItem.UpdateArmorSet(Player, string)"/> with that string.
	/// </summary>
	public static void UpdateArmorSet(Player player, Item head, Item body, Item legs)
	{
		if (head.ModItem != null && head.ModItem.IsArmorSet(head, body, legs))
			head.ModItem.UpdateArmorSet(player);

		if (body.ModItem != null && body.ModItem.IsArmorSet(head, body, legs))
			body.ModItem.UpdateArmorSet(player);

		if (legs.ModItem != null && legs.ModItem.IsArmorSet(head, body, legs))
			legs.ModItem.UpdateArmorSet(player);

		foreach (var g in HookUpdateArmorSet.Enumerate()) {
			string set = g.IsArmorSet(head, body, legs);
			if (!string.IsNullOrEmpty(set))
				g.UpdateArmorSet(player, set);
		}
	}

	private static HookList HookPreUpdateVanitySet = AddHook<Action<Player, string>>(g => g.PreUpdateVanitySet);

	/// <summary>
	/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's PreUpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.PreUpdateVanitySet, using player.head, player.body, and player.legs.
	/// </summary>
	public static void PreUpdateVanitySet(Player player)
	{
		EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
		EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
		EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

		if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			headTexture.PreUpdateVanitySet(player);

		if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			bodyTexture.PreUpdateVanitySet(player);

		if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			legTexture.PreUpdateVanitySet(player);

		foreach (var g in HookPreUpdateVanitySet.Enumerate()) {
			string set = g.IsVanitySet(player.head, player.body, player.legs);
			if (!string.IsNullOrEmpty(set))
				g.PreUpdateVanitySet(player, set);
		}
	}

	private static HookList HookUpdateVanitySet = AddHook<Action<Player, string>>(g => g.UpdateVanitySet);

	/// <summary>
	/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's UpdateVanitySet. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.UpdateVanitySet, using player.head, player.body, and player.legs.
	/// </summary>
	public static void UpdateVanitySet(Player player)
	{
		EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
		EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
		EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

		if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			headTexture.UpdateVanitySet(player);

		if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			bodyTexture.UpdateVanitySet(player);

		if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			legTexture.UpdateVanitySet(player);

		foreach (var g in HookUpdateVanitySet.Enumerate()) {
			string set = g.IsVanitySet(player.head, player.body, player.legs);
			if (!string.IsNullOrEmpty(set))
				g.UpdateVanitySet(player, set);
		}
	}

	private static HookList HookArmorSetShadows = AddHook<Action<Player, string>>(g => g.ArmorSetShadows);

	/// <summary>
	/// If the player's head texture's IsVanitySet returns true, calls the equipment texture's ArmorSetShadows. This is then repeated for the player's body, then the legs. Then for each GlobalItem, if GlobalItem.IsVanitySet returns a non-empty string, calls GlobalItem.ArmorSetShadows, using player.head, player.body, and player.legs.
	/// </summary>
	public static void ArmorSetShadows(Player player)
	{
		EquipTexture headTexture = EquipLoader.GetEquipTexture(EquipType.Head, player.head);
		EquipTexture bodyTexture = EquipLoader.GetEquipTexture(EquipType.Body, player.body);
		EquipTexture legTexture = EquipLoader.GetEquipTexture(EquipType.Legs, player.legs);

		if (headTexture != null && headTexture.IsVanitySet(player.head, player.body, player.legs))
			headTexture.ArmorSetShadows(player);

		if (bodyTexture != null && bodyTexture.IsVanitySet(player.head, player.body, player.legs))
			bodyTexture.ArmorSetShadows(player);

		if (legTexture != null && legTexture.IsVanitySet(player.head, player.body, player.legs))
			legTexture.ArmorSetShadows(player);

		foreach (var g in HookArmorSetShadows.Enumerate()) {
			string set = g.IsVanitySet(player.head, player.body, player.legs);
			if (!string.IsNullOrEmpty(set))
				g.ArmorSetShadows(player, set);
		}
	}

	private delegate void DelegateSetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes);
	private static HookList HookSetMatch = AddHook<DelegateSetMatch>(g => g.SetMatch);

	/// <summary>
	/// Calls EquipTexture.SetMatch, then all GlobalItem.SetMatch hooks.
	/// </summary>
	public static void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes)
	{
		EquipTexture texture = EquipLoader.GetEquipTexture((EquipType)armorSlot, type);

		texture?.SetMatch(male, ref equipSlot, ref robes);

		foreach (var g in HookSetMatch.Enumerate()) {
			g.SetMatch(armorSlot, type, male, ref equipSlot, ref robes);
		}
	}

	private static HookList HookCanRightClick = AddHook<Func<Item, bool>>(g => g.CanRightClick);

	/// <summary>
	/// Calls ModItem.CanRightClick, then all GlobalItem.CanRightClick hooks, until one of the returns true.
	/// Also returns true if ItemID.Sets.OpenableBag
	/// </summary>
	public static bool CanRightClick(Item item)
	{
		if (item.IsAir)
			return false;

		if (ItemID.Sets.OpenableBag[item.type])
			return true;

		if (item.ModItem != null && item.ModItem.CanRightClick())
			return true;

		foreach (var g in HookCanRightClick.Enumerate(item)) {
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
	public static void RightClick(Item item, Player player)
	{
		RightClickCallHooks(item, player);

		if (ConsumeItem(item, player) && --item.stack == 0)
			item.SetDefaults();

		SoundEngine.PlaySound(7);
		Main.stackSplit = 30;
		Main.mouseRightRelease = false;
		Recipe.FindRecipes();
	}

	internal static void RightClickCallHooks(Item item, Player player)
	{
		item.ModItem?.RightClick(player);

		foreach (var g in HookRightClick.Enumerate(item)) {
			g.RightClick(item, player);
		}
	}


	private static HookList HookModifyItemLoot = AddHook<Action<Item, ItemLoot>>(g => g.ModifyItemLoot);

	/// <summary>
	/// Calls each GlobalItem.ModifyItemLoot hooks.
	/// </summary>
	public static void ModifyItemLoot(Item item, ItemLoot itemLoot)
	{
		item.ModItem?.ModifyItemLoot(itemLoot);

		foreach (var g in HookModifyItemLoot.Enumerate(item)) {
			g.ModifyItemLoot(item, itemLoot);
		}
	}

	private static HookList HookCanStack = AddHook<Func<Item, Item, bool>>(g => g.CanStack);

	/// <summary>
	/// Returns false if item prefixes don't match. Then calls all GlobalItem.CanStack hooks until one returns false then ModItem.CanStack. Returns whether any of the hooks returned false.
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <returns>Whether or not the items are allowed to stack</returns>
	public static bool CanStack(Item destination, Item source)
	{
		if (destination.prefix != source.prefix) // #StackablePrefixWeapons
			return false;

		foreach (var g in HookCanStack.Enumerate(destination)) {
			if (!g.CanStack(destination, source))
				return false;
		}

		return destination.ModItem?.CanStack(source) ?? true;
	}

	private static HookList HookCanStackInWorld = AddHook<Func<Item, Item, bool>>(g => g.CanStackInWorld);

	/// <summary>
	/// Calls all GlobalItem.CanStackInWorld hooks until one returns false then ModItem.CanStackInWorld. Returns whether any of the hooks returned false.
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <returns>Whether or not the items are allowed to stack</returns>
	public static bool CanStackInWorld(Item destination, Item source)
	{
		foreach (var g in HookCanStackInWorld.Enumerate(destination)) {
			if (!g.CanStackInWorld(destination, source))
				return false;
		}

		return destination.ModItem?.CanStackInWorld(source) ?? true;
	}

	private static HookList HookOnStack = AddHook<Action<Item, Item, int>>(g => g.OnStack);

	/// <summary>
	/// Stacks <paramref name="source"/> onto <paramref name="destination"/> if CanStack permits the transfer
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <param name="numTransferred">The quantity of <paramref name="source"/> that was transferred to <paramref name="destination"/></param>
	/// <param name="infiniteSource">If true, <paramref name="source"/>.stack will not be decreased</param>
	/// <returns>Whether or not the items were allowed to stack</returns>
	public static bool TryStackItems(Item destination, Item source, out int numTransferred, bool infiniteSource = false)
	{
		numTransferred = 0;

		if (!CanStack(destination, source))
			return false;

		StackItems(destination, source, out numTransferred, infiniteSource);

		return true;
	}

	/// <summary>
	/// Stacks <paramref name="destination"/> onto <paramref name="source"/><br/>
	/// This method should not be called unless <see cref="CanStack(Item, Item)"/> returns true.  See: <see cref="TryStackItems(Item, Item, out int, bool)"/>
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <param name="numTransferred">The quantity of <paramref name="source"/> that was transferred to <paramref name="destination"/></param>
	/// <param name="infiniteSource">If true, <paramref name="source"/>.stack will not be decreased</param>
	/// <param name="numToTransfer">
	/// An optional argument used to specify the quantity of items to transfer from <paramref name="source"/> to <paramref name="destination"/>.<br/>
	/// By default, as many items as possible will be transferred.  That is, either source will be empty, or destination will be full (maxStack)
	/// </param>
	public static void StackItems(Item destination, Item source, out int numTransferred, bool infiniteSource = false, int? numToTransfer = null)
	{
		numTransferred = numToTransfer ?? Math.Min(source.stack, destination.maxStack - destination.stack);

		OnStack(destination, source, numTransferred);

		bool isSplittingToHand = numTransferred < source.stack && destination == Main.mouseItem;
		if (source.favorited && !isSplittingToHand) {
			destination.favorited = true;
			source.favorited = false;
		}

		if (destination.shopCustomPrice != source.shopCustomPrice) {
			// If attempting to stack items with custom prices, null them out to prevent exploits. Fixes #4370 while preserving normal resell behavior.
			destination.shopCustomPrice = null;
		}

		destination.stack += numTransferred;
		if (!infiniteSource)
			source.stack -= numTransferred;
	}

	/// <summary>
	/// Calls the GlobalItem.OnStack hooks in <paramref name="destination"/>, then the ModItem.OnStack hook in <paramref name="destination"/><br/>
	/// OnStack is called before the items are transferred from <paramref name="source"/> to <paramref name="destination"/>
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <param name="numToTransfer">The quantity of <paramref name="source"/> that will be transferred to <paramref name="destination"/></param>
	public static void OnStack(Item destination, Item source, int numToTransfer)
	{
		foreach (var g in HookOnStack.Enumerate(destination)) {
			g.OnStack(destination, source, numToTransfer);
		}

		destination.ModItem?.OnStack(source, numToTransfer);
	}

	private static HookList HookSplitStack = AddHook<Action<Item, Item, int>>(g => g.SplitStack);

	/// <summary>
	/// Extract up to <paramref name="limit"/> items from <paramref name="source"/>. If some items remain, <see cref="SplitStack"/> will be used.
	/// </summary>
	/// <param name="source">The original item instance</param>
	/// <param name="limit">How many items should be transferred</param>
	public static Item TransferWithLimit(Item source, int limit)
	{
		Item destination = source.Clone();
		if (source.stack <= limit) {
			source.TurnToAir();
		}
		else {
			SplitStack(destination, source, limit);
		}
		return destination;
	}

	/// <summary>
	/// Called when splitting a stack of items.
	/// </summary>
	/// <param name="destination">
	/// The item instance that <paramref name="source"/> will transfer items to, and is usually a clone of <paramref name="source"/>.<br/>
	/// This parameter's stack will be set to zero before any transfer occurs.
	/// </param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <param name="numToTransfer">The quantity of <paramref name="source"/> that will be transferred to <paramref name="destination"/></param>
	public static void SplitStack(Item destination, Item source, int numToTransfer)
	{
		destination.stack = 0;
		destination.favorited = false;

		foreach (var g in HookSplitStack.Enumerate(destination)) {
			g.SplitStack(destination, source, numToTransfer);
		}

		destination.ModItem?.SplitStack(source, numToTransfer);

		destination.stack += numToTransfer;
		source.stack -= numToTransfer;
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
	public static bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
	{
		bool b = item.ModItem?.ReforgePrice(ref reforgePrice, ref canApplyDiscount) ?? true;

		foreach (var g in HookReforgePrice.Enumerate(item)) {
			b &= g.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
		}

		return b;
	}

	private static HookList HookCanReforge = AddHook<Func<Item, bool>>(g => g.CanReforge);

	/// <summary>
	/// Calls ModItem.CanReforge, then all GlobalItem.CanReforge hooks. If any return false then false is returned.
	/// </summary>
	public static bool CanReforge(Item item)
	{
		bool b = item.ModItem?.CanReforge() ?? true;

		foreach (var g in HookCanReforge.Enumerate(item)) {
			b &= g.CanReforge(item);
		}

		return b;
	}

	private static HookList HookPreReforge = AddHook<Action<Item>>(g => g.PreReforge);

	/// <summary>
	/// Calls ModItem.PreReforge, then all GlobalItem.PreReforge hooks.
	/// </summary>
	public static void PreReforge(Item item)
	{
		item.ModItem?.PreReforge();

		foreach (var g in HookPreReforge.Enumerate(item)) {
			g.PreReforge(item);
		}
	}

	private static HookList HookPostReforge = AddHook<Action<Item>>(g => g.PostReforge);

	/// <summary>
	/// Calls ModItem.PostReforge, then all GlobalItem.PostReforge hooks.
	/// </summary>
	public static void PostReforge(Item item)
	{
		item.ModItem?.PostReforge();

		foreach (var g in HookPostReforge.Enumerate(item)) {
			g.PostReforge(item);
		}
	}

	private delegate void DelegateDrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor);
	private static HookList HookDrawArmorColor = AddHook<DelegateDrawArmorColor>(g => g.DrawArmorColor);

	/// <summary>
	/// Calls the item's equipment texture's DrawArmorColor hook, then all GlobalItem.DrawArmorColor hooks.
	/// </summary>
	public static void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color,
		ref int glowMask, ref Color glowMaskColor)
	{
		EquipTexture texture = EquipLoader.GetEquipTexture(type, slot);
		texture?.DrawArmorColor(drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);

		foreach (var g in HookDrawArmorColor.Enumerate()) {
			g.DrawArmorColor(type, slot, drawPlayer, shadow, ref color, ref glowMask, ref glowMaskColor);
		}
	}

	private delegate void DelegateArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color);
	private static HookList HookArmorArmGlowMask = AddHook<DelegateArmorArmGlowMask>(g => g.ArmorArmGlowMask);

	/// <summary>
	/// Calls the item's body equipment texture's ArmorArmGlowMask hook, then all GlobalItem.ArmorArmGlowMask hooks.
	/// </summary>
	public static void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color)
	{
		EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Body, slot);

		texture?.ArmorArmGlowMask(drawPlayer, shadow, ref glowMask, ref color);

		foreach (var g in HookArmorArmGlowMask.Enumerate()) {
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
		ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
	{
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

		foreach (var g in HookVerticalWingSpeeds.Enumerate(item)) {
			g.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising,
				ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
		}
	}

	private delegate void DelegateHorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration);
	private static HookList HookHorizontalWingSpeeds = AddHook<DelegateHorizontalWingSpeeds>(g => g.HorizontalWingSpeeds);

	/// <summary>
	/// If the player is using wings, this uses the result of GetWing, and calls ModItem.HorizontalWingSpeeds then all GlobalItem.HorizontalWingSpeeds hooks.
	/// </summary>
	public static void HorizontalWingSpeeds(Player player)
	{
		Item item = player.equippedWings;
		if (item == null) {
			EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wingsLogic);
			texture?.HorizontalWingSpeeds(player, ref player.accRunSpeed, ref player.runAcceleration);
			return;
		}

		item.ModItem?.HorizontalWingSpeeds(player, ref player.accRunSpeed, ref player.runAcceleration);

		foreach (var g in HookHorizontalWingSpeeds.Enumerate(item)) {
			g.HorizontalWingSpeeds(item, player, ref player.accRunSpeed, ref player.runAcceleration);
		}
	}

	private static HookList HookWingUpdate = AddHook<Func<int, Player, bool, bool>>(g => g.WingUpdate);

	/// <summary>
	/// If wings can be seen on the player, calls the player's wing's equipment texture's WingUpdate and all GlobalItem.WingUpdate hooks.
	/// </summary>
	public static bool WingUpdate(Player player, bool inUse)
	{
		if (player.wings <= 0)
			return false;

		EquipTexture texture = EquipLoader.GetEquipTexture(EquipType.Wings, player.wings);
		bool? retVal = texture?.WingUpdate(player, inUse);

		foreach (var g in HookWingUpdate.Enumerate()) {
			retVal |= g.WingUpdate(player.wings, player, inUse);
		}

		return retVal ?? false;
	}

	private delegate void DelegateUpdate(Item item, ref float gravity, ref float maxFallSpeed);
	private static HookList HookUpdate = AddHook<DelegateUpdate>(g => g.Update);

	/// <summary>
	/// Calls ModItem.Update, then all GlobalItem.Update hooks.
	/// </summary>
	public static void Update(Item item, ref float gravity, ref float maxFallSpeed)
	{
		item.ModItem?.Update(ref gravity, ref maxFallSpeed);

		foreach (var g in HookUpdate.Enumerate(item)) {
			g.Update(item, ref gravity, ref maxFallSpeed);
		}
	}

	private static HookList HookPostUpdate = AddHook<Action<Item>>(g => g.PostUpdate);

	/// <summary>
	/// Calls ModItem.PostUpdate and all GlobalItem.PostUpdate hooks.
	/// </summary>
	public static void PostUpdate(Item item)
	{
		item.ModItem?.PostUpdate();

		foreach (var g in HookPostUpdate.Enumerate(item)) {
			g.PostUpdate(item);
		}
	}

	private delegate void DelegateGrabRange(Item item, Player player, ref int grabRange);
	private static HookList HookGrabRange = AddHook<DelegateGrabRange>(g => g.GrabRange);

	/// <summary>
	/// Calls ModItem.GrabRange, then all GlobalItem.GrabRange hooks.
	/// </summary>
	public static void GrabRange(Item item, Player player, ref int grabRange)
	{
		item.ModItem?.GrabRange(player, ref grabRange);

		foreach (var g in HookGrabRange.Enumerate(item)) {
			g.GrabRange(item, player, ref grabRange);
		}
	}

	private static HookList HookGrabStyle = AddHook<Func<Item, Player, bool>>(g => g.GrabStyle);

	/// <summary>
	/// Calls all GlobalItem.GrabStyle hooks then ModItem.GrabStyle, until one of them returns true. Returns whether any of the hooks returned true.
	/// </summary>
	public static bool GrabStyle(Item item, Player player)
	{
		foreach (var g in HookGrabStyle.Enumerate(item)) {
			if (g.GrabStyle(item, player))
				return true;
		}

		return item.ModItem != null && item.ModItem.GrabStyle(player);
	}

	private static HookList HookCanPickup = AddHook<Func<Item, Player, bool>>(g => g.CanPickup);

	public static bool CanPickup(Item item, Player player)
	{
		foreach (var g in HookCanPickup.Enumerate(item)) {
			if (!g.CanPickup(item, player))
				return false;
		}

		return item.ModItem?.CanPickup(player) ?? true;
	}

	private static HookList HookOnPickup = AddHook<Func<Item, Player, bool>>(g => g.OnPickup);

	/// <summary>
	/// Calls all GlobalItem.OnPickup hooks then ModItem.OnPickup, until one of the returns false. Returns true if all of the hooks return true.
	/// </summary>
	public static bool OnPickup(Item item, Player player)
	{
		foreach (var g in HookOnPickup.Enumerate(item)) {
			if (!g.OnPickup(item, player))
				return false;
		}

		return item.ModItem?.OnPickup(player) ?? true;
	}

	private static HookList HookItemSpace = AddHook<Func<Item, Player, bool>>(g => g.ItemSpace);

	public static bool ItemSpace(Item item, Player player)
	{
		foreach (var g in HookItemSpace.Enumerate(item)) {
			if (g.ItemSpace(item, player))
				return true;
		}

		return item.ModItem?.ItemSpace(player) ?? false;
	}

	private static HookList HookGetAlpha = AddHook<Func<Item, Color, Color?>>(g => g.GetAlpha);

	/// <summary>
	/// Calls all GlobalItem.GetAlpha hooks then ModItem.GetAlpha, until one of them returns a color, and returns that color. Returns null if all of the hooks return null.
	/// </summary>
	public static Color? GetAlpha(Item item, Color lightColor)
	{
		if (item.IsAir)
			return null;

		foreach (var g in HookGetAlpha.Enumerate(item)) {
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
	public static bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		bool flag = true;
		if (item.ModItem != null)
			flag &= item.ModItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

		foreach (var g in HookPreDrawInWorld.Enumerate(item)) {
			flag &= g.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}

		return flag;
	}

	private static HookList HookPostDrawInWorld = AddHook<Action<Item, SpriteBatch, Color, Color, float, float, int>>(g => g.PostDrawInWorld);

	/// <summary>
	/// Calls ModItem.PostDrawInWorld, then all GlobalItem.PostDrawInWorld hooks.
	/// </summary>
	public static void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		item.ModItem?.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);

		foreach (var g in HookPostDrawInWorld.Enumerate(item)) {
			g.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
		}
	}

	private static HookList HookPreDrawInInventory = AddHook<Func<Item, SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float, bool>>(g => g.PreDrawInInventory);

	/// <summary>
	/// Returns the "and" operator on the results of all GlobalItem.PreDrawInInventory hooks and ModItem.PreDrawInInventory.
	/// </summary>
	public static bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
		Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		bool flag = true;
		foreach (var g in HookPreDrawInInventory.Enumerate(item)) {
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
		Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		item.ModItem?.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);

		foreach (var g in HookPostDrawInInventory.Enumerate(item)) {
			g.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
		}
	}

	private static HookList HookHoldoutOffset = AddHook<Func<int, Vector2?>>(g => g.HoldoutOffset);

	public static void HoldoutOffset(float gravDir, int type, ref Vector2 offset)
	{
		ModItem modItem = GetItem(type);

		if (modItem != null) {
			Vector2? modOffset = modItem.HoldoutOffset();

			if (modOffset.HasValue) {
				offset.X = modOffset.Value.X;
				offset.Y += gravDir * modOffset.Value.Y;
			}
		}

		foreach (var g in HookHoldoutOffset.Enumerate(type)) {
			Vector2? modOffset = g.HoldoutOffset(type);

			if (modOffset.HasValue) {
				offset.X = modOffset.Value.X;
				offset.Y = TextureAssets.Item[type].Value.Height / 2f + gravDir * modOffset.Value.Y;
			}
		}
	}

	private static HookList HookHoldoutOrigin = AddHook<Func<int, Vector2?>>(g => g.HoldoutOrigin);

	public static void HoldoutOrigin(Player player, ref Vector2 origin)
	{
		Item item = player.HeldItem;
		Vector2 modOrigin = Vector2.Zero;
		if (item.ModItem != null) {
			Vector2? modOrigin2 = item.ModItem.HoldoutOrigin();
			if (modOrigin2.HasValue) {
				modOrigin = modOrigin2.Value;
			}
		}
		foreach (var g in HookHoldoutOrigin.Enumerate(item)) {
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

	public static bool CanEquipAccessory(Item item, int slot, bool modded)
	{
		Player player = Main.player[Main.myPlayer];
		if (item.ModItem != null && !item.ModItem.CanEquipAccessory(player, slot, modded))
			return false;

		foreach (var g in HookCanEquipAccessory.Enumerate(item)) {
			if (!g.CanEquipAccessory(item, player, slot, modded))
				return false;
		}

		return true;
	}

	public static bool CanEquipAccessory(Player player, Item item, int slot, bool modded)
	{
		if (item.ModItem != null && !item.ModItem.CanEquipAccessory(player, slot, modded))
			return false;

		foreach (var g in HookCanEquipAccessory.Enumerate(item)) {
			if (!g.CanEquipAccessory(item, player, slot, modded))
				return false;
		}

		return true;
	}

	private static HookList HookCanAccessoryBeEquippedWith = AddHook<Func<Item, Item, Player, bool>>(g => g.CanAccessoryBeEquippedWith);

	public static bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem)
	{
		Player player = Main.player[Main.myPlayer];
		return CanAccessoryBeEquippedWith(equippedItem, incomingItem, player) && CanAccessoryBeEquippedWith(incomingItem, equippedItem, player);
	}

	public static bool CanAccessoryBeEquippedWith(Player player, Item equippedItem, Item incomingItem)
	{
		return CanAccessoryBeEquippedWith(equippedItem, incomingItem, player) && CanAccessoryBeEquippedWith(incomingItem, equippedItem, player);
	}

	private static bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
	{
		if (equippedItem.ModItem != null && !equippedItem.ModItem.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player))
			return false;

		if (incomingItem.ModItem != null && !incomingItem.ModItem.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player))
			return false;

		foreach (var g in HookCanAccessoryBeEquippedWith.Enumerate(incomingItem)) {
			if (!g.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player))
				return false;
		}

		return true;
	}

	private delegate void DelegateExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack);
	private static HookList HookExtractinatorUse = AddHook<DelegateExtractinatorUse>(g => g.ExtractinatorUse);

	public static void ExtractinatorUse(ref int resultType, ref int resultStack, int extractType, int extractinatorBlockType)
	{
		GetItem(extractType)?.ExtractinatorUse(extractinatorBlockType, ref resultType, ref resultStack);

		foreach (var g in HookExtractinatorUse.Enumerate()) {
			g.ExtractinatorUse(extractType, extractinatorBlockType, ref resultType, ref resultStack);
		}
	}

	private delegate void DelegateCaughtFishStack(int type, ref int stack);
	private static HookList HookCaughtFishStack = AddHook<DelegateCaughtFishStack>(g => g.CaughtFishStack);

	public static void CaughtFishStack(Item item)
	{
		item.ModItem?.CaughtFishStack(ref item.stack);

		foreach (var g in HookCaughtFishStack.Enumerate(item)) {
			g.CaughtFishStack(item.type, ref item.stack);
		}
	}

	private static HookList HookIsAnglerQuestAvailable = AddHook<Func<int, bool>>(g => g.IsAnglerQuestAvailable);

	public static void IsAnglerQuestAvailable(int itemID, ref bool notAvailable)
	{
		ModItem modItem = GetItem(itemID);
		if (modItem != null)
			notAvailable |= !modItem.IsAnglerQuestAvailable();

		foreach (var g in HookIsAnglerQuestAvailable.Enumerate(itemID)) {
			notAvailable |= !g.IsAnglerQuestAvailable(itemID);
		}
	}

	private delegate void DelegateAnglerChat(int type, ref string chat, ref string catchLocation);
	private static HookList HookAnglerChat = AddHook<DelegateAnglerChat>(g => g.AnglerChat);

	public static string AnglerChat(int type)
	{
		string chat = "";
		string catchLocation = "";
		GetItem(type)?.AnglerQuestChat(ref chat, ref catchLocation);

		foreach (var g in HookAnglerChat.Enumerate(type)) {
			g.AnglerChat(type, ref chat, ref catchLocation);
		}

		if (string.IsNullOrEmpty(chat) || string.IsNullOrEmpty(catchLocation))
			return null;

		return chat + "\n\n(" + catchLocation + ")";
	}

	private delegate bool DelegatePreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y);
	private static HookList HookPreDrawTooltip = AddHook<DelegatePreDrawTooltip>(g => g.PreDrawTooltip);

	public static bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
	{
		bool ret = item.ModItem?.PreDrawTooltip(lines, ref x, ref y) ?? true;

		foreach (var g in HookPreDrawTooltip.Enumerate(item)) {
			ret &= g.PreDrawTooltip(item, lines, ref x, ref y);
		}

		return ret;
	}

	private delegate void DelegatePostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines);
	private static HookList HookPostDrawTooltip = AddHook<DelegatePostDrawTooltip>(g => g.PostDrawTooltip);

	public static void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
	{
		item.ModItem?.PostDrawTooltip(lines);

		foreach (var g in HookPostDrawTooltip.Enumerate(item)) {
			g.PostDrawTooltip(item, lines);
		}
	}

	private delegate bool DelegatePreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset);
	private static HookList HookPreDrawTooltipLine = AddHook<DelegatePreDrawTooltipLine>(g => g.PreDrawTooltipLine);

	public static bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
	{
		bool ret = item.ModItem?.PreDrawTooltipLine(line, ref yOffset) ?? true;

		foreach (var g in HookPreDrawTooltipLine.Enumerate(item)) {
			ret &= g.PreDrawTooltipLine(item, line, ref yOffset);
		}

		return ret;
	}

	private delegate void DelegatePostDrawTooltipLine(Item item, DrawableTooltipLine line);
	private static HookList HookPostDrawTooltipLine = AddHook<DelegatePostDrawTooltipLine>(g => g.PostDrawTooltipLine);

	public static void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
	{
		item.ModItem?.PostDrawTooltipLine(line);

		foreach (var g in HookPostDrawTooltipLine.Enumerate(item)) {
			g.PostDrawTooltipLine(item, line);
		}
	}

	private static HookList HookModifyTooltips = AddHook<Action<Item, List<TooltipLine>>>(g => g.ModifyTooltips);

	public static List<TooltipLine> ModifyTooltips(Item item, ref int numTooltips, string[] names, ref string[] text, ref bool[] modifier, ref bool[] badModifier, ref int oneDropLogo, out Color?[] overrideColor, int prefixlineIndex)
	{
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

		if (item.prefix >= PrefixID.Count && prefixlineIndex != -1) {
			var tooltipLines = PrefixLoader.GetPrefix(item.prefix)?.GetTooltipLines(item);
			if (tooltipLines != null) {
				foreach (var line in tooltipLines) {
					tooltips.Insert(prefixlineIndex, line);
					prefixlineIndex++;
				}
			}
		}

		item.ModItem?.ModifyTooltips(tooltips);

		if (!item.IsAir) { // Prevents dummy items used in Main.HoverItem from getting unrelated tooltips
			foreach (var g in HookModifyTooltips.Enumerate(item)) {
				g.ModifyTooltips(item, tooltips);
			}
		}

		tooltips.RemoveAll(x => !x.Visible);

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

	public static void ModifyFishingLine(Projectile projectile, ref float polePosX, ref float polePosY, ref Color lineColor)
	{
		Player player = Main.player[projectile.owner];
		Item item = player.inventory[player.selectedItem];

		if (item.ModItem == null)
			return;

		Vector2 lineOriginOffset = Vector2.Zero;

		item.ModItem.ModifyFishingLine(projectile, ref lineOriginOffset, ref lineColor);

		polePosX += lineOriginOffset.X * player.direction;
		if (player.direction < 0)
			polePosX -= 13f;
		polePosY += lineOriginOffset.Y * player.gravDir;
	}

	internal static HookList HookSaveData = AddHook<Action<Item, TagCompound>>(g => g.SaveData);
	internal static HookList HookNetSend = AddHook<Action<Item, BinaryWriter>>(g => g.NetSend);
	internal static HookList HookNetReceive = AddHook<Action<Item, BinaryReader>>(g => g.NetReceive);

	internal static bool NeedsModSaving(Item item)
	{
		if (item.type <= ItemID.None)
			return false;

		if (item.ModItem != null || item.prefix >= PrefixID.Count)
			return true;

		return false;
	}
}
