using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to modify and use hooks for all NPCs, both vanilla and modded.
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// </summary>
public abstract class GlobalNPC : GlobalType<NPC, GlobalNPC>
{
	protected override void ValidateType()
	{
		base.ValidateType();

		LoaderUtils.MustOverrideTogether(this, g => g.SaveData, g => g.LoadData);
		LoaderUtils.MustOverrideTogether(this, g => g.SendExtraAI, g => g.ReceiveExtraAI);
	}

	protected sealed override void Register()
	{
		base.Register();
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Called after SetDefaults for NPCs with a negative <see cref="NPC.netID"/><br/>
	/// This hook is required because <see cref="NPC.SetDefaultsFromNetId"/> only sets <see cref="NPC.netID"/> after SetDefaults<br/>
	/// Remember that <see cref="NPC.type"/> does not support negative numbers and AppliesToEntity cannot distinguish between NPCs with the same type but different netID<br/>
	/// </summary>
	public virtual void SetDefaultsFromNetId(NPC npc)
	{
	}

	/// <summary>
	/// Gets called when any NPC spawns in world
	/// </summary>
	public virtual void OnSpawn(NPC npc, IEntitySource source)
	{
	}

	/// <summary>
	/// Allows you to customize this NPC's stats when the difficulty is expert or higher.<br/>
	/// This runs after <see cref="NPC.value"/>,  <see cref="NPC.lifeMax"/>,  <see cref="NPC.damage"/>,  <see cref="NPC.knockBackResist"/> have been adjusted for the current difficulty, (expert/master/FTW)<br/>
	/// It is common to multiply lifeMax by the balance factor, and sometimes adjust knockbackResist.<br/>
	/// <br/>
	/// Eg:<br/>
	/// <code>lifeMax = (int)(lifeMax * balance * bossAdjustment)</code>
	/// </summary>
	/// <param name="npc">The newly spawned NPC</param>
	/// <param name="numPlayers">The number of active players</param>
	/// <param name="balance">Scaling factor that increases by a fraction for each player</param>
	/// <param name="bossAdjustment">An extra reduction factor to be applied to boss life in high difficulty modes</param>
	public virtual void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
	{
	}

	/// <summary>
	/// Allows you to set an NPC's information in the Bestiary.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="database"></param>
	/// <param name="bestiaryEntry"></param>
	public virtual void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
	}

	/// <summary>
	/// Allows you to modify the type name of this NPC dynamically.
	/// </summary>
	public virtual void ModifyTypeName(NPC npc, ref string typeName)
	{
	}

	/// <summary>
	/// Allows you to modify the bounding box for hovering over the given NPC (affects things like whether or not its name is displayed).
	/// </summary>
	/// <param name="npc">The NPC in question.</param>
	/// <param name="boundingBox">The bounding box used for determining whether or not the NPC counts as being hovered over.</param>
	public virtual void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
	{
	}

	/// <summary>
	/// Allows you to set the town NPC profile that a given NPC uses.
	/// </summary>
	/// <param name="npc">The NPC in question.</param>
	/// <returns>The profile that you want the given NPC to use.<br></br>
	/// This will only influence their choice of profile if you do not return null.<br></br>
	/// By default, returns null, which causes no change.</returns>
	public virtual ITownNPCProfile ModifyTownNPCProfile(NPC npc)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the list of names available to the given town NPC.
	/// </summary>
	public virtual void ModifyNPCNameList(NPC npc, List<string> nameList)
	{
	}

	/// <summary>
	/// This is where you reset any fields you add to your subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied.
	/// </summary>
	/// <param name="npc"></param>
	public virtual void ResetEffects(NPC npc)
	{
	}

	/// <summary>
	/// Allows you to determine how any NPC behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	public virtual bool PreAI(NPC npc)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine how any NPC behaves. This will only be called if PreAI returns true.
	/// </summary>
	/// <param name="npc"></param>
	public virtual void AI(NPC npc)
	{
	}

	/// <summary>
	/// Allows you to determine how any NPC behaves. This will be called regardless of what PreAI returns.
	/// </summary>
	/// <param name="npc"></param>
	public virtual void PostAI(NPC npc)
	{
	}

