using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to modify and use hooks for all items, both vanilla and modded.
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// </summary>
public abstract class GlobalItem : GlobalType<Item, GlobalItem>
{
	protected override void ValidateType()
	{
		base.ValidateType();

		LoaderUtils.MustOverrideTogether(this, g => g.SaveData, g => g.LoadData);
		LoaderUtils.MustOverrideTogether(this, g => g.NetSend, g => g.NetReceive);

		if (!IsCloneable)
			Cloning.WarnNotCloneable(GetType());
	}

	protected sealed override void Register()
	{
		base.Register();
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Called when the <paramref name="item"/> is created. The <paramref name="context"/> parameter indicates the context of the item creation and can be used in logic for the desired effect.
	/// <para/> Called on the local client only.
	/// <para/> Known <see cref="ItemCreationContext"/> include: <see cref="InitializationItemCreationContext"/>, <see cref="BuyItemCreationContext"/>, <see cref="JourneyDuplicationItemCreationContext"/>, and <see cref="RecipeItemCreationContext"/>. Some of these provide additional context such as how <see cref="RecipeItemCreationContext"/> includes the items consumed to craft the <paramref name="item"/>.
	/// </summary>
	public virtual void OnCreated(Item item, ItemCreationContext context)
	{
	}

	/// <summary>
	/// Gets called when any item spawns in world
	/// <para/> Called on the local client or the server where Item.NewItem is called.
	/// </summary>
	public virtual void OnSpawn(Item item, IEntitySource source)
	{
	}

	/// <summary>
	/// Allows you to manually choose what prefix an item will get.
	/// </summary>
	/// <returns>The ID of the prefix to give the item, -1 to use default vanilla behavior</returns>
	public virtual int ChoosePrefix(Item item, UnifiedRandom rand) => -1;

	/// <summary>
	/// To prevent putting the item in the tinkerer slot, return false when pre is -3.
	/// To prevent rolling of a prefix on spawn, return false when pre is -1.
	/// To force rolling of a prefix on spawn, return true when pre is -1.
	///
	/// To reduce the probability of a prefix on spawn (pre == -1) to X%, return false 100-4X % of the time.
	/// To increase the probability of a prefix on spawn (pre == -1) to X%, return true (4X-100)/3 % of the time.
	///
	/// To delete a prefix from an item when the item is loaded, return false when pre is the prefix you want to delete.
	/// Use AllowPrefix to prevent rolling of a certain prefix.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="pre">The prefix being applied to the item, or the roll mode. -1 is when the item is naturally generated in a chest, crafted, purchased from an NPC, looted from a grab bag (excluding presents), or dropped by a slain enemy (if it's spawned with prefixGiven: -1). -2 is when the item is rolled in the tinkerer. -3 determines if the item can be placed in the tinkerer slot.</param>
	/// <param name="rand"></param>
	/// <returns></returns>
	public virtual bool? PrefixChance(Item item, int pre, UnifiedRandom rand) => null;

	/// <summary>
	/// Force a re-roll of a prefix by returning false.
	/// </summary>
	public virtual bool AllowPrefix(Item item, int pre) => true;

	/// <summary>
	/// Returns whether or not any item can be used. Returns true by default. The inability to use a specific item overrides this, so use this to stop an item from being used.
	/// <para/> Called on local, server, and remote clients.
	/// <br/><br/> The item may or not be used after this method is called, so logic in this method should have no side effects such as consuming items or resources.
	/// </summary>
	public virtual bool CanUseItem(Item item, Player player)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the autoswing (auto-reuse) behavior of any item without having to mess with Item.autoReuse.
	/// <para/> Useful to create effects like the Feral Claws which makes melee weapons and whips auto-reusable.
	/// <para/> Return true to enable autoswing (if not already enabled through autoReuse), return false to prevent autoswing. Returns null by default, which applies vanilla behavior.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item"> The item. </param>
	/// <param name="player"> The player. </param>
	public virtual bool? CanAutoReuseItem(Item item, Player player) => null;

	/// <summary>
	/// Allows you to modify the location and rotation of any item in its use animation.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item"> The item. </param>
	/// <param name="player"> The player. </param>
	/// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
	public virtual void UseStyle(Item item, Player player, Rectangle heldItemFrame) { }

	/// <summary>
	/// Allows you to modify the location and rotation of the item the player is currently holding.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item"> The item. </param>
	/// <param name="player"> The player. </param>
	/// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
	public virtual void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { }

	/// <summary>
	/// Allows you to make things happen when the player is holding an item (for example, torches make light and water candles increase spawn rate).
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void HoldItem(Item item, Player player)
	{
	}

	/// <summary>
	/// Allows you to change the effective useTime of an item.
	/// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <returns> The multiplier on the usage time. 1f by default. Values greater than 1 increase the item use's length. </returns>
	public virtual float UseTimeMultiplier(Item item, Player player) => 1f;

	/// <summary>
	/// Allows you to change the effective useAnimation of an item.
	/// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <returns>The multiplier on the animation time. 1f by default. Values greater than 1 increase the item animation's length. </returns>
	public virtual float UseAnimationMultiplier(Item item, Player player) => 1f;

	/// <summary>
	/// Allows you to safely change both useTime and useAnimation while keeping the values relative to each other.
	/// <para/> Useful for status effects.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <returns> The multiplier on the use speed. 1f by default. Values greater than 1 increase the overall item speed. </returns>
	public virtual float UseSpeedMultiplier(Item item, Player player) => 1f;

	/// <summary>
	/// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healLife"/> value.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
	/// <param name="healValue">The amount of life being healed.</param>
	public virtual void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
	{
	}

	/// <summary>
	/// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healMana"/> value.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
	/// <param name="healValue">The amount of mana being healed.</param>
	public virtual void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
	{
	}

	/// <summary>
	/// Allows you to temporarily modify the amount of mana an item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
	/// <para/> <b>Do not</b> modify <see cref="Item.mana"/>, modify the <paramref name="reduce"/> and <paramref name="mult"/> parameters.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
	/// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
	public virtual void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen when a player doesn't have enough mana for an item they are trying to use.
	/// If the player has high enough mana after this hook runs, mana consumption will happen normally.
	/// Only runs once per item use.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="neededMana">The mana needed to use the item.</param>
	public virtual void OnMissingMana(Item item, Player player, int neededMana)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen when a player consumes mana on use of an item.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="manaConsumed">The mana consumed from the player.</param>
	public virtual void OnConsumeMana(Item item, Player player, int manaConsumed)
	{
	}

	/// <summary>
	/// Allows you to dynamically modify the potion delay applied by a specific healing potion. <paramref name="baseDelay"/> and <see cref="Player.PotionDelayModifier"/> will be used to calculate a final potion delay, provided in <see cref="ApplyPotionDelay"/>.
	/// <br/><br/> Modders can modify <see cref="Player.PotionDelayModifier"/> directly in hooks like <see cref="ModItem.UpdateAccessory(Player, bool)"/> rather than use this hook to adjust potion delay times for all items.
	/// </summary>
	/// <param name="item">The healing item being used.</param>
	/// <param name="player">The player consuming the item.</param>
	/// <param name="baseDelay">The base potion delay length, in ticks.</param>
	public virtual void ModifyPotionDelay(Item item, Player player, ref int baseDelay)
	{
	}

	/// <summary>
	/// Called before the potion delay is applied to the player after consuming a healing potion.
	/// <br/><br/> Return false to prevent application of the <see cref="BuffID.PotionSickness"/> buff and setting <see cref="Player.potionDelay"/>.
	/// </summary>
	/// <param name="item">The healing item being used.</param>
	/// <param name="player">The player consuming the item.</param>
	/// <param name="potionDelay">The calculated potion delay.</param>
	public virtual bool ApplyPotionDelay(Item item, Player player, int potionDelay)
	{
		return true;
	}

	/// <summary>
	/// Allows you to dynamically modify a weapon's damage based on player and item conditions.
	/// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
	/// <para/> <b>Do not</b> modify <see cref="Item.damage"/>, modify the <paramref name="damage"/> parameter.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="damage">The StatModifier object representing the totality of the various modifiers to be applied to the item's base damage.</param>
	public virtual void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
	{
	}

	/// <summary>
	/// Allows you to set an item's sorting group in Journey Mode's duplication menu. This is useful for setting custom item types that group well together, or whenever the default vanilla sorting doesn't sort the way you want it.
	/// <para/> Note that this affects the order of the item in the listing, not which filters the item satisfies.
	/// </summary>
	/// <param name="item">The item being used</param>
	/// <param name="itemGroup">The item group this item is being assigned to</param>
	public virtual void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
	{
	}

	/// <summary>
	/// Allows you to choose if a given bait will be consumed by a given player
	/// Not consuming will always take priority over forced consumption
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="bait">The bait being used</param>
	/// <param name="player">The player using the item</param>
	public virtual bool? CanConsumeBait(Player player, Item bait)
	{
		return null;
	}

	/// <summary>
	/// Allows you to prevent an item from being researched by returning false. True is the default behavior.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item being researched</param>
	public virtual bool CanResearch(Item item)
	{
		return true;
	}

	/// <summary>
	/// Allows you to create custom behavior when an item is accepted by the Research function
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item being researched</param>
	/// <param name="fullyResearched">True if the item was completely researched, and is ready to be duplicated, false if only partially researched.</param>
	public virtual void OnResearched(Item item, bool fullyResearched)
	{
	}

	/// <summary>
	/// Allows you to dynamically modify a weapon's knockback based on player and item conditions.
	/// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
	/// <para/> <b>Do not</b> modify <see cref="Item.knockBack"/>, modify the <paramref name="knockback"/> parameter.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="knockback">The StatModifier object representing the totality of the various modifiers to be applied to the item's base knockback.</param>
	public virtual void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
	{
	}

	/// <summary>
	/// Allows you to dynamically modify a weapon's crit chance based on player and item conditions.
	/// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
	/// <para/> <b>Do not</b> modify <see cref="Item.crit"/>, modify the <paramref name="crit"/> parameter.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="crit">The total crit chance of the item after all normal crit chance calculations.</param>
	public virtual void ModifyWeaponCrit(Item item, Player player, ref float crit)
	{
	}

	/// <summary>
	/// Whether or not having no ammo prevents an item that uses ammo from shooting.
	/// Return false to allow shooting with no ammo in the inventory, in which case the item will act as if the default ammo for it is being used.
	/// Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual bool NeedsAmmo(Item item, Player player)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify various properties of the projectile created by a weapon based on the ammo it is using.
	/// <para/> Called on local and remote clients when a player picking ammo but only on the local client when held projectiles are picking ammo.
	/// </summary>
	/// <param name="weapon">The item that is using the given ammo.</param>
	/// <param name="ammo">The ammo item being used by the given weapon.</param>
	/// <param name="player">The player using the item.</param>
	/// <param name="type">The ID of the fired projectile.</param>
	/// <param name="speed">The speed of the fired projectile.</param>
	/// <param name="damage">
	/// The damage modifier for the projectile.<br></br>
	/// Total weapon damage is included as Flat damage.<br></br>
	/// Be careful not to apply flat or base damage bonuses which are already applied to the weapon.
	/// </param>
	/// <param name="knockback">The knockback of the fired projectile.</param>
	public virtual void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
	{
	}

	/// <summary>
	/// Whether or not the given ammo item is valid for the given weapon; called on the weapon. If this, or <see cref="CanBeChosenAsAmmo"/> on the ammo, returns false, then the ammo will not be valid for this weapon.
	/// <para/> By default, returns null and allows <see cref="Item.useAmmo"/> and <see cref="Item.ammo"/> to decide. Return true to make the ammo valid regardless of these fields, and return false to make it invalid.
	/// <para/> If false is returned, the <see cref="CanConsumeAmmo"/>, <see cref="CanBeConsumedAsAmmo"/>, <see cref="OnConsumeAmmo"/>, and <see cref="OnConsumedAsAmmo"/> hooks are never called.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="weapon">The weapon that this hook is being called for.</param>
	/// <param name="ammo">The ammo that the weapon is attempting to select.</param>
	/// <param name="player">The player which this weapon and the potential ammo belong to.</param>
	/// <returns></returns>
	public virtual bool? CanChooseAmmo(Item weapon, Item ammo, Player player)
	{
		return null;
	}

	/// <summary>
	/// Whether or not the given ammo item is valid for the given weapon; called on the ammo. If this, or <see cref="CanChooseAmmo"/> on the weapon, returns false, then the ammo will not be valid for this weapon.
	/// <para/> By default, returns null and allows <see cref="Item.useAmmo"/> and <see cref="Item.ammo"/> to decide. Return true to make the ammo valid regardless of these fields, and return false to make it invalid.
	/// <para/> If false is returned, the <see cref="CanConsumeAmmo"/>, <see cref="CanBeConsumedAsAmmo"/>, <see cref="OnConsumeAmmo"/>, and <see cref="OnConsumedAsAmmo"/> hooks are never called.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="ammo">The ammo that this hook is being called for.</param>
	/// <param name="weapon">The weapon attempting to select the ammo.</param>
	/// <param name="player">The player which the weapon and this potential ammo belong to.</param>
	/// <returns></returns>
	public virtual bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player)
	{
		return null;
	}

