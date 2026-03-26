using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader;

/// <summary>
/// A ModPlayer instance represents an extension of a Player instance.
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// <br/> You can store fields in the ModPlayer classes, much like how the Player class abuses field usage, to keep track of mod-specific information on the player that a ModPlayer instance represents. It also contains hooks to insert your code into the Player class.
/// </summary>
public abstract class ModPlayer : ModType<Player, ModPlayer>, IIndexed
{
	public ushort Index { get; internal set; }

	/// <summary>
	/// The Player instance that this ModPlayer instance is attached to.
	/// </summary>
	public Player Player => Entity;

	protected override Player CreateTemplateEntity() => null;

	public override ModPlayer NewInstance(Player entity)
	{
		var inst = base.NewInstance(entity);

		inst.Index = Index;

		return inst;
	}

	public bool TypeEquals(ModPlayer other)
	{
		return Mod == other.Mod && Name == other.Name;
	}

	protected override void ValidateType()
	{
		base.ValidateType();

		LoaderUtils.MustOverrideTogether(this, p => p.SaveData, p => p.LoadData);
		LoaderUtils.MustOverrideTogether(this, p => p.CopyClientState, p => p.SendClientChanges);
	}

	protected sealed override void Register()
	{
		ModTypeLookup<ModPlayer>.Register(this);
		PlayerLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Called whenever the player is loaded (on the player selection screen). This can be used to initialize data structures, etc.
	/// </summary>
	public virtual void Initialize()
	{
	}

	/// <summary>
	/// This is where you reset any fields you add to your ModPlayer subclass to their default states. This is necessary in order to reset your fields if they are conditionally set by a tick update but the condition is no longer satisfied.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void ResetEffects()
	{
	}

	/// <summary>
	/// This is where you reset any fields related to INFORMATION accessories to their "default" states. This is identical to ResetEffects(); but should ONLY be used to
	/// reset info accessories. It will cause unintended side-effects if used with other fields.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <remarks>
	/// This method is called in tandem with <seealso cref="ResetEffects"/>, but it also called in <seealso cref="Player.RefreshInfoAccs"/> even when the game is paused;
	/// this allows for info accessories to keep properly updating while the game is paused, a feature/fix added in 1.4.4.
	/// </remarks>
	public virtual void ResetInfoAccessories() { }

	/// <summary>
	/// This is where you set any fields related to INFORMATION accessories based on the passed in player argument.
	/// <para/> Called on the local client.
	/// <para/> Note that this hook is only called if all of the requirements
	/// for a "nearby teammate" is met, which is when the other player is on the same team and within a certain distance, determined by the following code:
	/// <code>(Main.player[i].Center - base.Center).Length() &lt; 800f</code>
	/// </summary>
	public virtual void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer) { }

	/// <summary>
	/// Allows you to modify the player's max stats.  This hook runs after vanilla increases from the Life Crystal, Life Fruit and Mana Crystal are applied
	/// <para/> Called on local, server, and remote clients.
	/// <para/> <b>NOTE:</b> You should NOT modify <see cref="Player.statLifeMax"/> nor <see cref="Player.statManaMax"/> here.  Use the <paramref name="health"/> and <paramref name="mana"/> parameters.
	/// <para/> Also note that unlike many other tModLoader hooks, the default implementation of this hook has code that will assign <paramref name="health"/> and <paramref name="mana"/> to <see cref="StatModifier.Default"/>. Take care to place <c>base.ModifyMaxStats(out health, out mana);</c> before any other code you add to this hook to avoid issues, if you use it.
	/// </summary>
	/// <param name="health">The modifier to the player's maximum health</param>
	/// <param name="mana">The modifier to the player's maximum mana</param>
	public virtual void ModifyMaxStats(out StatModifier health, out StatModifier mana)
	{
		health = StatModifier.Default;
		mana = StatModifier.Default;
	}

	/// <summary>
	/// Similar to <see cref="ResetEffects"/>, except this is only called when the player is dead. If this is called, then <see cref="ResetEffects"/> will not be called.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateDead()
	{
	}

	/// <summary>
	/// Currently never gets called, so this is useless.
	/// </summary>
	public virtual void PreSaveCustomData()
	{
	}

	/// <summary>
	/// Allows you to save custom data for this player.
	/// <para/> <b>NOTE:</b> The provided tag is always empty by default, and is provided as an argument only for the sake of convenience and optimization.
	/// <para/> <b>NOTE:</b> Try to only save data that isn't default values.
	/// </summary>
	/// <param name="tag"> The TagCompound to save data into. Note that this is always empty by default, and is provided as an argument only for the sake of convenience and optimization. </param>
	public virtual void SaveData(TagCompound tag) { }

	/// <summary>
	/// Allows you to load custom data that you have saved for this player.
	/// <para/> <b>Try to write defensive loading code that won't crash if something's missing.</b>
	/// </summary>
	/// <param name="tag"> The TagCompound to load data from. </param>
	public virtual void LoadData(TagCompound tag) { }

	/// <summary>
	/// PreSavePlayer and PostSavePlayer wrap the vanilla player saving code (both are before the ModPlayer.Save). Useful for advanced situations where a save might be corrupted or rendered unusable by the values that normally would save.
	/// </summary>
	public virtual void PreSavePlayer()
	{
	}

	/// <summary>
	/// PreSavePlayer and PostSavePlayer wrap the vanilla player saving code (both are before the ModPlayer.Save). Useful for advanced situations where a save might be corrupted or rendered unusable by the values that normally would save.
	/// </summary>
	public virtual void PostSavePlayer()
	{
	}

	/// <summary>
	/// Allows you to copy information to the <paramref name="targetCopy"/> parameter that you intend to sync between this local client and both the server and other clients.
	/// <br/><br/> You would then use the <see cref="SendClientChanges"/> hook to compare against that data and decide what needs synchronizing, sending that data in a <see cref="ModPacket"/> to the server. The server will then need to relay that information to the other remote clients.
	/// <br/><br/> This hook is called with every call of the <see cref="Player.clientClone"/> method (each game update).
	/// <br/><br/> See <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Netcode#player--modplayer">the Player / ModPlayer section of the Basic Netcode wiki page</see> for more information.
	/// <br/>
	/// <br/> <b>NOTE:</b> For performance reasons, avoid deep cloning or copying any excessive information.
	/// <br/> <b>NOTE:</b> Using <see cref="Item.CopyNetStateTo"/> is the recommended way of creating item snapshots.
	/// </summary>
	/// <param name="targetCopy"></param>
	public virtual void CopyClientState(ModPlayer targetCopy)
	{
	}

