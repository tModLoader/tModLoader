using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to modify and use hooks for all projectiles, both vanilla and modded.
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// </summary>
public abstract class GlobalProjectile : GlobalType<Projectile, GlobalProjectile>
{
	protected override void ValidateType()
	{
		base.ValidateType();

		LoaderUtils.MustOverrideTogether(this, g => g.SendExtraAI, g => g.ReceiveExtraAI);
	}

	protected sealed override void Register()
	{
		base.Register();
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Gets called when any projectiles spawns in world
	/// <para/> Called on the client or server spawning the projectile via Projectile.NewProjectile.
	/// </summary>
	public virtual void OnSpawn(Projectile projectile, IEntitySource source)
	{
	}

	/// <summary>
	/// Allows you to determine how any projectile behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
	/// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <returns></returns>
	public virtual bool PreAI(Projectile projectile)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine how any projectile behaves. This will only be called if PreAI returns true.
	/// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	public virtual void AI(Projectile projectile)
	{
	}

	/// <summary>
	/// Allows you to determine how any projectile behaves. This will be called regardless of what PreAI returns.
	/// <include file = 'CommonDocs.xml' path='Common/AIMethodOrder' />
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	public virtual void PostAI(Projectile projectile)
	{
	}

	/// <summary>
	/// Use this judiciously to avoid straining the network.
	/// <para/> Checks and methods such as <see cref="GlobalType{TEntity, TGlobal}.AppliesToEntity"/> can reduce how much data must be sent for how many projectiles.
	/// <para/> Called whenever <see cref="MessageID.SyncProjectile"/> is successfully sent, for example on projectile creation, or whenever Projectile.netUpdate is set to true in the update loop for that tick.
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile">The projectile.</param>
	/// <param name="bitWriter">The compressible bit writer. Booleans written via this are compressed across all mods to improve multiplayer performance.</param>
	/// <param name="binaryWriter">The writer.</param>
	public virtual void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
	}

	/// <summary>
	/// Use this to receive information that was sent in <see cref="SendExtraAI"/>.
	/// <para/> Called whenever <see cref="MessageID.SyncProjectile"/> is successfully received.
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile">The projectile.</param>
	/// <param name="bitReader">The compressible bit reader.</param>
	/// <param name="binaryReader">The reader.</param>
	public virtual void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
	{
	}

	/// <summary>
	/// Whether or not the given projectile should update its position based on factors such as its velocity, whether it is in liquid, etc. Return false to make its velocity have no effect on its position. Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <returns></returns>
	public virtual bool ShouldUpdatePosition(Projectile projectile)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine how a projectile interacts with tiles. Return false if you completely override or cancel a projectile's tile collision behavior. Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"> The projectile. </param>
	/// <param name="width"> The width of the hitbox the projectile will use for tile collision. If vanilla or a mod don't modify it, defaults to projectile.width. </param>
	/// <param name="height"> The height of the hitbox the projectile will use for tile collision. If vanilla or a mod don't modify it, defaults to projectile.height. </param>
	/// <param name="fallThrough"> Whether or not the projectile falls through platforms and similar tiles. </param>
	/// <param name="hitboxCenterFrac"> Determines by how much the tile collision hitbox's position (top left corner) will be offset from the projectile's real center. If vanilla or a mod don't modify it, defaults to half the hitbox size (new Vector2(0.5f, 0.5f)). </param>
	/// <returns></returns>
	public virtual bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine what happens when a projectile collides with a tile. OldVelocity is the velocity before tile collision. The velocity that takes tile collision into account can be found with projectile.velocity. Return true to allow the vanilla tile collision code to take place (which normally kills the projectile). Returns true by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="oldVelocity"></param>
	/// <returns></returns>
	public virtual bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether the vanilla code for Kill and the Kill hook will be called. Return false to stop them from being called. Returns true by default. Note that this does not stop the projectile from dying.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="timeLeft"></param>
	/// <returns></returns>
	public virtual bool PreKill(Projectile projectile, int timeLeft)
	{
		return true;
	}

	/// <summary>
	/// Allows you to control what happens when a projectile is killed (for example, creating dust or making sounds).
	/// <br/><br/> Be sure to check <c>if (Projectile.owner == Main.myPlayer)</c> for logic that should only run for the projectile owner. Some examples include spawning items, spawning projectiles, and modifying tiles.
	/// <br/><br/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="timeLeft"></param>
	public virtual void OnKill(Projectile projectile, int timeLeft)
	{
	}

	[Obsolete("Renamed to OnKill", error: true)] // Remove in 2023_10
	public virtual void Kill(Projectile projectile, int timeLeft)
	{
	}

	/// <summary>
	/// Return true or false to specify if the projectile can cut tiles like vines, pots, and Queen Bee larva. Return null for vanilla decision.
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile"></param>
	/// <returns></returns>
	public virtual bool? CanCutTiles(Projectile projectile)
	{
		return null;
	}

	/// <summary>
	/// Code ran when the projectile cuts tiles. Only runs if CanCutTiles() returns true. Useful when programming lasers and such.
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile"></param>
	public virtual void CutTiles(Projectile projectile)
	{
	}

	/// <summary>
	/// Whether or not the given projectile is capable of killing tiles (such as grass) and damaging NPCs/players.
	/// Return false to prevent it from doing any sort of damage.
	/// Return true if you want the projectile to do damage regardless of the default blacklist.
	/// Return null to let the projectile follow vanilla can-damage-anything rules. This is what happens by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <returns></returns>
	public virtual bool? CanDamage(Projectile projectile)
	{
		return null;
	}

	/// <summary>
	/// Whether or not a minion can damage NPCs by touching them. Returns false by default. Note that this will only be used if the projectile is considered a pet.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <returns></returns>
	public virtual bool MinionContactDamage(Projectile projectile)
	{
		return false;
	}