	/// <summary>
	/// Whether or not the given ammo item will be consumed; called on the weapon.
	/// <para/> By default, returns true; return false to prevent ammo consumption.
	/// <para/> If false is returned, the <see cref="OnConsumeAmmo"/> and <see cref="OnConsumedAsAmmo"/> hooks are never called.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="weapon">The weapon that this hook is being called for.</param>
	/// <param name="ammo">The ammo that the weapon is attempting to consume.</param>
	/// <param name="player">The player which this weapon and the ammo belong to.</param>
	/// <returns></returns>
	public virtual bool CanConsumeAmmo(Item weapon, Item ammo, Player player)
	{
		return true;
	}

	/// <summary>
	/// Whether or not the given ammo item will be consumed; called on the ammo.
	/// <para/> By default, returns true; return false to prevent ammo consumption.
	/// <para/> If false is returned, the <see cref="OnConsumeAmmo"/> and <see cref="OnConsumedAsAmmo"/> hooks are never called.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="ammo">The ammo that this hook is being called for.</param>
	/// <param name="weapon">The weapon attempting to consume the ammo.</param>
	/// <param name="player">The player which the weapon and this ammo belong to.</param>
	/// <returns></returns>
	public virtual bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make things happen when the given ammo is consumed by the given weapon. Called by the weapon.
	/// <para/> Called before the ammo stack is reduced, and is never called if the ammo isn't consumed in the first place.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="weapon">The currently-active weapon.</param>
	/// <param name="ammo">The ammo that the given weapon is currently using.</param>
	/// <param name="player">The player which the given weapon and the given ammo belong to.</param>
	public virtual void OnConsumeAmmo(Item weapon, Item ammo, Player player)
	{
	}