	/// <summary>
	/// Allows you to sync information about this player between server and client. This hook will be called whenever a player joins the game, both to sync the joining player's data to the server and other clients and to sync the data of the existing players to the joining player.
	/// <br/><br/> This should be used to sync all necessary modded data from this ModPlayer to other clients. The <see cref="SendClientChanges(ModPlayer)"/> hook is used to selectively sync changes that happen during gameplay.
	/// <br/><br/> The toWho and fromWho parameters correspond to the remoteClient/toClient and ignoreClient arguments, respectively, of NetMessage.SendData/ModPacket.Send. They should be passed in as-is. The newPlayer parameter is whether or not the player is joining the server (it is true on the joining client).
	/// <br/><br/> This hook will be called on the local client to send the data to the server and also on the server to send the data to other clients.
	/// <br/><br/> See <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Netcode#player--modplayer">the Player / ModPlayer section of the Basic Netcode wiki page</see> for more information.
	/// </summary>
	/// <param name="toWho"></param>
	/// <param name="fromWho"></param>
	/// <param name="newPlayer"></param>
	public virtual void SyncPlayer(int toWho, int fromWho, bool newPlayer)
	{
	}

	/// <summary>
	/// Allows you to sync any information that has changed for this ModPlayer since the last game update from this local client to the server. The server will need to take those changes and relay them to other remote clients.
	/// <br/><br/> Here, you should check the information you have copied in the clientClone parameter during <see cref="CopyClientState(ModPlayer)"/>; if they differ between this ModPlayer and the clientPlayer parameter, then you should send that information using NetMessage.SendData or ModPacket.Send. All of the differences are the changes that occurred during the last game update for the local player.
	/// <br/><br/> See <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Netcode#player--modplayer">the Player / ModPlayer section of the Basic Netcode wiki page</see> for more information.
	/// </summary>
	/// <param name="clientPlayer"></param>
	public virtual void SendClientChanges(ModPlayer clientPlayer)
	{
	}

	/// <summary>
	/// Allows you to give the player a negative life regeneration based on its state (for example, the "On Fire!" debuff makes the player take damage-over-time). This is typically done by setting Player.lifeRegen to 0 if it is positive, setting Player.lifeRegenTime to 0, and subtracting a number from Player.lifeRegen. The player will take damage at a rate of half the number you subtract per second.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateBadLifeRegen()
	{
	}

	/// <summary>
	/// Allows you to increase the player's life regeneration based on its state. This can be done by incrementing Player.lifeRegen by a certain number. The player will recover life at a rate of half the number you add per second. You can also increment Player.lifeRegenTime to increase the speed at which the player reaches its maximum natural life regeneration.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateLifeRegen()
	{
	}

	/// <summary>
	/// Allows you to modify the power of the player's natural life regeneration. This can be done by multiplying the regen parameter by any number. For example, campfires multiply it by 1.1, while walking multiplies it by 0.5.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="regen"></param>
	public virtual void NaturalLifeRegen(ref float regen)
	{
	}

	/// <summary>
	/// Allows you to modify the player's stats while the game is paused due to the autopause setting being on.
	/// This is called in single player only, some time before the player's tick update would happen when the game isn't paused.
	/// </summary>
	public virtual void UpdateAutopause()
	{
	}

	/// <summary>
	/// This is called at the beginning of every tick update for this player, after checking whether the player exists. <br/>
	/// This can be used to adjust timers and cooldowns.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PreUpdate()
	{
	}

	/// <summary>
	/// Use this to check on keybinds you have registered. While SetControls is set even while in text entry mode, this hook is only called during gameplay.
	/// <para/> Called on the local client only.
	/// <para/> Read <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/Players/ExampleKeybindPlayer.cs">ExampleKeybindPlayer.cs</see> for examples and information on using this hook.
	/// </summary>
	/// <param name="triggersSet"></param>
	public virtual void ProcessTriggers(TriggersSet triggersSet)
	{
	}

	/// <summary>
	/// This is called when the player activates their armor set bonus by double tapping down (or up if <see cref="Main.ReversedUpDownArmorSetBonuses"/> is true). As an example, the Vortex armor uses this to toggle stealth mode.
	/// <para/> Use this to implement armor set bonuses that need to be activated by the player.
	/// <para/> Don't forget to check if your armor set is active.
	/// <para/> While this technically can be used for other effects, it will likely be frustrating for your players if non-armor set effects are being triggered in tandem with armor set bonus effects. Modders can use <see cref="Player.holdDownCardinalTimer"/> and <see cref="Player.doubleTapCardinalTimer"/> directly in other hooks for similar effects if needed.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void ArmorSetBonusActivated()
	{
	}

	/// <summary>
	/// This is called when the player activates their armor set bonus by holding down (or up if <see cref="Main.ReversedUpDownArmorSetBonuses"/> is true) for some amount of time. The <paramref name="holdTime"/> parameter indicates how many ticks the key has been held down for. As an example, the Stardust armor prior to 1.4.4 used to use this to set the location of the Stardust Guardian if <paramref name="holdTime"/> was greater than 60.
	/// <para/> Use this to implement armor set bonuses that need to be activated by the player.
	/// <para/> Don't forget to check if your armor set is active.
	/// <para/> While this technically can be used for other effects, it will likely be frustrating for your players if non-armor set effects are being triggered in tandem with armor set bonus effects. Modders can use <see cref="Player.holdDownCardinalTimer"/> and <see cref="Player.doubleTapCardinalTimer"/> directly in other hooks for similar effects if needed.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void ArmorSetBonusHeld(int holdTime)
	{
	}

	/// <summary>
	/// Use this to modify the control inputs that the player receives. For example, the Confused debuff swaps the values of Player.controlLeft and Player.controlRight. This is called sometime after PreUpdate is called.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void SetControls()
	{
	}

	/// <summary>
	/// This is called sometime after SetControls is called, and right before all the buffs update on this player. This hook can be used to add buffs to the player based on the player's state (for example, the Campfire buff is added if the player is near a Campfire).
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PreUpdateBuffs()
	{
	}

	/// <summary>
	/// This is called right after all of this player's buffs update on the player. This can be used to modify the effects that the buff updates had on this player, and can also be used for general update tasks.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PostUpdateBuffs()
	{
	}

