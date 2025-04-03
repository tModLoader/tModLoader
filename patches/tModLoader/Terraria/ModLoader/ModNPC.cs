using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader;

/// <summary>
/// This class serves as a place for you to place all your properties and hooks for each NPC.
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// <para/> The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC">Basic NPC Guide</see> teaches the basics of making a modded NPC.
/// </summary>
public abstract class ModNPC : ModType<NPC, ModNPC>, ILocalizedModType
{
	/// <summary> The NPC object that this ModNPC controls. </summary>
	public NPC NPC => Entity;

	/// <summary> Shorthand for NPC.type; </summary>
	public int Type => NPC.type;

	public virtual string LocalizationCategory => "NPCs";

	/// <summary> The translations for the display name of this NPC. </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// The file name of this type's texture file in the mod loader's file space.<br/>
	/// The resulting  Asset&lt;Texture2D&gt; can be retrieved using <see cref="TextureAssets.Npc"/> indexed by <see cref="Type"/> if needed. <br/>
	/// You can use a vanilla texture by returning <c>$"Terraria/Images/NPC_{NPCID.NPCNameHere}"</c> <br/>
	/// </summary>
	public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');//GetType().FullName.Replace('.', '/');

	/// <summary> The file name of this NPC's head texture file, to be used in autoloading. </summary>
	public virtual string HeadTexture => Texture + "_Head";

	/// <summary> This file name of this NPC's boss head texture file, to be used in autoloading. </summary>
	public virtual string BossHeadTexture => Texture + "_Head_Boss";

	/// <summary> Determines which type of vanilla NPC this ModNPC will copy the behavior (AI) of. Leave as 0 to not copy any behavior. Defaults to 0. </summary>
	public int AIType { get; set; }

	/// <summary>
	/// Determines which type of vanilla NPC this ModNPC will copy the animation/framing logic of, which includes checks to sets such as <seealso cref="Main.npcFrameCount"/>
	/// and <seealso cref="NPCID.Sets.ExtraFramesCount"/>, and others. For example, selecting the Guide's type will copy how many frames, extra frames, and attack frames the
	/// Guide has, and use those value for animation of this NPC. This is entirely based off of type and not the ModNPC instance itself; so be cautious if you
	/// change this NPC's type.
	/// </summary>
	public int AnimationType { get; set; }

	/// <summary>
	/// The ID of the music that plays when this NPC is on or near the screen. Defaults to -1, which means music plays normally.
	/// </summary>
	/// <remarks>
	/// Note: This property gets ignored if the game would not play music for this NPC by default (i.e. it's not a boss, or it doesn't belong to an invasion)
	/// </remarks>
	/// Will be superseded by ModSceneEffect. Kept for legacy.
	public int Music { get; set; } = -1;

	/// <summary> The priority of the music that plays when this NPC is on or near the screen. </summary>
	/// Will be superseded by ModSceneEffect. Kept for legacy.
	public SceneEffectPriority SceneEffectPriority { get; set; } = SceneEffectPriority.BossLow;

	/// <summary> The vertical offset used for drawing this NPC. Defaults to 0. </summary>
	public float DrawOffsetY { get; set; }

	/// <summary> The type of NPC that this NPC will be considered as when determining banner drops and banner bonuses. Also known as the BannerID. By default this will be 0, which means this NPC is not associated with any banner. To give your NPC its own banner, set this field to the NPC's type and also set <see cref="BannerItem"/>:
	/// <code>
	/// Banner = Type;
	/// BannerItem = ModContent.ItemType&lt;ExampleWormHeadBanner&gt;();
	/// </code>
	/// <para/> For NPC with multiple parts, such as worms, be sure to set this to the same value for each ModNPC. Typically the head NPC type is used as the common Banner value. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExampleWorm.cs">ExampleWorm.cs</see> is an example of this:
	/// <code>
	/// // In ExampleWormBody.SetDefaults:
	/// Banner = ModContent.NPCType&lt;ExampleWormHead&gt;();</code>
	/// <para/> For NPC that are variants of each other, it is also useful to share a Banner value so that each variant counts towards the same banner kill count rather than each having their own banner drop. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/PartyZombie.cs">PartyZombie.cs</see> does this and shares the vanilla Zombie banner. By doing this it is affected by the Zombie banner and counts towards dropping it as well.
	/// <para/> To retrieve and use a vanilla banner value, use the <see cref="Item.NPCtoBanner"/> method:
	/// <code>Banner = Item.NPCtoBanner(NPCID.Zombie);</code>
	/// </summary>
	public int Banner { get; set; }

	/// <summary> The item type this NPC drops after some number of it is defeated. The exact number can be customized by using <see cref="ItemID.Sets.KillsToBanner"/> and defaults to 50. For any ModNPC whose <see cref="Banner"/> field is set to the type of this NPC, that ModNPC will drop this banner. Must be set along with <see cref="Banner"/>.
	/// <para/> If querying the banner item drop for a specific NPC that you know has an assigned banner, you'll need to use the following for accurate results since BannerItem isn't necessarily assigned for npc sharing a banner: <c>Item.BannerToItem(Item.NPCtoBanner(npc.BannerID()));</c>
	/// </summary>
	public int BannerItem { get; set; }