	/// <summary>
	/// Use this judiciously to avoid straining the network.
	/// <br/>Checks and methods such as <see cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/> can reduce how much data must be sent for how many projectiles.
	/// <br/>Called whenever <see cref="MessageID.SyncNPC"/> is successfully sent, for example on NPC creation, on player join, or whenever NPC.netUpdate is set to true in the update loop for that tick.
	/// <br/>Can be called on the server.
	/// </summary>
	/// <param name="npc">The NPC.</param>
	/// <param name="bitWriter">The compressible bit writer. Booleans written via this are compressed across all mods to improve multiplayer performance.</param>
	/// <param name="binaryWriter">The writer.</param>
	public virtual void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
	}

	/// <summary>
	/// Use this to receive information that was sent in <see cref="SendExtraAI"/>.
	/// <br/>Called whenever <see cref="MessageID.SyncNPC"/> is successfully received.
	/// <br/>Can be called on multiplayer clients.
	/// </summary>
	/// <param name="npc">The NPC.</param>
	/// <param name="bitReader">The compressible bit reader.</param>
	/// <param name="binaryReader">The reader.</param>
	public virtual void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
	}

	/// <summary>
	/// Allows you to modify the frame from an NPC's texture that is drawn, which is necessary in order to animate NPCs.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="frameHeight"></param>
	public virtual void FindFrame(NPC npc, int frameHeight)
	{
	}

	/// <summary>
	/// Allows you to make things happen whenever an NPC is hit, such as creating dust or gores. <br/>
	/// Called on local, server and remote clients. <br/>
	/// Usually when something happens when an npc dies such as item spawning, you use NPCLoot, but you can use HitEffect paired with a check for <c>if (npc.life &lt;= 0)</c> to do client-side death effects, such as spawning dust, gore, or death sounds. <br/>
	/// </summary>
	public virtual void HitEffect(NPC npc, NPC.HitInfo hit)
	{
	}

	/// <summary>
	/// Allows you to make the NPC either regenerate health or take damage over time by setting <see cref="NPC.lifeRegen"/>. This is useful for implementing damage over time debuffs such as <see cref="BuffID.Poisoned"/> or <see cref="BuffID.OnFire"/>. Regeneration or damage will occur at a rate of half of <see cref="NPC.lifeRegen"/> per second.
	/// <para/>Essentially, modders implementing damage over time debuffs should subtract from <see cref="NPC.lifeRegen"/> a number that is twice as large as the intended damage per second. See <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/GlobalNPCs/DamageOverTimeGlobalNPC.cs#L16">DamageOverTimeGlobalNPC.cs</see> for an example of this.
	/// <para/>The damage parameter is the number that appears above the NPC's head if it takes damage over time.
	/// <para/>Multiple debuffs work together by following some conventions: <see cref="NPC.lifeRegen"/> should not be assigned a number, rather it should be subtracted from. <paramref name="damage"/> should only be assigned if the intended popup text is larger then its current value.  
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="damage"></param>
	public virtual void UpdateLifeRegen(NPC npc, ref int damage)
	{
	}

	/// <summary>
	/// Whether or not to run the code for checking whether an NPC will remain active. Return false to stop the NPC from being despawned and to stop the NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	public virtual bool CheckActive(NPC npc)
	{
		return true;
	}

	/// <summary>
	/// Whether or not an NPC should be killed when it reaches 0 health. You may program extra effects in this hook (for example, how Golem's head lifts up for the second phase of its fight). Return false to stop the NPC from being killed. Returns true by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	public virtual bool CheckDead(NPC npc)
	{
		return true;
	}

	/// <summary>
	/// Allows you to call OnKill on your own when the NPC dies, rather then letting vanilla call it on its own. Returns false by default.
	/// </summary>
	/// <returns>Return true to stop vanilla from calling OnKill on its own. Do this if you call OnKill yourself.</returns>
	public virtual bool SpecialOnKill(NPC npc)
	{
		return false;
	}

	/// <inheritdoc cref="ModNPC.PreKill"/>
	public virtual bool PreKill(NPC npc)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make things happen when an NPC dies (for example, setting ModSystem fields). This hook runs on the server/single player. For client-side effects, such as dust, gore, and sounds, see HitEffect.
	/// <para/> Most item drops should be done via drop rules registered in <see cref="ModifyNPCLoot(NPC, NPCLoot)"/> or <see cref="ModifyGlobalLoot(GlobalLoot)"/>. Some dynamic NPC drops, such as additional hearts, are more suited for OnKill instead. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/MinionBoss/MinionBossMinion.cs#L101">MinionBossMinion.cs</see> shows an example of an NPC that drops additional hearts. See <see cref="NPC.lastInteraction"/> and <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4#player-who-killed-npc">Player who killed NPC wiki section</see> as well for determining which players attacked or killed this NPC.
	/// </summary>
	/// <param name="npc"></param>
	public virtual void OnKill(NPC npc)
	{
	}

	/// <summary>
	/// Allows you to determine how and when an NPC can fall through platforms and similar tiles.
	/// <br/>Return true to allow an NPC to fall through platforms, false to prevent it. Returns null by default, applying vanilla behaviors (based on aiStyle and type).
	/// </summary>
	public virtual bool? CanFallThroughPlatforms(NPC npc)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine whether the given item can catch the given NPC.<br></br>
	/// Return true or false to say the given NPC can or cannot be caught, respectively, regardless of vanilla rules.<br></br>
	/// Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.<br></br>
	/// If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.<br></br><br></br>
	/// NOTE: this does not classify the given item as an NPC-catching tool, which is necessary for catching NPCs in the first place.<br></br>
	/// To do that, you will need to use the "CatchingTool" set in ItemID.Sets.
	/// </summary>
	/// <param name="npc">The NPC that can potentially be caught.</param>
	/// <param name="item">The item with which the player is trying to catch the given NPC.</param>
	/// <param name="player">The player attempting to catch the given NPC.</param>
	/// <returns></returns>
	public virtual bool? CanBeCaughtBy(NPC npc, Item item, Player player)
	{
		return null;
	}

	/// <summary>
	/// Allows you to make things happen when the given item attempts to catch the given NPC.
	/// </summary>
	/// <param name="npc">The NPC which the player attempted to catch.</param>
	/// <param name="player">The player attempting to catch the given NPC.</param>
	/// <param name="item">The item used to catch the given NPC.</param>
	/// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
	public virtual void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
	{
	}

	/// <summary>
	/// Allows you to add and modify NPC loot tables to drop on death and to appear in the Bestiary.<br/>
	/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use this hook to modify npc loot.
	/// <br/> This hook only runs once per npc type during mod loading, any dynamic behavior must be contained in the rules themselves.
	/// </summary>
	/// <param name="npc">A default npc of the type being opened, not the actual npc instance.</param>
	/// <param name="npcLoot">A reference to the item drop database for this npc type.</param>
	public virtual void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
	{
	}

	/// <summary>
	/// Allows you to add and modify global loot rules that are conditional, i.e. vanilla's biome keys and souls.<br/>
	/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use this hook to modify npc loot.
	/// </summary>
	/// <param name="globalLoot"></param>
	public virtual void ModifyGlobalLoot(GlobalLoot globalLoot)
	{
	}

	/// <summary>
	/// Allows you to determine whether an NPC can hit the given player. Return false to block the NPC from hitting the target. Returns true by default. CooldownSlot determines which of the player's cooldown counters (<see cref="ImmunityCooldownID"/>) to use, and defaults to -1 (<see cref="ImmunityCooldownID.General"/>).
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="target"></param>
	/// <param name="cooldownSlot"></param>
	/// <returns></returns>
	public virtual bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, etc., that an NPC does to a player.
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when an NPC hits a player (for example, inflicting debuffs). <br/>
	/// Only runs on the local client in multiplayer.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="target"></param>
	/// <param name="hurtInfo"></param>
	public virtual void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
	{
	}

	/// <summary>
	/// Allows you to determine whether an NPC can hit the given friendly NPC. Return false to block the NPC from hitting the target, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool CanHitNPC(NPC npc, NPC target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether a friendly NPC can be hit by an NPC. Return false to block the attacker from hitting the NPC, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="attacker"></param>
	/// <returns></returns>
	public virtual bool CanBeHitByNPC(NPC npc, NPC attacker)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that an NPC does to a friendly NPC.
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when an NPC hits a friendly NPC.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	public virtual void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
	{
	}

	/// <summary>
	/// Allows you to determine whether an NPC can be hit by the given melee weapon when swung. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="player"></param>
	/// <param name="item"></param>
	/// <returns></returns>
	public virtual bool? CanBeHitByItem(NPC npc, Player player, Item item)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine whether an NPC can be collided with the player melee weapon when swung. <br/>
	/// Use <see cref="CanBeHitByItem(NPC, Player, Item)"/> instead for Guide Voodoo Doll-type effects.
	/// </summary>
	/// <param name="npc">The NPC being collided with</param>
	/// <param name="player">The player wielding this item.</param>
	/// <param name="item">The weapon item the player is holding.</param>
	/// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
	/// <returns>
	/// Return true to allow colliding with the melee attack, return false to block the weapon from colliding with the NPC, and return null to use the vanilla code for whether the attack can be colliding. Returns null by default.
	/// </returns>
	public virtual bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that an NPC takes from a melee weapon. 
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// <para/> Runs on the local client.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="player"></param>
	/// <param name="item"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when an NPC is hit by a melee weapon.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="player"></param>
	/// <param name="item"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to determine whether an NPC can be hit by the given projectile. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="projectile"></param>
	/// <returns></returns>
	public virtual bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that an NPC takes from a projectile.
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="projectile"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when an NPC is hit by a projectile.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="projectile"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to use a custom damage formula for when an NPC takes damage from any source. For example, you can change the way defense works or use a different crit multiplier.
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to customize the boss head texture used by an NPC based on its state. Set index to -1 to stop the texture from being displayed.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="index">The index for NPCID.Sets.BossHeadTextures</param>
	public virtual void BossHeadSlot(NPC npc, ref int index)
	{
	}

	/// <summary>
	/// Allows you to customize the rotation of an NPC's boss head icon on the map.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="rotation"></param>
	public virtual void BossHeadRotation(NPC npc, ref float rotation)
	{
	}

	/// <summary>
	/// Allows you to flip an NPC's boss head icon on the map.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="spriteEffects"></param>
	public virtual void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
	{
	}

	/// <summary>
	/// Allows you to determine the color and transparency in which an NPC is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="drawColor"></param>
	/// <returns></returns>
	public virtual Color? GetAlpha(NPC npc, Color drawColor)
	{
		return null;
	}

	/// <summary>
	/// Allows you to add special visual effects to an NPC (such as creating dust), and modify the color in which the NPC is drawn.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="drawColor"></param>
	public virtual void DrawEffects(NPC npc, ref Color drawColor)
	{
	}

	/// <summary>
	/// Allows you to draw things behind an NPC, or to modify the way the NPC is drawn. Substract screenPos from the draw position before drawing. Return false to stop the game from drawing the NPC (useful if you're manually drawing the NPC). Returns true by default.
	/// </summary>
	/// <param name="npc">The NPC that is being drawn</param>
	/// <param name="spriteBatch">The spritebatch to draw on</param>
	/// <param name="screenPos">The screen position used to translate world position into screen position</param>
	/// <param name="drawColor">The color the NPC is drawn in</param>
	/// <returns></returns>
	public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of this NPC. Substract screenPos from the draw position before drawing. This method is called even if PreDraw returns false.
	/// </summary>
	/// <param name="npc">The NPC that is being drawn</param>
	/// <param name="spriteBatch">The spritebatch to draw on</param>
	/// <param name="screenPos">The screen position used to translate world position into screen position</param>
	/// <param name="drawColor">The color the NPC is drawn in</param>
	public virtual void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
	}

	/// <summary>
	/// When used in conjunction with "npc.hide = true", allows you to specify that this npc should be drawn behind certain elements. Add the index to one of Main.DrawCacheNPCsMoonMoon, DrawCacheNPCsOverPlayers, DrawCacheNPCProjectiles, or DrawCacheNPCsBehindNonSolidTiles.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="index"></param>
	public virtual void DrawBehind(NPC npc, int index)
	{
	}

	/// <summary>
	/// Allows you to control how the health bar for the given NPC is drawn. The hbPosition parameter is the same as Main.hbPosition; it determines whether the health bar gets drawn above or below the NPC by default. The scale parameter is the health bar's size. By default, it will be the normal 1f; most bosses set this to 1.5f. Return null to let the normal vanilla health-bar-drawing code to run. Return false to stop the health bar from being drawn. Return true to draw the health bar in the position specified by the position parameter (note that this is the world position, not screen position).
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="hbPosition"></param>
	/// <param name="scale"></param>
	/// <param name="position"></param>
	/// <returns></returns>
	public virtual bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the chance of NPCs spawning around the given player and the maximum number of NPCs that can spawn around the player. Lower spawnRates mean a higher chance for NPCs to spawn.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="spawnRate"></param>
	/// <param name="maxSpawns"></param>
	public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
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
		ref int safeRangeX, ref int safeRangeY)
	{
	}

	/// <summary>
	/// Allows you to control which NPCs can spawn and how likely each one is to spawn. The pool parameter maps NPC types to their spawning weights (likelihood to spawn compared to other NPCs). A type of 0 in the pool represents the default vanilla NPC spawning.
	/// </summary>
	/// <param name="pool"></param>
	/// <param name="spawnInfo"></param>
	public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
	}

	/// <summary>
	/// Allows you to customize an NPC (for example, its position or ai array) after it naturally spawns and before it is synced between servers and clients. As of right now, this only works for modded NPCs.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="tileX"></param>
	/// <param name="tileY"></param>
	public virtual void SpawnNPC(int npc, int tileX, int tileY)
	{
	}

	/// <summary>
	/// Allows you to determine whether this NPC can talk with the player. Return true to allow talking with the player, return false to block this NPC from talking with the player, and return null to use the vanilla code for whether the NPC can talk. Returns null by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	public virtual bool? CanChat(NPC npc)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the chat message of any NPC that the player can talk to.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="chat"></param>
	public virtual void GetChat(NPC npc, ref string chat)
	{
	}

	/// <summary>
	/// Allows you to determine if something can happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked. Return false to prevent the normal code for this button from running. Returns true by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="firstButton"></param>
	/// <returns></returns>
	public virtual bool PreChatButtonClicked(NPC npc, bool firstButton)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make something happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="firstButton"></param>
	public virtual void OnChatButtonClicked(NPC npc, bool firstButton)
	{
	}

	/// <summary>
	/// Allows you to modify existing shop. Be aware that this hook is called just one time during loading.
	/// </summary>
	/// <param name="shop">A <seealso cref="NPCShop"/> instance.</param>
	public virtual void ModifyShop(NPCShop shop)
	{
	}

	/// <summary>
	/// Allows you to modify the contents of a shop whenever player opens it. <br/>
	/// If possible, use <see cref="ModifyShop(NPCShop)"/> instead, to reduce mod conflicts and improve compatibility.
	/// Note that for special shops like travelling merchant, the <paramref name="shopName"/> may not correspond to a <see cref="NPCShop"/> in the <see cref="NPCShopDatabase"/>
	/// <para/> Also note that unused slots in <paramref name="items"/> are null while <see cref="Item.IsAir"/> entries are entries that have a reserved slot (<see cref="NPCShop.Entry.SlotReserved"/>) but did not have their conditions met. These should not be overwritten.
	/// </summary>
	/// <param name="npc">An instance of <seealso cref="NPC"/> that currently player talks to.</param>
	/// <param name="shopName">The full name of the shop being opened. See <see cref="NPCShopDatabase.GetShopName"/> for the format. </param>
	/// <param name="items">Items in the shop including 'air' items in empty slots.</param>
	public virtual void ModifyActiveShop(NPC npc, string shopName, Item[] items)
	{
	}

	/// <summary>
	/// Allows you to add items to the traveling merchant's shop. Add an item by setting shop[nextSlot] to the ID of the item you are adding then incrementing nextSlot. In the end, nextSlot must have a value of 1 greater than the highest index in shop that represents an item ID. If you want to remove an item, you will have to be familiar with programming.
	/// </summary>
	/// <param name="shop"></param>
	/// <param name="nextSlot"></param>
	public virtual void SetupTravelShop(int[] shop, ref int nextSlot)
	{
	}

	/* Disabled until #2083 is addressed. Originally introduced in #1323, but was refactored and now would be for additional features outside PR scope.
	/// <summary>
	/// Allows you to set an NPC's biome preferences and nearby npc preferences for the NPC happiness system. Recommended to only be used with NPCs that have shops.
	/// </summary>
	/// <param name="npc">The current npc being talked to</param>
	/// <param name="shopHelperInstance">The vanilla shop modifier instance to invoke methods such as LikeNPC and HateBiome on</param>
	/// <param name="primaryPlayerBiome">The current biome the player is in for purposes of npc happiness, referred by PrimaryBiomeID </param>
	/// <param name="nearbyNPCsByType">The boolean array of if each type of npc is nearby</param>
	public virtual void ModifyNPCHappiness(NPC npc, int primaryPlayerBiome, ShopHelper shopHelperInstance, bool[] nearbyNPCsByType) {
	}
	*/

	/// <summary>
	/// Whether this NPC can be teleported to a King or Queen statue. Return true to allow the NPC to teleport to the statue, return false to block this NPC from teleporting to the statue, and return null to use the vanilla code for whether the NPC can teleport to the statue. Returns null by default.
	/// </summary>
	/// <param name="npc">The NPC</param>
	/// <param name="toKingStatue">Whether the NPC is being teleported to a King or Queen statue.</param>
	public virtual bool? CanGoToStatue(NPC npc, bool toKingStatue)
	{
		return null;
	}

	/// <summary>
	/// Allows you to make things happen when this NPC teleports to a King or Queen statue.
	/// This method is only called server side.
	/// </summary>
	/// <param name="npc">The NPC</param>
	/// <param name="toKingStatue">Whether the NPC was teleported to a King or Queen statue.</param>
	public virtual void OnGoToStatue(NPC npc, bool toKingStatue)
	{
	}

	/// <summary>
	/// Allows you to modify the stats of town NPCs. Useful for buffing town NPCs when certain bosses are defeated, etc.
	/// </summary>
	/// <param name="damageMult"></param>
	/// <param name="defense"></param>
	public virtual void BuffTownNPC(ref float damageMult, ref int defense)
	{
	}

	/// <summary>
	/// Allows you to determine the damage and knockback of a town NPC's attack before the damage is scaled. (More information on scaling in GlobalNPC.BuffTownNPCs.)
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="damage"></param>
	/// <param name="knockback"></param>
	public virtual void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
	{
	}

	/// <summary>
	/// Allows you to determine the cooldown between each of a town NPC's attack. The cooldown will be a number greater than or equal to the first parameter, and less then the sum of the two parameters.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="cooldown"></param>
	/// <param name="randExtraCooldown"></param>
	public virtual void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
	{
	}

	/// <summary>
	/// Allows you to determine the projectile type of a town NPC's attack, and how long it takes for the projectile to actually appear. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="projType"></param>
	/// <param name="attackDelay"></param>
	public virtual void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
	{
	}

	/// <summary>
	/// Allows you to determine the speed at which a town NPC throws a projectile when it attacks. Multiplier is the speed of the projectile, gravityCorrection is how much extra the projectile gets thrown upwards, and randomOffset allows you to randomize the projectile's velocity in a square centered around the original velocity. This hook is only used when the town NPC has an attack type of 0 (throwing), 1 (shooting), or 2 (magic).
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="multiplier"></param>
	/// <param name="gravityCorrection"></param>
	/// <param name="randomOffset"></param>
	public virtual void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection,
		ref float randomOffset)
	{
	}

	/// <summary>
	/// Allows you to tell the game that a town NPC has already created a projectile and will still create more projectiles as part of a single attack so that the game can animate the NPC's attack properly. Only used when the town NPC has an attack type of 1 (shooting).
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="inBetweenShots"></param>
	public virtual void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
	{
	}

	/// <summary>
	/// Allows you to control the brightness of the light emitted by a town NPC's aura when it performs a magic attack. Only used when the town NPC has an attack type of 2 (magic)
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="auraLightMultiplier"></param>
	public virtual void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
	{
	}

	/// <summary>
	/// Allows you to determine the width and height of the item a town NPC swings when it attacks, which controls the range of the NPC's swung weapon. Only used when the town NPC has an attack type of 3 (swinging).
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="itemWidth"></param>
	/// <param name="itemHeight"></param>
	public virtual void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
	{
	}

	/// <summary>
	/// Allows you to customize how a town NPC's weapon is drawn when the NPC is shooting (the NPC must have an attack type of 1). <paramref name="scale"/> is a multiplier for the item's drawing size, <paramref name="item"/> is the Texture2D instance of the item to be drawn, <paramref name="itemFrame"/> is the section of the texture to draw, and <paramref name="horizontalHoldoutOffset"/> is how far away the item should be drawn from the NPC.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="item"></param>
	/// <param name="itemFrame"></param>
	/// <param name="scale"></param>
	/// <param name="horizontalHoldoutOffset"></param>
	public virtual void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
	{
	}


	/// <inheritdoc cref="ModNPC.DrawTownAttackSwing" />
	public virtual void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
	{
	}

	/// <summary>
	/// Allows you to modify the NPC's <seealso cref="ID.ImmunityCooldownID"/>, damage multiplier, and hitbox. Useful for implementing dynamic damage hitboxes that change in dimensions or deal extra damage. Returns false to prevent vanilla code from running. Returns true by default.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="victimHitbox"></param>
	/// <param name="immunityCooldownSlot"></param>
	/// <param name="damageMultiplier"></param>
	/// <param name="npcHitbox"></param>
	/// <returns></returns>
	public virtual bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make a npc be saved even if it's not a townNPC and NPCID.Sets.SavesAndLoads[npc.type] is false.
	/// <br/><b>NOTE:</b> A town NPC will always be saved (except the Travelling Merchant that never will).
	/// <br/><b>NOTE:</b> A NPC that needs saving will not despawn naturally.
	/// </summary>
	/// <param name="npc"></param>
	/// <returns></returns>
	public virtual bool NeedSaving(NPC npc)
	{
		return false;
	}

	/// <summary>
	/// Allows you to save custom data for the given npc.
	/// <br/>
	/// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
	/// <br/><b>NOTE:</b> Try to only save data that isn't default values.
	/// <br/><b>NOTE:</b> The npc may be saved even if NeedSaving returns false and npc is not a townNPC, if another mod returns true on NeedSaving.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="tag">The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument</param>
	public virtual void SaveData(NPC npc, TagCompound tag)
	{
	}

	/// <summary>
	/// Allows you to load custom data that you have saved for the given npc.
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="tag"></param>
	public virtual void LoadData(NPC npc, TagCompound tag)
	{
	}

	/// <summary>
	/// Allows you to change the emote that the NPC will pick
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="closestPlayer">The <see cref="Player"/> closest to the NPC. You can check the biome the player is in and let the NPC pick the emote that corresponds to the biome.</param>
	/// <param name="emoteList">A list of emote IDs from which the NPC will randomly select one</param>
	/// <param name="otherAnchor">A <see cref="WorldUIAnchor"/> instance that indicates the target of this emote conversation. Use this to get the instance of the <see cref="NPC"/> or <see cref="Player"/> this NPC is talking to.</param>
	/// <returns>Return null to use vanilla mechanic (pick one from the list), otherwise pick the emote by the returned ID. Returning -1 will prevent the emote from being used. Returns null by default</returns>
	public virtual int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
	{
		return null;
	}

	/// <inheritdoc cref="ModNPC.ChatBubblePosition(ref Vector2, ref SpriteEffects)"/>
	public virtual void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
	{
	}

	/// <inheritdoc cref="ModNPC.PartyHatPosition(ref Vector2, ref SpriteEffects)"/>
	public virtual void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
	{
	}

	/// <inheritdoc cref="ModNPC.EmoteBubblePosition(ref Vector2, ref SpriteEffects)"/>
	public virtual void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
	{
	}
}
