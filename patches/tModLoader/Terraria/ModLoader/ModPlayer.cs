using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModPlayer instance represents an extension of a Player instance. You can store fields in the ModPlayer classes, much like how the Player class abuses field usage, to keep track of mod-specific information on the player that a ModPlayer instance represents. It also contains hooks to insert your code into the Player class.
	/// </summary>
	public abstract class ModPlayer : ModType
	{
		/// <summary>
		/// The Player instance that this ModPlayer instance is attached to.
		/// </summary>
		public Player Player { get; internal set; }

		internal int index;

		internal ModPlayer CreateFor(Player newPlayer) {
			ModPlayer modPlayer = (ModPlayer)(CloneNewInstances ? MemberwiseClone() : Activator.CreateInstance(GetType()));
			modPlayer.Mod = Mod;
			modPlayer.Player = newPlayer;
			modPlayer.index = index;
			modPlayer.Initialize();
			return modPlayer;
		}

		public bool TypeEquals(ModPlayer other) {
			return Mod == other.Mod && Name == other.Name;
		}

		/// <summary>
		/// Whether each player gets a ModPlayer by cloning the ModPlayer added to the Mod or by creating a new ModPlayer object with the same type as the ModPlayer added to the Mod. The accessor returns true by default. Return false if you want to assign fields through the constructor.
		/// </summary>
		public virtual bool CloneNewInstances => true;
		
		protected sealed override void Register() {
			ModTypeLookup<ModPlayer>.Register(this);
			PlayerLoader.Add(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();

		/// <summary>
		/// Called whenever the player is loaded (on the player selection screen). This can be used to initialize data structures, etc.
		/// </summary>
		public virtual void Initialize() {
		}

		/// <summary>
		/// This is where you reset any fields you add to your ModPlayer subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied.
		/// </summary>
		public virtual void ResetEffects() {
		}

		/// <summary>
		/// Similar to UpdateDead, except this is only called when the player is dead. If this is called, then ResetEffects will not be called.
		/// </summary>
		public virtual void UpdateDead() {
		}

		/// <summary>
		/// Currently never gets called, so this is useless.
		/// </summary>
		public virtual void PreSaveCustomData() {
		}

		/// <summary>
		/// Allows you to save custom data for this player. Returns null by default.
		/// </summary>
		/// <returns></returns>
		public virtual TagCompound Save() {
			return null;
		}

		/// <summary>
		/// Allows you to load custom data you have saved for this player.
		/// </summary>
		/// <param name="tag"></param>
		public virtual void Load(TagCompound tag) {
		}

		/// <summary>
		/// PreSavePlayer and PostSavePlayer wrap the vanilla player saving code (both are before the ModPlayer.Save). Useful for advanced situations where a save might be corrupted or rendered unusable by the values that normally would save.
		/// </summary>
		public virtual void PreSavePlayer() {
		}

		/// <summary>
		/// PreSavePlayer and PostSavePlayer wrap the vanilla player saving code (both are before the ModPlayer.Save). Useful for advanced situations where a save might be corrupted or rendered unusable by the values that normally would save.
		/// </summary>
		public virtual void PostSavePlayer() {
		}

		/// <summary>
		/// Allows you to copy information about this player to the clientClone parameter. You should copy information that you intend to sync between server and client. This hook is called in the Player.clientClone method. See SendClientChanges for more info.
		/// </summary>
		/// <param name="clientClone"></param>
		public virtual void clientClone(ModPlayer clientClone) {
		}

		/// <summary>
		/// Allows you to sync information about this player between server and client. The toWho and fromWho parameters correspond to the remoteClient/toClient and ignoreClient arguments, respectively, of NetMessage.SendData/ModPacket.Send. The newPlayer parameter is whether or not the player is joining the server (it is true on the joining client).
		/// </summary>
		/// <param name="toWho"></param>
		/// <param name="fromWho"></param>
		/// <param name="newPlayer"></param>
		public virtual void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
		}

		/// <summary>
		/// Allows you to sync any information that has changed between the server and client. Here, you should check the information you have copied in the clientClone parameter; if they differ between this player and the clientPlayer parameter, then you should send that information using NetMessage.SendData or ModPacket.Send.
		/// </summary>
		/// <param name="clientPlayer"></param>
		public virtual void SendClientChanges(ModPlayer clientPlayer) {
		}

		/// <summary>
		/// Allows you to change the background that displays when viewing the map. Return null if you do not want to change the background. Returns null by default.
		/// </summary>
		/// <returns></returns>
		public virtual Texture2D GetMapBackgroundImage() {
			return null;
		}

		/// <summary>
		/// Allows you to give the player a negative life regeneration based on its state (for example, the "On Fire!" debuff makes the player take damage-over-time). This is typically done by setting player.lifeRegen to 0 if it is positive, setting player.lifeRegenTime to 0, and subtracting a number from player.lifeRegen. The player will take damage at a rate of half the number you subtract per second.
		/// </summary>
		public virtual void UpdateBadLifeRegen() {
		}

		/// <summary>
		/// Allows you to increase the player's life regeneration based on its state. This can be done by incrementing player.lifeRegen by a certain number. The player will recover life at a rate of half the number you add per second. You can also increment player.lifeRegenTime to increase the speed at which the player reaches its maximum natural life regeneration.
		/// </summary>
		public virtual void UpdateLifeRegen() {
		}

		/// <summary>
		/// Allows you to modify the power of the player's natural life regeneration. This can be done by multiplying the regen parameter by any number. For example, campfires multiply it by 1.1, while walking multiplies it by 0.5.
		/// </summary>
		/// <param name="regen"></param>
		public virtual void NaturalLifeRegen(ref float regen) {
		}

		/// <summary>
		/// Allows you to modify the player's stats while the game is paused due to the autopause setting being on.
		/// This is called in single player only, some time before the player's tick update would happen when the game isn't paused.
		/// </summary>
		public virtual void UpdateAutopause() {
		}

		/// <summary>
		/// This is called at the beginning of every tick update for this player, after checking whether the player exists.
		/// </summary>
		public virtual void PreUpdate() {
		}

		/// <summary>
		/// Use this to check on keybinds you have registered. While SetControls is set even while in text entry mode, this hook is only called during gameplay.
		/// </summary>
		/// <param name="triggersSet"></param>
		public virtual void ProcessTriggers(TriggersSet triggersSet) {
		}

		/// <summary>
		/// Use this to modify the control inputs that the player receives. For example, the Confused debuff swaps the values of player.controlLeft and player.controlRight. This is called sometime after PreUpdate is called.
		/// </summary>
		public virtual void SetControls() {
		}

		/// <summary>
		/// This is called sometime after SetControls is called, and right before all the buffs update on this player. This hook can be used to add buffs to the player based on the player's state (for example, the Campfire buff is added if the player is near a Campfire).
		/// </summary>
		public virtual void PreUpdateBuffs() {
		}

		/// <summary>
		/// This is called right after all of this player's buffs update on the player. This can be used to modify the effects that the buff updates had on this player, and can also be used for general update tasks.
		/// </summary>
		public virtual void PostUpdateBuffs() {
		}

		/// <summary>
		/// Called after Update Accessories. 
		/// </summary>
		public virtual void UpdateEquips() {
		}

		/// <summary>
		/// This is called right after all of this player's equipment and armor sets update on the player, which is sometime after PostUpdateBuffs is called. This can be used to modify the effects that the equipment had on this player, and can also be used for general update tasks.
		/// </summary>
		public virtual void PostUpdateEquips() {
		}

		/// <summary>
		/// This is called after miscellaneous update code is called in Player.Update, which is sometime after PostUpdateEquips is called. This can be used for general update tasks.
		/// </summary>
		public virtual void PostUpdateMiscEffects() {
		}

		/// <summary>
		/// This is called after the player's horizontal speeds are modified, which is sometime after PostUpdateMiscEffects is called, and right before the player's horizontal position is updated. Use this to modify maxRunSpeed, accRunSpeed, runAcceleration, and similar variables before the player moves forwards/backwards.
		/// </summary>
		public virtual void PostUpdateRunSpeeds() {
		}

		/// <summary>
		/// This is called right before modifying the player's position based on velocity. Use this to make direct changes to the velocity.
		/// </summary>
		public virtual void PreUpdateMovement() {
		}

		/// <summary>
		/// This is called at the very end of the Player.Update method. Final general update tasks can be placed here.
		/// </summary>
		public virtual void PostUpdate() {
		}

		/// <summary>
		/// This is called after VanillaUpdateVanityAccessory() in player.UpdateEquips()
		/// </summary>
		public virtual void UpdateVanityAccessories() {
		}

		/// <summary>
		/// Allows you to modify the armor and accessories that visually appear on the player. In addition, you can create special effects around this character, such as creating dust.
		/// </summary>
		public virtual void FrameEffects() {
		}

		/// <summary>
		/// This hook is called before every time the player takes damage. The pvp parameter is whether the damage was from another player. The quiet parameter determines whether the damage will be communicated to the server. The damage, hitDirection, and crit parameters can be modified. Set the customDamage parameter to true if you want to use your own damage formula (this parameter will disable automatically subtracting the player's defense from the damage). Set the playSound parameter to false to disable the player's hurt sound, and the genGore parameter to false to disable the dust particles that spawn. (These are useful for creating your own sound or gore.) The deathText parameter can be modified to change the player's death message if the player dies. Return false to stop the player from taking damage. Returns true by default.
		/// </summary>
		/// <param name="pvp"></param>
		/// <param name="quiet"></param>
		/// <param name="damage"></param>
		/// <param name="hitDirection"></param>
		/// <param name="crit"></param>
		/// <param name="customDamage"></param>
		/// <param name="playSound"></param>
		/// <param name="genGore"></param>
		/// <param name="damageSource"></param>
		/// <returns></returns>
		public virtual bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			return true;
		}

		/// <summary>
		/// Allows you to make anything happen right before damage is subtracted from the player's health.
		/// </summary>
		/// <param name="pvp"></param>
		/// <param name="quiet"></param>
		/// <param name="damage"></param>
		/// <param name="hitDirection"></param>
		/// <param name="crit"></param>
		public virtual void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
		}

		/// <summary>
		/// Allows you to make anything happen when the player takes damage.
		/// </summary>
		/// <param name="pvp"></param>
		/// <param name="quiet"></param>
		/// <param name="damage"></param>
		/// <param name="hitDirection"></param>
		/// <param name="crit"></param>
		public virtual void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
		}

		/// <summary>
		/// This hook is called whenever the player is about to be killed after reaching 0 health. Set the playSound parameter to false to stop the death sound from playing. Set the genGore parameter to false to stop the gore and dust from being created. (These are useful for creating your own sound or gore.) Return false to stop the player from being killed. Only return false if you know what you are doing! Returns true by default.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="hitDirection"></param>
		/// <param name="pvp"></param>
		/// <param name="playSound"></param>
		/// <param name="genGore"></param>
		/// <param name="damageSource"></param>
		/// <returns></returns>
		public virtual bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore,
			ref PlayerDeathReason damageSource) {
			return true;
		}

		/// <summary>
		/// Allows you to make anything happen when the player dies.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="hitDirection"></param>
		/// <param name="pvp"></param>
		/// <param name="damageSource"></param>
		public virtual void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
		}

		/// <summary>
		/// Called before vanilla makes any luck calculations. Return false to prevent vanilla from making their luck calculations. Returns true by default.
		/// </summary>
		/// <param name="luck"></param>
		public virtual bool PreModifyLuck(ref float luck) {
			return true;
		}

		/// <summary>
		/// Allows you to modify a player's luck amount.
		/// </summary>
		/// <param name="luck"></param>
		public virtual void ModifyLuck(ref float luck) {
		}

		/// <summary>
		/// Allows you to do anything before the update code for the player's held item is run. Return false to stop the held item update code from being run (for example, if the player is frozen). Returns true by default.
		/// </summary>
		/// <returns></returns>
		public virtual bool PreItemCheck() {
			return true;
		}

		/// <summary>
		/// Allows you to do anything after the update code for the player's held item is run. Hooks for the middle of the held item update code have more specific names in ModItem and ModPlayer.
		/// </summary>
		public virtual void PostItemCheck() {
		}

		/// <summary>
		/// Allows you to change the effective useTime of an item.
		/// <br/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
		/// </summary>
		/// <returns> The multiplier on the usage time. 1f by default. Values greater than 1 increase the item use's length. </returns>
		public virtual float UseTimeMultiplier(Item item) => 1f;

		/// <summary>
		/// Allows you to change the effective useAnimation of an item.
		/// <br/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
		/// </summary>
		/// <returns>The multiplier on the animation time. 1f by default. Values greater than 1 increase the item animation's length. </returns>
		public virtual float UseAnimationMultiplier(Item item) => 1f;

		/// <summary>
		/// Allows you to safely change both useTime and useAnimation while keeping the values relative to each other.
		/// <br/> Useful for status effects.
		/// </summary>
		/// <returns> The multiplier on the use speed. 1f by default. Values greater than 1 increase the overall item speed. </returns>
		public virtual float UseSpeedMultiplier(Item item) => 1f;

		/// <summary>
		/// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a healLife value.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
		/// <param name="healValue">The amount of life being healed.</param>
		public virtual void GetHealLife(Item item, bool quickHeal, ref int healValue) {
		}

		/// <summary>
		/// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a healMana value.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
		/// <param name="healValue">The amount of mana being healed.</param>
		public virtual void GetHealMana(Item item, bool quickHeal, ref int healValue) {
		}

		/// <summary>
		/// Allows you to temporarily modify the amount of mana an item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
		/// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
		public virtual void ModifyManaCost(Item item, ref float reduce, ref float mult) {
		}

		/// <summary>
		/// Allows you to make stuff happen when a player doesn't have enough mana for the item they are trying to use.
		/// If the player has high enough mana after this hook runs, mana consumption will happen normally.
		/// Only runs once per item use.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="neededMana">The mana needed to use the item.</param>
		public virtual void OnMissingMana(Item item, int neededMana) {
		}

		/// <summary>
		/// Allows you to make stuff happen when a player consumes mana on use of an item.
		/// </summary>
		/// <param name="item">The item being used.</param>
		/// <param name="manaConsumed">The mana consumed from the player.</param>
		public virtual void OnConsumeMana(Item item, int manaConsumed) {
		}

		/// <summary>
		/// Allows you to temporarily modify this weapon's damage based on player buffs, etc. This is useful for creating new classes of damage, or for making subclasses of damage (for example, Shroomite armor set boosts).
		/// </summary>
		/// <param name="item">The item being used</param>
		/// <param name="add">Used for additively stacking buffs (most common). Only ever use += on this field. Things with effects like "5% increased MyDamageClass damage" would use this: `add += 0.05f`</param>
		/// <param name="mult">Use to directly multiply the player's effective damage. Good for debuffs, or things which should stack separately (eg ammo type buffs)</param>
		/// <param name="flat">This is a flat damage bonus that will be added after add and mult are applied. It facilitates effects like "4 more damage from weapons"</param>
		public virtual void ModifyWeaponDamage(Item item, ref StatModifier damage, ref float flat) {
		}

		/// <summary>
		/// Allows you to temporarily modify a weapon's knockback based on player buffs, etc. This allows you to customize knockback beyond the Player class's limited fields.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="knockback"></param>
		public virtual void ModifyWeaponKnockback(Item item, ref StatModifier knockback, ref float flat) {
		}

		/// <summary>
		/// Allows you to temporarily modify a weapon's crit chance based on player buffs, etc.
		/// </summary>
		/// <param name="item">The item</param>
		/// <param name="crit">The crit chance, ranging from 0 to 100</param>
		public virtual void ModifyWeaponCrit(Item item, ref int crit) {
		}

		/// <summary>
		/// Whether or not ammo will be consumed upon usage. Return false to stop the ammo from being depleted. Returns true by default.
		/// If false is returned, the OnConsumeAmmo hook is never called.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="ammo"></param>
		/// <returns></returns>
		public virtual bool ConsumeAmmo(Item weapon, Item ammo) {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when ammo is consumed.
		/// Called before the ammo stack is reduced.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="ammo"></param>
		/// <returns></returns>
		public virtual void OnConsumeAmmo(Item weapon, Item ammo) {
		}

		/// <summary>
		/// Allows you to prevent an item from shooting a projectile on use. Returns true by default.
		/// </summary>
		/// <param name="item"> The item being used. </param>
		/// <returns></returns>
		public virtual bool CanShoot(Item item) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by an item.
		/// </summary>
		/// <param name="item"> The item being used. </param>
		/// <param name="position"> The center position of the projectile. </param>
		/// <param name="velocity"> The velocity of the projectile. </param>
		/// <param name="type"> The ID of the projectile. </param>
		/// <param name="damage"> The damage of the projectile. </param>
		/// <param name="knockback"> The knockback of the projectile. </param>
		public virtual void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		}

		/// <summary>
		/// Allows you to modify an item's shooting mechanism. Return false to prevent vanilla's shooting code from running. Returns true by default.
		/// </summary>
		/// <param name="item"> The item being used. </param>
		/// <param name="source"> The projectile source's information. </param>
		/// <param name="position"> The center position of the projectile. </param>
		/// <param name="velocity"> The velocity of the projectile. </param>
		/// <param name="type"> The ID of the projectile. </param>
		/// <param name="damage"> The damage of the projectile. </param>
		/// <param name="knockback"> The knockback of the projectile. </param>
		public virtual bool Shoot(Item item, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return true;
		}

		/// <summary>
		/// Allows you to give this player's melee weapon special effects, such as creating light or dust.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="hitbox"></param>
		public virtual void MeleeEffects(Item item, Rectangle hitbox) {
		}

		/// <summary>
		/// This hook is called when a player damages anything, whether it be an NPC or another player, using anything, whether it be a melee weapon or a projectile. The x and y parameters are the coordinates of the victim parameter's center.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="victim"></param>
		public virtual void OnHitAnything(float x, float y, Entity victim) {
		}

		/// <summary>
		/// Allows you to determine whether a player can hit the given NPC by swinging a melee weapon. Return true to allow hitting the target, return false to block this player from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool? CanHitNPC(Item item, NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that this player does to an NPC by swinging a melee weapon.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this player hits an NPC by swinging a melee weapon (for example how the Pumpkin Sword creates pumpkin heads).
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether a projectile created by this player can hit the given NPC. Return true to allow hitting the target, return false to block this projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool? CanHitNPCWithProj(Projectile proj, NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that a projectile created by this player does to an NPC.
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		/// <param name="hitDirection"></param>
		public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
		}

		/// <summary>
		/// Allows you to create special effects when a projectile created by this player hits an NPC (for example, inflicting debuffs).
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether a melee weapon swung by this player can hit the given opponent player. Return false to block this weapon from hitting the target. Returns true by default.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool CanHitPvp(Item item, Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that a melee weapon swung by this player does to an opponent player.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this player's melee weapon hits an opponent player.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitPvp(Item item, Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether a projectile created by this player can hit the given opponent player. Return false to block the projectile from hitting the target. Returns true by default.
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool CanHitPvpWithProj(Projectile proj, Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that a projectile created by this player does to an opponent player.
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when a projectile created by this player hits an opponent player.
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether the given NPC can hit this player. Return false to block this player from being hit by the NPC. Returns true by default. CooldownSlot determines which of the player's cooldown counters to use (-1, 0, or 1), and defaults to -1.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="cooldownSlot"></param>
		/// <returns></returns>
		public virtual bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that an NPC does to this player.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when an NPC hits this player (for example, inflicting debuffs).
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitByNPC(NPC npc, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether the given hostile projectile can hit this player. Return false to block this player from being hit. Returns true by default.
		/// </summary>
		/// <param name="proj"></param>
		/// <returns></returns>
		public virtual bool CanBeHitByProjectile(Projectile proj) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that a hostile projectile does to this player.
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when a hostile projectile hits this player.
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitByProjectile(Projectile proj, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to change the item the player gains from catching a fish. The fishingRod and bait parameters refer to the said items in the player's inventory. The liquidType parameter is 0 if the player is fishing in water, 1 for lava, and 2 for honey. The poolSize parameter is the tile size of the pool the player is fishing in. The worldLayer parameter is 0 if the player is in the sky, 1 if the player is on the surface, 2 if the player is underground, 3 if the player is in the caverns, and 4 if the player is in the underworld. The questFish parameter is the item ID for the day's Angler quest. Modify the caughtType parameter to change the item the player catches. The junk parameter is whether the player catches junk; you can set this to true if you make the player catch a junk item, and is mostly used to pass information (has no effect on the game).
		/// </summary>
		/// <param name="fishingRod"></param>
		/// <param name="bait"></param>
		/// <param name="power"></param>
		/// <param name="liquidType"></param>
		/// <param name="poolSize"></param>
		/// <param name="worldLayer"></param>
		/// <param name="questFish"></param>
		/// <param name="caughtType"></param>
		public virtual void CatchFish(Item fishingRod, Item bait, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType) {
		}

		/// <summary>
		/// Allows you to modify the player's fishing power. As an example of the type of stuff that should go here, the phase of the moon can influence fishing power.
		/// </summary>
		/// <param name="fishingRod"></param>
		/// <param name="bait"></param>
		/// <param name="fishingLevel"></param>
		public virtual void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) {
		}

		/// <summary>
		/// Allows you to add to, change, or remove from the items the player earns when finishing an Angler quest. The rareMultiplier is a number between 0.15 and 1 inclusively; the lower it is the higher chance there should be for the player to earn rare items.
		/// </summary>
		/// <param name="rareMultiplier"></param>
		/// <param name="rewardItems"></param>
		public virtual void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems) {
		}

		/// <summary>
		/// Allows you to modify what items are possible for the player to earn when giving a Strange Plant to the Dye Trader.
		/// </summary>
		/// <param name="rewardPool"></param>
		public virtual void GetDyeTraderReward(List<int> rewardPool) {
		}

		/// <summary>
		/// Allows you to create special effects when this player is drawn, such as creating dust, modifying the color the player is drawn in, etc. The fullBright parameter makes it so that the drawn player ignores the modified color and lighting. Note that the fullBright parameter only works if r, g, b, and/or a is not equal to 1. Make sure to add the indexes of any dusts you create to drawInfo.DustCache, and the indexes of any gore you create to drawInfo.GoreCache.
		/// </summary>
		/// <param name="drawInfo"></param>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <param name="a"></param>
		/// <param name="fullBright"></param>
		public virtual void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		}

		/// <summary>
		/// Allows you to modify the drawing parameters of the player before drawing begins.
		/// </summary>
		/// <param name="drawInfo"></param>
		public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
		}

		/// <summary>
		/// Allows you to reorder the player draw layers. 
		/// This is called once at the end of mod loading, not during the game.
		/// Use with extreme caution, or risk breaking other mods.
		/// </summary>
		/// <param name="positions">Add/remove/change the positions applied to each layer here</param>
		public virtual void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) {
		}

		/// <summary>
		/// Allows you to modify the visiblity of layers about to be drawn
		/// </summary>
		/// <param name="layers"></param>
		public virtual void HideDrawLayers(PlayerDrawSet drawInfo) {
		}

		/// <summary>
		/// Use this hook to modify Main.screenPosition after weapon zoom and camera lerp have taken place.
		/// </summary>
		public virtual void ModifyScreenPosition() {
		}

		/// <summary>
		/// Use this to modify the zoom factor for the player. The zoom correlates to the percentage of half the screen size the zoom can reach. A value of -1 passed in means no vanilla scope is in effect. A value of 1.0 means the scope can zoom half a screen width/height away, putting the player on the edge of the game screen. Vanilla values include .8, .6666, and .5.
		/// </summary>
		/// <param name="zoom"></param>
		public virtual void ModifyZoom(ref float zoom) {
		}

		/// <summary>
		/// Called on clients when a player connects.
		/// </summary>
		/// <param name="player">The player that connected.</param>
		public virtual void PlayerConnect(Player player) {
		}

		/// <summary>
		/// Called when a player disconnects.
		/// </summary>
		/// <param name="player">The player that disconnected.</param>
		public virtual void PlayerDisconnect(Player player) {
		}

		/// <summary>
		/// Called on the LocalPlayer when that player enters the world. SP and Client. Only called on the player who is entering. A possible use is ensuring that UI elements are reset to the configuration specified in data saved to the ModPlayer. Can also be used for informational messages.
		/// </summary>
		/// <param name="player">The player that entered the world.</param>
		public virtual void OnEnterWorld(Player player) {
		}

		/// <summary>
		/// Called when a player respawns in the world.
		/// </summary>
		/// <param name="player">The player that respawns</param>
		public virtual void OnRespawn(Player player) {
		}

		/// <summary>
		/// Called whenever the player shift-clicks an item slot. This can be used to override default clicking behavior (ie. selling, trashing, moving items).
		/// </summary>
		/// <param name="inventory">The array of items the slot is part of.</param>
		/// <param name="context">The Terraria.UI.ItemSlot.Context of the inventory.</param>
		/// <param name="slot">The index in the inventory of the clicked slot.</param>
		/// <returns>Whether or not to block the default code (sell, trash, move, etc) from running. Returns false by default.</returns>
		public virtual bool ShiftClickSlot(Item[] inventory, int context, int slot) {
			return false;
		}

		/// <summary>
		/// Called whenever the player sells an item to an NPC.
		/// </summary>
		/// <param name="vendor">The NPC vendor.</param>
		/// <param name="shopInventory">The current inventory of the NPC shop.</param>
		/// <param name="item">The item the player just sold.</param>
		public virtual void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
		}

		/// <summary>
		/// Return false to prevent a transaction. Called before the transaction.
		/// </summary>
		/// <param name="vendor">The NPC vendor.</param>
		/// <param name="shopInventory">The current inventory of the NPC shop.</param>
		/// <param name="item">The item the player is attempting to sell.</param>
		/// <returns></returns>
		public virtual bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) {
			return true;
		}

		/// <summary>
		/// Called whenever the player buys an item from an NPC.
		/// </summary>
		/// <param name="vendor">The NPC vendor.</param>
		/// <param name="shopInventory">The current inventory of the NPC shop.</param>
		/// <param name="item">The item the player just purchased.</param>
		public virtual void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
		}

		/// <summary>
		/// Return false to prevent a transaction. Called before the transaction.
		/// </summary>
		/// <param name="vendor">The NPC vendor.</param>
		/// <param name="shopInventory">The current inventory of the NPC shop.</param>
		/// <param name="item">The item the player is attempting to buy.</param>
		/// <returns></returns>
		public virtual bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item) {
			return true;
		}

		/// <summary>
		/// Return false to prevent an item from being used. By default returns true.
		/// </summary>
		/// <param name="item">The item the player is attempting to use.</param>
		public virtual bool CanUseItem(Item item) {
			return true;
		}

		/// <summary>
		/// Called on the Client while the nurse chat is displayed. Return false to prevent the player from healing. If you return false, you need to set chatText so the user knows why they can't heal.
		/// </summary>
		/// <param name="nurse">The Nurse NPC instance.</param>
		/// <param name="health">How much health the player gains.</param>
		/// <param name="removeDebuffs">If set to false, debuffs will not be healed.</param>
		/// <param name="chatText">Set this to the Nurse chat text that will display if healing is prevented.</param>
		/// <returns>True by default. False to prevent nurse services.</returns>
		public virtual bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) {
			return true;
		}

		/// <summary>
		/// Called on the Client while the nurse chat is displayed and after ModifyNurseHeal. Allows custom pricing for Nurse services. See https://terraria.gamepedia.com/Nurse for the default pricing.
		/// </summary>
		/// <param name="nurse">The Nurse NPC instance.</param>
		/// <param name="health">How much health the player gains.</param>
		/// <param name="removeDebuffs">Whether or not debuffs will be healed.</param>
		/// <param name="price"></param>
		public virtual void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price) {
		}

		/// <summary>
		/// Called on the Client after the player heals themselves with the Nurse NPC.
		/// </summary>
		/// <param name="nurse">The Nurse npc providing the heal.</param>
		/// <param name="health">How much health the player gained.</param>
		/// /// <param name="removeDebuffs">Whether or not debuffs were healed.</param>
		/// <param name="price">The price the player paid in copper coins.</param>
		public virtual void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) {
		}

		/// <summary>
		/// Called when the player is created in the menu.
		/// You can use this method to add items to the player's starting inventory, as well as their inventory when they respawn in mediumcore.
		/// </summary>
		/// <param name="mediumCoreDeath">Whether you are setting up a mediumcore player's inventory after their death.</param>
		/// <returns>An enumerable of the items you want to add. If you want to add nothing, return Enumerable.Empty<Item>().</returns>
		public virtual IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			return Enumerable.Empty<Item>();
		}

		/// <summary>
		/// Allows you to modify the items that will be added to the player's inventory. Useful if you want to stop vanilla or other mods from adding an item.
		/// You can access a mod's items by using the mod's internal name as the indexer, such as: additions["ModName"]. To access vanilla items you can use "Terraria" as the index.
		/// </summary>
		/// <param name="itemsByMod">The items that will be added. Each key is the internal mod name of the mod adding the items. Vanilla items use the "Terraria" key.</param>
		/// <param name="mediumCoreDeath">Whether you are setting up a mediumcore player's inventory after their death.</param>
		public virtual void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath) {
		}
	}
}