	/// <summary>
	/// Allows you to change the hitbox used by a projectile for damaging players and NPCs.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="hitbox"></param>
	public virtual void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
	{
	}

	/// <summary>
	/// Allows you to determine whether a projectile can hit the given NPC. Return true to allow hitting the target, return false to block the projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool? CanHitNPC(Projectile projectile, NPC target)
	{
		return null;
	}

	/// <summary>
	/// Allows you to modify the damage, knockback, etc., that a projectile does to an NPC.
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when a projectile hits an NPC (for example, inflicting debuffs).
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public virtual void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
	{
	}

	/// <summary>
	/// Allows you to determine whether a projectile can hit the given opponent player. Return false to block the projectile from hitting the target. Returns true by default.
	/// <para/> Called on the client hitting the target.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool CanHitPvp(Projectile projectile, Player target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine whether a hostile projectile can hit the given player. Return false to block the projectile from hitting the target. Returns true by default.
	/// <para/> Called on the server only.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public virtual bool CanHitPlayer(Projectile projectile, Player target)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the damage, etc., that a hostile projectile does to a player.
	/// <para/> Called on the client taking damage.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="target"></param>
	/// <param name="modifiers"></param>
	public virtual void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
	{
	}

	/// <summary>
	/// Allows you to create special effects when a hostile projectile hits a player. <br/>
	/// <para/> Called on the client taking damage.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="target"></param>
	/// <param name="info"></param>
	public virtual void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
	{
	}

	/// <summary>
	/// Allows you to use custom collision detection between a projectile and a player or NPC that the projectile can damage. Useful for things like diagonal lasers, projectiles that leave a trail behind them, etc.
	/// <para/> Can be called on the local client or server, depending on who owns the projectile.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="projHitbox"></param>
	/// <param name="targetHitbox"></param>
	/// <returns></returns>
	public virtual bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine the color and transparency in which a projectile is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="lightColor"></param>
	/// <returns></returns>
	public virtual Color? GetAlpha(Projectile projectile, Color lightColor)
	{
		return null;
	}

	/// <summary>
	/// Allows you to draw things behind a projectile. Use the <c>Main.EntitySpriteDraw</c> method for drawing. Returns false to stop the game from drawing extras textures related to the projectile (for example, the chains for grappling hooks), useful if you're manually drawing the extras. Returns true by default.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="projectile"> The projectile. </param>
	public virtual bool PreDrawExtras(Projectile projectile)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things behind a projectile, or to modify the way the projectile is drawn. Use the <c>Main.EntitySpriteDraw</c> method for drawing. Return false to stop the vanilla projectile drawing code (useful if you're manually drawing the projectile). Returns true by default.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="projectile"> The projectile. </param>
	/// <param name="lightColor"> The color of the light at the projectile's center. </param>
	public virtual bool PreDraw(Projectile projectile, ref Color lightColor)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of a projectile. Use the <c>Main.EntitySpriteDraw</c> method for drawing. This method is called even if PreDraw returns false.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="projectile"> The projectile. </param>
	/// <param name="lightColor"> The color of the light at the projectile's center, after being modified by vanilla and other mods. </param>
	public virtual void PostDraw(Projectile projectile, Color lightColor)
	{
	}

	/// <summary>
	/// When used in conjunction with "projectile.hide = true", allows you to specify that this projectile should be drawn behind certain elements. Add the index to one and only one of the lists. For example, the Nebula Arcanum projectile draws behind NPCs and tiles.
	/// <para/> Called on local and remote clients.
	/// </summary>
	/// <param name="projectile"></param>
	/// <param name="index"></param>
	/// <param name="behindNPCsAndTiles"></param>
	/// <param name="behindNPCs"></param>
	/// <param name="behindProjectiles"></param>
	/// <param name="overPlayers"></param>
	/// <param name="overWiresUI"></param>
	public virtual void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
	}

	/// <summary>
	/// Whether or not a grappling hook that shoots this type of projectile can be used by the given player. Return null to use the default code (whether or not the player is in the middle of firing the grappling hook). Returns null by default.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual bool? CanUseGrapple(int type, Player player)
	{
		return null;
	}

	/// <summary>
	/// This code is called whenever the player uses a grappling hook that shoots this type of projectile. Use it to change what kind of hook is fired (for example, the Dual Hook does this), to kill old hook projectiles, etc.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void UseGrapple(Player player, ref int type)
	{
	}

	/// <summary>
	/// How many of this type of grappling hook the given player can latch onto blocks before the hooks start disappearing. Change the numHooks parameter to determine this; by default it will be 3.
	/// <para/> Called on the local client only.
	/// </summary>
	public virtual void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
	{
	}

	/// <summary>
	/// The speed at which the grapple retreats back to the player after not hitting anything. Defaults to 11, but vanilla hooks go up to 24.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
	{
	}

	/// <summary>
	/// The speed at which the grapple pulls the player after hitting something. Defaults to 11, but the Bat Hook uses 16.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
	{
	}

	/// <summary>
	/// The location that the grappling hook pulls the player to. Defaults to the center of the hook projectile.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY)
	{
	}

	/// <summary>
	/// Whether or not the grappling hook can latch onto the given position in tile coordinates.
	/// <para/> This position may be air or an actuated tile!
	/// <para/> Return true to make it latch, false to prevent it, or null to apply vanilla conditions. Returns null by default.
	/// <para/> Called on local, server, and remote clients.
	/// </summary>
	public virtual bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
	{
		return null;
	}

	/// <inheritdoc cref="ModProjectile.PrepareBombToBlow"/>
	public virtual void PrepareBombToBlow(Projectile projectile)
	{
	}

	/// <inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
	public virtual void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
	{
	}
}
