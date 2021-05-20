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

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to modify and use hooks for all items, including vanilla items. Create an instance of an overriding class then call Mod.AddGlobalItem to use this.
	/// </summary>
	public abstract class GlobalItem : GlobalType<Item>
	{
		protected sealed override void Register() {
			ItemLoader.VerifyGlobalItem(this);

			ModTypeLookup<GlobalItem>.Register(this);
			
			index = (ushort)ItemLoader.globalItems.Count;

			ItemLoader.globalItems.Add(this);
		}

		public GlobalItem Instance(Item item) => Instance(item.globalItems, index);

		/// <summary>
		/// Create a copy of this instanced GlobalItem. Called when an item is cloned.
		/// </summary>
		/// <param name="item">The item being cloned</param>
		/// <param name="itemClone">The new item</param>
		public virtual GlobalItem Clone(Item item, Item itemClone) => (GlobalItem)MemberwiseClone();

		/// <summary>
		/// Allows you to set the properties of any and every item that gets created.
		/// </summary>
		public virtual void SetDefaults(Item item) {
		}

		public virtual void OnCreate(Item item, ItemCreationContext context) {
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
		/// <param name="pre">The prefix being applied to the item, or the roll mode. -1 is when the item is naturally generated in a chest, crafted, purchased from an NPC, looted from a grab bag (excluding presents), or dropped by a slain enemy (if it's spawned with prefixGiven: -1). -2 is when the item is rolled in the tinkerer. -3 determines if the item can be placed in the tinkerer slot.</param>
		/// <returns></returns>
		public virtual bool? PrefixChance(Item item, int pre, UnifiedRandom rand) => null;

		/// <summary>
		/// Force a re-roll of a prefix by returning false.
		/// </summary>
		public virtual bool AllowPrefix(Item item, int pre) => true;

		/// <summary>
		/// Returns whether or not any item can be used. Returns true by default. The inability to use a specific item overrides this, so use this to stop an item from being used.
		/// </summary>
		public virtual bool CanUseItem(Item item, Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the location and rotation of any item in its use animation.
		/// </summary>
		/// <param name="item"> The item. </param>
		/// <param name="player"> The player. </param>
		/// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
		public virtual void UseStyle(Item item, Player player, Rectangle heldItemFrame) { }

		/// <summary>
		/// Allows you to modify the location and rotation of the item the player is currently holding.
		/// </summary>
		/// <param name="item"> The item. </param>
		/// <param name="player"> The player. </param>
		/// <param name="heldItemFrame"> The source rectangle for the held item's texture. </param>
		public virtual void HoldStyle(Item item, Player player, Rectangle heldItemFrame) { }

		/// <summary>
		/// Allows you to make things happen when the player is holding an item (for example, torches make light and water candles increase spawn rate).
		/// </summary>
		public virtual void HoldItem(Item item, Player player) {
		}

		/// <summary>
		/// Allows you to change the effective useTime of an item.
		/// <br/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
		/// </summary>
		/// <returns> The multiplier on the usage time. 1f by default. Values greater than 1 increase the item use's length. </returns>
		public virtual float UseTimeMultiplier(Item item, Player player) => 1f;

		/// <summary>
		/// Allows you to change the effective useAnimation of an item.
		/// <br/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
		/// </summary>
		/// <returns>The multiplier on the animation time. 1f by default. Values greater than 1 increase the item animation's length. </returns>
		public virtual float UseAnimationMultiplier(Item item, Player player) => 1f;

		/// <summary>
		/// Allows you to safely change both useTime and useAnimation while keeping the values relative to each other.
		/// <br/> Useful for status effects.
		/// </summary>
		/// <returns> The multiplier on the use speed. 1f by default. Values greater than 1 increase the overall item speed. </returns>
		public virtual float UseSpeedMultiplier(Item item, Player player) => 1f;

		/// <summary>
		/// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a healLife value.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="player">The player using the item.</param>
		/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
		/// <param name="healValue">The amount of life being healed.</param>
		public virtual void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue) {
		}

		/// <summary>
		/// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a healMana value.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="player">The player using the item.</param>
		/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
		/// <param name="healValue">The amount of mana being healed.</param>
		public virtual void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue) {
		}

		/// <summary>
		/// Allows you to temporarily modify the amount of mana an item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="player">The player using the item.</param>
		/// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
		/// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
		public virtual void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult) {
		}

		/// <summary>
		/// Allows you to make stuff happen when a player doesn't have enough mana for an item they are trying to use.
		/// If the player has high enough mana after this hook runs, mana consumption will happen normally.
		/// Only runs once per item use.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="player">The player using the item.</param>
		/// <param name="neededMana">The mana needed to use the item.</param>
		public virtual void OnMissingMana(Item item, Player player, int neededMana) {
		}

		/// <summary>
		/// Allows you to make stuff happen when a player consumes mana on use of an item.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="player">The player using the item.</param>
		/// <param name="manaConsumed">The mana consumed from the player.</param>
		public virtual void OnConsumeMana(Item item, Player player, int manaConsumed) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's damage based on player buffs, etc. This is useful for creating new classes of damage, or for making subclasses of damage (for example, Shroomite armor set boosts).
		/// </summary>
		/// <param name="item">The item being used</param>
		/// <param name="player">The player using the item</param>
		/// <param name="add">Used for additively stacking buffs (most common). Only ever use += on this field. Things with effects like "5% increased MyDamageClass damage" would use this: `add += 0.05f`</param>
		/// <param name="mult">Use to directly multiply the player's effective damage. Good for debuffs, or things which should stack separately (eg ammo type buffs)</param>
		/// <param name="flat">This is a flat damage bonus that will be added after add and mult are applied. It facilitates effects like "4 more damage from weapons"</param>
		public virtual void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage, ref float flat) {
		}

		/// <summary>
		/// Allows you to set an item's sorting group in Journey Mode's duplication menu. This is useful for setting custom item types that group well together, or whenever the default vanilla sorting doesn't sort the way you want it.
		/// </summary>
		/// <param name="item">The item being used</param>
		/// <param name="itemGroup">The item group this item is being assigned to</param>
		public virtual void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's knockback based on player buffs, etc. This allows you to customize knockback beyond the Player class's limited fields.
		/// </summary>
		/// <param name="item">The item being used</param>
		/// <param name="player">The player using the item</param>
		/// <param name="knockback">The knockback</param>
		public virtual void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback, ref float flat) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's crit chance based on player buffs, etc.
		/// </summary>
		/// <param name="item">The item being used</param>
		/// <param name="player">The player using the item</param>
		/// <param name="crit">The critical strike chance</param>
		public virtual void ModifyWeaponCrit(Item item, Player player, ref int crit) {
		}

		/// <summary>
		/// Allows you to modify the projectile created by a weapon based on the ammo it is using.
		/// </summary>
		/// <param name="weapon">The item that is using this ammo</param>
		/// <param name="ammo">The ammo item</param>
		/// <param name="player">The player using the item</param>
		/// <param name="type">The ID of the projectile shot</param>
		/// <param name="speed">The speed of the projectile shot</param>
		/// <param name="damage">The damage of the projectile shot</param>
		/// <param name="knockback">The speed of the projectile shot</param>
		public virtual void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
		}

		/// <summary>
		/// Whether or not ammo will be consumed upon usage. Called both by the gun and by the ammo; if at least one returns false then the ammo will not be used. By default returns true.
		/// If false is returned, the OnConsumeAmmo hook is never called.
		/// </summary>
		public virtual bool ConsumeAmmo(Item item, Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when ammo is consumed. Called both by the gun and by the ammo.
		/// Called before the ammo stack is reduced.
		/// </summary>
		public virtual void OnConsumeAmmo(Item item, Player player) {
		}

		/// <summary>
		/// Allows you to prevent an item from shooting a projectile on use. Returns true by default.
		/// </summary>
		/// <param name="item"> The item being used. </param>
		/// <param name="player"> The player using the item. </param>
		/// <returns></returns>
		public virtual bool CanShoot(Item item, Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by an item.
		/// </summary>
		/// <param name="item"> The item being used. </param>
		/// <param name="player"> The player using the item. </param>
		/// <param name="position"> The center position of the projectile. </param>
		/// <param name="velocity"> The velocity of the projectile. </param>
		/// <param name="type"> The ID of the projectile. </param>
		/// <param name="damage"> The damage of the projectile. </param>
		/// <param name="knockback"> The knockback of the projectile. </param>
		public virtual void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		}

		/// <summary>
		/// Allows you to modify an item's shooting mechanism. Return false to prevent vanilla's shooting code from running. Returns true by default.
		/// </summary>
		/// <param name="item"> The item being used. </param>
		/// <param name="player"> The player using the item. </param>
		/// <param name="source"> The projectile source's information. </param>
		/// <param name="position"> The center position of the projectile. </param>
		/// <param name="velocity"> The velocity of the projectile. </param>
		/// <param name="type"> The ID of the projectile. </param>
		/// <param name="damage"> The damage of the projectile. </param>
		/// <param name="knockback"> The knockback of the projectile. </param>
		public virtual bool Shoot(Item item, Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return true;
		}

		/// <summary>
		/// Changes the hitbox of a melee weapon when it is used.
		/// </summary>
		public virtual void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
		}

		/// <summary>
		/// Allows you to give melee weapons special effects, such as creating light or dust.
		/// </summary>
		public virtual void MeleeEffects(Item item, Player player, Rectangle hitbox) {
		}

		/// <summary>
		/// Allows you to determine whether a melee weapon can hit the given NPC when swung. Return true to allow hitting the target, return false to block the weapon from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		public virtual bool? CanHitNPC(Item item, Player player, NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that a melee weapon does to an NPC.
		/// </summary>
		public virtual void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when a melee weapon hits an NPC (for example how the Pumpkin Sword creates pumpkin heads).
		/// </summary>
		public virtual void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether a melee weapon can hit the given opponent player when swung. Return false to block the weapon from hitting the target. Returns true by default.
		/// </summary>
		public virtual bool CanHitPvp(Item item, Player player, Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that a melee weapon does to a player.
		/// </summary>
		public virtual void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when a melee weapon hits a player.
		/// </summary>
		public virtual void OnHitPvp(Item item, Player player, Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to make things happen when an item is used. The return value controls whether or not ApplyItemTime will be called for the player.
		/// <br/> Return true if the item actually did something, to force itemTime.
		/// <br/> Return false to keep itemTime at 0.
		/// <br/> Return null for vanilla behavior.
		/// </summary>
		public virtual bool? UseItem(Item item, Player player) => null;

		/// <summary>
		/// Allows you to make things happen when an item's use animation starts.
		/// </summary>
		public virtual void UseAnimation(Item item, Player player) { }

		/// <summary>
		/// If the item is consumable and this returns true, then the item will be consumed upon usage. Returns true by default.
		/// If false is returned, the OnConsumeItem hook is never called.
		/// </summary>
		public virtual bool ConsumeItem(Item item, Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when this item is consumed.
		/// Called before the item stack is reduced.
		/// </summary>
		public virtual void OnConsumeItem(Item item, Player player) {
		}

		/// <summary>
		/// Allows you to modify the player's animation when an item is being used.
		/// </summary>
		public virtual void UseItemFrame(Item item, Player player) { }

		/// <summary>
		/// Allows you to modify the player's animation when the player is holding an item.
		/// </summary>
		public virtual void HoldItemFrame(Item item, Player player) { }

		/// <summary>
		/// Allows you to make an item usable by right-clicking. Returns false by default. When the item is used by right-clicking, player.altFunctionUse will be set to 2.
		/// </summary>
		public virtual bool AltFunctionUse(Item item, Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to make things happen when an item is in the player's inventory (for example, how the cell phone makes information display).
		/// </summary>
		public virtual void UpdateInventory(Item item, Player player) {
		}

		/// <summary>
		/// Allows you to give effects to armors and accessories, such as increased damage.
		/// </summary>
		public virtual void UpdateEquip(Item item, Player player) {
		}

		/// <summary>
		/// Allows you to give effects to accessories. The hideVisual parameter is whether the player has marked the accessory slot to be hidden from being drawn on the player.
		/// </summary>
		public virtual void UpdateAccessory(Item item, Player player, bool hideVisual) {
		}

		/// <summary>
		/// Allows you to give effects to this accessory when equipped in a vanity slot. Vanilla uses this for boot effects, wings and merman/werewolf visual flags
		/// </summary>
		public virtual void UpdateVanity(Item item, Player player) {
		}

		/// <summary>
		/// Allows you to determine whether the player is wearing an armor set, and return a name for this set. 
		/// If there is no armor set, return the empty string.
		/// Returns the empty string by default.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual string IsArmorSet(Item head, Item body, Item legs) {
			return "";
		}

		/// <summary>
		/// Allows you to give set bonuses to your armor set with the given name. 
		/// The set name will be the same as returned by IsArmorSet.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void UpdateArmorSet(Player player, string set) {
		}

		/// <summary>
		/// Returns whether or not the head armor, body armor, and leg armor textures make up a set.
		/// This hook is used for the PreUpdateVanitySet, UpdateVanitySet, and ArmorSetShadow hooks, and will use items in the social slots if they exist.
		/// By default this will return the same value as the IsArmorSet hook, so you will not have to use this hook unless you want vanity effects to be entirely separate from armor sets.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual string IsVanitySet(int head, int body, int legs) {
			Item headItem = new Item();
			if (head >= 0) {
				headItem.SetDefaults(Item.headType[head], true);
			}
			Item bodyItem = new Item();
			if (body >= 0) {
				bodyItem.SetDefaults(Item.bodyType[body], true);
			}
			Item legItem = new Item();
			if (legs >= 0) {
				legItem.SetDefaults(Item.legType[legs], true);
			}
			return IsArmorSet(headItem, bodyItem, legItem);
		}

		/// <summary>
		/// Allows you to create special effects (such as the necro armor's hurt noise) when the player wears the vanity set with the given name returned by IsVanitySet.
		/// This hook is called regardless of whether the player is frozen in any way.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void PreUpdateVanitySet(Player player, string set) {
		}

		/// <summary>
		/// Allows you to create special effects (such as dust) when the player wears the vanity set with the given name returned by IsVanitySet. This hook will only be called if the player is not frozen in any way.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void UpdateVanitySet(Player player, string set) {
		}

		/// <summary>
		/// Allows you to determine special visual effects a vanity has on the player without having to code them yourself.
		/// 
		/// This method is not instanced.
		/// </summary>
		/// <example><code>player.armorEffectDrawShadow = true;</code></example>
		public virtual void ArmorSetShadows(Player player, string set) {
		}

		/// <summary>
		/// Allows you to modify the equipment that the player appears to be wearing.
		/// 
		/// Note that type and equipSlot are not the same as the item type of the armor the player will appear to be wearing. Worn equipment has a separate set of IDs.
		/// You can find the vanilla equipment IDs by looking at the headSlot, bodySlot, and legSlot fields for items, and modded equipment IDs by looking at EquipLoader.
		/// 
		/// This method is not instanced.
		/// </summary>
		/// <param name="armorSlot">head armor (0), body armor (1) or leg armor (2).</param>
		/// <param name="type">The equipment texture ID of the item that the player is wearing.</param>
		/// <param name="equipSlot">The altered equipment texture ID for the legs (armorSlot 1 and 2) or head (armorSlot 0)</param>
		/// <param name="robes">Set to true if you modify equipSlot when armorSlot == 1 to set Player.wearsRobe, otherwise ignore it</param>
		public virtual void SetMatch(int armorSlot, int type, bool male, ref int equipSlot, ref bool robes) {
		}

		/// <summary>
		/// Returns whether or not an item does something when right-clicked in the inventory. Returns false by default.
		/// </summary>
		public virtual bool CanRightClick(Item item) {
			return false;
		}

		/// <summary>
		/// Allows you to make things happen when an item is right-clicked in the inventory. Useful for goodie bags.
		/// </summary>
		public virtual void RightClick(Item item, Player player) {
		}

		/// <summary>
		/// Allows you to make vanilla bags drop your own items and stop the default items from being dropped. 
		/// Return false to stop the default items from being dropped; returns true by default. 
		/// Context will either be "present", "bossBag", "crate", "lockBox", "herbBag", or "goodieBag". 
		/// For boss bags and crates, arg will be set to the type of the item being opened.
		/// This method is also called for modded bossBags that are properly implemented.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual bool PreOpenVanillaBag(string context, Player player, int arg) {
			return true;
		}

		/// <summary>
		/// Allows you to make vanilla bags drop your own items in addition to the default items.
		/// This method will not be called if any other GlobalItem returns false for PreOpenVanillaBag.
		/// Context will either be "present", "bossBag", "crate", "lockBox", "herbBag", or "goodieBag".
		/// For boss bags and crates, arg will be set to the type of the item being opened.
		/// This method is also called for modded bossBags that are properly implemented.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void OpenVanillaBag(string context, Player player, int arg) {
		}

		/// <summary>
		/// Allows you to prevent vanilla items from stacking in the world.
		/// This is only called when two items of the same type attempt to stack.
		/// </summary>
		/// <param name="item1">The item that is attempting to stack</param>
		/// <param name="item2">The item that the other item is attempting to stack with</param>
		/// <returns>Whether or not the items are allowed to stack</returns>
		public virtual bool CanStackInWorld(Item item1, Item item2) {
			return true;
		}

		/// <summary>
		/// Returns if the normal reforge pricing is applied. 
		/// If true or false is returned and the price is altered, the price will equal the altered price.
		/// The passed reforge price equals the item.value. Vanilla pricing will apply 20% discount if applicable and then price the reforge at a third of that value.
		/// </summary>
		public virtual bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount) {
			return true;
		}

		/// <summary>
		/// This hook gets called when the player clicks on the reforge button and can afford the reforge.
		/// Returns whether the reforge will take place. If false is returned, the PostReforge hook is never called.
		/// Reforging preserves modded data on the item. 
		/// </summary>
		public virtual bool PreReforge(Item item) {
			return true;
		}

		/// <summary>
		/// This hook gets called immediately after an item gets reforged by the Goblin Tinkerer.
		/// Useful for modifying modded data based on the reforge result.
		/// </summary>
		public virtual void PostReforge(Item item) {
		}

		/// <summary>
		/// Allows you to determine whether the skin/shirt on the player's arms and hands are drawn when a body armor is worn.
		/// Note that if drawHands is false, the arms will not be drawn either.
		/// "body" is the player's associated body equipment texture.
		/// This method is not instanced.
		/// </summary>
		public virtual void DrawHands(int body, ref bool drawHands, ref bool drawArms) {
		}

		/// <summary>
		/// Allows you to determine whether the player's hair or alt (hat) hair will be drawn when a head armor is worn.
		/// "head" is the player's associated head equipment texture.
		/// This method is not instanced.
		/// </summary>
		public virtual void DrawHair(int head, ref bool drawHair, ref bool drawAltHair) {
		}

		/// <summary>
		/// Return false to hide the player's head when a head armor is worn. Returns true by default.
		/// "head" is the player's associated head equipment texture.
		/// This method is not instanced.
		/// </summary>
		public virtual bool DrawHead(int head) {
			return true;
		}

		/// <summary>
		/// Return false to hide the player's body when a body armor is worn. Returns true by default.
		/// "body" is the player's associated body equipment texture.
		/// This method is not instanced.
		/// </summary>
		public virtual bool DrawBody(int body) {
			return true;
		}

		/// <summary>
		/// Return false to hide the player's legs when a leg armor or shoe accessory is worn. Returns true by default.
		/// "legs" and "shoes" are the player's associated legs and shoes equipment textures.
		/// This method is not instanced.
		/// </summary>
		public virtual bool DrawLegs(int legs, int shoes) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the colors in which the player's armor and their surrounding accessories are drawn, in addition to which glow mask and in what color is drawn.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void DrawArmorColor(EquipType type, int slot, Player drawPlayer, float shadow, ref Color color,
			ref int glowMask, ref Color glowMaskColor) {
		}

		/// <summary>
		/// Allows you to modify which glow mask and in what color is drawn on the player's arms. Note that this is only called for body armor.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void ArmorArmGlowMask(int slot, Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
		}

		/// <summary>
		/// Allows you to modify the speeds at which you rise and fall when wings are equipped.
		/// </summary>
		public virtual void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
	ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		}

		/// <summary>
		/// Allows you to modify the horizontal flight speed and acceleration of wings.
		/// </summary>
		public virtual void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration) {
		}

		/// <summary>
		/// Allows for Wings to do various things while in use. "inUse" is whether or not the jump button is currently pressed.
		/// Called when wings visually appear on the player.
		/// Use to animate wings, create dusts, invoke sounds, and create lights. False will keep everything the same.
		/// True, you need to handle all animations in your own code.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual bool WingUpdate(int wings, Player player, bool inUse) {
			return false;
		}

		/// <summary>
		/// Allows you to customize an item's movement when lying in the world. Note that this will not be called if the item is currently being grabbed by a player.
		/// </summary>
		public virtual void Update(Item item, ref float gravity, ref float maxFallSpeed) {
		}

		/// <summary>
		/// Returns whether or not this item will burn in lava regardless of any conditions. Returns null by default (follow vanilla behaviour).
		/// </summary>
		public virtual bool? CanBurnInLava(Item item) {
			return null;
		}

		/// <summary>
		/// Allows you to make things happen when an item is lying in the world. This will always be called, even when the item is being grabbed by a player. This hook should be used for adding light, or for increasing the age of less valuable items.
		/// </summary>
		public virtual void PostUpdate(Item item) {
		}

		/// <summary>
		/// Allows you to modify how close an item must be to the player in order to move towards the player.
		/// </summary>
		public virtual void GrabRange(Item item, Player player, ref int grabRange) {
		}

		/// <summary>
		/// Allows you to modify the way an item moves towards the player. Return false to allow the vanilla grab style to take place. Returns false by default.
		/// </summary>
		public virtual bool GrabStyle(Item item, Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to determine whether or not the item can be picked up
		/// </summary>
		public virtual bool CanPickup(Item item, Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to make special things happen when the player picks up an item. Return false to stop the item from being added to the player's inventory; returns true by default.
		/// </summary>
		public virtual bool OnPickup(Item item, Player player) {
			return true;
		}

		/// <summary>
		/// Return true to specify that the item can be picked up despite not having enough room in inventory. Useful for something like hearts or experience items. Use in conjunction with OnPickup to actually consume the item and handle it.
		/// </summary>
		public virtual bool ItemSpace(Item item, Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to determine the color and transparency in which an item is drawn. Return null to use the default color (normally light color). Returns null by default.
		/// </summary>
		public virtual Color? GetAlpha(Item item, Color lightColor) {
			return null;
		}

		/// <summary>
		/// Allows you to draw things behind an item, or to modify the way an item is drawn in the world. Return false to stop the game from drawing the item (useful if you're manually drawing the item). Returns true by default.
		/// </summary>
		public virtual bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of an item. This method is called even if PreDrawInWorld returns false.
		/// </summary>
		public virtual void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
		}

		/// <summary>
		/// Allows you to draw things behind an item in the inventory. Return false to stop the game from drawing the item (useful if you're manually drawing the item). Returns true by default.
		/// </summary>
		public virtual bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of an item in the inventory. This method is called even if PreDrawInInventory returns false.
		/// </summary>
		public virtual void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) {
		}

		/// <summary>
		/// Allows you to determine the offset of an item's sprite when used by the player.
		/// This is only used for items with a useStyle of 5 that aren't staves.
		/// Return null to use the item's default holdout offset; returns null by default.
		/// 
		/// This method is not instanced.
		/// </summary>
		/// <example><code>return new Vector2(10, 0);</code></example>
		public virtual Vector2? HoldoutOffset(int type) {
			return null;
		}

		/// <summary>
		/// Allows you to determine the point on an item's sprite that the player holds onto when using the item.
		/// The origin is from the bottom left corner of the sprite. This is only used for staves with a useStyle of 5.
		/// Return null to use the item's default holdout origin; returns null by default.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual Vector2? HoldoutOrigin(int type) {
			return null;
		}

		/// <summary>
		/// Allows you to disallow the player from equipping an accessory. Return false to disallow equipping the accessory. Returns true by default.
		/// </summary>
		/// <param name="item">The item that is attepting to equip.</param>
		/// <param name="player">The player.</param>
		/// <param name="slot">The inventory slot that the item is attempting to occupy.</param>
		public virtual bool CanEquipAccessory(Item item, Player player, int slot) {
			return true;
		}

		/// <summary>
		/// Allows you to modify what item, and in what quantity, is obtained when an item of the given type is fed into the Extractinator. 
		/// An extractType of 0 represents the default extraction (Silt and Slush). 
		/// By default the parameters will be set to the output of feeding Silt/Slush into the Extractinator.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void ExtractinatorUse(int extractType, ref int resultType, ref int resultStack) {
		}

		/// <summary>
		/// Allows you to modify how many of an item a player obtains when the player fishes that item.
		/// </summary>
		public virtual void CaughtFishStack(int type, ref int stack) {
		}

		/// <summary>
		/// Whether or not specific conditions have been satisfied for the Angler to be able to request the given item. (For example, Hardmode.) 
		/// Returns true by default.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual bool IsAnglerQuestAvailable(int type) {
			return true;
		}

		/// <summary>
		/// Allows you to set what the Angler says when the Quest button is clicked in his chat. 
		/// The chat parameter is his dialogue, and catchLocation should be set to "Caught at [location]" for the given type.
		/// 
		/// This method is not instanced.
		/// </summary>
		public virtual void AnglerChat(int type, ref string chat, ref string catchLocation) {
		}

		/// <summary>
		/// This is essentially the same as Mod.AddRecipes or ModItem.AddRecipes. Use whichever method makes organizational sense for your mod.
		/// </summary>
		public virtual void AddRecipes() {
		}

		/// <summary>
		/// Allows you to do things before this item's tooltip is drawn.
		/// </summary>
		/// <param name="item">The item</param>
		/// <param name="lines">The tooltip lines for this item</param>
		/// <param name="x">The top X position for this tooltip. It is where the first line starts drawing</param>
		/// <param name="y">The top Y position for this tooltip. It is where the first line starts drawing</param>
		/// <returns>Whether or not to draw this tooltip</returns>
		public virtual bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) {
			return true;
		}

		/// <summary>
		/// Allows you to do things after this item's tooltip is drawn. The lines contain draw information as this is ran after drawing the tooltip.
		/// </summary>
		/// <param name="item">The item</param>
		/// <param name="lines">The tooltip lines for this item</param>
		public virtual void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines) {
		}

		/// <summary>
		/// Allows you to do things before a tooltip line of this item is drawn. The line contains draw info.
		/// </summary>
		/// <param name="item">The item</param>
		/// <param name="line">The line that would be drawn</param>
		/// <param name="yOffset">The Y offset added for next tooltip lines</param>
		/// <returns>Whether or not to draw this tooltip line</returns>
		public virtual bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
			return true;
		}

		/// <summary>
		/// Allows you to do things after a tooltip line of this item is drawn. The line contains draw info.
		/// </summary>
		/// <param name="item">The item</param>
		/// <param name="line">The line that was drawn</param>
		public virtual void PostDrawTooltipLine(Item item, DrawableTooltipLine line) {
		}

		/// <summary>
		/// Allows you to modify all the tooltips that display for the given item. See here for information about TooltipLine.
		/// </summary>
		public virtual void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
		}

		/// <summary>
		/// Whether or not the given item needs to save custom data. Returning false will save on the memory used in saving an item, but returning true is necessary in order to save data across all items or vanilla items. Returns false by default. Note that the return value of this hook must be deterministic (randomness is not allowed).
		/// </summary>
		public virtual bool NeedsSaving(Item item) {
			return false;
		}

		/// <summary>
		/// Allows you to save custom data for the given item. Only called when NeedsCustomSaving returns true. Returns false by default.
		/// </summary>
		public virtual TagCompound Save(Item item) {
			return null;
		}

		/// <summary>
		/// Allows you to load custom data that you have saved for the given item.
		/// </summary>
		public virtual void Load(Item item, TagCompound tag) {
		}

		/// <summary>
		/// Allows you to send custom data for the given item between client and server.
		/// </summary>
		public virtual void NetSend(Item item, BinaryWriter writer) {
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void NetReceive(Item item, BinaryReader reader) {
		}
	}
}
