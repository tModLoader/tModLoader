using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to place all your properties and hooks for each item. Create instances of ModItem (preferably overriding this class) to pass as parameters to Mod.AddItem.
	/// </summary>
	public class ModItem
	{
		//add modItem property to Terraria.Item (internal set)
		//set modItem to null at beginning of Terraria.Item.ResetStats		
		/// <summary>
		/// The item object that this ModItem controls.
		/// </summary>
		/// <value>
		/// The item.
		/// </value>
		public Item item {
			get;
			internal set;
		}

		/// <summary>
		/// Gets the mod.
		/// </summary>
		/// <value>
		/// The mod that added this ModItem.
		/// </value>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The internal name of this ModItem.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The translations for the display name of this item.
		/// </summary>
		public ModTranslation DisplayName {
			get;
			internal set;
		}

		/// <summary>
		/// The translations for the display name of this tooltip.
		/// </summary>
		public ModTranslation Tooltip {
			get;
			internal set;
		}

		/// <summary>
		/// The file name of this item's texture file in the mod loader's file space.
		/// </summary>
		public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

		[Obsolete("override ModItem.OnlyShootOnSwing property", true)]
		public bool projOnSwing;

		[Obsolete("override ModItem.BossBagNPC property", true)]
		public int bossBagNPC;

		[Obsolete]
		private bool ProjOnSwing_Obsolete {
			get => projOnSwing;
			set => projOnSwing = value;
		}

		[Obsolete]
		private int BossBagNPC_Obsolete {
			get => bossBagNPC;
			set => bossBagNPC = value;
		}

		public ModItem() {
			item = new Item { modItem = this };
		}

		/// <summary>
		/// Allows you to automatically load an item instead of using Mod.AddItem. 
		/// Return true to allow autoloading; by default returns the mod's autoload property. 
		/// Use this method to force or stop an autoload or change the internal name.
		/// </summary>
		/// <param name="name">The name, initialized to the name of this type.</param>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Whether instances of this ModItem are created through Clone or constructor (by default implementations of NewInstance and Clone(Item, Item)). 
		/// Defaults to false (using default constructor).
		/// </summary>
		public virtual bool CloneNewInstances => false;

		/// <summary>
		/// Returns a clone of this ModItem. 
		/// Allows you to decide which fields of your ModItem class are copied over when an item stack is split or something similar happens. 
		/// By default this will return a memberwise clone; you will want to override this if your ModItem contains object references. 
		/// Only called if CloneNewInstances is set to true.
		/// Since several ModItem class fields are also set by the default implementation of this method, you'll most likely want to call base.Clone() as the first statement of your override.
		/// </summary>
		/// <example><code>var clone = (ExampleHookItem)base.Clone();
		/// clone.targets = (int[])this.targets.Clone(); // Or whatever deep copy operations are relevant.
		/// return clone;</code></example>
		public virtual ModItem Clone() => (ModItem)MemberwiseClone();

		/// <summary>
		/// Create a copy of this instanced ModItem. Called when an item is cloned.
		/// Defaults to NewInstance(item)
		/// </summary>
		/// <param name="item">The item being cloned</param>
		/// <param name="itemClone">The new item</param>
		public virtual ModItem Clone(Item item) => NewInstance(item);

		/// <summary>
		/// Create a new instance of this ModItem for an Item instance. 
		/// Called at the end of Item.SetDefaults.
		/// If CloneNewInstances is true, just calls Clone()
		/// Otherwise calls the default constructor and copies fields
		/// </summary>
		public virtual ModItem NewInstance(Item itemClone) {
			if (CloneNewInstances) {
				var clone = Clone();
				clone.item = itemClone;
				return clone;
			}

			var copy = (ModItem)Activator.CreateInstance(GetType());
			copy.item = itemClone;
			copy.mod = mod;
			copy.Name = Name;
			copy.ProjOnSwing_Obsolete = ProjOnSwing_Obsolete;
			copy.BossBagNPC_Obsolete = BossBagNPC_Obsolete;
			return copy;
		}

		/// <summary>
		/// This is where you set all your item's properties, such as width, damage, shootSpeed, defense, etc. 
		/// For those that are familiar with tAPI, this has the same function as .json files.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Automatically sets certain defaults. Override this if you do not want the properties to be set for you.
		/// </summary>
		public virtual void AutoDefaults() {
			EquipLoader.SetSlot(item);
		}

		/// <summary>
		/// This is where you set all your item's static properties, such as names/translations and the arrays in ItemID.Sets.
		/// This is called after SetDefaults on the initial ModItem
		/// </summary>
		public virtual void SetStaticDefaults() {
		}

		/// <summary>
		/// Automatically sets certain static defaults. Override this if you do not want the properties to be set for you.
		/// </summary>
		public virtual void AutoStaticDefaults() {
			Main.itemTexture[item.type] = ModContent.GetTexture(Texture);

			var flameTexture = Texture + "_Flame";
			if (ModContent.TextureExists(flameTexture)) {
				Main.itemFlameTexture[item.type] = ModContent.GetTexture(flameTexture);
				Main.itemFlameLoaded[item.type] = true;
			}

			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(Name, "([A-Z])", " $1").Trim());
		}

		/// <summary>
		/// Allows you to manually choose what prefix an item will get.
		/// </summary>
		/// <returns>The ID of the prefix to give the item, -1 to use default vanilla behavior</returns>
		public virtual int ChoosePrefix(UnifiedRandom rand) => -1;

		/// <summary>
		/// To prevent putting the item in the tinkerer slot, return false when pre is -3
		/// To prevent rolling of a prefix on spawn, return false when pre is -1
		/// To force rolling of a prefix on spawn, return true when pre is -1
		/// 
		/// To reduce the probability of a prefix on spawn (pre == -1) to X%, return false 100-4X % of the time
		/// To increase the probability of a prefix on spawn (pre == -1) to X%, return true (4X-100)/3 % of the time
		/// 
		/// To delete a prefix from an item when the item is loaded, return false when pre is the prefix you want to delete.
		/// Use AllowPrefix to prevent rolling of a certain prefix
		/// </summary>
		/// <param name="pre">The prefix being applied to the item, or the roll mode. -1 is when an item is bought from a shop, crafted or generated in a chest. -2 is when the item is rolled in the tinkerer. -3 determines if the item can be placed in the tinkerer slot.</param>
		/// <returns></returns>
		public virtual bool? PrefixChance(int pre, UnifiedRandom rand) => null;

		/// <summary>
		/// Force a re-roll of a prefix by returning false.
		/// </summary>
		public virtual bool AllowPrefix(int pre) => true;

		/// <summary>
		/// Returns whether or not this item can be used. By default returns true.
		/// </summary>
		/// <param name="player">The player using the item.</param>
		public virtual bool CanUseItem(Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the location and rotation of this item in its use animation.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void UseStyle(Player player) {
		}

		/// <summary>
		/// Allows you to modify the location and rotation of this item when the player is holding it.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void HoldStyle(Player player) {
		}

		/// <summary>
		/// Allows you to make things happen when the player is holding this item (for example, torches make light and water candles increase spawn rate).
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void HoldItem(Player player) {
		}

		/// <summary>
		/// Allows you to change the effective useTime of this item.
		/// </summary>
		/// <param name="player"></param>
		/// <returns>The multiplier on the usage speed. 1f by default.</returns>
		public virtual float UseTimeMultiplier(Player player) {
			return 1f;
		}

		/// <summary>
		/// Allows you to change the effective useAnimation of this item.
		/// </summary>
		/// <param name="player"></param>
		/// <returns>The multiplier on the animation speed. 1f by default.</returns>
		public virtual float MeleeSpeedMultiplier(Player player) {
			return 1f;
		}

		/// <summary>
		/// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a healLife value.
		/// </summary>
		/// <param name="player">The player using the item.</param>
		/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
		/// <param name="healValue">The amount of life being healed.</param>
		public virtual void GetHealLife(Player player, bool quickHeal, ref int healValue) {
		}

		/// <summary>
		/// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a healMana value.
		/// </summary>
		/// <param name="player">The player using the item.</param>
		/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
		/// <param name="healValue">The amount of mana being healed.</param>
		public virtual void GetHealMana(Player player, bool quickHeal, ref int healValue) {
		}

		/// <summary>
		/// Allows you to temporarily modify the amount of mana this item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
		/// </summary>
		/// <param name="player">The player using the item.</param>
		/// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
		/// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
		public virtual void ModifyManaCost(Player player, ref float reduce, ref float mult) {
		}

		/// <summary>
		/// Allows you to make stuff happen when a player doesn't have enough mana for the item they are trying to use.
		/// If the player has high enough mana after this hook runs, mana consumption will happen normally.
		/// Only runs once per item use.
		/// </summary>
		/// <param name="player">The player using the item.</param>
		/// <param name="neededMana">The mana needed to use the item.</param>
		public virtual void OnMissingMana(Player player, int neededMana) {
		}

		/// <summary>
		/// Allows you to make stuff happen when a player consumes mana on use of this item.
		/// </summary>
		/// <param name="player">The player using the item.</param>
		/// <param name="manaConsumed">The mana consumed from the player.</param>
		public virtual void OnConsumeMana(Player player, int manaConsumed) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's damage based on player buffs, etc. This is useful for creating new classes of damage, or for making subclasses of damage (for example, Shroomite armor set boosts).
		/// Note that tModLoader follows vanilla principle of only allowing one effective damage class at a time.
		/// This means that if you want your own custom damage class, all vanilla damage classes must be set to false.
		/// Vanilla checks classes in this order: melee, ranged, magic, thrown, summon
		/// So if you set both melee class and another class to true, only the melee damage will actually be used.
		/// </summary>
		/// <param name="player">The player using the item</param>
		/// <param name="damage">The damage.</param>
		[Obsolete("Use ModifyWeaponDamage", true)]
		public virtual void GetWeaponDamage(Player player, ref int damage) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's damage based on player buffs, etc. This is useful for creating new classes of damage, or for making subclasses of damage (for example, Shroomite armor set boosts).
		/// </summary>
		/// <param name="player">The player using the item</param>
		/// <param name="add">Used for additively stacking buffs (most common). Only ever use += on this field.</param>
		/// <param name="mult">Use to directly multiply the player's effective damage. Good for debuffs, or things which should stack separately (eg ammo type buffs)</param>
		[Obsolete("Use ModifyWeaponDamage overload with the additional flat parameter")]
		public virtual void ModifyWeaponDamage(Player player, ref float add, ref float mult) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's damage based on player buffs, etc. This is useful for creating new classes of damage, or for making subclasses of damage (for example, Shroomite armor set boosts).
		/// </summary>
		/// <param name="player">The player using the item</param>
		/// <param name="add">Used for additively stacking buffs (most common). Only ever use += on this field. Things with effects like "5% increased MyDamageClass damage" would use this: `add += 0.05`</param>
		/// <param name="mult">Use to directly multiply the player's effective damage. Good for debuffs, or things which should stack separately (eg ammo type buffs)</param>
		/// <param name="flat">This is a flat damage bonus that will be added after add and mult are applied. It facilitates effects like "4 more damage from weapons"</param>
		public virtual void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's knockback based on player buffs, etc. This allows you to customize knockback beyond the Player class's limited fields.
		/// Note that tModLoader follows vanilla principle of only allowing one effective damage class at a time.
		/// This means that if you want your own custom damage class, all vanilla damage classes must be set to false.
		/// Vanilla checks classes in this order: melee, ranged, magic, thrown, summon
		/// So if you set both melee class and another class to true, only the melee knockback will actually be used.
		/// </summary>
		/// <param name="player">The player using the item</param>
		/// <param name="knockback">The knockback</param>
		public virtual void GetWeaponKnockback(Player player, ref float knockback) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's crit chance based on player buffs, etc.
		/// Note that tModLoader follows vanilla principle of only allowing one effective damage class at a time.
		/// This means that if you want your own custom damage class, all vanilla damage classes must be set to false.
		/// If you use a custom damage class, the crit value will equal item.crit
		/// Vanilla checks classes in this order: melee, ranged, magic, thrown, and summon cannot crit.
		/// So if you set both melee class and another class to true, only the melee crit will actually be used.
		/// </summary>
		/// <param name="player">The player using this item</param>
		/// <param name="crit">The critical strike chance, at 0 it will never trigger a crit and at 100 or above it will always trigger a crit</param>
		public virtual void GetWeaponCrit(Player player, ref int crit) {
		}

		/// <summary>
		/// Allows you to modify the projectile created by a weapon based on the ammo it is using. This hook is called on the ammo.
		/// </summary>
		/// <param name="weapon">The item that is using this ammo</param>
		/// <param name="player">The player using the item</param>
		/// <param name="type">The ID of the projectile shot</param>
		/// <param name="speed">The speed of the projectile shot</param>
		/// <param name="damage">The damage of the projectile shot</param>
		/// <param name="knockback">The speed of the projectile shot</param>
		public virtual void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
		}
		
		[Obsolete("PickAmmo now has a weapon parameter that represents the item using the ammo.")]
		public virtual void PickAmmo(Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
		}

		/// <summary>
		/// Whether or not ammo will be consumed upon usage. Called both by the gun and by the ammo; if at least one returns false then the ammo will not be used. By default returns true.
		/// If false is returned, the OnConsumeAmmo hook is never called.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool ConsumeAmmo(Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to makes things happen when ammo is consumed. Called both by the gun and by the ammo.
		/// Called before the ammo stack is reduced.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void OnConsumeAmmo(Player player) {
		}

		/// <summary>
		/// This is called before the weapon creates a projectile. You can use it to create special effects, such as changing the speed, changing the initial position, and/or firing multiple projectiles. Return false to stop the game from shooting the default projectile (do this if you manually spawn your own projectile). Returns true by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="position">The shoot spawn position.</param>
		/// <param name="speedX">The speed x calculated from shootSpeed and mouse position.</param>
		/// <param name="speedY">The speed y calculated from shootSpeed and mouse position.</param>
		/// <param name="type">The projectile type choosen by ammo and weapon.</param>
		/// <param name="damage">The projectile damage.</param>
		/// <param name="knockBack">The projectile knock back.</param>
		/// <returns></returns>
		public virtual bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			return true;
		}

		/// <summary>
		/// Changes the hitbox of this melee weapon when it is used.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="hitbox">The hitbox.</param>
		/// <param name="noHitbox">if set to <c>true</c> [no hitbox].</param>
		public virtual void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
		}

		/// <summary>
		/// Allows you to give this melee weapon special effects, such as creating light or dust.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="hitbox">The hitbox.</param>
		public virtual void MeleeEffects(Player player, Rectangle hitbox) {
		}

		/// <summary>
		/// Allows you to determine whether this melee weapon can hit the given NPC when swung. Return true to allow hitting the target, return false to block this weapon from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		public virtual bool? CanHitNPC(Player player, NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that this melee weapon does to an NPC.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="target">The target.</param>
		/// <param name="damage">The damage.</param>
		/// <param name="knockBack">The knock back.</param>
		/// <param name="crit">if set to <c>true</c> [crit].</param>
		public virtual void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this melee weapon hits an NPC (for example how the Pumpkin Sword creates pumpkin heads).
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="target">The target.</param>
		/// <param name="damage">The damage.</param>
		/// <param name="knockBack">The knock back.</param>
		/// <param name="crit">if set to <c>true</c> [crit].</param>
		public virtual void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether this melee weapon can hit the given opponent player when swung. Return false to block this weapon from hitting the target. Returns true by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="target">The target.</param>
		/// <returns>
		///   <c>true</c> if this instance [can hit PVP] the specified player; otherwise, <c>false</c>.
		/// </returns>
		public virtual bool CanHitPvp(Player player, Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that this melee weapon does to a player.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="target">The target.</param>
		/// <param name="damage">The damage.</param>
		/// <param name="crit">if set to <c>true</c> [crit].</param>
		public virtual void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this melee weapon hits a player.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="target">The target.</param>
		/// <param name="damage">The damage.</param>
		/// <param name="crit">if set to <c>true</c> [crit].</param>
		public virtual void OnHitPvp(Player player, Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to make things happen when this item is used. Return true if using this item actually does stuff. Returns false by default.
		/// Runs on all clients and server. Use <code>if (player.whoAmI == Main.myPlayer)</code> and <code>if (Main.netMode == NetmodeID.??)</code> if appropriate.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool UseItem(Player player) {
			return false;
		}

		/// <summary>
		/// If this item is consumable and this returns true, then this item will be consumed upon usage. Returns true by default.
		/// If false is returned, the OnConsumeItem hook is never called.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool ConsumeItem(Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when this item is consumed.
		/// Called before the item stack is reduced.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void OnConsumeItem(Player player) {
		}

		/// <summary>
		/// Allows you to modify the player's animation when this item is being used. Return true if you modify the player's animation. Returns false by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool UseItemFrame(Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to modify the player's animation when the player is holding this item. Return true if you modify the player's animation. Returns false by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool HoldItemFrame(Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to make this item usable by right-clicking. Returns false by default. When this item is used by right-clicking, player.altFunctionUse will be set to 2.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool AltFunctionUse(Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to make things happen when this item is in the player's inventory (for example, how the cell phone makes information display).
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void UpdateInventory(Player player) {
		}

		/// <summary>
		/// Allows you to give effects to this armor or accessory, such as increased damage.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void UpdateEquip(Player player) {
		}

		/// <summary>
		/// Allows you to give effects to this accessory. The hideVisual parameter is whether the player has marked the accessory slot to be hidden from being drawn on the player.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="hideVisual">if set to <c>true</c> the accessory is hidden.</param>
		public virtual void UpdateAccessory(Player player, bool hideVisual) {
		}

		/// <summary>
		/// Allows you to create special effects (such as dust) when this item's equipment texture of the given equipment type is displayed on the player. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="type">The type.</param>
		public virtual void UpdateVanity(Player player, EquipType type) {
		}

		/// <summary>
		/// Returns whether or not the head armor, body armor, and leg armor make up a set. If this returns true, then this item's UpdateArmorSet method will be called. Returns false by default.
		/// </summary>
		/// <param name="head">The head.</param>
		/// <param name="body">The body.</param>
		/// <param name="legs">The legs.</param>
		public virtual bool IsArmorSet(Item head, Item body, Item legs) {
			return false;
		}

		/// <summary>
		/// Allows you to give set bonuses to the armor set that this armor is in. Set player.setBonus to a string for the bonus description.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void UpdateArmorSet(Player player) {
		}

		/// <summary>
		/// Returns whether or not the head armor, body armor, and leg armor textures make up a set. This hook is used for the PreUpdateVanitySet, UpdateVanitySet, and ArmorSetShadow hooks. By default, this will return the same value as the IsArmorSet hook (passing the equipment textures' associated items as parameters), so you will not have to use this hook unless you want vanity effects to be entirely separate from armor sets. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="head">The head.</param>
		/// <param name="body">The body.</param>
		/// <param name="legs">The legs.</param>
		public virtual bool IsVanitySet(int head, int body, int legs) {
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
		/// Allows you to create special effects (such as the necro armor's hurt noise) when the player wears this item's vanity set. This hook is called regardless of whether the player is frozen in any way. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void PreUpdateVanitySet(Player player) {
		}

		/// <summary>
		/// Allows you to create special effects (such as dust) when the player wears this item's vanity set. This hook will only be called if the player is not frozen in any way. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void UpdateVanitySet(Player player) {
		}

		/// <summary>
		/// Allows you to determine special visual effects this vanity set has on the player without having to code them yourself. Note that this hook is only ever called through this item's associated equipment texture. Use the player.armorEffectDraw bools to activate the desired effects.
		/// </summary>
		/// <example><code>player.armorEffectDrawShadow = true;</code></example>
		/// <param name="player">The player.</param>
		public virtual void ArmorSetShadows(Player player) {
		}

		/// <summary>
		/// Allows you to modify the equipment that the player appears to be wearing. This hook will only be called for body armor and leg armor. Note that equipSlot is not the same as the item type of the armor the player will appear to be wearing. Worn equipment has a separate set of IDs. You can find the vanilla equipment IDs by looking at the headSlot, bodySlot, and legSlot fields for items, and modded equipment IDs by looking at EquipLoader.
		/// If this hook is called on body armor, equipSlot allows you to modify the leg armor the player appears to be wearing. If you modify it, make sure to set robes to true. If this hook is called on leg armor, equipSlot allows you to modify the leg armor the player appears to be wearing, and the robes parameter is useless.
		/// Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="male">if set to <c>true</c> [male].</param>
		/// <param name="equipSlot">The equip slot.</param>
		/// <param name="robes">if set to <c>true</c> [robes].</param>
		public virtual void SetMatch(bool male, ref int equipSlot, ref bool robes) {
		}

		/// <summary>
		/// Returns whether or not this item does something when it is right-clicked in the inventory. Returns false by default.
		/// </summary>
		public virtual bool CanRightClick() {
			return false;
		}

		/// <summary>
		/// Allows you to make things happen when this item is right-clicked in the inventory. Useful for goodie bags.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void RightClick(Player player) {
		}

		/// <summary>
		/// Allows you to give items to the given player when this item is right-clicked in the inventory if the bossBagNPC field has been set to a positive number. This ignores the CanRightClick and RightClick hooks.
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual void OpenBossBag(Player player) {
		}

		/// <summary>
		/// Returns if the normal reforge pricing is applied. 
		/// If true or false is returned and the price is altered, the price will equal the altered price.
		/// The passed reforge price equals the item.value. Vanilla pricing will apply 20% discount if applicable and then price the reforge at a third of that value.
		/// </summary>
		public virtual bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount) {
			return true;
		}

		/// <summary>
		/// This hook gets called when the player clicks on the reforge button and can afford the reforge.
		/// Returns whether the reforge will take place. If false is returned, the PostReforge hook is never called.
		/// Reforging preserves modded data on the item. 
		/// </summary>
		public virtual bool NewPreReforge() {
			return true;
		}

		// @todo: PreReforge marked obsolete until v0.11
		[method: Obsolete("PreReforge now returns a bool to control whether the reforge takes place. For now, use NewPreReforge")]
		public virtual void PreReforge() {
			item.modItem?.NewPreReforge();
		}

		/// <summary>
		/// This hook gets called immediately after an item gets reforged by the Goblin Tinkerer.
		/// Useful for modifying modded data based on the reforge result.
		/// </summary>
		public virtual void PostReforge() {
		}

		/// <summary>
		/// Allows you to determine whether the skin/shirt on the player's arms and hands are drawn when this body armor is worn. By default both flags will be false. Note that if drawHands is false, the arms will not be drawn either. Also note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="drawHands">if set to <c>true</c> [draw hands].</param>
		/// <param name="drawArms">if set to <c>true</c> [draw arms].</param>
		public virtual void DrawHands(ref bool drawHands, ref bool drawArms) {
		}

		/// <summary>
		/// Allows you to determine whether the player's hair or alt (hat) hair draws when this head armor is worn. By default both flags will be false. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="drawHair">if set to <c>true</c> [draw hair].</param>
		/// <param name="drawAltHair">if set to <c>true</c> [draw alt hair].</param>
		public virtual void DrawHair(ref bool drawHair, ref bool drawAltHair) {
		}

		/// <summary>
		/// Return false to hide the player's head when this head armor is worn. Returns true by default. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <returns></returns>
		public virtual bool DrawHead() {
			return true;
		}

		/// <summary>
		/// Return false to hide the player's body when this body armor is worn. Returns true by default. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <returns></returns>
		public virtual bool DrawBody() {
			return true;
		}

		/// <summary>
		/// Return false to hide the player's legs when this leg armor or shoe accessory is worn. Returns true by default. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <returns></returns>
		public virtual bool DrawLegs() {
			return true;
		}

		/// <summary>
		/// Allows you to modify the colors in which this armor and surrounding accessories are drawn, in addition to which glow mask and in what color is drawn. Note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="drawPlayer">The draw player.</param>
		/// <param name="shadow">The shadow.</param>
		/// <param name="color">The color.</param>
		/// <param name="glowMask">The glow mask.</param>
		/// <param name="glowMaskColor">Color of the glow mask.</param>
		public virtual void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
		}

		/// <summary>
		/// Allows you to modify which glow mask and in what color is drawn on the player's arms. Note that this is only called for body armor. Also note that this hook is only ever called through this item's associated equipment texture.
		/// </summary>
		/// <param name="drawPlayer">The draw player.</param>
		/// <param name="shadow">The shadow.</param>
		/// <param name="glowMask">The glow mask.</param>
		/// <param name="color">The color.</param>
		public virtual void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
		}

		/// <summary>
		/// Allows you to modify the speeds at which you rise and fall when these wings are equipped.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="ascentWhenFalling">The ascent when falling.</param>
		/// <param name="ascentWhenRising">The ascent when rising.</param>
		/// <param name="maxCanAscendMultiplier">The maximum can ascend multiplier.</param>
		/// <param name="maxAscentMultiplier">The maximum ascent multiplier.</param>
		/// <param name="constantAscend">The constant ascend.</param>
		public virtual void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
	ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
		}

		/// <summary>
		/// Allows you to modify these wing's horizontal flight speed and acceleration.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="speed">The speed.</param>
		/// <param name="acceleration">The acceleration.</param>
		public virtual void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration) {
		}

		/// <summary>
		/// Allows for Wings to do various things while in use. "inUse" is whether or not the jump button is currently pressed. Called when these wings visually appear on the player. Use to animate wings, create dusts, invoke sounds, and create lights. Note that this hook is only ever called through this item's associated equipment texture. False will keep everything the same. True, you need to handle all animations in your own code.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="inUse">if set to <c>true</c> [in use].</param>
		/// <returns></returns>
		public virtual bool WingUpdate(Player player, bool inUse) {
			return false;
		}

		/// <summary>
		/// Allows you to customize this item's movement when lying in the world. Note that this will not be called if this item is currently being grabbed by a player.
		/// </summary>
		/// <param name="gravity">The gravity.</param>
		/// <param name="maxFallSpeed">The maximum fall speed.</param>
		public virtual void Update(ref float gravity, ref float maxFallSpeed) {
		}

		/// <summary>
		/// Allows you to make things happen when this item is lying in the world. This will always be called, even when it is being grabbed by a player. This hook should be used for adding light, or for increasing the age of less valuable items.
		/// </summary>
		public virtual void PostUpdate() {
		}

		/// <summary>
		/// Allows you to modify how close this item must be to the player in order to move towards the player.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="grabRange">The grab range.</param>
		public virtual void GrabRange(Player player, ref int grabRange) {
		}

		/// <summary>
		/// Allows you to modify the way this item moves towards the player. Return true if you override this hook; returning false will allow the vanilla grab style to take place. Returns false by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool GrabStyle(Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to determine whether or not the item can be picked up
		/// </summary>
		/// <param name="player">The player.</param>
		public virtual bool CanPickup(Player player) {
			return true;
		}

		/// <summary>
		/// Allows you to make special things happen when the player picks up this item. Return false to stop the item from being added to the player's inventory; returns true by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool OnPickup(Player player) {
			return true;
		}

		/// <summary>
		/// Return true to specify that the item can be picked up despite not having enough room in inventory. Useful for something like hearts or experience items. Use in conjunction with OnPickup to actually consume the item and handle it.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <returns></returns>
		public virtual bool ItemSpace(Player player) {
			return false;
		}

		/// <summary>
		/// Allows you to determine the color and transparency in which this item is drawn. Return null to use the default color (normally light color). Returns null by default.
		/// </summary>
		/// <param name="lightColor">Color of the light.</param>
		/// <returns></returns>
		public virtual Color? GetAlpha(Color lightColor) {
			return null;
		}

		/// <summary>
		/// Allows you to draw things behind this item, or to modify the way this item is drawn in the world. Return false to stop the game from drawing the item (useful if you're manually drawing the item). Returns true by default.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		/// <param name="lightColor">Color of the light.</param>
		/// <param name="alphaColor">Color of the alpha.</param>
		/// <param name="rotation">The rotation.</param>
		/// <param name="scale">The scale.</param>
		/// <param name="whoAmI">The who am i.</param>
		/// <returns></returns>
		public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of this item. This method is called even if PreDrawInWorld returns false.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		/// <param name="lightColor">Color of the light.</param>
		/// <param name="alphaColor">Color of the alpha.</param>
		/// <param name="rotation">The rotation.</param>
		/// <param name="scale">The scale.</param>
		/// <param name="whoAmI">The who am i.</param>
		public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
		}

		/// <summary>
		/// Allows you to draw things behind this item in the inventory. Return false to stop the game from drawing the item (useful if you're manually drawing the item). Returns true by default.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		/// <param name="position">The position.</param>
		/// <param name="frame">The frame.</param>
		/// <param name="drawColor">Color of the draw.</param>
		/// <param name="itemColor">Color of the item.</param>
		/// <param name="origin">The origin.</param>
		/// <param name="scale">The scale.</param>
		/// <returns></returns>
		public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
			Color itemColor, Vector2 origin, float scale) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of this item in the inventory. This method is called even if PreDrawInInventory returns false.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		/// <param name="position">The position.</param>
		/// <param name="frame">The frame.</param>
		/// <param name="drawColor">Color of the draw.</param>
		/// <param name="itemColor">Color of the item.</param>
		/// <param name="origin">The origin.</param>
		/// <param name="scale">The scale.</param>
		public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
			Color itemColor, Vector2 origin, float scale) {
		}

		/// <summary>
		/// Allows you to determine the offset of this item's sprite when used by the player. This is only used for items with a useStyle of 5 that aren't staves. Return null to use the vanilla holdout offset; returns null by default.
		/// </summary>
		/// <returns></returns>
		public virtual Vector2? HoldoutOffset() {
			return null;
		}

		/// <summary>
		/// Allows you to determine the point on this item's sprite that the player holds onto when using this item. The origin is from the bottom left corner of the sprite. This is only used for staves with a useStyle of 5. Return null to use the vanilla holdout origin (zero); returns null by default.
		/// </summary>
		/// <returns></returns>
		public virtual Vector2? HoldoutOrigin() {
			return null;
		}

		/// <summary>
		/// Allows you to disallow the player from equipping this accessory. Return false to disallow equipping this accessory. Returns true by default.
		/// </summary>
		/// <param name="player">The player.</param>
		/// <param name="slot">The inventory slot that the item is attempting to occupy.</param>
		public virtual bool CanEquipAccessory(Player player, int slot) {
			return true;
		}

		/// <summary>
		/// Allows you to modify what item, and in what quantity, is obtained when this item is fed into the Extractinator. By default the parameters will be set to the output of feeding Silt/Slush into the Extractinator.
		/// </summary>
		/// <param name="resultType">Type of the result.</param>
		/// <param name="resultStack">The result stack.</param>
		public virtual void ExtractinatorUse(ref int resultType, ref int resultStack) {
		}

		/// <summary>
		/// Allows you to tell the game whether this item is a torch that cannot be placed in liquid, a torch that can be placed in liquid, or a glowstick. This information is used for when the player is holding down the auto-select hotkey.
		/// </summary>
		/// <param name="dryTorch">if set to <c>true</c> [dry torch].</param>
		/// <param name="wetTorch">if set to <c>true</c> [wet torch].</param>
		/// <param name="glowstick">if set to <c>true</c> [glowstick].</param>
		public virtual void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick) {
		}

		/// <summary>
		/// Allows you to determine how many of this item a player obtains when the player fishes this item.
		/// </summary>
		/// <param name="stack">The stack.</param>
		public virtual void CaughtFishStack(ref int stack) {
		}

		/// <summary>
		/// Whether or not the Angler can ever randomly request this type of item for his daily quest. Returns false by default.
		/// </summary>
		public virtual bool IsQuestFish() {
			return false;
		}

		/// <summary>
		/// Whether or not specific conditions have been satisfied for the Angler to be able to request this item. (For example, Hardmode.) Returns true by default.
		/// </summary>
		public virtual bool IsAnglerQuestAvailable() {
			return true;
		}

		/// <summary>
		/// Allows you to set what the Angler says when he requests for this item. The description parameter is his dialogue, and catchLocation should be set to "\n(Caught at [location])".
		/// </summary>
		/// <param name="description">The description.</param>
		/// <param name="catchLocation">The catch location.</param>
		public virtual void AnglerQuestChat(ref string description, ref string catchLocation) {
		}

		/// <summary>
		/// Setting this to true makes it so that this weapon can shoot projectiles only at the beginning of its animation. Set this to true if you want a sword and its projectile creation to be in sync (for example, the Terra Blade). Defaults to false.
		/// </summary>
		public virtual bool OnlyShootOnSwing => ProjOnSwing_Obsolete;

		/// <summary>
		/// The type of NPC that drops this boss bag. Used to determine how many coins this boss bag contains. Defaults to 0, which means this isn't a boss bag.
		/// </summary>
		public virtual int BossBagNPC => BossBagNPC_Obsolete;

		/// <summary>
		/// Set this to true to prevent this weapon or ammo item from being adjusted by damage modifiers.
		/// </summary>
		public virtual bool IgnoreDamageModifiers => false;

		/// <summary>
		/// Allows you to save custom data for this item. Returns null by default.
		/// </summary>
		/// <returns></returns>
		public virtual TagCompound Save() {
			return null;
		}

		/// <summary>
		/// Allows you to load custom data that you have saved for this item.
		/// </summary>
		/// <param name="tag">The tag.</param>
		public virtual void Load(TagCompound tag) {
		}

		/// <summary>
		/// Allows you to load pre-v0.9 custom data that you have saved for this item.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public virtual void LoadLegacy(BinaryReader reader) {
		}

		/// <summary>
		/// Allows you to send custom data for this item between client and server.
		/// </summary>
		/// <param name="writer">The writer.</param>
		public virtual void NetSend(BinaryWriter writer) {
		}

		/// <summary>
		/// Receives the custom data sent in the NetSend hook.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public virtual void NetRecieve(BinaryReader reader) {
		}

		/// <summary>
		/// This is essentially the same as Mod.AddRecipes. Do note that this will be called for every instance of the overriding ModItem class that is added to the game. This allows you to avoid clutter in your overriding Mod class by adding recipes for which this item is the result.
		/// </summary>
		public virtual void AddRecipes() {
		}

		/// <summary>
		/// Allows you to make anything happen when the player crafts this item using the given recipe.
		/// </summary>
		/// <param name="recipe">The recipe that was used to craft this item.</param>
		public virtual void OnCraft(Recipe recipe) {
		}

		/// <summary>
		/// Allows you to do things before this item's tooltip is drawn.
		/// </summary>
		/// <param name="lines">The tooltip lines for this item</param>
		/// <param name="x">The top X position for this tooltip. It is where the first line starts drawing</param>
		/// <param name="y">The top Y position for this tooltip. It is where the first line starts drawing</param>
		/// <returns>Whether or not to draw this tooltip</returns>
		public virtual bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y) {
			return true;
		}

		/// <summary>
		/// Allows you to do things after this item's tooltip is drawn. The lines contain draw information as this is ran after drawing the tooltip.
		/// </summary>
		/// <param name="lines">The tooltip lines for this item</param>
		public virtual void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines) {
		}

		/// <summary>
		/// Allows you to do things before a tooltip line of this item is drawn. The line contains draw info.
		/// </summary>
		/// <param name="line">The line that would be drawn</param>
		/// <param name="yOffset">The Y offset added for next tooltip lines</param>
		/// <returns>Whether or not to draw this tooltip line</returns>
		public virtual bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset) {
			return true;
		}

		/// <summary>
		/// Allows you to do things after a tooltip line of this item is drawn. The line contains draw info.
		/// </summary>
		/// <param name="line">The line that was drawn</param>
		public virtual void PostDrawTooltipLine(DrawableTooltipLine line) {
		}

		/// <summary>
		/// Allows you to modify all the tooltips that display for this item. See here for information about TooltipLine.
		/// </summary>
		/// <param name="tooltips">The tooltips.</param>
		public virtual void ModifyTooltips(List<TooltipLine> tooltips) {
		}
	}
}