	/// <summary>
	/// Allows you to make things happen when the given ammo is consumed by the given weapon. Called by the ammo.
	/// <para/> Called before the ammo stack is reduced, and is never called if the ammo isn't consumed in the first place.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="ammo">The currently-active ammo.</param>
	/// <param name="weapon">The weapon that is currently using the given ammo.</param>
	/// <param name="player">The player which the given weapon and the given ammo belong to.</param>
	public virtual void OnConsumedAsAmmo(Item ammo, Item weapon, Player player)
	{
	}

	/// <summary>
	/// Allows you to prevent an item from shooting a projectile on use. Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item"> The item being used. </param>
	/// <param name="player"> The player using the item. </param>
	/// <returns></returns>
	public virtual bool CanShoot(Item item, Player player)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by an item.
	/// <para/> These parameters will be provided to <see cref="Shoot(Item, Player, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float)"/> where the projectile will actually be spawned.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item"> The item being used. </param>
	/// <param name="player"> The player using the item. </param>
	/// <param name="position"> The center position of the projectile. </param>
	/// <param name="velocity"> The velocity of the projectile. </param>
	/// <param name="type"> The ID of the projectile. </param>
	/// <param name="damage"> The damage of the projectile. </param>
	/// <param name="knockback"> The knockback of the projectile. </param>
	public virtual void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
	}

	/// <summary>
	/// Allows you to modify an item's shooting mechanism. Return false to prevent vanilla's shooting code from running. Returns true by default.
	/// <para/> This method is called after the <see cref="ModifyShootStats"/> hook has had a chance to adjust the spawn parameters.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item"> The item being used. </param>
	/// <param name="player"> The player using the item. </param>
	/// <param name="source"> The projectile source's information. </param>
	/// <param name="position"> The center position of the projectile. </param>
	/// <param name="velocity"> The velocity of the projectile. </param>
	/// <param name="type"> The ID of the projectile. </param>
	/// <param name="damage"> The damage of the projectile. </param>
	/// <param name="knockback"> The knockback of the projectile. </param>
	public virtual bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		return true;
	}

	/// <summary>
	/// Changes the hitbox of a melee weapon when it is used.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
	{
	}

	/// <summary>
	/// Allows you to give melee weapons special effects, such as creating light or dust.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void MeleeEffects(Item item, Player player, Rectangle hitbox)
	{
	}

	/// <summary>
	/// Allows you to determine whether the given item can catch the given NPC.
	/// <para/> Return true or false to say the given NPC can or cannot be caught, respectively, regardless of vanilla rules.
	/// <para/> Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.
	/// <para/> If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.
	/// <para/> NOTE: this does not classify the given item as an NPC-catching tool, which is necessary for catching NPCs in the first place. To do that, you will need to use <see cref="ItemID.Sets.CatchingTool"/>.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item with which the player is trying to catch the target NPC.</param>
	/// <param name="target">The NPC the player is trying to catch.</param>
	/// <param name="player">The player attempting to catch the NPC.</param>
	/// <returns></returns>
	public virtual bool? CanCatchNPC(Item item, NPC target, Player player)
	{
		return null;
	}

	/// <summary>
	/// Allows you to make things happen when the given item attempts to catch the given NPC.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item used to catch the given NPC.</param>
	/// <param name="npc">The NPC which the player attempted to catch.</param>
	/// <param name="player">The player attempting to catch the given NPC.</param>
	/// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
	public virtual void OnCatchNPC(Item item, NPC npc, Player player, bool failed)
	{
	}

	/// <summary>
	/// Allows you to dynamically modify the given item's size for the given player, similarly to the effect of the Titan Glove.
	/// <para/> <b>Do not</b> modify <see cref="Item.scale"/>, modify the <paramref name="scale"/> parameter.
	/// <para/> Called on local and remote clients
	/// </summary>
	/// <param name="item">The item to modify the scale of.</param>
	/// <param name="player">The player wielding the given item.</param>
	/// <param name="scale">
	/// The scale multiplier to be applied to the given item.<br></br>
	/// Will be 1.1 if the Titan Glove is equipped, and 1 otherwise.
	/// </param>
	public virtual void ModifyItemScale(Item item, Player player, ref float scale)
	{
	}

	/// <summary>
	/// Allows you to determine whether a melee weapon can hit the given NPC when swung. Return true to allow hitting the target, return false to block the weapon from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	public virtual bool? CanHitNPC(Item item, Player player, NPC target)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine whether a melee weapon can collide with the given NPC when swung.
	/// <para/> Use <see cref="CanHitNPC(Item, Player, NPC)"/> instead for Flymeal-type effects.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="item">The weapon item the player is holding.</param>
	/// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
	/// <param name="player">The player wielding this item.</param>
	/// <param name="target">The target npc.</param>
	/// <returns>
	/// Return true to allow colliding with target, return false to block the weapon from colliding with target, and return null to use the vanilla code for whether the target can be colliding. Returns null by default.
	/// </returns>
	public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, Player player, NPC target)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that a melee weapon does to an NPC.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	public virtual void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when a melee weapon hits an NPC (for example how the Pumpkin Sword creates pumpkin heads).
	/// <para/> Called on the client hitting the target.
	/// </summary>
	public virtual void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to determine whether a melee weapon can hit the given opponent player when swung. Return false to block the weapon from hitting the target. Returns true by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	public virtual bool CanHitPvp(Item item, Player player, Player target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, etc., that a melee weapon does to a player.
	/// <para/> Called on the client taking damage.
	/// </summary>
	public virtual void ModifyHitPvp(Item item, Player player, Player target, ref Player.HurtModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when a melee weapon hits a player.
	/// <para/> Called on the client taking damage.
	/// </summary>
	public virtual void OnHitPvp(Item item, Player player, Player target, Player.HurtInfo hurtInfo)
	{
	}

	/// <summary>
	/// Allows you to make things happen when an item is used. The return value controls whether or not ApplyItemTime will be called for the player.
	/// <para/> Return true if the item actually did something, to force itemTime.
	/// <para/> Return false to keep itemTime at 0.
	/// <para/> Return null for vanilla behavior.
	/// <para/> Called on local, server, and remote clients.
	/// <br/><br/> Note the for right-click actions, this is currently only called on the local client.
	/// </summary>
	public virtual bool? UseItem(Item item, Player player) => null;

	/// <summary>
	/// Allows you to make things happen when an item's use animation starts.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UseAnimation(Item item, Player player) { }

	/// <summary>
	/// If the item is consumable and this returns true, then the item will be consumed upon usage. Returns true by default.
	/// If false is returned, the OnConsumeItem hook is never called.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual bool ConsumeItem(Item item, Player player)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make things happen when this item is consumed.
	/// Called before the item stack is reduced.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void OnConsumeItem(Item item, Player player)
	{
	}

	/// <summary>
	/// Allows you to modify the player's animation when an item is being used.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UseItemFrame(Item item, Player player) { }

	/// <summary>
	/// Allows you to modify the player's animation when the player is holding an item.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void HoldItemFrame(Item item, Player player) { }

	/// <inheritdoc cref="ModItem.AltFunctionUse(Player)"/>
	public virtual bool AltFunctionUse(Item item, Player player)
	{
		return false;
	}

	/// <summary>
	/// Allows you to make things happen when an item is in the player's inventory. This should NOT be used for information accessories;
	/// use <seealso cref="UpdateInfoAccessory"/> for those instead.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateInventory(Item item, Player player)
	{
	}

	/// <summary>
	/// Allows you to set information accessory fields with the passed in player argument. This hook should only be used for information
	/// accessory fields such as the Radar, Lifeform Analyzer, and others. Using it for other fields will likely cause weird side-effects.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateInfoAccessory(Item item, Player player) { }

	/// <summary>
	/// Allows you to give effects to armors and accessories, such as increased damage.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateEquip(Item item, Player player)
	{
	}

	/// <summary>
	/// Allows you to give effects to accessories. The hideVisual parameter is whether the player has marked the accessory slot to be hidden from being drawn on the player.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateAccessory(Item item, Player player, bool hideVisual)
	{
	}

	/// <summary>
	/// Allows you to give effects to this accessory when equipped in a vanity slot. Vanilla uses this for boot effects, wings and merman/werewolf visual flags
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateVanity(Item item, Player player)
	{
	}

	/// <inheritdoc cref="ModItem.UpdateVisibleAccessory(Player, bool)"/>
	public virtual void UpdateVisibleAccessory(Item item, Player player, bool hideVisual)
	{
	}

	/// <inheritdoc cref="ModItem.UpdateItemDye(Player, int, bool)"/>
	public virtual void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
	{
	}

	/// <summary>
	/// Allows you to determine whether the player is wearing an armor set, and return a name for this set. If there is no armor set, return the empty string. Returns the empty string by default.
	/// <br/><br/> This return value should then be checked for in <see cref="UpdateArmorSet(Player, string)"/> to provide the actual effects of the armor set.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual string IsArmorSet(Item head, Item body, Item legs)
	{
		return "";
	}

	/// <summary>
	/// Allows you to give set bonuses to your armor set with the given name.
	/// The set name will be the same as returned by <see cref="IsArmorSet(Item, Item, Item)"/>.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateArmorSet(Player player, string set)
	{
	}

	/// <summary>
	/// Returns whether or not the head armor, body armor, and leg armor textures make up a set.
	/// This hook is used for the <see cref="GlobalItem.PreUpdateVanitySet(Player, string)"/>, <see cref="GlobalItem.UpdateVanitySet(Player, string)"/>, and <see cref="GlobalItem.ArmorSetShadows(Player, string)"/> hooks, and will use items in the social slots if they exist.
	/// By default this will return the same value as the <see cref="IsArmorSet(Item, Item, Item)"/> hook, so you will not have to use this hook unless you want vanity effects to be entirely separate from armor sets.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual string IsVanitySet(int head, int body, int legs)
	{
		int headItemType = 0;
		if (head >= 0)
			headItemType = Item.headType[head];

		Item headItem = ContentSamples.ItemsByType[headItemType];

		int bodyItemType = 0;
		if (body >= 0)
			bodyItemType = Item.bodyType[body];

		Item bodyItem = ContentSamples.ItemsByType[bodyItemType];

		int legsItemType = 0;
		if (legs >= 0)
			legsItemType = Item.legType[legs];

		Item legItem = ContentSamples.ItemsByType[legsItemType];

		return IsArmorSet(headItem, bodyItem, legItem);
	}

	/// <summary>
	/// Allows you to create special effects (such as the necro armor's hurt noise) when the player wears the vanity set with the given name returned by IsVanitySet.
	/// This hook is called regardless of whether the player is frozen in any way.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PreUpdateVanitySet(Player player, string set)
	{
	}

	/// <summary>
	/// Allows you to create special effects (such as dust) when the player wears the vanity set with the given name returned by IsVanitySet. This hook will only be called if the player is not frozen in any way.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateVanitySet(Player player, string set)
	{
	}

	/// <summary>
	/// Allows you to determine special visual effects a vanity has on the player without having to code them yourself.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// <example><code>player.armorEffectDrawShadow = true;</code></example>
	/// </summary>
	public virtual void ArmorSetShadows(Player player, string set)
	{
	}

	/// <summary>
	/// Allows you to modify the equipment that the player appears to be wearing.
	/// <para/> Note that type and equipSlot are not the same as the item type of the armor the player will appear to be wearing. Worn equipment has a separate set of IDs.
	/// <para/> You can find the vanilla equipment IDs by looking at the headSlot, bodySlot, and legSlot fields for items, and modded equipment IDs by looking at EquipLoader.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="armorSlot">head armor (0), body armor (1) or leg armor (2).</param>
	/// <param name="type">The equipment texture ID of the item that the player is wearing.</param>
	/// <param name="male">True if the player is male.</param>
	/// <param name="equipSlot">The altered equipment texture ID for the legs (armorSlot 1 and 2) or head (armorSlot 0)</param>
	/// <param name="robes">Set to true if you modify equipSlot when armorSlot == 1 to set Player.wearsRobe, otherwise ignore it</param>
	public virtual void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes)
	{
	}

	/// <summary>
	/// Returns whether or not an item does something when right-clicked in the inventory. Returns false by default.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual bool CanRightClick(Item item)
	{
		return false;
	}

	/// <summary>
	/// Allows you to make things happen when this item is right-clicked in the inventory. By default this will consume the item by 1 stack, so return false in <see cref="ConsumeItem(Item, Player)"/> if that behavior is undesired.
	/// <para/> This is only called if the item can be right-clicked, meaning <see cref="ItemID.Sets.OpenableBag"/> is true for the item type or <see cref="GlobalItem.CanRightClick"/> returns true.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void RightClick(Item item, Player player)
	{
	}

	/// <summary>
	/// Allows you to add and modify the loot items that spawn from bag items when opened.
	/// <para/> The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use the <see cref="ModNPC.ModifyNPCLoot(NPCLoot)"/> hook to modify NPC loot as well as this hook. A common usage is to use this hook and <see cref="ModNPC.ModifyNPCLoot(NPCLoot)"/> to edit non-expert exclusive drops for bosses.
	/// <para/> This hook only runs once per item type during mod loading, any dynamic behavior must be contained in the rules themselves.
	/// <para/> This hook is not instanced.
	/// </summary>
	/// <param name="item">A default item of the type being opened, not the actual item instance</param>
	/// <param name="itemLoot">A reference to the item drop database for this item type</param>
	public virtual void ModifyItemLoot(Item item, ItemLoot itemLoot)
	{
	}

	/// <summary>
	/// Allows you to prevent items from stacking.
	/// <para/>This is only called when two items of the same type attempt to stack.
	/// <para/>This is usually not called for coins and ammo in the inventory/UI.
	/// <para/>This covers all scenarios, if you just need to change in-world stacking behavior, use <see cref="CanStackInWorld"/>.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <returns>Whether or not the items are allowed to stack</returns>
	public virtual bool CanStack(Item destination, Item source)
	{
		return true;
	}

	/// <summary>
	/// Allows you to prevent items from stacking in the world.
	/// <para/> This is only called when two items of the same type attempt to stack.
	/// <para/> Called on the local client or server, depending on who the item is reserved for.
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <returns>Whether or not the items are allowed to stack</returns>
	public virtual bool CanStackInWorld(Item destination, Item source)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make things happen when items stack together.
	/// <para/> This hook is called before the items are transferred from <paramref name="source"/> to <paramref name="destination"/>
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="destination">The item instance that <paramref name="source"/> will attempt to stack onto</param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <param name="numToTransfer">The quantity of <paramref name="source"/> that will be transferred to <paramref name="destination"/></param>
	public virtual void OnStack(Item destination, Item source, int numToTransfer)
	{
	}

	/// <summary>
	/// Allows you to make things happen when an item stack is split. This hook is called before the stack values are modified.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="destination">
	/// The item instance that <paramref name="source"/> will transfer items to, and is usually a clone of <paramref name="source"/>.<br/>
	/// This parameter's stack will always be zero.
	/// </param>
	/// <param name="source">The item instance being stacked onto <paramref name="destination"/></param>
	/// <param name="numToTransfer">The quantity of <paramref name="source"/> that will be transferred to to <paramref name="destination"/></param>
	public virtual void SplitStack(Item destination, Item source, int numToTransfer)
	{
	}

	/// <summary>
	/// Returns if the normal reforge pricing is applied.
	/// If true or false is returned and the price is altered, the price will equal the altered price.
	/// The passed reforge price equals the item.value. Vanilla pricing will apply 20% discount if applicable and then price the reforge at a third of that value.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
	{
		return true;
	}

	/// <summary>
	/// This hook gets called when the player clicks on the reforge button and can afford the reforge.
	/// Returns whether the reforge will take place. If false is returned by the ModItem or any GlobalItem, the item will not be reforged, the cost to reforge will not be paid, and PreReforge and PostReforge hooks will not be called.
	/// Reforging preserves modded data on the item.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual bool CanReforge(Item item)
	{
		return true;
	}

	/// <summary>
	/// This hook gets called immediately before an item gets reforged by the Goblin Tinkerer.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void PreReforge(Item item)
	{
	}

	/// <summary>
	/// This hook gets called immediately after an item gets reforged by the Goblin Tinkerer.
	/// Useful for modifying modded data based on the reforge result.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void PostReforge(Item item)
	{
	}

	/// <summary>
	/// Allows you to modify the colors in which the player's armor and their surrounding accessories are drawn, in addition to which glow mask and in what color is drawn.
	/// <para/> This method is not instanced.
	/// <para/> Called on local and remote clients.
	/// </summary>
	public virtual void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color,
		ref int glowMask, ref Color glowMaskColor)
	{
	}

	/// <summary>
	/// Allows you to modify which glow mask and in what color is drawn on the player's arms. Note that this is only called for body armor.
	/// <para/> This method is not instanced.
	/// <para/> Called on local and remote clients.
	/// </summary>
	public virtual void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color)
	{
	}

	/// <summary>
	/// Allows you to modify the speeds at which you rise and fall when wings are equipped.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
	{
	}

	/// <summary>
	/// Allows you to modify the horizontal flight speed and acceleration of wings.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
	{
	}

	/// <summary>
	/// Allows for Wings to do various things while in use. "inUse" is whether or not the jump button is currently pressed.
	/// Called when wings visually appear on the player.
	/// Use to animate wings, create dusts, invoke sounds, and create lights. False will keep everything the same.
	/// True, you need to handle all animations in your own code.
	/// <para/> This method is not instanced.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual bool WingUpdate(int wings, Player player, bool inUse)
	{
		return false;
	}

	/// <summary>
	/// Allows you to customize an item's movement when lying in the world. Note that this will not be called if the item is currently being grabbed by a player.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void Update(Item item, ref float gravity, ref float maxFallSpeed)
	{
	}

	/// <summary>
	/// Allows you to make things happen when an item is lying in the world. This will always be called, even when the item is being grabbed by a player. This hook should be used for adding light, or for increasing the age of less valuable items.
	/// <para/> Called on all clients and the server.
	/// </summary>
	public virtual void PostUpdate(Item item)
	{
	}

	/// <summary>
	/// Allows you to modify how close an item must be to the player in order to move towards the player.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void GrabRange(Item item, Player player, ref int grabRange)
	{
	}

	/// <summary>
	/// Allows you to modify the way an item moves towards the player. Return false to allow the vanilla grab style to take place. Returns false by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual bool GrabStyle(Item item, Player player)
	{
		return false;
	}

	/// <summary>
	/// Allows you to determine whether or not the item can be picked up
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual bool CanPickup(Item item, Player player)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make special things happen when the player picks up an item. Return false to stop the item from being added to the player's inventory; returns true by default.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual bool OnPickup(Item item, Player player)
	{
		return true;
	}

	/// <summary>
	/// Return true to specify that the item can be picked up despite not having enough room in inventory. Useful for something like hearts or experience items. Use in conjunction with OnPickup to actually consume the item and handle it.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual bool ItemSpace(Item item, Player player)
	{
		return false;
	}

	/// <summary>
	/// Allows you to determine the color and transparency in which an item is drawn. Return null to use the default color (normally light color). Returns null by default.
	/// <para/> Called on all clients.
	/// </summary>
	public virtual Color? GetAlpha(Item item, Color lightColor)
	{
		return null;
	}

	/// <summary>
	/// <inheritdoc cref="ModItem.PreDrawInWorld(SpriteBatch, Color, Color, ref float, ref float, int)"/>
	/// </summary>
	public virtual bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		return true;
	}

	/// <summary>
	/// <inheritdoc cref="ModItem.PostDrawInWorld(SpriteBatch, Color, Color, float, float, int)"/>
	/// </summary>
	public virtual void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
	}

	/// <summary>
	/// <inheritdoc cref="ModItem.PreDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/>
	/// </summary>
	public virtual bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
		Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		return true;
	}

	/// <summary>
	/// <inheritdoc cref="ModItem.PostDrawInInventory(SpriteBatch, Vector2, Rectangle, Color, Color, Vector2, float)"/>
	/// </summary>
	public virtual void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
		Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
	}

	/// <summary>
	/// Allows you to determine the offset of an item's sprite when used by the player.
	/// This is only used for items with a useStyle of 5 that aren't staves.
	/// Return null to use the item's default holdout offset; returns null by default.
	/// <para/> This method is not instanced.
	/// <para/> Called on local and remote clients.
	/// <example><code>return new Vector2(10, 0);</code></example>
	/// </summary>
	public virtual Vector2? HoldoutOffset(int type)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine the point on an item's sprite that the player holds onto when using the item.
	/// The origin is from the bottom left corner of the sprite. This is only used for staves with a useStyle of 5.
	/// Return null to use the item's default holdout origin; returns null by default.
	/// <para/> This method is not instanced.
	/// <para/> Called on local and remote clients.
	/// </summary>
	public virtual Vector2? HoldoutOrigin(int type)
	{
		return null;
	}

	/// <summary>
	/// Allows you to disallow the player from equipping an accessory. Return false to disallow equipping the accessory. Returns true by default.
	/// </summary>
	/// <param name="item">The item that is attempting to equip.</param>
	/// <param name="player">The player.</param>
	/// <param name="slot">The inventory slot that the item is attempting to occupy.</param>
	/// <param name="modded">If the inventory slot index is for modded slots.</param>
	public virtual bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
	{
		return true;
	}

	/// <summary>
	/// Allows you to prevent similar accessories from being equipped multiple times. For example, vanilla Wings.
	/// Return false to have the currently equipped item swapped with the incoming item - ie both can't be equipped at same time.
	/// </summary>
	public virtual bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify what item, and in what quantity, is obtained when an item of the given type is fed into the Extractinator. Use <see cref="ItemID.Sets.ExtractinatorMode"/> to allow an item to be fed into the Extractinator.
	/// <para/> An extractType of 0 represents the default extraction (Silt and Slush). 0, <see cref="ItemID.DesertFossil"/>, <see cref="ItemID.OldShoe"/>, and <see cref="ItemID.LavaMoss"/> are vanilla extraction types. Modded types by convention will correspond to the iconic item of the extraction type. The <see href="https://terraria.wiki.gg/wiki/Extractinator">Extractinator wiki page</see> has more info.
	/// <para/> By default the parameters will be set to the output of feeding Silt/Slush into the Extractinator.
	/// <para/> Use <paramref name="extractinatorBlockType"/> to provide different behavior for <see cref="TileID.ChlorophyteExtractinator"/> if desired.
	/// <para/> If the Chlorophyte Extractinator item swapping behavior is desired, see the example in <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/GlobalItems/TorchExtractinatorGlobalItem.cs">TorchExtractinatorGlobalItem.cs</see>.
	/// <para/> This method is not instanced.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="extractType">The extractinator type corresponding to the items being processed</param>
	/// <param name="extractinatorBlockType">Which Extractinator tile is being used, <see cref="TileID.Extractinator"/> or <see cref="TileID.ChlorophyteExtractinator"/>.</param>
	/// <param name="resultType">Type of the result.</param>
	/// <param name="resultStack">The result stack.</param>
	public virtual void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
	{
	}

	/// <summary>
	/// Allows you to modify how many of an item a player obtains when the player fishes that item.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void CaughtFishStack(int type, ref int stack)
	{
	}

	/// <summary>
	/// Whether or not specific conditions have been satisfied for the Angler to be able to request the given item. (For example, Hardmode.)
	/// Returns true by default.
	/// <para/> This method is not instanced.
	/// <para/> Called on the server only.
	/// </summary>
	public virtual bool IsAnglerQuestAvailable(int type)
	{
		return true;
	}

	/// <summary>
	/// Allows you to set what the Angler says when the Quest button is clicked in his chat.
	/// The chat parameter is his dialogue, and catchLocation should be set to "Caught at [location]" for the given type.
	/// <para/> This method is not instanced.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void AnglerChat(int type, ref string chat, ref string catchLocation)
	{
	}

	/// <summary>
	/// Override this method to add <see cref="Recipe"/>s to the game.<br/>
	/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes">Basic Recipes Guide</see> teaches how to add new recipes to the game and how to manipulate existing recipes.<br/>
	/// </summary>
	public virtual void AddRecipes()
	{
	}

	/// <summary>
	/// Allows you to do things before this item's tooltip is drawn.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item</param>
	/// <param name="lines">The tooltip lines for this item</param>
	/// <param name="x">The top X position for this tooltip. It is where the first line starts drawing</param>
	/// <param name="y">The top Y position for this tooltip. It is where the first line starts drawing</param>
	/// <returns>Whether or not to draw this tooltip</returns>
	public virtual bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
	{
		return true;
	}

	/// <summary>
	/// Allows you to do things after this item's tooltip is drawn. The lines contain draw information as this is ran after drawing the tooltip.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item</param>
	/// <param name="lines">The tooltip lines for this item</param>
	public virtual void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
	{
	}

	/// <summary>
	/// Allows you to do things before a tooltip line of this item is drawn. The line contains draw info.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item</param>
	/// <param name="line">The line that would be drawn</param>
	/// <param name="yOffset">The Y offset added for next tooltip lines</param>
	/// <returns>Whether or not to draw this tooltip line</returns>
	public virtual bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
	{
		return true;
	}

	/// <summary>
	/// Allows you to do things after a tooltip line of this item is drawn. The line contains draw info.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item</param>
	/// <param name="line">The line that was drawn</param>
	public virtual void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
	{
	}

	/// <summary>
	/// Allows you to modify all the tooltips that display for the given item. See here for information about TooltipLine. To hide tooltips, please use <see cref="TooltipLine.Hide"/> and defensive coding.
	/// <para/> Called on a clone of the item, not the original. Modifying instanced fields will have no effect.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
	}

	/// <summary>
	/// Allows you to save custom data for this item.
	/// <br/>
	/// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
	/// <br/><b>NOTE:</b> Try to only save data that isn't default values.
	/// </summary>
	/// <param name="item"> The item. </param>
	/// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
	public virtual void SaveData(Item item, TagCompound tag) { }

	/// <summary>
	/// Allows you to load custom data that you have saved for this item.
	/// <br/><b>Try to write defensive loading code that won't crash if something's missing.</b>
	/// </summary>
	/// <param name="item"> The item. </param>
	/// <param name="tag"> The TagCompound to load data from. </param>
	public virtual void LoadData(Item item, TagCompound tag) { }

	/// <inheritdoc cref="ModItem.NetSend"/>
	public virtual void NetSend(Item item, BinaryWriter writer)
	{
	}

	/// <inheritdoc cref="ModItem.NetReceive"/>
	public virtual void NetReceive(Item item, BinaryReader reader)
	{
	}
}
