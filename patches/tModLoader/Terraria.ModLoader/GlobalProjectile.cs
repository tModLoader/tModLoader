using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to modify and use hooks for all projectiles, including vanilla projectiles. Create an instance of an overriding class then call Mod.AddGlobalProjectile to use this.
	/// </summary>
	public class GlobalProjectile
	{
		/// <summary>
		/// The mod to which this GlobalProjectile belongs.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this GlobalProjectile instance.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		internal int index;
		internal int instanceIndex;

		/// <summary>
		/// Allows you to automatically load a GlobalProjectile instead of using Mod.AddGlobalProjectile. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name. Use this method to either force or stop an autoload or to control the internal name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual bool Autoload(ref string name) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Whether to create a new GlobalProjectile instance for every Projectile that exists. 
		/// Useful for storing information on a projectile. Defaults to false. 
		/// Return true if you need to store information (have non-static fields).
		/// </summary>
		public virtual bool InstancePerEntity => false;

		public GlobalProjectile Instance(Projectile projectile) => InstancePerEntity ? projectile.globalProjectiles[instanceIndex] : this;

		/// <summary>
		/// Whether instances of this GlobalProjectile are created through Clone or constructor (by default implementations of NewInstance and Clone()). 
		/// Defaults to false (using default constructor).
		/// </summary>
		public virtual bool CloneNewInstances => false;

		/// <summary>
		/// Returns a clone of this GlobalProjectile. 
		/// By default this will return a memberwise clone; you will want to override this if your GlobalProjectile contains object references. 
		/// Only called if CloneNewInstances && InstancePerEntity
		/// </summary>
		public virtual GlobalProjectile Clone() => (GlobalProjectile)MemberwiseClone();

		/// <summary>
		/// Create a new instance of this GlobalProjectile for a Projectile instance. 
		/// Called at the end of Projectile.SetDefaults.
		/// If CloneNewInstances is true, just calls Clone()
		/// Otherwise calls the default constructor and copies fields
		/// </summary>
		public virtual GlobalProjectile NewInstance(Projectile projectile) {
			if (CloneNewInstances) {
				return Clone();
			}
			GlobalProjectile copy = (GlobalProjectile)Activator.CreateInstance(GetType());
			copy.mod = mod;
			copy.Name = Name;
			copy.index = index;
			copy.instanceIndex = instanceIndex;
			return copy;
		}

		/// <summary>
		/// Allows you to set the properties of any and every projectile that gets created.
		/// </summary>
		/// <param name="projectile"></param>
		public virtual void SetDefaults(Projectile projectile) {
		}

		/// <summary>
		/// Allows you to determine how any projectile behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public virtual bool PreAI(Projectile projectile) {
			return true;
		}

		/// <summary>
		/// Allows you to determine how any projectile behaves. This will only be called if PreAI returns true.
		/// </summary>
		/// <param name="projectile"></param>
		public virtual void AI(Projectile projectile) {
		}

		/// <summary>
		/// Allows you to determine how any projectile behaves. This will be called regardless of what PreAI returns.
		/// </summary>
		/// <param name="projectile"></param>
		public virtual void PostAI(Projectile projectile) {
		}

		/// <summary>
		/// Whether or not the given projectile should update its position based on factors such as its velocity, whether it is in liquid, etc. Return false to make its velocity have no effect on its position. Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public virtual bool ShouldUpdatePosition(Projectile projectile) {
			return true;
		}

		/// <summary>
		/// Allows you to determine how a projectile interacts with tiles. Width and height determine the projectile's hitbox for tile collision, and default to -1. Leave them as -1 to use the projectile's real size. Fallthrough determines whether the projectile can fall through platforms, etc., and defaults to true.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="fallThrough"></param>
		/// <returns></returns>
		public virtual bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough) {
			return true;
		}

		/// <summary>
		/// Allows you to determine what happens when a projectile collides with a tile. OldVelocity is the velocity before tile collision. The velocity that takes tile collision into account can be found with projectile.velocity. Return true to allow the vanilla tile collision code to take place (which normally kills the projectile). Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="oldVelocity"></param>
		/// <returns></returns>
		public virtual bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) {
			return true;
		}

		/// <summary>
		/// Allows you to determine whether the vanilla code for Kill and the Kill hook will be called. Return false to stop them from being called. Returns true by default. Note that this does not stop the projectile from dying.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="timeLeft"></param>
		/// <returns></returns>
		public virtual bool PreKill(Projectile projectile, int timeLeft) {
			return true;
		}

		/// <summary>
		/// Allows you to control what happens when a projectile is killed (for example, creating dust or making sounds).
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="timeLeft"></param>
		public virtual void Kill(Projectile projectile, int timeLeft) {
		}

		/// <summary>
		/// Return true or false to specify if the projectile can cut tiles, like vines. Return null for vanilla decision.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public virtual bool? CanCutTiles(Projectile projectile) {
			return null;
		}

		/// <summary>
		/// Code ran when the projectile cuts tiles. Only runs if CanCutTiles() returns true. Useful when programming lasers and such.
		/// </summary>
		/// <param name="projectile"></param>
		public virtual void CutTiles(Projectile projectile) {
		}

		/// <summary>
		/// Whether or not the given projectile is capable of killing tiles (such as grass) and damaging NPCs/players. Return false to prevent it from doing any sort of damage. Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public virtual bool CanDamage(Projectile projectile) {
			return true;
		}

		/// <summary>
		/// Whether or not a minion can damage NPCs by touching them. Returns false by default. Note that this will only be used if the projectile is considered a pet.
		/// </summary>
		/// <param name="projectile"></param>
		/// <returns></returns>
		public virtual bool MinionContactDamage(Projectile projectile) {
			return false;
		}

		/// <summary>
		/// Allows you to change the hitbox used by a projectile for damaging players and NPCs.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="hitbox"></param>
		public virtual void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
		}

		/// <summary>
		/// Allows you to determine whether a projectile can hit the given NPC. Return true to allow hitting the target, return false to block the projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool? CanHitNPC(Projectile projectile, NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that a projectile does to an NPC.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		/// <param name="hitDirection"></param>
		public virtual void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
		}

		/// <summary>
		/// Allows you to create special effects when a projectile hits an NPC (for example, inflicting debuffs).
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="knockback"></param>
		/// <param name="crit"></param>
		public virtual void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether a projectile can hit the given opponent player. Return false to block the projectile from hitting the target. Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool CanHitPvp(Projectile projectile, Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that a projectile does to an opponent player.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when a projectile hits an opponent player.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitPvp(Projectile projectile, Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether a hostile projectile can hit the given player. Return false to block the projectile from hitting the target. Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual bool CanHitPlayer(Projectile projectile, Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that a hostile projectile does to a player.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when a hostile projectile hits a player.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="crit"></param>
		public virtual void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to use custom collision detection between a projectile and a player or NPC that the projectile can damage. Useful for things like diagonal lasers, projectiles that leave a trail behind them, etc.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="projHitbox"></param>
		/// <param name="targetHitbox"></param>
		/// <returns></returns>
		public virtual bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) {
			return null;
		}

		/// <summary>
		/// Allows you to determine the color and transparency in which a projectile is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="lightColor"></param>
		/// <returns></returns>
		public virtual Color? GetAlpha(Projectile projectile, Color lightColor) {
			return null;
		}

		/// <summary>
		/// Allows you to draw things behind a projectile. Returns false to stop the game from drawing extras textures related to the projectile (for example, the chains for grappling hooks), useful if you're manually drawing the extras. Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="spriteBatch"></param>
		/// <returns></returns>
		public virtual bool PreDrawExtras(Projectile projectile, SpriteBatch spriteBatch) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things behind a projectile, or to modify the way the projectile is drawn. Return false to stop the game from drawing the projectile (useful if you're manually drawing the projectile). Returns true by default.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="spriteBatch"></param>
		/// <param name="lightColor"></param>
		/// <returns></returns>
		public virtual bool PreDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of a projectile. This method is called even if PreDraw returns false.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="spriteBatch"></param>
		/// <param name="lightColor"></param>
		public virtual void PostDraw(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
		}

		/// <summary>
		/// When used in conjunction with "projectile.hide = true", allows you to specify that this projectile should be drawn behind certain elements. Add the index to one and only one of the lists. For example, the Nebula Arcanum projectile draws behind NPCs and tiles.
		/// </summary>
		/// <param name="projectile"></param>
		/// <param name="index"></param>
		/// <param name="drawCacheProjsBehindNPCsAndTiles"></param>
		/// <param name="drawCacheProjsBehindNPCs"></param>
		/// <param name="drawCacheProjsBehindProjectiles"></param>
		/// <param name="drawCacheProjsOverWiresUI"></param>
		public virtual void DrawBehind(Projectile projectile, int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI) {
		}

		/// <summary>
		/// Whether or not a grappling hook that shoots this type of projectile can be used by the given player. Return null to use the default code (whether or not the player is in the middle of firing the grappling hook). Returns null by default.
		/// </summary>
		public virtual bool? CanUseGrapple(int type, Player player) {
			return null;
		}

		/// <summary>
		/// Whether or not a grappling hook can only have one hook per player in the world at a time. Return null to use the vanilla code. Returns null by default.
		/// </summary>
		public virtual bool? SingleGrappleHook(int type, Player player) {
			return null;
		}

		/// <summary>
		/// This code is called whenever the player uses a grappling hook that shoots this type of projectile. Use it to change what kind of hook is fired (for example, the Dual Hook does this), to kill old hook projectiles, etc.
		/// </summary>
		public virtual void UseGrapple(Player player, ref int type) {
		}

		/// <summary>
		/// How many of this type of grappling hook the given player can latch onto blocks before the hooks start disappearing. Change the numHooks parameter to determine this; by default it will be 3.
		/// </summary>
		public virtual void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks) {
		}

		/// <summary>
		/// The speed at which the grapple retreats back to the player after not hitting anything. Defaults to 11, but vanilla hooks go up to 24.
		/// </summary>
		public virtual void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed) {
		}

		/// <summary>
		/// The speed at which the grapple pulls the player after hitting something. Defaults to 11, but the Bat Hook uses 16.
		/// </summary>
		public virtual void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) {
		}
	}
}