	/// <summary>
	/// Called after Update Accessories.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void UpdateEquips()
	{
	}

	/// <summary>
	/// This is called right after all of this player's equipment and armor sets update on the player, which is sometime after PostUpdateBuffs is called. This can be used to modify the effects that the equipment had on this player, and can also be used for general update tasks.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PostUpdateEquips()
	{
	}

	/// <summary>
	/// Is called in Player.Frame() after vanilla functional slots are evaluated, including selection screen to prepare and denote visible accessories. Player Instance sensitive.
	/// </summary>
	public virtual void UpdateVisibleAccessories()
	{
	}

	/// <summary>
	/// Is called in Player.Frame() after vanilla vanity slots are evaluated, including selection screen to prepare and denote visible accessories. Player Instance sensitive.
	/// </summary>
	public virtual void UpdateVisibleVanityAccessories()
	{
	}

	/// <summary>
	/// Is called in Player.UpdateDyes(), including selection screen. Player Instance sensitive.
	/// </summary>
	public virtual void UpdateDyes()
	{
	}

	/// <summary>
	/// This is called after miscellaneous update code is called in Player.Update, which is sometime after PostUpdateEquips is called. This can be used for general update tasks.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PostUpdateMiscEffects()
	{
	}

	/// <summary>
	/// This is called after the player's horizontal speeds are modified, which is sometime after PostUpdateMiscEffects is called, and right before the player's horizontal position is updated. Use this to modify maxRunSpeed, accRunSpeed, runAcceleration, and similar variables before the player moves forwards/backwards.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PostUpdateRunSpeeds()
	{
	}

	/// <summary>
	/// This is called right before modifying the player's position based on velocity. Use this to make direct changes to the velocity.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PreUpdateMovement()
	{
	}

	/// <summary>
	/// This is called at the very end of the Player.Update method. Final general update tasks can be placed here.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PostUpdate()
	{
	}

