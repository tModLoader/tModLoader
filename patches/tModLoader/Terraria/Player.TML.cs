using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;

public partial class Player : IEntityWithInstances<ModPlayer>
{
	internal IList<string> usedMods;
	/// <summary> Contains error messages from ModPlayer.SaveData from a previous player save retrieved from the .tplr. Shown when entering a world and on player select menu. Maps ModSystem.FullName.MethodName to exception string.</summary>
	internal Dictionary<string, string> ModSaveErrors { get; set; } = new Dictionary<string, string>();
	internal string modPack;
	internal ModPlayer[] modPlayers = Array.Empty<ModPlayer>();

	public Item equippedWings = null;

	/// <summary>
	/// Set by any gem robe when worn by the player in the functional armor slot. Increases the spawn rate of <see cref="NPCID.Tim"/>.
	/// </summary>
	public bool hasGemRobe = false;

	/// <summary>
	/// Causes <see cref="SmartSelectLookup"/> to run the next time an item animation is finished, even if <see cref="controlUseItem"/> is held. <br/>
	/// Used internally by tML to when a hotbar key is pressed while using an item.
	/// </summary>
	public bool selectItemOnNextUse;

	private int consumedLifeCrystals;

	/// <summary>
	/// How many Life Crystals this player has consumed
	/// </summary>
	public int ConsumedLifeCrystals {
		get => consumedLifeCrystals;
		set => consumedLifeCrystals = Utils.Clamp(value, 0, LifeCrystalMax);
	}

	/// <summary>
	/// The maximum amount of Life Crystals this player is allowed to consume total
	/// </summary>
	public const int LifeCrystalMax = 15;

	private int consumedLifeFruit;

	/// <summary>
	/// How many Life Fruit this player has consumed
	/// </summary>
	public int ConsumedLifeFruit {
		get => consumedLifeFruit;
		set => consumedLifeFruit = Utils.Clamp(value, 0, LifeFruitMax);
	}

	/// <summary>
	/// The maximum amount of Life Fruit this player is allowed to consume total
	/// </summary>
	public const int LifeFruitMax = 20;

	private int consumedManaCrystals;

	/// <summary>
	/// How many Mana Crystals this player has consumed
	/// </summary>
	public int ConsumedManaCrystals {
		get => consumedManaCrystals;
		set => consumedManaCrystals = Utils.Clamp(value, 0, ManaCrystalMax);
	}

	/// <summary>
	/// Checks or sets <see cref="meleeEnchant"/>, indicating if a melee enchantment is active or not. Mods can check the <see cref="meleeEnchant"/> value directly to act on existing melee enchantments, but modded melee enchantments do not have specific assigned values.
	/// </summary>
	public bool MeleeEnchantActive {
		get => meleeEnchant > 0;
		set => meleeEnchant = 255;
	}

	/// <summary>
	/// The maximum amount of Mana Crystals this player is allowed to consume total
	/// </summary>
	public const int ManaCrystalMax = 9;

	/// <summary>
	/// How effectively the player can hold their breath underwater. Controls how long it takes for <see cref="breath"/> to decrease. Breathing Reed adds 1 (100%) to this value and Diving Gear multiplies it by 6.
	/// <para/> Modded effects should add to this instead of multiplying to avoid values getting unreasonable large. Adding 1.5f for example will increase breath time by 150%.
	/// <para/> Applied in the calculation of <see cref="breathCDMax"/>.
	/// </summary>
	public StatModifier breathEffectiveness = StatModifier.Default;

	/// <summary>
	/// Modifies the cooldown of health potions. Can be used to adjust potion cooldown calculations, similar to the Philosopher's Stone.
	/// </summary>
	public StatModifier PotionDelayModifier = StatModifier.Default;

	public RefReadOnlyArray<ModPlayer> ModPlayers => modPlayers;

	RefReadOnlyArray<ModPlayer> IEntityWithInstances<ModPlayer>.Instances => modPlayers;

	public HashSet<int> NearbyModTorch { get; private set; } = new HashSet<int>();

	// Get

	/// <summary> Gets the instance of the specified ModPlayer type. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="IndexOutOfRangeException"/>
	public T GetModPlayer<T>() where T : ModPlayer
		=> GetModPlayer(ModContent.GetInstance<T>());

