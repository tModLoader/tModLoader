using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to modify and use hooks for all NPCs, including vanilla mobs. Create an instance of an overriding class then call Mod.AddGlobalNPC to use this.
	/// </summary>
	public class GlobalNPC
	{
		/// <summary>
		/// The mod to which this GlobalNPC belongs.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this GlobalNPC instance.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		internal int index;
		internal int instanceIndex;

		/// <summary>
		/// Allows you to automatically load a GlobalNPC instead of using Mod.AddGlobalNPC. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this method to either force or stop an autoload or to control the internal name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Whether to create a new GlobalNPC instance for every NPC that exists. 
		/// Useful for storing information on an npc. Defaults to false. 
		/// Return true if you need to store information (have non-static fields).
		/// </summary>
		public virtual bool InstancePerEntity => false;

		public GlobalNPC Instance(NPC npc) => InstancePerEntity ? npc.globalNPCs[instanceIndex] : this;

		/// <summary>
		/// Whether instances of this GlobalNPC are created through Clone or constructor (by default implementations of NewInstance and Clone()). 
		/// Defaults to false (using default constructor).
		/// </summary>
		public virtual bool CloneNewInstances => false;

		/// <summary>
		/// Returns a clone of this GlobalNPC. 
		/// By default this will return a memberwise clone; you will want to override this if your GlobalNPC contains object references. 
		/// Only called if CloneNewInstances && InstancePerEntity
		/// </summary>
		public virtual GlobalNPC Clone() => (GlobalNPC)MemberwiseClone();

		/// <summary>
		/// Create a new instance of this GlobalNPC for an NPC instance. 
		/// Called at the end of NPC.SetDefaults.
		/// If CloneNewInstances is true, just calls Clone()
		/// Otherwise calls the default constructor and copies fields
		/// </summary>
		public virtual GlobalNPC NewInstance(NPC npc) {
			if (CloneNewInstances) {
				return Clone();
			}
			GlobalNPC copy = (GlobalNPC)Activator.CreateInstance(GetType());
			copy.mod = mod;
			copy.Name = Name;
			copy.index = index;
			copy.instanceIndex = instanceIndex;
			return copy;
		}

		/// <summary>
		/// Allows you to set the properties of any and every NPC that gets created.
		/// </summary>
		/// <param name="npc"></param>
		public virtual void SetDefaults(NPC npc) {
		}

		/// <summary>
		/// Allows you to customize an NPC's stats in expert mode.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="numPlayers"></param>
		/// <param name="bossLifeScale"></param>
		public virtual void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale) {
		}

		/// <summary>
		/// This is where you reset any fields you add to your subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied.
		/// </summary>
		/// <param name="npc"></param>
		public virtual void ResetEffects(NPC npc) {
		}

		/// <summary>
		/// Allows you to determine how any NPC behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <returns></returns>
		public virtual bool PreAI(NPC npc) {
			return true;
		}

		/// <summary>
		/// Allows you to determine how any NPC behaves. This will only be called if PreAI returns true.
		/// </summary>
		/// <param name="npc"></param>
		public virtual void AI(NPC npc) {
		}

		/// <summary>
		/// Allows you to determine how any NPC behaves. This will be called regardless of what PreAI returns.
		/// </summary>
		/// <param name="npc"></param>
		public virtual void PostAI(NPC npc) {
		}

		/// <summary>
		/// Allows you to modify the frame from an NPC's texture that is drawn, which is necessary in order to animate NPCs.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="frameHeight"></param>
		public virtual void FindFrame(NPC npc, int frameHeight) {
		}

		/// <summary>
		/// Allows you to make things happen whenever an NPC is hit, such as creating dust or gores. This hook is client side. Usually when something happens when an npc dies such as item spawning, you use NPCLoot, but you can use HitEffect paired with a check for `if (npc.life <= 0)` to do client-side death effects, such as spawning dust, gore, or death sounds.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="hitDirection"></param>
		/// <param name="damage"></param>
		public virtual void HitEffect(NPC npc, int hitDirection, double damage) {
		}

		/// <summary>
		/// Allows you to make the NPC either regenerate health or take damage over time by setting npc.lifeRegen. Regeneration or damage will occur at a rate of half of npc.lifeRegen per second. The damage parameter is the number that appears above the NPC's head if it takes damage over time.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="damage"></param>
		public virtual void UpdateLifeRegen(NPC npc, ref int damage) {
		}

		/// <summary>
		/// Whether or not to run the code for checking whether an NPC will remain active. Return false to stop the NPC from being despawned and to stop the NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <returns></returns>
		public virtual bool CheckActive(NPC npc) {
			return true;
		}

		/// <summary>
		/// Whether or not an NPC should be killed when it reaches 0 health. You may program extra effects in this hook (for example, how Golem's head lifts up for the second phase of its fight). Return false to stop the NPC from being killed. Returns true by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <returns></returns>
		public virtual bool CheckDead(NPC npc) {
			return true;
		}

		/// <summary>
		/// Allows you to call NPCLoot on your own when the NPC dies, rather then letting vanilla call it on its own. Useful for things like dropping loot from the nearest segment of a worm boss. Returns false by default.
		/// </summary>
		/// <returns>Return true to stop vanilla from calling NPCLoot on its own. Do this if you call NPCLoot yourself.</returns>
		public virtual bool SpecialNPCLoot(NPC npc) {
			return false;
		}

		/// <summary>
		/// Allows you to determine whether or not the NPC will drop anything at all. Return false to stop the NPC from dropping anything. Returns true by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <returns></returns>
		public virtual bool PreNPCLoot(NPC npc) {
			return true;
		}

		/// <summary>
		/// Allows you to make things happen when an NPC dies (for example, dropping items and setting ModWorld fields). This hook runs on the server/single player. For client-side effects, such as dust, gore, and sounds, see HitEffect
		/// </summary>
		/// <param name="npc"></param>
		public virtual void NPCLoot(NPC npc) {
		}

        /// <summary>
        /// Allows you to make things happen when an NPC is caught. Ran Serverside.
        /// </summary>
        /// <param name="npc">The caught NPC</param>
        /// <param name="player">The player catching the NPC</param>
        /// <param name="item">The item that will be spawned</param>
        public virtual void OnCatchNPC(NPC npc, Player player, Item item) {
		}

		/// <summary>
		/// Allows you to determine whether an NPC can hit the given player. Return false to block the NPC from hitting the target. Returns true by default. CooldownSlot determines which of the player's cooldown counters to use (-1, 0, or 1), and defaults to -1.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="target"></param>
		/// <param name="cooldownSlot"></param>
		/// <returns></returns>
		public virtual bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that an NPC does to a player.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when an NPC hits a player (for example, inflicting debuffs).
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitPlayer(NPC npc, Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether an NPC can hit the given friendly NPC. Return true to allow hitting the target, return false to block the NPC from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool? CanHitNPC(NPC npc, NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that an NPC does to a friendly NPC.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitNPC(NPC npc, NPC target, ref int damage, ref float knockback, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when an NPC hits a friendly NPC.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitNPC(NPC npc, NPC target, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether an NPC can be hit by the given melee weapon when swung. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool? CanBeHitByItem(NPC npc, Player player, Item item) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that an NPC takes from a melee weapon.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when an NPC is hit by a melee weapon.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether an NPC can be hit by the given projectile. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public virtual bool? CanBeHitByProjectile(NPC npc, Projectile projectile) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that an NPC takes from a projectile.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="projectile"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		/// <param name="hitDirection"></param>
		public virtual void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
		}

		/// <summary>
		/// Allows you to create special effects when an NPC is hit by a projectile.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="projectile"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to use a custom damage formula for when an NPC takes damage from any source. For example, you can change the way defense works or use a different crit multiplier. Return false to stop the game from running the vanilla damage formula; returns true by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="damage"></param>
		/// <param name="defense"></param>
		/// <param name="knockback"></param>
		/// <param name="hitDirection"></param>
		/// <param name="crit"></param>
		/// <returns></returns>
		public virtual bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			return true;
		}

		/// <summary>
		/// Allows you to customize the boss head texture used by an NPC based on its state.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="index"></param>
		public virtual void BossHeadSlot(NPC npc, ref int index) {
		}

		/// <summary>
		/// Allows you to customize the rotation of an NPC's boss head icon on the map.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="rotation"></param>
		public virtual void BossHeadRotation(NPC npc, ref float rotation) {
		}

		/// <summary>
		/// Allows you to flip an NPC's boss head icon on the map.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="spriteEffects"></param>
		public virtual void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects) {
		}

		/// <summary>
		/// Allows you to determine the color and transparency in which an NPC is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="drawColor"></param>
		/// <returns></returns>
		public virtual Color? GetAlpha(NPC npc, Color drawColor) {
			return null;
		}

		/// <summary>
		/// Allows you to add special visual effects to an NPC (such as creating dust), and modify the color in which the NPC is drawn.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="drawColor"></param>
		public virtual void DrawEffects(NPC npc, ref Color drawColor) {
		}

		/// <summary>
		/// Allows you to draw things behind an NPC, or to modify the way the NPC is drawn. Return false to stop the game from drawing the NPC (useful if you're manually drawing the NPC). Returns true by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="spriteBatch"></param>
		/// <param name="drawColor"></param>
		/// <returns></returns>
		public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of this NPC. This method is called even if PreDraw returns false.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="spriteBatch"></param>
		/// <param name="drawColor"></param>
		public virtual void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
		}

		/// <summary>
		/// Allows you to control how the health bar for the given NPC is drawn. The hbPosition parameter is the same as Main.hbPosition; it determines whether the health bar gets drawn above or below the NPC by default. The scale parameter is the health bar's size. By default, it will be the normal 1f; most bosses set this to 1.5f. Return null to let the normal vanilla health-bar-drawing code to run. Return false to stop the health bar from being drawn. Return true to draw the health bar in the position specified by the position parameter (note that this is the world position, not screen position).
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="hbPosition"></param>
		/// <param name="scale"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public virtual bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the chance of NPCs spawning around the given player and the maximum number of NPCs that can spawn around the player. Lower spawnRates mean a higher chance for NPCs to spawn.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="spawnRate"></param>
		/// <param name="maxSpawns"></param>
		public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
		}

		/// <summary>
		/// Allows you to modify the range at which NPCs can spawn around the given player. The spawnRanges determine that maximum distance NPCs can spawn from the player, and the safeRanges determine the minimum distance.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="spawnRangeX"></param>
		/// <param name="spawnRangeY"></param>
		/// <param name="safeRangeX"></param>
		/// <param name="safeRangeY"></param>
		public virtual void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY) {
		}

		/// <summary>
		/// Allows you to control which NPCs can spawn and how likely each one is to spawn. The pool parameter maps NPC types to their spawning weights (likelihood to spawn compared to other NPCs). A type of 0 in the pool represents the default vanilla NPC spawning.
		/// </summary>
		/// <param name="pool"></param>
		/// <param name="spawnInfo"></param>
		public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
		}

		/// <summary>
		/// Allows you to customize an NPC (for example, its position or ai array) after it naturally spawns and before it is synced between servers and clients. As of right now, this only works for modded NPCs.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="tileX"></param>
		/// <param name="tileY"></param>
		public virtual void SpawnNPC(int npc, int tileX, int tileY) {
		}

		/// <summary>
		/// Allows you to determine whether this NPC can talk with the player. Return true to allow talking with the player, return false to block this NPC from talking with the player, and return null to use the vanilla code for whether the NPC can talk. Returns null by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <returns></returns>
		public virtual bool? CanChat(NPC npc) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the chat message of any NPC that the player can talk to.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="chat"></param>
		public virtual void GetChat(NPC npc, ref string chat) {
		}

		/// <summary>
		/// Allows you to determine if something can happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked. Return false to prevent the normal code for this button from running. Returns true by default.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="firstButton"></param>
		/// <returns></returns>
		public virtual bool PreChatButtonClicked(NPC npc, bool firstButton) {
			return true;
		}

		/// <summary>
		/// Allows you to make something happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="firstButton"></param>
		public virtual void OnChatButtonClicked(NPC npc, bool firstButton) {
		}

		/// <summary>
		/// Allows you to add items to an NPC's shop. The type parameter is the type of the NPC that this shop belongs to. Add an item by setting the defaults of shop.item[nextSlot] then incrementing nextSlot. In the end, nextSlot must have a value of 1 greater than the highest index in shop.item that contains an item. If you want to remove an item, you will have to be familiar with programming.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="shop"></param>
		/// <param name="nextSlot"></param>
		public virtual void SetupShop(int type, Chest shop, ref int nextSlot) {
		}

		/// <summary>
		/// Allows you to add items to the traveling merchant's shop. Add an item by setting shop[nextSlot] to the ID of the item you are adding then incrementing nextSlot. In the end, nextSlot must have a value of 1 greater than the highest index in shop that represents an item ID. If you want to remove an item, you will have to be familiar with programming.
		/// </summary>
		/// <param name="shop"></param>
		/// <param name="nextSlot"></param>
		public virtual void SetupTravelShop(int[] shop, ref int nextSlot) {
		}

		/// <summary>
		/// Whether this NPC can be telported a King or Queen statue. Return true to allow the NPC to teleport to the statue, return false to block this NPC from teleporting to the statue, and return null to use the vanilla code for whether the NPC can teleport to the statue. Returns null by default.
		/// </summary>
		/// <param name="npc">The NPC</param>
		/// <param name="toKingStatue">Whether the NPC is being teleported to a King or Queen statue.</param>
		public virtual bool? CanGoToStatue(NPC npc, bool toKingStatue) {
			return null;
		}

		/// <summary>
		/// Allows you to make things happen when this NPC teleports to a King or Queen statue.
		/// This method is only called server side.
		/// </summary>
		/// <param name="npc">The NPC</param>
		/// <param name="toKingStatue">Whether the NPC was teleported to a King or Queen statue.</param>
		public virtual void OnGoToStatue(NPC npc, bool toKingStatue) {
		}

		/// <summary>
		/// Allows you to modify the stats of town NPCs. Useful for buffing town NPCs when certain bosses are defeated, etc.
		/// </summary>
		/// <param name="damageMult"></param>
		/// <param name="defense"></param>
		public virtual void BuffTownNPC(ref float damageMult, ref int defense) {
		}

		/// <summary>
		/// Allows you to determine the damage and knockback of a town NPC's attack before the damage is scaled. (More information on scaling in GlobalNPC.BuffTownNPCs.)
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		public virtual void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) {
		}

		/// <summary>
		/// Allows you to determine the cooldown between each of a town NPC's attack. The cooldown will be a number greater than or equal to the first parameter, and less then the sum of the two parameters.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="cooldown"></param>
		/// <param name="randExtraCooldown"></param>
		public virtual void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown) {
		}

		/// <summary>
		/// Allows you to determine the projectile type of a town NPC's attack, and how long it takes for the projectile to actually appear. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="projType"></param>
		/// <param name="attackDelay"></param>
		public virtual void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) {
		}

		/// <summary>
		/// Allows you to determine the speed at which a town NPC throws a projectile when it attacks. Multiplier is the speed of the projectile, gravityCorrection is how much extra the projectile gets thrown upwards, and randomOffset allows you to randomize the projectile's velocity in a square centered around the original velocity. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="multiplier"></param>
		/// <param name="gravityCorrection"></param>
		/// <param name="randomOffset"></param>
		public virtual void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
			ref float randomOffset) {
		}

		/// <summary>
		/// Allows you to tell the game that a town NPC has already created a projectile and will still create more projectiles as part of a single attack so that the game can animate the NPC's attack properly. Only used when the town NPC has an attack type of 1 (shooting).
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="inBetweenShots"></param>
		public virtual void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots) {
		}

		/// <summary>
		/// Allows you to control the brightness of the light emitted by a town NPC's aura when it performs a magic attack. Only used when the town NPC has an attack type of 2 (magic)
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="auraLightMultiplier"></param>
		public virtual void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier) {
		}

		/// <summary>
		/// Allows you to determine the width and height of the item a town NPC swings when it attacks, which controls the range of the NPC's swung weapon. Only used when the town NPC has an attack type of 3 (swinging).
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="itemWidth"></param>
		/// <param name="itemHeight"></param>
		public virtual void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight) {
		}

		/// <summary>
		/// Allows you to customize how a town NPC's weapon is drawn when the NPC is shooting (the NPC must have an attack type of 1). Scale is a multiplier for the item's drawing size, item is the ID of the item to be drawn, and closeness is how close the item should be drawn to the NPC.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="scale"></param>
		/// <param name="item"></param>
		/// <param name="closeness"></param>
		public virtual void DrawTownAttackGun(NPC npc, ref float scale, ref int item, ref int closeness) {
		}

		/// <summary>
		/// Allows you to customize how a town NPC's weapon is drawn when the NPC is swinging it (the NPC must have an attack type of 3). Item is the Texture2D instance of the item to be drawn (use Main.itemTexture[id of item]), itemSize is the width and height of the item's hitbox (the same values for TownNPCAttackSwing), scale is the multiplier for the item's drawing size, and offset is the offset from which to draw the item from its normal position.
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="item"></param>
		/// <param name="itemSize"></param>
		/// <param name="scale"></param>
		/// <param name="offset"></param>
		public virtual void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref int itemSize, ref float scale, ref Vector2 offset) {
		}
	}
}