	/// <summary>
	/// Use this hook to modify the jump duration from an extra jump.
	/// <para/> Called on local, server, and remote clients.
	/// <para/> Vanilla's extra jumps use the following values:
	/// <para>
	/// Basilisk mount: 0.75<br/>
	/// Blizzard in a Bottle: 1.5<br/>
	/// Cloud in a Bottle: 0.75<br/>
	/// Fart in a Jar: 2<br/>
	/// Goat mount: 2<br/>
	/// Sandstorm in a Bottle: 3<br/>
	/// Santank mount: 2<br/>
	/// Tsunami in a Bottle: 1.25<br/>
	/// Unicorn mount: 2
	/// </para>
	/// </summary>
	/// <param name="jump">The jump being performed</param>
	/// <param name="duration">A modifier to the player's jump height, which when combined effectively acts as the duration for the extra jump</param>
	public virtual void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration)
	{
	}

	/// <summary>
	/// An extra condition for whether an extra jump can be started.  Returns <see langword="true"/> by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="jump">The jump that would be performed</param>
	/// <returns><see langword="true"/> to let the jump be started, <see langword="false"/> otherwise.</returns>
	public virtual bool CanStartExtraJump(ExtraJump jump)
	{
		return true;
	}

	/// <summary>
	/// Effects that should appear when the extra jump starts should happen here.
	/// <para/> For example, the Cloud in a Bottle's initial puff of smoke is spawned here.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="jump">The jump being performed</param>
	/// <param name="playSound">Whether the poof sound should play.  Set this parameter to <see langword="false"/> if you want to play a different sound.</param>
	public virtual void OnExtraJumpStarted(ExtraJump jump, ref bool playSound)
	{
	}

	/// <summary>
	/// This hook runs before the <see cref="ExtraJumpState.Active"/> flag for an extra jump is set from <see langword="true"/> to <see langword="false"/> when the extra jump's duration has expired
	/// <para/> This occurs when a grappling hook is thrown, the player grabs onto a rope, the jump's duration has finished and when the player's frozen, turned to stone or webbed.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="jump">The jump that was performed</param>
	public virtual void OnExtraJumpEnded(ExtraJump jump)
	{
	}

	/// <summary>
	/// This hook runs before the <see cref="ExtraJumpState.Available"/> flag for an extra jump is set to <see langword="true"/> in <see cref="Player.RefreshDoubleJumps"/>
	/// <para/> This occurs at the start of the grounded jump and while the player is grounded.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="jump">The jump instance</param>
	public virtual void OnExtraJumpRefreshed(ExtraJump jump)
	{
	}

	/// <summary>
	/// Effects that should appear while the player is performing an extra jump should happen here.
	/// <para/> For example, the Sandstorm in a Bottle's dusts are spawned here.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void ExtraJumpVisuals(ExtraJump jump)
	{
	}

	/// <summary>
	/// Return <see langword="false"/> to prevent <see cref="ExtraJump.ShowVisuals(Player)"/> from executing on <paramref name="jump"/>.
	/// <para/> By default, this hook returns whether the player is moving upwards with respect to <see cref="Player.gravDir"/>
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="jump">The jump instance</param>
	public virtual bool CanShowExtraJumpVisuals(ExtraJump jump)
	{
		return true;
	}

	/// <summary>
	/// This hook runs before the <see cref="ExtraJumpState.Available"/> flag for an extra jump is set to <see langword="false"/>  in <see cref="Player.Update(int)"/> due to the jump being unavailable or when calling <see cref="Player.ConsumeAllExtraJumps"/> (vanilla calls it when a mount that blocks jumps is active)
	/// </summary>
	/// <param name="jump">The jump instance</param>
	public virtual void OnExtraJumpCleared(ExtraJump jump)
	{
	}

	/// <summary>
	/// Allows you to modify the armor and accessories that visually appear on the player. In addition, you can create special effects around this character, such as creating dust.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void FrameEffects()
	{
	}

	/// <summary>
	/// Allows you to make a player immune to damage from a certain source, or at a certain time.
	/// Vanilla examples include shimmer and journey god mode. Runs before dodges are used, or any damage calculations are performed.
	/// <para/> If immunity is determined on the local player, the hit will not be sent across the network.
	/// <para/> In pvp the hit will be sent regardless, and all clients will determine immunity independently, though it only really matters for the receiving player.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="damageSource">The source of the damage (projectile, NPC, etc)</param>
	/// <param name="cooldownCounter">The <see cref="ImmunityCooldownID"/> of the hit</param>
	/// <param name="dodgeable">Whether the hit is dodgeable</param>
	/// <returns>True to completely ignore the hit</returns>
	public virtual bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
	{
		return false;
	}

	/// <summary>
	/// Allows you to dodge damage for a player. Intended for guaranteed 'free' or random dodges. Vanilla example is black belt.
	/// <para/> For dodges which consume a stack/buff or have a cooldown, use <see cref="ConsumableDodge"/> instead.
	/// <para/> Called on the local client receiving damage.
	/// <para/> If dodge is determined on the local player, the hit will not be sent across the network. If visual indication of the dodge is required on remote clients, you will need to send your own packet.
	/// </summary>
	/// <returns>True to completely ignore the hit</returns>
	public virtual bool FreeDodge(Player.HurtInfo info)
	{
		return false;
	}

	/// <summary>
	/// Allows you to dodge damage for a player. Vanilla examples include hallowed armor shadow dodge, and brain of confusion.
	/// <para/> For dodges which are 'free' and should be used before triggering consumables, use <see cref="FreeDodge"/> instead.
	/// <para/> Called on the local client receiving damage.
	/// <para/> If dodge is determined on the local player, the hit will not be sent across the network.
	/// <para/> You may need to send your own packet to synchronize the consumption of the effect, or application of the cooldown in multiplayer.
	/// </summary>
	/// <returns>True to completely ignore the hit</returns>
	public virtual bool ConsumableDodge(Player.HurtInfo info)
	{
		return false;
	}

	/// <summary>
	/// Allows you to adjust an instance of player taking damage.
	/// <para/> Called on the local client taking damage.
	/// <para/> Only use this hook if you need to modify the hurt parameters in some way, eg consuming a buff which reduces the damage of the next hit. Use <see cref="OnHurt"/> or <see cref="PostHurt"/> instead where possible.
	/// <para/> The player will always take at least 1 damage. To prevent damage use <see cref="ImmuneTo"/> or <see cref="FreeDodge"/> <br/>
	/// </summary>
	public virtual void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to make anything happen when the player takes damage.
	/// <para/> Called on the local client taking damage.
	/// <para/> Called right before health is reduced.
	/// </summary>
	public virtual void OnHurt(Player.HurtInfo info)
	{
	}

	/// <summary>
	/// Allows you to make anything happen when the player takes damage.
	/// <para/> Called on the local client taking damage
	/// <para/> Only called if the player survives the hit.
	/// </summary>
	public virtual void PostHurt(Player.HurtInfo info)
	{
	}

	/// <summary>
	/// This hook is called whenever the player is about to be killed after reaching 0 health.
	/// <para/> Called on local, server, and remote clients.
	/// <para/> Set the <paramref name="playSound"/> parameter to false to stop the death sound from playing. Set the <paramref name="genDust"/> parameter to false to stop the dust from being created. These are useful for creating your own sound or dust to replace the normal death effects, such as how the Frost armor set spawns <see cref="DustID.IceTorch"/> instead of <see cref="DustID.Blood"/>. For mod compatibility, it is recommended to check if these values are true before setting them to true and spawning dust or playing sounds to avoid overlapping sounds and dust effects.
	/// <para/> Return false to stop the player from being killed. Only return false if you know what you are doing! Returns true by default.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="hitDirection"></param>
	/// <param name="pvp"></param>
	/// <param name="playSound"></param>
	/// <param name="genDust"></param>
	/// <param name="damageSource"></param>
	/// <returns></returns>
	public virtual bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust,
		ref PlayerDeathReason damageSource)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make anything happen when the player dies.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="hitDirection"></param>
	/// <param name="pvp"></param>
	/// <param name="damageSource"></param>
	public virtual void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
	{
	}

	/// <summary>
	/// Called before vanilla makes any luck calculations. Return false to prevent vanilla from making their luck calculations. Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="luck"></param>
	public virtual bool PreModifyLuck(ref float luck)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify a player's luck amount.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="luck"></param>
	public virtual void ModifyLuck(ref float luck)
	{
	}

	/// <summary>
	/// Allows you to do anything before the update code for the player's held item is run. Return false to stop the held item update code from being run (for example, if the player is frozen). Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <returns></returns>
	public virtual bool PreItemCheck()
	{
		return true;
	}

	/// <summary>
	/// Allows you to do anything after the update code for the player's held item is run. Hooks for the middle of the held item update code have more specific names in ModItem and ModPlayer.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void PostItemCheck()
	{
	}

	/// <summary>
	/// Allows you to change the effective useTime of an item.
	/// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <returns> The multiplier on the usage time. 1f by default. Values greater than 1 increase the item use's length. </returns>
	public virtual float UseTimeMultiplier(Item item) => 1f;

	/// <summary>
	/// Allows you to change the effective useAnimation of an item.
	/// <para/> Note that this hook may cause items' actions to run less or more times than they should per a single use.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <returns>The multiplier on the animation time. 1f by default. Values greater than 1 increase the item animation's length. </returns>
	public virtual float UseAnimationMultiplier(Item item) => 1f;

	/// <summary>
	/// Allows you to safely change both useTime and useAnimation while keeping the values relative to each other.
	/// <para/> Useful for status effects.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <returns> The multiplier on the use speed. 1f by default. Values greater than 1 increase the overall item speed. </returns>
	public virtual float UseSpeedMultiplier(Item item) => 1f;

	/// <summary>
	/// Allows you to temporarily modify the amount of life a life healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healLife"/> value.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
	/// <param name="healValue">The amount of life being healed.</param>
	public virtual void GetHealLife(Item item, bool quickHeal, ref int healValue)
	{
	}

	/// <summary>
	/// Allows you to temporarily modify the amount of mana a mana healing item will heal for, based on player buffs, accessories, etc. This is only called for items with a <see cref="Item.healMana"/> value.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="quickHeal">Whether the item is being used through quick heal or not.</param>
	/// <param name="healValue">The amount of mana being healed.</param>
	public virtual void GetHealMana(Item item, bool quickHeal, ref int healValue)
	{
	}

	/// <summary>
	/// Allows you to temporarily modify the amount of mana an item will consume on use, based on player buffs, accessories, etc. This is only called for items with a mana value.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="reduce">Used for decreasingly stacking buffs (most common). Only ever use -= on this field.</param>
	/// <param name="mult">Use to directly multiply the item's effective mana cost. Good for debuffs, or things which should stack separately (eg meteor armor set bonus).</param>
	public virtual void ModifyManaCost(Item item, ref float reduce, ref float mult)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen when a player doesn't have enough mana for the item they are trying to use.
	/// If the player has high enough mana after this hook runs, mana consumption will happen normally.
	/// Only runs once per item use.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="neededMana">The mana needed to use the item.</param>
	public virtual void OnMissingMana(Item item, int neededMana)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen when a player consumes mana on use of an item.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="manaConsumed">The mana consumed from the player.</param>
	public virtual void OnConsumeMana(Item item, int manaConsumed)
	{
	}

	/// <summary>
	/// Called before the potion delay is applied to the player after consuming a healing potion.
	/// <br/><br/> Return false to prevent application of the <see cref="BuffID.PotionSickness"/> buff and setting <see cref="Player.potionDelay"/>.
	/// </summary>
	/// <param name="item">The healing item being used.</param>
	/// <param name="potionDelay">The calculated potion delay.</param>
	public virtual bool ApplyPotionDelay(Item item, int potionDelay)
	{
		return true;
	}

	/// <summary>
	/// Allows you to dynamically modify a weapon's damage based on player and item conditions.
	/// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="damage">The StatModifier object representing the totality of the various modifiers to be applied to the item's base damage.</param>
	public virtual void ModifyWeaponDamage(Item item, ref StatModifier damage)
	{
	}

	/// <summary>
	/// Allows you to dynamically modify a weapon's knockback based on player and item conditions.
	/// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item being used.</param>
	/// <param name="knockback">The StatModifier object representing the totality of the various modifiers to be applied to the item's base knockback.</param>
	public virtual void ModifyWeaponKnockback(Item item, ref StatModifier knockback)
	{
	}

	/// <summary>
	/// Allows you to dynamically modify a weapon's crit chance based on player and item conditions.
	/// Can be utilized to modify damage beyond the tools that DamageClass has to offer.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="crit">The total crit chance of the item after all normal crit chance calculations.</param>
	public virtual void ModifyWeaponCrit(Item item, ref float crit)
	{
	}

	/// <summary>
	/// Whether or not the given ammo item will be consumed by this weapon.<br></br>
	/// By default, returns true; return false to prevent ammo consumption. <br></br>
	/// If false is returned, the <see cref="OnConsumeAmmo"/> hook is never called.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="weapon">The weapon that this player is attempting to use.</param>
	/// <param name="ammo">The ammo that the given weapon is attempting to consume.</param>
	/// <returns></returns>
	public virtual bool CanConsumeAmmo(Item weapon, Item ammo)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make things happen when the given ammo is consumed by the given weapon.<br></br>
	/// Called before the ammo stack is reduced, and is never called if the ammo isn't consumed in the first place.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="weapon">The weapon that is currently using the given ammo.</param>
	/// <param name="ammo">The ammo that the given weapon is currently using.</param>
	public virtual void OnConsumeAmmo(Item weapon, Item ammo)
	{
	}

	/// <summary>
	/// Allows you to prevent an item from shooting a projectile on use. Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="item"> The item being used. </param>
	/// <returns></returns>
	public virtual bool CanShoot(Item item)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by an item.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item"> The item being used. </param>
	/// <param name="position"> The center position of the projectile. </param>
	/// <param name="velocity"> The velocity of the projectile. </param>
	/// <param name="type"> The ID of the projectile. </param>
	/// <param name="damage"> The damage of the projectile. </param>
	/// <param name="knockback"> The knockback of the projectile. </param>
	public virtual void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
	}

	/// <summary>
	/// Allows you to modify an item's shooting mechanism. Return false to prevent vanilla's shooting code from running. Returns true by default.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item"> The item being used. </param>
	/// <param name="source"> The projectile source's information. </param>
	/// <param name="position"> The center position of the projectile. </param>
	/// <param name="velocity"> The velocity of the projectile. </param>
	/// <param name="type"> The ID of the projectile. </param>
	/// <param name="damage"> The damage of the projectile. </param>
	/// <param name="knockback"> The knockback of the projectile. </param>
	public virtual bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		return true;
	}

	/// <summary>
	/// Allows you to give this player's melee weapon special effects, such as creating light or dust. This is typically used to implement a weapon enchantment, similar to flasks, frost armor, or magma stone effects.
	/// <para/> If implementing a weapon enchantment, also implement <see cref="EmitEnchantmentVisualsAt(Projectile, Vector2, int, int)"/> to support enchantment visuals for projectiles as well.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="hitbox"></param>
	public virtual void MeleeEffects(Item item, Rectangle hitbox)
	{
	}

	/// <inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
	public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
	{
	}

	/// <summary>
	/// Allows you to determine whether the given item can catch the given NPC.<br></br>
	/// Return true or false to say the target can or cannot be caught, respectively, regardless of vanilla rules.<br></br>
	/// Returns null by default, which allows vanilla's NPC catching rules to decide the target's fate.<br></br>
	/// If this returns false, <see cref="CombinedHooks.OnCatchNPC"/> is never called.<br></br>
	/// <para/> Called on the local client only.
	/// <para/> NOTE: this does not classify the given item as a catch tool, which is necessary for catching NPCs in the first place.
	/// To do that, you will need to use the "CatchingTool" set in ItemID.Sets.
	/// </summary>
	/// <param name="target">The NPC the player is trying to catch.</param>
	/// <param name="item">The item with which the player is trying to catch the target NPC.</param>
	public virtual bool? CanCatchNPC(NPC target, Item item)
	{
		return null;
	}

	/// <summary>
	/// Allows you to make things happen when the given item attempts to catch the given NPC.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="npc">The NPC which the player attempted to catch.</param>
	/// <param name="item">The item used to catch the given NPC.</param>
	/// <param name="failed">Whether or not the given NPC has been successfully caught.</param>
	public virtual void OnCatchNPC(NPC npc, Item item, bool failed)
	{
	}

	/// <summary>
	/// Allows you to dynamically modify the given item's size for this player, similarly to the effect of the Titan Glove.
	/// <para/> Called on local and remote clients
	/// </summary>
	/// <param name="item">The item to modify the scale of.</param>
	/// <param name="scale">
	/// The scale multiplier to be applied to the given item.<br></br>
	/// Will be 1.1 if the Titan Glove is equipped, and 1 otherwise.
	/// </param>
	public virtual void ModifyItemScale(Item item, ref float scale)
	{
	}

	/// <summary>
	/// This hook is called when a player damages anything, whether it be an NPC or another player, using anything, whether it be a melee weapon or a projectile. The x and y parameters are the coordinates of the victim parameter's center.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="victim"></param>
	public virtual void OnHitAnything(float x, float y, Entity victim)
	{
	}

	/// <summary>
	/// Allows you to determine whether a player can hit the given NPC. Returns true by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="target"></param>
	/// <returns>True by default</returns>
	public virtual bool CanHitNPC(NPC target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether a player melee attack can collide the given NPC by swinging a melee weapon.
	/// Use <see cref="CanHitNPCWithItem(Item, NPC)"/> instead for Guide Voodoo Doll-type effects.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="item">The weapon item the player is holding.</param>
	/// <param name="meleeAttackHitbox">Hitbox of melee attack.</param>
	/// <param name="target">The target npc.</param>
	/// <returns>
	/// Return true to allow colliding the target, return false to block the player weapon from colliding the target, and return null to use the vanilla code for whether the target can be colliding by melee weapon. Returns null by default.
	/// </returns>
	public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc that this player does to an NPC.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when this player hits an NPC.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to determine whether a player can hit the given NPC by swinging a melee weapon. Return true to allow hitting the target, return false to block this player from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool? CanHitNPCWithItem(Item item, NPC target)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that this player does to an NPC by swinging a melee weapon.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when this player hits an NPC by swinging a melee weapon (for example how the Pumpkin Sword creates pumpkin heads).
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to determine whether a projectile created by this player can hit the given NPC. Return true to allow hitting the target, return false to block this projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="proj"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool? CanHitNPCWithProj(Projectile proj, NPC target)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that a projectile created by this player does to an NPC.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="proj"></param>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when a projectile created by this player hits an NPC (for example, inflicting debuffs).
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="proj"></param>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to determine whether a melee weapon swung by this player can hit the given opponent player. Return false to block this weapon from hitting the target. Returns true by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool CanHitPvp(Item item, Player target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether a projectile created by this player can hit the given opponent player. Return false to block the projectile from hitting the target. Returns true by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="proj"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool CanHitPvpWithProj(Projectile proj, Player target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether the given NPC can hit this player. Return false to block this player from being hit by the NPC. Returns true by default. CooldownSlot determines which of the player's cooldown counters (<see cref="ImmunityCooldownID"/>) to use, and defaults to -1 (<see cref="ImmunityCooldownID.General"/>).
	/// <para/> Called on the client taking damage
	/// </summary>
	/// <param name="npc"></param>
	/// <param name="cooldownSlot"></param>
	/// <returns></returns>
	public virtual bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, etc., that an NPC does to this player.
	/// <para/> Called on the client taking damage
	/// </summary>
	public virtual void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when an NPC hits this player (for example, inflicting debuffs).
	/// <para/> Called on the client taking damage
	/// </summary>
	public virtual void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
	{
	}

	/// <summary>
	/// Allows you to determine whether the given hostile projectile can hit this player. Return false to block this player from being hit. Returns true by default.
	/// <para/> Called on the client taking damage
	/// </summary>
	/// <param name="proj"></param>
	/// <returns></returns>
	public virtual bool CanBeHitByProjectile(Projectile proj)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, etc., that a hostile projectile does to this player.
	/// <para/> Called on the client taking damage
	/// </summary>
	public virtual void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when a hostile projectile hits this player.
	/// <para/> Called on the client taking damage
	/// </summary>
	public virtual void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
	{
	}

	/// <summary>
	/// Allows you to change information about the ongoing fishing attempt before caught items/NPCs are decided, after all vanilla information has been gathered.
	/// <para/> Will not be called if various conditions for getting a catch aren't met, meaning you can't modify those.
	/// <para/> Setting <see cref="FishingAttempt.rolledItemDrop"/> or <see cref="FishingAttempt.rolledEnemySpawn"/> is not allowed and will be reset, use <see cref="CatchFish"/> for that.
	/// <para/> Called for the local client only.
	/// </summary>
	/// <param name="attempt">The structure containing most data from the vanilla fishing attempt</param>
	public virtual void ModifyFishingAttempt(ref FishingAttempt attempt)
	{
	}

	/// <summary>
	/// Allows you to change the item or enemy the player gets when successfully catching an item or NPC. The Fishing Attempt structure contains most information about the vanilla event, including the Item Rod and Bait used by the player, the liquid it is being fished on, and so on.
	/// The Sonar and Sonar position fields allow you to change the text, color, velocity and position of the catch's name (be it item or NPC) freely
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="attempt">The structure containing most data from the vanilla fishing attempt</param>
	/// <param name="itemDrop">The item that will be created when this fishing attempt succeeds. leave &lt;0 for no item</param>
	/// <param name="npcSpawn">The enemy that will be spawned if there is no item caught. leave &lt;0 for no NPC spawn</param>
	/// <param name="sonar">Fill all of this structure's fields to override the sonar text, or make sonar.Text null to disable custom sonar</param>
	/// <param name="sonarPosition">The position the Sonar text will spawn. Bobber location by default.</param>
	public virtual void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
	{
	}

	/// <summary>
	/// Allows you to modify the item caught by the fishing player, including stack
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="fish">The item (Fish) to modify</param>
	public virtual void ModifyCaughtFish(Item fish)
	{
	}

	/// <summary>
	/// Choose if this bait will be consumed or not when used for fishing. return null for vanilla behavior.
	/// Not consuming will always take priority over forced consumption
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="bait">The item (bait) that would be consumed</param>
	public virtual bool? CanConsumeBait(Item bait)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the player's fishing power. As an example of the type of stuff that should go here, the phase of the moon can influence fishing power.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="fishingRod"></param>
	/// <param name="bait"></param>
	/// <param name="fishingLevel"></param>
	public virtual void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
	{
	}

	/// <summary>
	/// Allows you to add to, change, or remove from the items the player earns when finishing an Angler quest. The rareMultiplier is a number between 0.15 and 1 inclusively; the lower it is the higher chance there should be for the player to earn rare items.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="rareMultiplier"></param>
	/// <param name="rewardItems"></param>
	public virtual void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
	{
	}

	/// <summary>
	/// Allows you to modify what items are possible for the player to earn when giving a Strange Plant to the Dye Trader.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="rewardPool"></param>
	public virtual void GetDyeTraderReward(List<int> rewardPool)
	{
	}

	/// <summary>
	/// Allows you to create special effects when this player is drawn, such as creating dust, modifying the color the player is drawn in, etc. The fullBright parameter makes it so that the drawn player ignores the modified color and lighting. Make sure to add the indexes of any dusts you create to drawInfo.DustCache, and the indexes of any gore you create to drawInfo.GoreCache.
	/// <para/> This will be called multiple times a frame if a player afterimage is being drawn. Check <code>if(drawinfo.shadow == 0f)</code> to do some logic only when drawing the original player image. For example, spawning dust only for the original player image is commonly the desired behavior.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="drawInfo"></param>
	/// <param name="r"></param>
	/// <param name="g"></param>
	/// <param name="b"></param>
	/// <param name="a"></param>
	/// <param name="fullBright"></param>
	public virtual void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
	{
	}

	/// <summary>
	/// Allows you to modify the drawing parameters of the player before drawing begins (before every <see cref="PlayerDrawLayer"/> runs).
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="drawInfo"></param>
	public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
	}

	/// <summary>
	/// Allows modifying player draw data after all <see cref="PlayerDrawLayer"/> have run (and added <see cref="PlayerDrawSet.DrawDataCache"/> entries) and immediately before the <see cref="PlayerDrawSet.DrawDataCache"/> entries are actually drawn.
	/// <br/><br/> This can be used to modify all <see cref="PlayerDrawSet.DrawDataCache"/> entries, such as modifying the draw color of each entry. Vanilla uses this to apply the scale parameter of <see cref="Graphics.Renderers.IPlayerRenderer.DrawPlayer"/> and to customize how the First Fractal clones are rendered (unobtainable weapon).
	/// </summary>
	public virtual void TransformDrawData(ref PlayerDrawSet drawInfo)
	{
	}

	/// <summary>
	/// Allows you to reorder the player draw layers.
	/// This is called once at the end of mod loading, not during the game.
	/// Use with extreme caution, or risk breaking other mods.
	/// </summary>
	/// <param name="positions">Add/remove/change the positions applied to each layer here</param>
	public virtual void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
	{
	}

	/// <summary>
	/// Allows you to modify the visibility of layers about to be drawn. All layers can be accessed via <see cref="PlayerDrawLayerLoader.Layers"/>. Individual layers can be accessed by the <see cref="PlayerDrawLayers"/> fields for vanilla layers or <see cref="ModContent.GetInstance{T}"/> for modded layers. The <see cref="PlayerDrawLayer.Hide"/> method is how a layer can be hidden.
	/// <br/><br/> For example: <code>PlayerDrawLayers.Wings.Hide();
	/// ModContent.GetInstance&lt;ExamplePlayerDrawLayer&gt;().Hide();</code>
	/// <br/><br/> Called on local and remote clients.
	/// </summary>
	/// <param name="drawInfo"></param>
	public virtual void HideDrawLayers(PlayerDrawSet drawInfo)
	{
	}

	/// <summary>
	/// Use this hook to modify <see cref="Main.screenPosition"/> after weapon zoom and camera lerp have taken place.
	/// <para/> Called on the local client only.
	/// <para/> Also consider using <c>Main.instance.CameraModifiers.Add(CameraModifier);</c> as shown in ExampleMods MinionBossBody for screen shakes.
	/// </summary>
	public virtual void ModifyScreenPosition()
	{
	}

	/// <summary>
	/// Use this to modify the zoom factor for the player. The zoom correlates to the percentage of half the screen size the zoom can reach. A value of -1 passed in means no vanilla scope is in effect. A value of 1.0 means the scope can zoom half a screen width/height away, putting the player on the edge of the game screen. Vanilla values include .8, .6666, and .5.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="zoom"></param>
	public virtual void ModifyZoom(ref float zoom)
	{
	}

	/// <summary>
	/// Called on remote clients when a player connects.
	/// </summary>
	public virtual void PlayerConnect()
	{
	}

	/// <summary>
	/// Called on the server and remote clients when a player disconnects.
	/// </summary>
	public virtual void PlayerDisconnect()
	{
	}

	/// <summary>
	/// Called when the player enters the world. A possible use is ensuring that UI elements are reset to the configuration specified in data saved to the ModPlayer. Can also be used for informational messages.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void OnEnterWorld()
	{
	}

	/// <summary>
	/// Called when a player respawns in the world.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void OnRespawn()
	{
	}

	/// <summary>
	/// Called whenever the player shift-clicks an item slot. This can be used to override default clicking behavior (ie. selling, trashing, moving items).
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="inventory">The array of items the slot is part of.</param>
	/// <param name="context">The Terraria.UI.ItemSlot.Context of the inventory.</param>
	/// <param name="slot">The index in the inventory of the clicked slot.</param>
	/// <returns>Whether or not to block the default code (sell, trash, move, etc) from running. Returns false by default.</returns>
	public virtual bool ShiftClickSlot(Item[] inventory, int context, int slot)
	{
		return false;
	}

	/// <summary>
	/// Called whenever the player hovers over an item slot. This can be used to override <see cref="Main.cursorOverride"/>
	/// <para/> Called on the local client only.
	/// <para/> See <see cref="ID.CursorOverrideID"/> for cursor override style IDs
	/// </summary>
	/// <param name="inventory">The array of items the slot is part of.</param>
	/// <param name="context">The Terraria.UI.ItemSlot.Context of the inventory.</param>
	/// <param name="slot">The index in the inventory of the hover slot.</param>
	/// <returns>Whether or not to block the default code that modifies <see cref="Main.cursorOverride"/> from running. Returns false by default.</returns>
	public virtual bool HoverSlot(Item[] inventory, int context, int slot)
	{
		return false;
	}

	/// <summary>
	/// Called whenever the player sells an item to an NPC.
	/// <para/> Called on the local client only.
	/// <para/> Note that <paramref name="item"/> might be an item sold by the NPC, not an item to buy back. Check <see cref="Item.buyOnce"/> if relevant to your logic.
	/// </summary>
	/// <param name="vendor">The NPC vendor.</param>
	/// <param name="shopInventory">The current inventory of the NPC shop.</param>
	/// <param name="item">The item the player just sold.</param>
	public virtual void PostSellItem(NPC vendor, Item[] shopInventory, Item item)
	{
	}

	/// <summary>
	/// Return false to prevent a transaction. Called before the transaction.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="vendor">The NPC vendor.</param>
	/// <param name="shopInventory">The current inventory of the NPC shop.</param>
	/// <param name="item">The item the player is attempting to sell.</param>
	/// <returns></returns>
	public virtual bool CanSellItem(NPC vendor, Item[] shopInventory, Item item)
	{
		return true;
	}

	/// <summary>
	/// Called whenever the player buys an item from an NPC.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="vendor">The NPC vendor.</param>
	/// <param name="shopInventory">The current inventory of the NPC shop.</param>
	/// <param name="item">The item the player just purchased.</param>
	public virtual void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
	{
	}

	/// <summary>
	/// Return false to prevent a transaction. Called before the transaction.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="vendor">The NPC vendor.</param>
	/// <param name="shopInventory">The current inventory of the NPC shop.</param>
	/// <param name="item">The item the player is attempting to buy.</param>
	/// <returns></returns>
	public virtual bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item)
	{
		return true;
	}

	/// <summary>
	/// Return false to prevent an item from being used. By default returns true.
	/// <para/> Called on local, server, and remote clients.
	/// <br/><br/> The item may or not be used after this method is called, so logic in this method should have no side effects such as consuming items or resources.
	/// </summary>
	/// <param name="item">The item the player is attempting to use.</param>
	public virtual bool CanUseItem(Item item)
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
	public virtual bool? CanAutoReuseItem(Item item) => null;

	/// <summary>
	/// Called while the nurse chat is displayed. Return false to prevent the player from healing. If you return false, you need to set chatText so the user knows why they can't heal.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="nurse">The Nurse NPC instance.</param>
	/// <param name="health">How much health the player gains.</param>
	/// <param name="removeDebuffs">If set to false, debuffs will not be healed.</param>
	/// <param name="chatText">Set this to the Nurse chat text that will display if healing is prevented.</param>
	/// <returns>True by default. False to prevent nurse services.</returns>
	public virtual bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
	{
		return true;
	}

	/// <summary>
	/// Called while the nurse chat is displayed and after ModifyNurseHeal. Allows custom pricing for Nurse services. See the <see href="https://terraria.wiki.gg/wiki/Nurse">Nurse wiki page</see> for the default pricing.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="nurse">The Nurse NPC instance.</param>
	/// <param name="health">How much health the player gains.</param>
	/// <param name="removeDebuffs">Whether or not debuffs will be healed.</param>
	/// <param name="price"></param>
	public virtual void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price)
	{
	}

	/// <summary>
	/// Called after the player heals themselves with the Nurse NPC.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="nurse">The Nurse npc providing the heal.</param>
	/// <param name="health">How much health the player gained.</param>
	/// /// <param name="removeDebuffs">Whether or not debuffs were healed.</param>
	/// <param name="price">The price the player paid in copper coins.</param>
	public virtual void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price)
	{
	}

	/// <summary>
	/// Called when the player is created in the menu.
	/// You can use this method to add items to the player's starting inventory, as well as their inventory when they respawn in mediumcore.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="mediumCoreDeath">Whether you are setting up a mediumcore player's inventory after their death.</param>
	/// <returns>An enumerable of the items you want to add. If you want to add nothing, return Enumerable.Empty&lt;Item&gt;().</returns>
	public virtual IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
	{
		return Enumerable.Empty<Item>();
	}

	/// <summary>
	/// Allows you to modify the items that will be added to the player's inventory. Useful if you want to stop vanilla or other mods from adding an item.
	/// You can access a mod's items by using the mod's internal name as the indexer, such as: additions["ModName"]. To access vanilla items you can use "Terraria" as the index.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="itemsByMod">The items that will be added. Each key is the internal mod name of the mod adding the items. Vanilla items use the "Terraria" key.</param>
	/// <param name="mediumCoreDeath">Whether you are setting up a mediumcore player's inventory after their death.</param>
	public virtual void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
	{
	}

	/// <summary>
	/// An action to be invoked when an item is partially or fully consumed
	/// </summary>
	/// <param name="item">The item that has been consumed. May have been set to air if the item was fully consumed.</param>
	/// <param name="index">The index of the item enumerated in IEnumerable&lt;Item&gt;</param>
	public delegate void ItemConsumedCallback(Item item, int index);

	/// <summary>
	/// Called when Recipe.FindRecipes is called or the player is crafting an item
	/// You can use this method to add items as the materials that may be used for crafting items
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="itemConsumedCallback">The action that gets invoked when the item is consumed</param>
	/// <returns>A list of the items that may be used as crafting materials or null if none are available.</returns>
	public virtual IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
	{
		itemConsumedCallback = null;
		return null;
	}

	/// <summary>
	/// Allows you to make special things happen when this player picks up an item. Return false to stop the item from being added to the player's inventory; returns true by default.
	/// <para/> Called on the local client only.
	/// </summary>
	/// <param name="item">The item being picked up</param>
	/// <returns></returns>
	public virtual bool OnPickup(Item item)
	{
		return true;
	}

	/// <summary>
	/// Whether or not the player can be teleported to the given coordinates with methods such as Teleportation Potions or the Rod of Discord.
	/// <para/> The coordinates correspond to the top left corner of the player position after teleporting.
	/// <para/> This gets called in <see cref="Player.CheckForGoodTeleportationSpot(ref bool, int, int, int, int, Player.RandomTeleportationAttemptSettings)"/> and <see cref="Player.ItemCheck_UseTeleportRod(Item)"/>. The <paramref name="context"/> will have a value of "CheckForGoodTeleportationSpot" or "TeleportRod" respectively indicating which type of teleport is being attempted.
	/// </summary>
	public virtual bool CanBeTeleportedTo(Vector2 teleportPosition, string context)
	{
		return true;
	}

	/// <summary>
	/// Allows to execute some code if the equipment loadout was switched.
	/// <br/><br/> Called on the server and on all clients.
	/// </summary>
	/// <param name="oldLoadoutIndex">The old loadout index.</param>
	/// <param name="loadoutIndex">The new loadout index.</param>
	public virtual void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex)
	{
	}

	/// <summary>
	/// Allows drawing additional copies of the player, usually to implement an armor set shadow visual effect, dodge effect, or dash effect. Use <c>Main.PlayerRenderer.DrawPlayer(...)</c> to draw the additional player copies. The normal player will be drawn after this method runs.
	/// <br/><br/> Called during <see cref="Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayerFull(Graphics.Camera, Player)"/>.
	/// </summary>
	public virtual void DrawPlayer(Camera camera)
	{
	}
}