	//TODO: Find a better solution in the future.
	/// <summary> The ModBiome Types associated with this NPC spawning, if applicable. Used in Bestiary.<para/>Vanilla biomes are added to the bestiary via <see cref="SetBestiary(BestiaryDatabase, BestiaryEntry)"/> directly.</summary>
	public int[] SpawnModBiomes { get; set; } = new int[0];

	/// <summary> Setting this to true will make the NPC not appear in the housing menu nor make it find an house. </summary>
	public bool TownNPCStayingHomeless { get; set; }

	protected override NPC CreateTemplateEntity() => new() { ModNPC = this };

	protected sealed override void Register()
	{
		ModTypeLookup<ModNPC>.Register(this);
		NPC.type = NPCLoader.Register(this);

		Type type = GetType();
		var autoloadHead = type.GetAttribute<AutoloadHead>();
		if (autoloadHead != null) {
			Mod.AddNPCHeadTexture(NPC.type, HeadTexture);
		}
		var autoloadBossHead = type.GetAttribute<AutoloadBossHead>();
		if (autoloadBossHead != null) {
			Mod.AddBossHeadTexture(BossHeadTexture, NPC.type);
		}
	}

	/// <summary>
	/// Allows you to change the emote that the NPC will pick
	/// </summary>
	/// <param name="closestPlayer">The <see cref="Player"/> closest to the NPC. You can check the biome the player is in and let the NPC pick the emote that corresponds to the biome.</param>
	/// <param name="emoteList">A list of emote IDs from which the NPC will randomly select one</param>
	/// <param name="otherAnchor">A <see cref="WorldUIAnchor"/> instance that indicates the target of this emote conversation. Use this to get the instance of the <see cref="NPC"/> or <see cref="Player"/> this NPC is talking to.</param>
	/// <returns>Return null to use vanilla mechanic (pick one from the list), otherwise pick the emote by the returned ID. Returning -1 will prevent the emote from being used. Returns null by default</returns>
	public virtual int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) {
		return null;
	}

	public sealed override void SetupContent()
	{
		NPCLoader.SetDefaults(NPC, createModNPC: false);
		AutoStaticDefaults();
		SetStaticDefaults();
		NPCID.Search.Add(FullName, Type);
	}

	/// <summary>
	/// Allows you to set all your NPC's properties, such as width, damage, aiStyle, lifeMax, etc.
	/// </summary>
	public virtual void SetDefaults() { }

	/// <summary>
	/// Gets called when your NPC spawns in world
	/// </summary>
	public virtual void OnSpawn(IEntitySource source) { }

	/// <summary>
	/// Automatically sets certain static defaults. Override this if you do not want the properties to be set for you.
	/// </summary>
	public virtual void AutoStaticDefaults()
	{
		TextureAssets.Npc[NPC.type] = ModContent.Request<Texture2D>(Texture);

		if (Banner != 0 && BannerItem != 0) {
			NPCLoader.bannerToItem[Banner] = BannerItem;
			NPCLoader.itemToBanner[BannerItem] = Banner;
		}

		if (NPC.lifeMax > 32767 || NPC.boss) {
			Main.npcLifeBytes[NPC.type] = 4;
		}
		else if (NPC.lifeMax > 127) {
			Main.npcLifeBytes[NPC.type] = 2;
		}
		else {
			Main.npcLifeBytes[NPC.type] = 1;
		}
	}

	/// <summary>
	/// Allows you to customize this NPC's stats when the difficulty is expert or higher.<br/>
	/// This runs after <see cref="NPC.value"/>,  <see cref="NPC.lifeMax"/>,  <see cref="NPC.damage"/>,  <see cref="NPC.knockBackResist"/> have been adjusted for the current difficulty, (expert/master/FTW)<br/>
	/// It is common to multiply lifeMax by the balance factor, and sometimes adjust knockbackResist.<br/>
	/// <br/>
	/// Eg:<br/>
	/// <code>lifeMax = (int)(lifeMax * balance * bossAdjustment)</code>
	/// </summary>
	/// <param name="numPlayers">The number of active players</param>
	/// <param name="balance">Scaling factor that increases by a fraction for each player</param>
	/// <param name="bossAdjustment">An extra reduction factor to be applied to boss life in high difficulty modes</param>
	public virtual void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
	}

	/// <summary>
	/// Allows you to set an NPC's information in the Bestiary.
	/// </summary>
	/// <param name="database"></param>
	/// <param name="bestiaryEntry"></param>
	public virtual void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
	}

	/// <summary>
	/// Allows you to modify the type name of this NPC dynamically.
	/// </summary>
	public virtual void ModifyTypeName(ref string typeName)
	{
	}

	/// <summary>
	/// Allows you to modify the bounding box for hovering over this NPC (affects things like whether or not its name is displayed).
	/// </summary>
	/// <param name="boundingBox">The bounding box used for determining whether or not the NPC counts as being hovered over.</param>
	public virtual void ModifyHoverBoundingBox(ref Rectangle boundingBox)
	{
	}

	/// <summary>
	/// Allows you to give a list of names this NPC can be given on spawn.<br></br>
	/// By default, returns a blank list, which means the NPC will simply use its type name as its given name when prompted.
	/// </summary>
	/// <returns></returns>
	public virtual List<string> SetNPCNameList()
	{
		return new List<string>();
	}

	/// <summary>
	/// Allows you to set the town NPC profile that this NPC uses.<br></br>
	/// By default, returns null, meaning that the NPC doesn't use one.
	/// </summary>
	public virtual ITownNPCProfile TownNPCProfile()
	{
		return null;
	}

	/// <summary>
	/// This is where you reset any fields you add to your subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied. (Note: This hook is only really useful for GlobalNPC, but is included in ModNPC for completion.)
	/// </summary>
	public virtual void ResetEffects()
	{
	}

	/// <summary>
	/// Allows you to determine how this NPC behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
	/// </summary>
	/// <returns></returns>
	public virtual bool PreAI()
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine how this NPC behaves. This will only be called if PreAI returns true.
	/// </summary>
	public virtual void AI()
	{
	}

	//Allows you to determine how this NPC behaves. This will be called regardless of what PreAI returns.
	public virtual void PostAI()
	{
	}

	/// <summary>
	/// If you are storing AI information outside of the NPC.ai array, use this to send that AI information between clients and servers, which will be handled in <see cref="ReceiveExtraAI"/>.
	/// <br/>Called whenever <see cref="MessageID.SyncNPC"/> is successfully sent, for example on NPC creation, on player join, or whenever NPC.netUpdate is set to true in the update loop for that tick.
	/// <br/>Only called on the server.
	/// </summary>
	/// <param name="writer">The writer.</param>
	public virtual void SendExtraAI(BinaryWriter writer)
	{
	}

	/// <summary>
	/// Use this to receive information that was sent in <see cref="SendExtraAI"/>.
	/// <br/>Called whenever <see cref="MessageID.SyncNPC"/> is successfully received.
	/// <br/>Only called on the client.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public virtual void ReceiveExtraAI(BinaryReader reader)
	{
	}

	/// <summary>
	/// Allows you to modify the frame from this NPC's texture that is drawn, which is necessary in order to animate NPCs.
	/// </summary>
	/// <param name="frameHeight"></param>
	public virtual void FindFrame(int frameHeight)
	{
	}

	/// <summary>
	/// Allows you to make things happen whenever this NPC is hit, such as creating dust or gores. <br/> 
	/// Called on local, server and remote clients. <br/> 
	/// Usually when something happens when an NPC dies such as item spawning, you use NPCLoot, but you can use HitEffect paired with a check for <c>if (NPC.life &lt;= 0)</c> to do client-side death effects, such as spawning dust, gore, or death sounds. <br/> 
	/// </summary>
	public virtual void HitEffect(NPC.HitInfo hit)
	{
	}

	/// <summary>
	/// Allows you to make the NPC either regenerate health or take damage over time by setting <see cref="NPC.lifeRegen"/>. This is useful for implementing damage over time debuffs such as <see cref="BuffID.Poisoned"/> or <see cref="BuffID.OnFire"/>. Regeneration or damage will occur at a rate of half of <see cref="NPC.lifeRegen"/> per second.
	/// <para/>Essentially, modders implementing damage over time debuffs should subtract from <see cref="NPC.lifeRegen"/> a number that is twice as large as the intended damage per second. See <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/GlobalNPCs/DamageOverTimeGlobalNPC.cs#L16">DamageOverTimeGlobalNPC.cs</see> for an example of this.
	/// <para/>The damage parameter is the number that appears above the NPC's head if it takes damage over time.
	/// <para/>Multiple debuffs work together by following some conventions: <see cref="NPC.lifeRegen"/> should not be assigned a number, rather it should be subtracted from. <paramref name="damage"/> should only be assigned if the intended popup text is larger then its current value.  
	/// </summary>
	/// <param name="damage"></param>
	public virtual void UpdateLifeRegen(ref int damage)
	{
	}

	/// <summary>
	/// Whether or not to run the code for checking whether this NPC will remain active. Return false to stop this NPC from being despawned and to stop this NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
	/// </summary>
	/// <returns></returns>
	public virtual bool CheckActive()
	{
		return true;
	}

	/// <summary>
	/// Whether or not this NPC should be killed when it reaches 0 health. You may program extra effects in this hook (for example, how Golem's head lifts up for the second phase of its fight). Return false to stop this NPC from being killed. Returns true by default.
	/// </summary>
	/// <returns></returns>
	public virtual bool CheckDead()
	{
		return true;
	}

	/// <summary>
	/// Allows you to call OnKill on your own when the NPC dies, rather then letting vanilla call it on its own. Returns false by default.
	/// </summary>
	/// <returns>Return true to stop vanilla from calling OnKill on its own. Do this if you call OnKill yourself.</returns>
	public virtual bool SpecialOnKill()
	{
		return false;
	}

	/// <summary>
	/// Allows you to determine whether or not this NPC will do anything upon death (besides dying). This method can also be used to dynamically prevent specific item loot using <see cref="NPCLoader.blockLoot"/>, but editing the drop rules themselves is usually the better approach.
	/// <para/> Returning false will skip dropping loot, the <see cref="NPCLoader.OnKill(NPC)"/> methods, and logic setting boss flags (<see cref="NPC.DoDeathEvents"/>).
	/// <para/> Returns true by default. 
	/// </summary>
	/// <returns></returns>
	public virtual bool PreKill()
	{
		return true;
	}

	/// <summary>
	/// Allows you to make things happen when this NPC dies (for example, dropping items manually and setting ModSystem fields). This hook runs on the server/single player. For client-side effects, such as dust, gore, and sounds, see HitEffect.
	/// <para/> Most item drops should be done via drop rules registered in <see cref="ModifyNPCLoot(NPCLoot)"/>. Some dynamic NPC drops, such as additional hearts, are more suited for OnKill instead. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/MinionBoss/MinionBossMinion.cs#L101">MinionBossMinion.cs</see> shows an example of an NPC that drops additional hearts. See <see cref="NPC.lastInteraction"/> and <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4#player-who-killed-npc">Player who killed NPC wiki section</see> as well for determining which players attacked or killed this NPC.
	/// <para/> Bosses need to set flags when they are defeated, and some bosses run world generation code such as spawning new ore. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/MinionBoss/MinionBossBody.cs#L218">MinionBossMinion.cs</see> shows an example of these effects.
	/// </summary>
	public virtual void OnKill()
	{
	}

	/// <summary>
	/// Allows you to determine how and when this NPC can fall through platforms and similar tiles.
	/// <br/>Return true to allow this NPC to fall through platforms, false to prevent it. Returns null by default, applying vanilla behaviors (based on aiStyle and type).
	/// </summary>
	public virtual bool? CanFallThroughPlatforms()
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine whether the given item can catch this NPC.<br></br>
	/// Return true or false to say this NPC can or cannot be caught, respectively, regardless of vanilla rules.<br></br>
	/// Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.<br></br>
	/// If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.<br></br><br></br>
	/// NOTE: this does not classify the given item as an NPC-catching tool, which is necessary for catching NPCs in the first place.<br></br>
	/// To do that, you will need to use the "CatchingTool" set in ItemID.Sets.
	/// </summary>
	/// <param name="item">The item with which the player is trying to catch this NPC.</param>
	/// <param name="player">The player attempting to catch this NPC.</param>
	/// <returns></returns>
	public virtual bool? CanBeCaughtBy(Item item, Player player)
	{
		return null;
	}

	/// <summary>
	/// Allows you to make things happen when the given item attempts to catch this NPC.
	/// </summary>
	/// <param name="player">The player attempting to catch this NPC.</param>
	/// <param name="item">The item used to catch this NPC.</param>
	/// <param name="failed">Whether or not this NPC has been successfully caught.</param>
	public virtual void OnCaughtBy(Player player, Item item, bool failed)
	{
	}

	/// <summary>
	/// Allows you to add and modify NPC loot tables to drop on death and to appear in the Bestiary.<br/>
	/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4">Basic NPC Drops and Loot 1.4 Guide</see> explains how to use this hook to modify NPC loot.
	/// <br/> This hook only runs once during mod loading, any dynamic behavior must be contained in the rules themselves.
	/// </summary>
	/// <param name="npcLoot">A reference to the item drop database for this npc type</param>
	public virtual void ModifyNPCLoot(NPCLoot npcLoot)
	{
	}

	/// <summary>
	/// Allows you to customize what happens when this boss dies, such as which name is displayed in the defeat message and what type of potion it drops.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="potionType"></param>
	public virtual void BossLoot(ref string name, ref int potionType)
	{
	}

	/// <summary>
	/// Allows you to determine whether this NPC can hit the given player. Return false to block this NPC from hitting the target. Returns true by default. CooldownSlot determines which of the player's cooldown counters (<see cref="ImmunityCooldownID"/>) to use, and defaults to -1 (<see cref="ImmunityCooldownID.General"/>).
	/// </summary>
	/// <param name="target"></param>
	/// <param name="cooldownSlot"></param>
	/// <returns></returns>
	public virtual bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, etc., that this NPC does to a player. 
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// <para/> Runs on the local client.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when this NPC hits a player (for example, inflicting debuffs). <br/>
	/// Runs on the local client. <br/>
	/// </summary>
	/// <param name="target"></param>
	/// <param name="hurtInfo"></param>
	public virtual void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
	{
	}

	/// <summary>
	/// Allows you to determine whether this NPC can hit the given friendly NPC. Return false to block the NPC from hitting the target, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool CanHitNPC(NPC target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether a friendly NPC can be hit by an NPC. Return false to block the attacker from hitting the NPC, and return true to use the vanilla code for whether the target can be hit. Returns true by default.
	/// </summary>
	/// <param name="attacker"></param>
	/// <returns></returns>
	public virtual bool CanBeHitByNPC(NPC attacker)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that this NPC does to a friendly NPC. 
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// <para/> Runs in single player or on the server.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when this NPC hits a friendly NPC. <br/>
	/// Runs in single player or on the server. <br/>
	/// </summary>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	public virtual void OnHitNPC(NPC target, NPC.HitInfo hit)
	{
	}

	/// <summary>
	/// Allows you to determine whether this NPC can be hit by the given melee weapon when swung. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="item"></param>
	/// <returns></returns>
	public virtual bool? CanBeHitByItem(Player player, Item item)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine whether an NPC can be collided with the player melee weapon when swung. <br/>
	/// Use <see cref="CanBeHitByItem(Player, Item)"/> instead for Guide Voodoo Doll-type effects.
	/// </summary>
	/// <param name="player">The player wielding this item.</param>
	/// <param name="item">The weapon item the player is holding.</param>
	/// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
	/// <returns>
	/// Return true to allow colliding with the melee attack, return false to block the weapon from colliding with the NPC, and return null to use the vanilla code for whether the attack can be colliding. Returns null by default.
	/// </returns>
	public virtual bool? CanCollideWithPlayerMeleeAttack(Player player, Item item, Rectangle meleeAttackHitbox)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that this NPC takes from a melee weapon. 
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// <para/> Runs on the local client.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="item"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when this NPC is hit by a melee weapon. <br/>
	/// Runs on the client or server doing the damage. <br/>
	/// </summary>
	/// <param name="player"></param>
	/// <param name="item"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to determine whether this NPC can be hit by the given projectile. Return true to allow hitting the NPC, return false to block hitting the NPC, and return null to use the vanilla code for whether the NPC can be hit. Returns null by default.
	/// </summary>
	/// <param name="projectile"></param>
	/// <returns></returns>
	public virtual bool? CanBeHitByProjectile(Projectile projectile)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that this NPC takes from a projectile. This method is only called for the owner of the projectile, meaning that in multi-player, projectiles owned by a player call this method on that client, and projectiles owned by the server such as enemy projectiles call this method on the server.
	/// <para/> This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when this NPC is hit by a projectile.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to use a custom damage formula for when this NPC takes damage from any source. For example, you can change the way defense works or use a different crit multiplier.
	/// This hook should be used ONLY to modify properties of the HitModifiers. Any extra side effects should occur in OnHit hooks instead.
	/// </summary>
	/// <param name="modifiers"></param>
	public virtual void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to customize the boss head texture used by an NPC based on its state. Set index to -1 to stop the texture from being displayed.
	/// </summary>
	/// <param name="index">The index for NPCID.Sets.BossHeadTextures</param>
	public virtual void BossHeadSlot(ref int index)
	{
	}

	/// <summary>
	/// Allows you to customize the rotation of this NPC's boss head icon on the map.
	/// </summary>
	/// <param name="rotation"></param>
	public virtual void BossHeadRotation(ref float rotation)
	{
	}

	/// <summary>
	/// Allows you to flip this NPC's boss head icon on the map.
	/// </summary>
	/// <param name="spriteEffects"></param>
	public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
	{
	}

	/// <summary>
	/// Allows you to determine the color and transparency in which this NPC is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
	/// </summary>
	/// <param name="drawColor"></param>
	/// <returns></returns>
	public virtual Color? GetAlpha(Color drawColor)
	{
		return null;
	}

	/// <summary>
	/// Allows you to add special visual effects to this NPC (such as creating dust), and modify the color in which the NPC is drawn.
	/// </summary>
	/// <param name="drawColor"></param>
	public virtual void DrawEffects(ref Color drawColor)
	{
	}

	/// <summary>
	/// Allows you to draw things behind this NPC, or to modify the way this NPC is drawn. Substract screenPos from the draw position before drawing. Return false to stop the game from drawing the NPC (useful if you're manually drawing the NPC). Returns true by default.
	/// </summary>
	/// <param name="spriteBatch">The spritebatch to draw on</param>
	/// <param name="screenPos">The screen position used to translate world position into screen position</param>
	/// <param name="drawColor">The color the NPC is drawn in</param>
	/// <returns></returns>
	public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of this NPC. Substract screenPos from the draw position before drawing. This method is called even if PreDraw returns false.
	/// </summary>
	/// <param name="spriteBatch">The spritebatch to draw on</param>
	/// <param name="screenPos">The screen position used to translate world position into screen position</param>
	/// <param name="drawColor">The color the NPC is drawn in</param>
	public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
	}

	/// <summary>
	/// When used in conjunction with "NPC.hide = true", allows you to specify that this NPC should be drawn behind certain elements. Add the index to one of Main.DrawCacheNPCsMoonMoon, DrawCacheNPCsOverPlayers, DrawCacheNPCProjectiles, or DrawCacheNPCsBehindNonSolidTiles.
	/// </summary>
	/// <param name="index"></param>
	public virtual void DrawBehind(int index)
	{
	}

	/// <summary>
	/// Allows you to control how the health bar for this NPC is drawn. The hbPosition parameter is the same as Main.hbPosition; it determines whether the health bar gets drawn above or below the NPC by default. The scale parameter is the health bar's size. By default, it will be the normal 1f; most bosses set this to 1.5f. Return null to let the normal vanilla health-bar-drawing code to run. Return false to stop the health bar from being drawn. Return true to draw the health bar in the position specified by the position parameter (note that this is the world position, not screen position).
	/// </summary>
	/// <param name="hbPosition"></param>
	/// <param name="scale"></param>
	/// <param name="position"></param>
	/// <returns></returns>
	public virtual bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
	{
		return null;
	}

	/// <summary>
	/// Whether or not this NPC can spawn with the given spawning conditions. Return the weight for the chance of this NPC to spawn compared to vanilla mobs. All vanilla mobs combined have a total weight of 1. Returns 0 by default, which disables natural spawning. Remember to always use spawnInfo.player and not Main.LocalPlayer when checking Player or ModPlayer fields, otherwise your mod won't work in Multiplayer.
	/// </summary>
	/// <param name="spawnInfo"></param>
	/// <returns></returns>
	public virtual float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		return 0f;
	}

	/// <summary>
	/// Allows you to customize how this NPC is created when it naturally spawns (for example, its position or ai array). Return the return value of NPC.NewNPC. By default this method spawns this NPC on top of the tile at the given coordinates.
	/// </summary>
	/// <param name="tileX"></param>
	/// <param name="tileY"></param>
	/// <returns></returns>
	public virtual int SpawnNPC(int tileX, int tileY)
	{
		//TODO: Add IEntitySource in '1.4_onspawn'.
		return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
	}

	/// <summary>
	/// Whether or not the conditions have been met for this town NPC to be able to move into town. For example, the Demolitionist requires that any player has an explosive.
	/// <para/> Town NPC spawn conditions typically check if specific bosses have been defeated (<see cref="NPC.downedGolemBoss"/>), the npc has been "saved" somewhere in the world, or any player has specific items in their inventory. To check for inventory items, iterate over <see cref="Main.ActivePlayers"/> and check <see cref="Player.HasItem(int)"/> or <see cref="Player.CountItem(int, int)"/>, returning true if any player satisfies the requirement.
	/// <para/> To support allowing town NPC to respawn without needing to meet the original respawn requirements, a feature added in Terraria v1.4.4, store a bool in a <see cref="ModSystem"/> and check it. <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/NPCs/ExamplePerson.cs#L184">ExamplePerson.CanTownNPCSpawn</see> and <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/Systems/TownNPCRespawnSystem.cs">TownNPCRespawnSystem.cs</see> show an example of this.
	/// <para/> Returns false by default, preventing the town NPC from spawning.
	/// </summary>
	/// <param name="numTownNPCs"></param>
	/// <returns></returns>
	public virtual bool CanTownNPCSpawn(int numTownNPCs)
	{
		return false;
	}

	/* Disabled until #2083 is addressed. Originally introduced in #1323, but was refactored and now would be for additional features outside PR scope.
	/// <summary>
	/// Allows you to set an NPC's biome preferences and nearby NPC preferences for the NPC happiness system. Recommended to only be used with NPCs that have shops.
	/// </summary>
	/// <param name="shopHelperInstance">The vanilla shop modifier instance to invoke methods such as LikeNPC and HateBiome on</param>
	/// <param name="primaryPlayerBiome">The current biome the player is in for purposes of NPC happiness, referred by PrimaryBiomeID </param>
	/// <param name="nearbyNPCsByType">The boolean array of if each type of NPC is nearby</param>
	public virtual void ModifyNPCHappiness(int primaryPlayerBiome, ShopHelper shopHelperInstance, bool[] nearbyNPCsByType) {
	}
	*/

	/// <summary>
	/// Allows you to define special conditions required for this town NPC's house. For example, Truffle requires the house to be in an aboveground mushroom biome.
	/// <para/> The <paramref name="left"/>, <paramref name="right"/>, <paramref name="top"/>, and <paramref name="bottom"/> parameters define the bounds of the room being checked.
	/// <para/> Methods like <see cref="WorldGen.Housing_GetTestedRoomBounds(out int, out int, out int, out int)"/> and <see cref="WorldGen.CountTileTypesInArea(int[], int, int, int, int)"/> can facilitate implementing specific checks.
	/// <para/> Return false to prevent the npc from spawning due to failed condition checks.
	/// </summary>
	public virtual bool CheckConditions(int left, int right, int top, int bottom)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether this town NPC wears a party hat during a party. Returns true by default.
	/// </summary>
	/// <returns></returns>
	public virtual bool UsesPartyHat()
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether this NPC can talk with the player. By default, returns if the NPC is a town NPC.
	/// <para></para>
	/// This hook is not based on the type of the NPC, and is queried specifically on the ModNPC itself, regardless of if,
	/// for example, the type of the NPC instance is changed. Returning true in all circumstances will *always* make the NPC
	/// able to be chatted with no matter what else you do the NPC instance itself.
	/// </summary>
	/// <returns></returns>
	public virtual bool CanChat()
	{
		return NPC.townNPC;
	}

	/// <summary>
	/// Allows you to give this NPC a chat message when a player talks to it. By default returns something embarrassing.
	/// </summary>
	/// <returns></returns>
	public virtual string GetChat()
	{
		return Language.GetTextValue("tModLoader.DefaultTownNPCChat");
	}

	/// <summary>
	/// Allows you to set the text for the buttons that appear on this NPC's chat window. A parameter left as an empty string will not be included as a button on the chat window.
	/// </summary>
	/// <param name="button"></param>
	/// <param name="button2"></param>
	public virtual void SetChatButtons(ref string button, ref string button2)
	{
	}

	/// <summary>
	/// Allows you to make something happen whenever a button is clicked on this NPC's chat window. The firstButton parameter tells whether the first button or second button (button and button2 from SetChatButtons) was clicked. Set the shopName parameter to "Shop" to open this NPC's shop.
	/// </summary>
	/// <param name="firstButton"></param>
	/// <param name="shopName"></param>
	public virtual void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
	}

	/// <summary>
	/// Allows you to add shops to this NPC, similar to adding recipes for items. <br/>
	/// Make a new <see cref="NPCShop"/>, and items to it, and call <see cref="AbstractNPCShop.Register"/>
	/// </summary>
	public virtual void AddShops()
	{
	}

	/// <summary>
	/// Allows you to modify the contents of a shop whenever player opens it. <br/>
	/// To create a shop, use <see cref="AddShops"/> <br/>
	/// Note that for special shops like travelling merchant, the <paramref name="shopName"/> may not correspond to a <see cref="NPCShop"/> in the <see cref="NPCShopDatabase"/>
	/// <para/> Also note that unused slots in <paramref name="items"/> are null while <see cref="Item.IsAir"/> entries are entries that have a reserved slot (<see cref="NPCShop.Entry.SlotReserved"/>) but did not have their conditions met. These should not be overwritten.
	/// </summary>
	/// <param name="shopName">The full name of the shop being opened. See <see cref="NPCShopDatabase.GetShopName"/> for the format. </param>
	/// <param name="items">Items in the shop including 'air' items in empty slots.</param>
	public virtual void ModifyActiveShop(string shopName, Item[] items)
	{
	}

	/// <summary>
	/// Whether this NPC can be teleported to a King or Queen statue. Returns false by default.
	/// </summary>
	/// <param name="toKingStatue">Whether the NPC is being teleported to a King or Queen statue.</param>
	public virtual bool CanGoToStatue(bool toKingStatue)
	{
		return false;
	}

	/// <summary>
	/// Allows you to make things happen when this NPC teleports to a King or Queen statue.
	/// This method is only called server side.
	/// </summary>
	/// <param name="toKingStatue">Whether the NPC was teleported to a King or Queen statue.</param>
	public virtual void OnGoToStatue(bool toKingStatue)
	{
	}

	/// <summary>
	/// Allows you to determine the damage and knockback of this town NPC's attack before the damage is scaled. (More information on scaling in GlobalNPC.BuffTownNPCs.)
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="knockback"></param>
	public virtual void TownNPCAttackStrength(ref int damage, ref float knockback)
	{
	}

	/// <summary>
	/// Allows you to determine the cooldown between each of this town NPC's attack. The cooldown will be a number greater than or equal to the first parameter, and less then the sum of the two parameters.
	/// </summary>
	/// <param name="cooldown"></param>
	/// <param name="randExtraCooldown"></param>
	public virtual void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
	{
	}

	/// <summary>
	/// Allows you to determine the projectile type of this town NPC's attack, and how long it takes for the projectile to actually appear. This hook is only used when the town NPC has an attack type (<see cref="NPCID.Sets.AttackType"/>) of 0 (throwing), 1 (shooting), or 2 (magic).
	/// </summary>
	/// <param name="projType"></param>
	/// <param name="attackDelay"></param>
	public virtual void TownNPCAttackProj(ref int projType, ref int attackDelay)
	{
	}

	/// <summary>
	/// Allows you to determine the speed at which this town NPC throws a projectile when it attacks. Multiplier is the speed of the projectile, gravityCorrection is how much extra the projectile gets thrown upwards, and randomOffset allows you to randomize the projectile's velocity in a square centered around the original velocity. This hook is only used when the town NPC has an attack type (<see cref="NPCID.Sets.AttackType"/>) of 0 (throwing), 1 (shooting), or 2 (magic).
	/// </summary>
	/// <param name="multiplier"></param>
	/// <param name="gravityCorrection"></param>
	/// <param name="randomOffset"></param>
	public virtual void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
	{
	}

	/// <summary>
	/// Allows you to tell the game that this town NPC has already created a projectile and will still create more projectiles as part of a single attack so that the game can animate the NPC's attack properly. Only used when the town NPC has an attack type (<see cref="NPCID.Sets.AttackType"/>) of 1 (shooting).
	/// </summary>
	/// <param name="inBetweenShots"></param>
	public virtual void TownNPCAttackShoot(ref bool inBetweenShots)
	{
	}

	/// <summary>
	/// Allows you to control the brightness of the light emitted by this town NPC's aura when it performs a magic attack. Only used when the town NPC has an attack type (<see cref="NPCID.Sets.AttackType"/>) of 2 (magic)
	/// </summary>
	/// <param name="auraLightMultiplier"></param>
	public virtual void TownNPCAttackMagic(ref float auraLightMultiplier)
	{
	}

	/// <summary>
	/// Allows you to determine the width and height of the item this town NPC swings when it attacks, which controls the range of this NPC's swung weapon. Only used when the town NPC has an attack type (<see cref="NPCID.Sets.AttackType"/>) of 3 (swinging).
	/// </summary>
	/// <param name="itemWidth"></param>
	/// <param name="itemHeight"></param>
	public virtual void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight)
	{
	}

	/// <summary>
	/// Allows you to customize how this town NPC's weapon is drawn when this NPC is shooting (this NPC must have an attack type (<see cref="NPCID.Sets.AttackType"/>) of 1). <paramref name="scale"/> is a multiplier for the item's drawing size, <paramref name="item"/> is the Texture2D instance of the item to be drawn, <paramref name="itemFrame"/> is the section of the texture to draw, and <paramref name="horizontalHoldoutOffset"/> is how far away the item should be drawn from the NPC.<br/>
	/// To use an actual item sprite, use <code>Main.GetItemDrawFrame(itemTypeHere, out item, out itemFrame);
	/// horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, itemType).X - someOffsetHere</code>
	/// </summary>
	/// <param name="item"></param>
	/// <param name="itemFrame"></param>
	/// <param name="scale"></param>
	/// <param name="horizontalHoldoutOffset"></param>
	public virtual void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
	{
	}

	/// <summary>
	/// Allows you to customize how this town NPC's weapon is drawn when this NPC is swinging it (this NPC must have an attack type (<see cref="NPCID.Sets.AttackType"/>) of 3). <paramref name="item"/> is the Texture2D instance of the item to be drawn, <paramref name="itemFrame"/> is the section of the texture to draw, <paramref name="itemSize"/> is the width and height of the item's hitbox (the same values for TownNPCAttackSwing), <paramref name="scale"/> is the multiplier for the item's drawing size, and <paramref name="offset"/> is the offset from which to draw the item from its normal position. The item texture can be any texture, but if it is an item texture you can use  <see cref="Main.GetItemDrawFrame(int, out Texture2D, out Rectangle)"/> to set <paramref name="item"/> and <paramref name="itemFrame"/> easily.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="itemFrame"></param>
	/// <param name="itemSize"></param>
	/// <param name="scale"></param>
	/// <param name="offset"></param>
	public virtual void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
	{
	}

	/// <summary>
	/// Allows you to modify the NPC's <seealso cref="ImmunityCooldownID"/>, damage multiplier, and hitbox. Useful for implementing dynamic damage hitboxes that change in dimensions or deal extra damage. Returns false to prevent vanilla code from running. Returns true by default.
	/// </summary>
	/// <param name="victimHitbox"></param>
	/// <param name="immunityCooldownSlot"></param>
	/// <param name="damageMultiplier"></param>
	/// <param name="npcHitbox"></param>
	/// <returns></returns>
	public virtual bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
	{
		return true;
	}

	/// <summary>
	/// Makes this ModNPC save along the world even if it's not a townNPC. Defaults to false.
	/// <br/><b>NOTE:</b> A town NPC will always be saved.
	/// <br/><b>NOTE:</b> A NPC that needs saving will not despawn naturally.
	/// </summary>
	public virtual bool NeedSaving()
	{
		return false;
	}

	/// <summary>
	/// Allows you to save custom data for the given item.
	/// Allows you to save custom data for the given npc.
	/// <br/>
	/// <br/><b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
	/// <br/><b>NOTE:</b> Try to only save data that isn't default values.
	/// <br/><b>NOTE:</b> The npc may be saved even if NeedSaving returns false and this is not a townNPC, if another mod returns true on NeedSaving.
	/// </summary>
	/// <param name="tag">The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument</param>
	public virtual void SaveData(TagCompound tag)
	{
	}

	/// <summary>
	/// Allows you to load custom data that you have saved for this npc.
	/// </summary>
	/// <param name="tag">The tag.</param>
	public virtual void LoadData(TagCompound tag)
	{
	}

	/// <summary>
	/// Allows you to change the location and sprite direction of the chat bubble that shows up while hovering over a Town NPC.
	/// </summary>
	/// <param name="position">
	/// <br>The default position is:</br>
	/// <br>The X component is set to the NPC's Center - half the width of the chat bubble texture. Then +/- half of the NPC's width + 8 pixels for facing right/left respectively.</br>
	/// <br>Code: <c>npc.Center.X - screenPosition.X - (TextureAssets.Chat.Width() / 2f) +/- (npc.width / 2f + 8)</c></br>
	/// <br>The Y component is set to the top of the NPC - the height of the chat bubble texture. (Negative Y is up.)</br>
	/// <br>Code: <c>npc.position.Y - TextureAssets.Chat.Height() - (float)(int)screenPosition.Y</c></br>
	/// </param>
	/// <param name="spriteEffects">Allows you to change which way the chat bubble is flipped.</param>
	public virtual void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects)
	{
	}

	/// <summary>
	/// <br>Allows you to fully control the location of the party and sprite direction of the party while an NPC is wearing it.</br>
	/// <br><seealso cref="NPCID.Sets.HatOffsetY"/> can be used instead of this hook for a constant Y offset.</br>
	/// <br><seealso cref="NPCID.Sets.NPCFramingGroup"/> can be additionally be used for the Y offset for the Town NPC's animations.</br>
	/// </summary>
	/// <param name="position">
	/// <br>This is the final position right before the party hat gets drawn which is generally the top center of the NPC's hitbox.</br>
	/// <br><seealso cref="NPCID.Sets.HatOffsetY"/> and <seealso cref="NPCID.Sets.NPCFramingGroup"/> are already taken into account.</br>
	/// </param>
	/// <param name="spriteEffects">Allows you to change which way the party hat is flipped.</param>
	public virtual void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
	{
	}

	/// <summary>
	/// Allows you to change the location and sprite direction of the emote bubble when anchored to an NPC.
	/// </summary>
	/// <param name="position">
	/// <br>The default position is:</br>
	/// <br>The X component is set to the NPC's Top + 75% of their width.</br>
	/// <br>Code: <c>entity.Top.X + ((-entity.direction * entity.width) * 0.75f)</c></br>
	/// <br>The Y component is set to the NPC's Y position + 2 pixels. (Positive Y is down.)</br>
	/// <br>Code: <c>entity.VisualPosition.Y + 2f</c></br>
	/// <br>(<seealso cref="Entity.VisualPosition"/> is only used for the player for <seealso cref="Player.gfxOffY"/>)</br>
	/// </param>
	/// <param name="spriteEffects">Allows you to change which way the emote bubble is flipped.</param>
	public virtual void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects)
	{
	}
}