	/// <summary> Gets the local instance of the type of the specified ModPlayer instance. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="IndexOutOfRangeException"/>
	/// <exception cref="NullReferenceException"/>
	public T GetModPlayer<T>(T baseInstance) where T : ModPlayer
		=> modPlayers[baseInstance.Index] as T ?? throw new KeyNotFoundException($"Instance of '{typeof(T).Name}' does not exist on the current player.");

	// TryGet

	/// <summary> Gets the instance of the specified ModPlayer type. </summary>
	public bool TryGetModPlayer<T>(out T result) where T : ModPlayer
		=> TryGetModPlayer(ModContent.GetInstance<T>(), out result);

	/// <summary> Safely attempts to get the local instance of the type of the specified ModPlayer instance. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public bool TryGetModPlayer<T>(T baseInstance, out T result) where T : ModPlayer
	{
		if (baseInstance == null || baseInstance.Index < 0 || baseInstance.Index >= modPlayers.Length) {
			result = default;

			return false;
		}

		result = modPlayers[baseInstance.Index] as T;

		return result != null;
	}

	public void DropFromItem(int itemType)
	{
		DropAttemptInfo info = new() {
			player = this,
			item = itemType,
			IsExpertMode = Main.expertMode,
			IsMasterMode = Main.masterMode,
			IsInSimulation = false,
			rng = Main.rand,
		};
		Main.ItemDropSolver.TryDropping(info);
	}

	/// <summary>
	/// Will spawn an item like <see cref="Player.QuickSpawnItem(IEntitySource, int, int)"/>, but clones it (handy when you need to retain item infos)
	/// </summary>
	/// <param name="source">The spawn context</param>
	/// <param name="item">The item you want to be cloned</param>
	/// <param name="stack">The stack to give the item. Note that this will override maxStack if it's higher.</param>
	// TODO: 1.4.4, delete this and move code to Player.QuickSpawnItem(IEntitySource source, Item item, int stack).
	[Obsolete("Use Player.QuickSpawnItem(IEntitySource source, Item item, int stack) instead.")]
	public int QuickSpawnClonedItem(IEntitySource source, Item item, int stack = 1)
	{
		int index = Item.NewItem(source, getRect(), item, false, false, false);
		Main.item[index].stack = stack;

		// Sync the item for mp
		if (Main.netMode == NetmodeID.MultiplayerClient)
			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f, 0f, 0f, 0, 0, 0);

		return index;
	}

	/// <inheritdoc cref="QuickSpawnClonedItem"/>
	public int QuickSpawnItem(IEntitySource source, Item item, int stack = 1)
		=> QuickSpawnClonedItem(source, item, stack);

	/// <summary><inheritdoc cref="QuickSpawnClonedItem"/></summary>
	/// <returns>Returns the Item instance</returns>
	public Item QuickSpawnClonedItemDirect(IEntitySource source, Item item, int stack = 1)
		=> Main.item[QuickSpawnClonedItem(source, item, stack)];

	/// <summary><inheritdoc cref="QuickSpawnClonedItem"/></summary>
	/// <returns>Returns the Item instance</returns>
	public Item QuickSpawnItemDirect(IEntitySource source, Item item, int stack = 1)
		=> Main.item[QuickSpawnClonedItem(source, item, stack)];

	/// <summary><inheritdoc cref="QuickSpawnItem(IEntitySource, int, int)"/></summary>
	/// <returns>Returns the Item instance</returns>
	public Item QuickSpawnItemDirect(IEntitySource source, int type, int stack = 1)
		=> Main.item[QuickSpawnItem(source, type, stack)];

	/// <summary> Returns whether or not this Player currently has a (de)buff of the provided type. </summary>
	public bool HasBuff(int type) => FindBuffIndex(type) != -1;

	/// <inheritdoc cref="HasBuff(int)" />
	public bool HasBuff<T>() where T : ModBuff
		=> HasBuff(ModContent.BuffType<T>());

	// Damage Classes

	private DamageClassData[] damageData;

	internal void ResetDamageClassData()
	{
		damageData = new DamageClassData[DamageClassLoader.DamageClassCount];

		for (int i = 0; i < damageData.Length; i++) {
			damageData[i] = new DamageClassData();
			DamageClassLoader.DamageClasses[i].SetDefaultStats(this);
		}
	}

	/// <summary>
	/// Gets the damage modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// </summary>
	public ref StatModifier GetDamage<T>() where T : DamageClass => ref GetDamage(ModContent.GetInstance<T>());

	/// <summary>
	/// Gets the damage modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// </summary>
	public ref StatModifier GetDamage(DamageClass damageClass) => ref damageData[damageClass.Type].damage;


	/// <summary>
	/// Gets the crit chance modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// <para/> Note that crit values are percentage values ranging from 0 to 100, unlike damage multipliers. Adding 4, for example, would add 4% to the crit chance.
	/// </summary>
	public ref float GetCritChance<T>() where T : DamageClass => ref GetCritChance(ModContent.GetInstance<T>());

	/// <summary>
	/// Gets the crit chance modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// <para/> Note that crit values are percentage values ranging from 0 to 100, unlike damage multipliers. Adding 4, for example, would add 4% to the crit chance.
	/// </summary>
	public ref float GetCritChance(DamageClass damageClass) => ref damageData[damageClass.Type].critChance;

	/// <summary>
	/// Gets the attack speed modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return values with operators.
	/// Setting this such that it results in zero or a negative value will throw an exception.
	/// NOTE: Due to the nature of attack speed modifiers, modifications to Flat will do nothing for this modifier.
	/// <para/> Note that attack speed is a multiplier. Adding 0.15f, for example, would add 15% to the attack speed stat.
	/// </summary>
	public ref float GetAttackSpeed<T>() where T : DamageClass => ref GetAttackSpeed(ModContent.GetInstance<T>());

	/// <summary>
	/// Gets the attack speed modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return values with operators.
	/// <para/> Note that attack speed is a multiplier. Adding 0.15f, for example, would add 15% to the attack speed stat.
	/// </summary>
	public ref float GetAttackSpeed(DamageClass damageClass) => ref damageData[damageClass.Type].attackSpeed;

	/// <summary>
	/// Gets the armor penetration modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// <para/> Note that armor penetration value are typically whole numbers. Adding 5, for example, would add 5 to the armor penetration stat, similar to the Shark Tooth Necklace accessory.
	/// </summary>
	public ref float GetArmorPenetration<T>() where T : DamageClass => ref GetArmorPenetration(ModContent.GetInstance<T>());

	/// <summary>
	/// Gets the armor penetration modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// <para/> Note that armor penetration value are typically whole numbers. Adding 5, for example, would add 5 to the armor penetration stat, similar to the Shark Tooth Necklace accessory.
	/// </summary>
	public ref float GetArmorPenetration(DamageClass damageClass) => ref damageData[damageClass.Type].armorPen;

	/// <summary>
	/// Gets the knockback modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// <para/> Note that knockback values are multipliers. Adding 0.12f, for example, would add 12% to the knockback stat.
	/// </summary>
	public ref StatModifier GetKnockback<T>() where T : DamageClass => ref GetKnockback(ModContent.GetInstance<T>());

	/// <summary>
	/// Gets the knockback modifier for this damage type on this player.
	/// This returns a reference, and as such, you can freely modify this method's return value with operators.
	/// <para/> Note that knockback values are multipliers. Adding 0.12f, for example, would add 12% to the knockback stat.
	/// </summary>
	public ref StatModifier GetKnockback(DamageClass damageClass) => ref damageData[damageClass.Type].knockback;

	/// <inheritdoc cref="GetTotalDamage"/>
	public StatModifier GetTotalDamage<T>() where T : DamageClass => GetTotalDamage(ModContent.GetInstance<T>());

	/// <summary>
	/// Calculates a total damage modifier for the player for the provided <see cref="DamageClass"/>.<br/>
	/// Use in conjunction with <see cref="StatModifier.ApplyTo(float)"/> to calculate a final damage value for a given <see cref="DamageClass"/> and base damage: <c>int finalDamage = (int)player.GetTotalDamage(item.DamageType).ApplyTo(30);</c>
	/// </summary>
	/// <remarks>The modifiers calculated here are important due to the possibility of
	/// damage classes inheriting modifiers from other damage classes. For instance, an attack
	/// can be classified as multiple damage types and each could have different modifiers to apply to the damage
	/// </remarks>
	/// <param name="damageClass">The <see cref="DamageClass"/> to use for total damage calculation</param>
	/// <returns>All modifiers combined</returns>
	public StatModifier GetTotalDamage(DamageClass damageClass)
	{
		StatModifier stat = damageData[damageClass.Type].damage;

		for (int i = 0; i < damageData.Length; i++) {
			if (i != damageClass.Type) {
				StatInheritanceData inheritanceData = damageClass.GetModifierInheritance(DamageClassLoader.DamageClasses[i]);
				stat = stat.CombineWith(damageData[i].damage.Scale(inheritanceData.damageInheritance));
			}
		}

		return stat;
	}

	public float GetTotalCritChance<T>() where T : DamageClass => GetTotalCritChance(ModContent.GetInstance<T>());

	public float GetTotalCritChance(DamageClass damageClass)
	{
		float stat = damageData[damageClass.Type].critChance;

		for (int i = 0; i < damageData.Length; i++) {
			if (i != damageClass.Type) {
				StatInheritanceData inheritanceData = damageClass.GetModifierInheritance(DamageClassLoader.DamageClasses[i]);
				stat += damageData[i].critChance * inheritanceData.critChanceInheritance;
			}
		}

		return stat;
	}

	public float GetTotalAttackSpeed<T>() where T : DamageClass => GetTotalAttackSpeed(ModContent.GetInstance<T>());

	public float GetTotalAttackSpeed(DamageClass damageClass)
	{
		float stat = damageData[damageClass.Type].attackSpeed;

		for (int i = 0; i < damageData.Length; i++) {
			if (i != damageClass.Type) {
				StatInheritanceData inheritanceData = damageClass.GetModifierInheritance(DamageClassLoader.DamageClasses[i]);
				stat += (damageData[i].attackSpeed - 1) * inheritanceData.attackSpeedInheritance;
			}
		}

		return stat;
	}

	public float GetTotalArmorPenetration<T>() where T : DamageClass => GetTotalArmorPenetration(ModContent.GetInstance<T>());

	public float GetTotalArmorPenetration(DamageClass damageClass)
	{
		float stat = damageData[damageClass.Type].armorPen;

		for (int i = 0; i < damageData.Length; i++) {
			if (i != damageClass.Type) {
				StatInheritanceData inheritanceData = damageClass.GetModifierInheritance(DamageClassLoader.DamageClasses[i]);
				stat += damageData[i].armorPen * inheritanceData.armorPenInheritance;
			}
		}

		return stat;
	}

	public StatModifier GetTotalKnockback<T>() where T : DamageClass => GetTotalKnockback(ModContent.GetInstance<T>());

	public StatModifier GetTotalKnockback(DamageClass damageClass)
	{
		StatModifier stat = damageData[damageClass.Type].knockback;

		for (int i = 0; i < damageData.Length; i++) {
			if (i != damageClass.Type) {
				StatInheritanceData inheritanceData = damageClass.GetModifierInheritance(DamageClassLoader.DamageClasses[i]);
				stat = stat.CombineWith(damageData[i].knockback.Scale(inheritanceData.knockbackInheritance));
			}
		}

		return stat;
	}

	public int GetWeaponArmorPenetration(Item sItem)
	{
		int armorPen = (int)(sItem.ArmorPenetration + GetTotalArmorPenetration(sItem.DamageType));
		// TODO: CombinedHooks.ModifyWeaponArmorPenetration(this, sItem, ref armorPen);
		return armorPen;
	}

	public float GetWeaponAttackSpeed(Item sItem)
	{
		float attackSpeed = GetTotalAttackSpeed(sItem.DamageType);
		// apply a scale based on the set. It's not recommended for mods to use this, but vanilla does for super fast melee weapons so here we are
		attackSpeed = 1 + ((attackSpeed - 1) * ItemID.Sets.BonusAttackSpeedMultiplier[sItem.type]);
		return attackSpeed;
	}

	// Legacy Thrower properties (uppercase+property in TML)

	/// <summary>
	/// Multiplier to shot projectile velocity before throwing. Result will be capped to 16f.
	/// <br/>Only applies to items counted as the <see cref="DamageClass.Throwing"/> damage type
	/// </summary>
	public float ThrownVelocity { get; set; }

	/// <summary>
	/// If true, player has a 33% chance of not consuming the thrown item.
	/// <br/>Only applies to consumable items and projectiles counted as the <see cref="DamageClass.Throwing"/> damage type.
	/// <br/>Projectiles spawned from a player who holds such item will set <see cref="Projectile.noDropItem"/> to prevent duplication.
	/// <br/>Stacks with <see cref="ThrownCost50"/> multiplicatively
	/// </summary>
	public bool ThrownCost33 { get; set; }

	/// <summary>
	/// If true, player has a 50% chance of not consuming the thrown item.
	/// <br/>Only applies to consumable items counted as the <see cref="DamageClass.Throwing"/> damage type.
	/// <br/>Projectiles spawned from a player who holds such item will set <see cref="Projectile.noDropItem"/> to prevent duplication.
	/// <br/>Stacks with <see cref="ThrownCost33"/> multiplicatively
	/// </summary>
	public bool ThrownCost50 { get; set; }

	/// <summary>
	/// Returns true if either <see cref="ThrownCost33"/> or <see cref="ThrownCost50"/> are true
	/// </summary>
	public bool AnyThrownCostReduction => ThrownCost33 || ThrownCost50;

	/// <summary>
	/// Container for current SceneEffect client properties such as: Backgrounds, music, and water styling
	/// </summary>
	public SceneEffectLoader.SceneEffectInstance CurrentSceneEffect { get; set; } = new SceneEffectLoader.SceneEffectInstance();

	/// <summary>
	/// Stores whether or not the player is in a modbiome using boolean bits.
	/// </summary>
	internal BitArray modBiomeFlags = new BitArray(0);

	/// <summary>
	/// Determines if the player is in specified ModBiome. This will throw exceptions on failure.
	/// </summary>
	/// <exception cref="IndexOutOfRangeException"/>
	/// <exception cref="NullReferenceException"/>
	public bool InModBiome(ModBiome baseInstance) => modBiomeFlags[baseInstance.ZeroIndexType];

	/// <inheritdoc cref="InModBiome"/>
	public bool InModBiome<T>() where T : ModBiome => InModBiome(ModContent.GetInstance<T>());

	/// <summary>
	/// The zone property storing if the player is not in any particular biome. Updated in <see cref="UpdateBiomes"/>
	/// Does NOT account for height. Please use ZoneForest / ZoneNormalX for height based derivatives.
	/// </summary>
	public bool ZonePurity { get; set; } = false;

	/// <summary>
	/// Calculates whether or not the player is in the purity/forest biome.
	/// </summary>
	public bool InZonePurity()
	{
		bool one = ZoneBeach || ZoneCorrupt || ZoneCrimson || ZoneDesert || ZoneDungeon || ZoneGemCave;
		bool two = ZoneGlowshroom || ZoneGranite || ZoneGraveyard || ZoneHallow || ZoneHive || ZoneJungle;
		bool three = ZoneLihzhardTemple || ZoneMarble || ZoneMeteor || ZoneSnow || ZoneUnderworldHeight;
		bool four = modBiomeFlags.Cast<bool>().Contains(true);
		return !(one || two || three || four);
	}

	// Convenience Zone properties for Modders

	/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneOverworldHeight</code></summary>
	public bool ZoneForest => ZonePurity && ZoneOverworldHeight;

	/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneRockLayerHeight</code></summary>
	public bool ZoneNormalCaverns => ZonePurity && ZoneRockLayerHeight;

	/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneDirtLayerHeight</code></summary>
	public bool ZoneNormalUnderground => ZonePurity && ZoneDirtLayerHeight;

	/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneSkyHeight</code></summary>
	public bool ZoneNormalSpace => ZonePurity && ZoneSkyHeight;

	/// <summary>
	/// Invoked in UpdateVisibleAccessories. Runs common code for both modded slots and vanilla slots based on provided Items.
	/// </summary>
	public void UpdateVisibleAccessories(Item item, bool invisible, int slot = -1, bool modded = false)
	{
		if (eocDash > 0 && shield == -1 && item.shieldSlot != -1) {
			shield = item.shieldSlot;
			if (cShieldFallback != -1)
				cShield = cShieldFallback;
		}

		if (shieldRaised && shield == -1 && item.shieldSlot != -1) {
			shield = item.shieldSlot;
			if (cShieldFallback != -1)
				cShield = cShieldFallback;
		}

		if (ItemIsVisuallyIncompatible(item))
			return;

		if (item.wingSlot > 0) {
			if (invisible && (velocity.Y == 0f || mount.Active))
				return;

			wings = item.wingSlot;
		}

		if (!invisible)
			UpdateVisibleAccessory(slot, item, modded);
		else
			ItemLoader.UpdateVisibleAccessory(item, this, true);
	}

	[Obsolete("Removed in 1.4.5. Use Player.TryDroppingSingleItem instead.")]
	public void DropItem(IEntitySource source, Vector2 position, ref Item item) => TryDroppingSingleItem(source, item);

	public int GetHealLife(Item item, bool quickHeal = false)
	{
		int healValue = item.healLife;
		ItemLoader.GetHealLife(item, this, quickHeal, ref healValue);
		PlayerLoader.GetHealLife(this, item, quickHeal, ref healValue);
		return healValue > 0 ? healValue : 0;
	}

	public int GetHealMana(Item item, bool quickHeal = false)
	{
		int healValue = item.healMana;
		ItemLoader.GetHealMana(item, this, quickHeal, ref healValue);
		PlayerLoader.GetHealMana(this, item, quickHeal, ref healValue);
		return healValue > 0 ? healValue : 0;
	}

	/// <summary>
	/// Calculates the mana needed to use the given item.
	/// </summary>
	/// <param name="item">The item to check.</param>
	/// <returns>The amount of mana needed to use <paramref name="item"/>. Cannot be less than <c>0</c>.</returns>
	public int GetManaCost(Item item)
	{
		float reduce = manaCost;
		float mult = 1;

		if (spaceGun && ItemID.Sets.IsSpaceGun[item.type])
			mult = 0;

		if(item.type == ItemID.BookStaff && altFunctionUse == 2)
			mult = 2;

		CombinedHooks.ModifyManaCost(this, item, ref reduce, ref mult);
		int mana = (int)(item.mana * reduce * mult);
		return mana >= 0 ? mana : 0;
	}

	/// <summary>
	/// Determines if this player has enough mana to use an item.
	/// <br/> If the player doesn't have enough mana and <paramref name="blockQuickMana"/> is <see langword="false"/>, the player will activate any missing mana effects they have and try again.
	/// <br/> The <paramref name="pay"/> parameter can be used to consume the mana amount.
	/// </summary>
	/// <param name="item">The item to use.</param>
	/// <param name="amount">The amount of mana needed. If <c>-1</c>, calculate using <see cref="GetManaCost(Item)"/>.</param>
	/// <param name="pay">If <see langword="true"/>, actually use the mana requested.</param>
	/// <param name="blockQuickMana">If <see langword="true"/>, prevent on missing mana effects like the Mana Flower from activating if the player doesn't have enough mana.</param>
	/// <returns><see langword="true"/> if the player has enough mana to use the item, <see langword="false"/> otherwise.</returns>
	public bool CheckMana(Item item, int amount = -1, bool pay = false, bool blockQuickMana = false)
	{
		if (amount <= -1)
			amount = GetManaCost(item);

		if (statMana >= amount) {
			if (pay) {
				CombinedHooks.OnConsumeMana(this, item, amount);
				statMana -= amount;
			}

			return true;
		}

		if (blockQuickMana)
			return false;

		CombinedHooks.OnMissingMana(this, item, amount);
		if (statMana < amount && manaFlower)
			QuickMana();

		if (statMana >= amount) {
			if (pay) {
				CombinedHooks.OnConsumeMana(this, item, amount);
				statMana -= amount;
			}

			return true;
		}

		return false;

	}

	/// <summary>
	/// Returns true if an item animation is currently running.
	/// </summary>
	public bool ItemAnimationActive => itemAnimation > 0;

	/// <summary>
	/// Returns true if the item animation is on or after its last frame. Meaning it could (if the player clicks etc) start again next frame. <br/>
	/// Vanilla uses it to despawn spears, but it's not recommended because it will desync in multiplayer <br/>
	/// (a remote player could get the packet for a new projectile just as they're finishing a swing). <br/>
	/// It is recommended to use ai counters for the lifetime of animation bound projectiles instead.
	/// </summary>
	public bool ItemAnimationEndingOrEnded => itemAnimation <= 1;

	/// <summary>
	/// The number of times the item has been used/fired this animation (swing)
	/// </summary>
	public int ItemUsesThisAnimation { get; private set; }

	/// <summary>
	/// Adds to either Player.immuneTime or Player.hurtCooldowns based on the cooldownCounterId
	/// </summary>
	/// <param name="cooldownCounterId">See <see cref="ImmunityCooldownID"/> for valid ids.</param>
	/// <param name="immuneTime">Extra immunity time to add</param>
	public void AddImmuneTime(int cooldownCounterId, int immuneTime)
	{
		if (cooldownCounterId < 0) {
			this.immuneTime += immuneTime;
		}
		else {
			hurtCooldowns[cooldownCounterId] += immuneTime;
		}
	}

	// Extra jumps
	private ExtraJumpState[] extraJumps = new ExtraJumpState[ExtraJumpLoader.ExtraJumpCount];

	public ref ExtraJumpState GetJumpState<T>(T baseInstance) where T : ExtraJump => ref extraJumps[baseInstance.Type];

	public ref ExtraJumpState GetJumpState<T>() where T : ExtraJump => ref GetJumpState(ModContent.GetInstance<T>());

	public Span<ExtraJumpState> ExtraJumps => extraJumps.AsSpan();

	/// <summary>
	/// When <see langword="true"/>, all extra jumps will be blocked, including Flipper usage.<br/>
	/// Setting this field to <see langword="true"/> will not stop any currently active extra jumps.
	/// </summary>
	public bool blockExtraJumps;

	/// <summary>
	/// Returns <see langword="true"/> if any extra jump is <see cref="ExtraJumpState.Available"/> and <see cref="ExtraJump.CanStart"/>.<br/>
	/// Setting <see cref="blockExtraJumps"/> will cause this method to return <see langword="false"/> instead.
	/// </summary>
	public bool AnyExtraJumpUsable()
	{
		if (blockExtraJumps)
			return false;

		foreach (ExtraJump jump in ExtraJumpLoader.OrderedJumps) {
			if (GetJumpState(jump).Available && jump.CanStart(this) && PlayerLoader.CanStartExtraJump(jump, this))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Cancels any extra jump in progress.<br/>
	/// Sets all <see cref="ExtraJumpState.Active"/> flags to <see langword="false"/> and calls OnExtraJumpEnded hooks.<br/>
	/// Also sets <see cref="jump"/> to 0 if a an extra jump was active.<br/><br/>
	///
	/// Used by vanilla when performing an action which would cancel jumping, such as grappling, grabbing a rope or getting frozen.<br/><br/>
	///
	/// To prevent the use of remaining jumps, use <see cref="ConsumeAllExtraJumps"/> or <see cref="blockExtraJumps"/>.<br/>
	/// To cancel a regular jump as well, do <c>Player.jump = 0;</c>
	/// </summary>
	public void StopExtraJumpInProgress()
	{
		ExtraJumpLoader.StopActiveJump(this, out bool anyJumpCancelled);

		if (anyJumpCancelled)
			jump = 0;
	}

	/// <summary>
	/// Checks if the player has any item in their <see cref="inventory"/> that appears in the provided Item ID set (<paramref name="itemSet"/>).
	/// <br/><br/> For example <c>if (player.HasItem(ItemID.Sets.Glowsticks))</c> would return true if the player has any glowstick item.
	/// <br/><br/> Does not check Void Bag.
	/// </summary>
	/// <param name="itemSet">A set of length <see cref="ItemLoader.ItemCount"/> to check against</param>
	/// <returns>True if the player has such an item</returns>
	public bool HasItem(bool[] itemSet)
	{
		for (int i = 0; i < 58; i++) {
			if (itemSet[inventory[i].type] && inventory[i].stack > 0)
				return true;
		}

		return false;
	}
}
