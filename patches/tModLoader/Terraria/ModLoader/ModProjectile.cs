using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to place all your properties and hooks for each projectile. Create instances of ModProjectile (preferably overriding this class) to pass as parameters to Mod.AddProjectile.
	/// </summary>
	public abstract class ModProjectile : ModTexturedType
	{
		/// <summary> The projectile object that this ModProjectile controls. </summary>
		public Projectile Projectile { get; internal set; }

		/// <summary> The translations for the display name of this projectile. </summary>
		public ModTranslation DisplayName { get; internal set; }

		/// <summary> Determines which type of vanilla projectile this ModProjectile will copy the behavior (AI) of. Leave as 0 to not copy any behavior. Defaults to 0. </summary>
		public int AIType { get; set; }

		/// <summary> Determines which of the player's cooldown counters to use (-1, 0, or 1) when this projectile damages it. Defaults to -1. </summary>
		public int CooldownSlot { get; set; } = -1;

		/// <summary> How far to the right of its position this projectile should be drawn. Defaults to 0. </summary>
		public int DrawOffsetX { get; set; }

		/// <summary> The vertical origin offset from the projectile's center when it is drawn. The origin is essentially the point of rotation. This field will also determine the vertical drawing offset of the projectile. </summary>
		public int DrawOriginOffsetY { get; set; }

		/// <summary> The horizontal origin offset from the projectile's center when it is drawn. </summary>
		public float DrawOriginOffsetX { get; set; }

		/// <summary> If this projectile is held by the player, determines whether it is drawn in front of or behind the player's arms. Defaults to false. </summary>
		public bool DrawHeldProjInFrontOfHeldItemAndArms { get; set; }

		/// <summary> The file name of this projectile's glow texture file in the mod loader's file space. If it does not exist it is ignored. </summary>
		public virtual string GlowTexture => Texture + "_Glow"; //TODO: this is wasteful. We should consider AutoStaticDefaults or something... requesting assets regularly is bad perf

		/// <summary>  Shorthand for projectile.type; </summary>
		public int Type => Projectile.type;

		public ModProjectile() {
			Projectile = new Projectile { ModProjectile = this };
		}

		protected sealed override void Register() {
			ModTypeLookup<ModProjectile>.Register(this);

			Projectile.type = ProjectileLoader.ReserveProjectileID();
			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"ProjectileName.{Name}");

			ProjectileLoader.projectiles.Add(this);
		}

		public sealed override void SetupContent() {
			ProjectileLoader.SetDefaults(Projectile, false);
			AutoStaticDefaults();
			SetStaticDefaults();

			ProjectileID.Search.Add(FullName, Type);
		}

		/// <summary>
		/// Whether instances of this ModProjectile are created through a memberwise clone or its constructor. Defaults to false.
		/// </summary>
		public virtual bool CloneNewInstances => false;

		/// <summary>
		/// Returns a clone of this ModProjectile. 
		/// Allows you to decide which fields of your ModProjectile class are copied over when a new Projectile is created. 
		/// By default this will return a memberwise clone; you will want to override this if your ModProjectile contains object references. 
		/// Only called if CloneNewInstances is set to true.
		/// </summary>
		public virtual ModProjectile Clone() => (ModProjectile)MemberwiseClone();

		/// <summary>
		/// Create a new instance of this ModProjectile for a Projectile instance. 
		/// Called at the end of Projectile.SetDefaults.
		/// If CloneNewInstances is true, just calls Clone()
		/// Otherwise calls the default constructor and copies fields
		/// </summary>
		public virtual ModProjectile NewInstance(Projectile projectileClone) {
			if (CloneNewInstances) {
				ModProjectile clone = Clone();
				clone.Projectile = projectileClone;
				return clone;
			}

			ModProjectile copy = (ModProjectile)Activator.CreateInstance(GetType());
			copy.Projectile = projectileClone;
			copy.Mod = Mod;
			copy.AIType = AIType;
			copy.CooldownSlot = CooldownSlot;
			copy.DrawOffsetX = DrawOffsetX;
			copy.DrawOriginOffsetY = DrawOriginOffsetY;
			copy.DrawOriginOffsetX = DrawOriginOffsetX;
			copy.DrawHeldProjInFrontOfHeldItemAndArms = DrawHeldProjInFrontOfHeldItemAndArms;
			return copy;
		}

		/// <summary>
		/// Allows you to set all your projectile's properties, such as width, damage, aiStyle, penetrate, etc.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to set all your projectile's static properties, such as names/translations and the arrays in ProjectileID.Sets.
		/// </summary>
		public virtual void SetStaticDefaults() {
		}

		/// <summary>
		/// Automatically sets certain static defaults. Override this if you do not want the properties to be set for you.
		/// </summary>
		public virtual void AutoStaticDefaults() {
			TextureAssets.Projectile[Projectile.type] = ModContent.Request<Texture2D>(Texture);
			Main.projFrames[Projectile.type] = 1;
			if (Projectile.hostile) {
				Main.projHostile[Projectile.type] = true;
			}
			if (Projectile.aiStyle == 7) {
				Main.projHook[Projectile.type] = true;
			}
			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(Name, "([A-Z])", " $1").Trim());
		}

		/// <summary>
		/// Allows you to determine how this projectile behaves. Return false to stop the vanilla AI and the AI hook from being run. Returns true by default.
		/// </summary>
		/// <returns>Whether or not to stop other AI.</returns>
		public virtual bool PreAI() {
			return true;
		}

		/// <summary>
		/// Allows you to determine how this projectile behaves. This will only be called if PreAI returns true.
		/// </summary>
		public virtual void AI() {
		}

		/// <summary>
		/// Allows you to determine how this projectile behaves. This will be called regardless of what PreAI returns.
		/// </summary>
		public virtual void PostAI() {
		}

		/// <summary>
		/// If you are storing AI information outside of the projectile.ai array, use this to send that AI information between clients and servers.
		/// </summary>
		public virtual void SendExtraAI(BinaryWriter writer) {
		}

		/// <summary>
		/// Use this to receive information that was sent in SendExtraAI.
		/// </summary>
		public virtual void ReceiveExtraAI(BinaryReader reader) {
		}

		/// <summary>
		/// Whether or not this projectile should update its position based on factors such as its velocity, whether it is in liquid, etc. Return false to make its velocity have no effect on its position. Returns true by default.
		/// </summary>
		public virtual bool ShouldUpdatePosition() {
			return true;
		}

		/// <summary>
		/// Allows you to determine how this projectile interacts with tiles. Width and height determine the projectile's hitbox for tile collision, and default to -1. Leave them as -1 to use the projectile's real size. Fallthrough determines whether the projectile can fall through platforms, etc., and defaults to true.
		/// </summary>
		/// <param name="width">Width of the hitbox.</param>
		/// <param name="height">Height of the hitbox.</param>
		/// <param name="fallThrough">If the projectile can fall through platforms etc.</param>
		public virtual bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
			return true;
		}

		/// <summary>
		/// Allows you to determine what happens when this projectile collides with a tile. OldVelocity is the velocity before tile collision. The velocity that takes tile collision into account can be found with projectile.velocity. Return true to allow the vanilla tile collision code to take place (which normally kills the projectile). Returns true by default.
		/// </summary>
		/// <param name="oldVelocity">The velocity of the projectile upon collision.</param>
		public virtual bool OnTileCollide(Vector2 oldVelocity) {
			return true;
		}

		/// <summary>
		/// Return true or false to specify if the projectile can cut tiles, like vines. Return null for vanilla decision.
		/// </summary>
		public virtual bool? CanCutTiles() {
			return null;
		}

		/// <summary>
		/// Code ran when the projectile cuts tiles. Only runs if CanCutTiles() returns true. Useful when programming lasers and such.
		/// </summary>
		public virtual void CutTiles() {
		}

		/// <summary>
		/// Allows you to determine whether the vanilla code for Kill and the Kill hook will be called. Return false to stop them from being called. Returns true by default. Note that this does not stop the projectile from dying.
		/// </summary>
		public virtual bool PreKill(int timeLeft) {
			return true;
		}

		/// <summary>
		/// Allows you to control what happens when this projectile is killed (for example, creating dust or making sounds). Also useful for creating retrievable ammo. Called on all clients and the server in multiplayer, so be sure to use `if (projectile.owner == Main.myPlayer)` if you are spawning retrievable ammo. (As seen in ExampleJavelinProjectile)
		/// </summary>
		public virtual void Kill(int timeLeft) {
		}

		/// <summary>
		/// Whether or not this projectile is capable of killing tiles (such as grass) and damaging NPCs/players.
		/// Return false to prevent it from doing any sort of damage.
		/// Return true if you want the projectile to do damage regardless of the default blacklist.
		/// Return null to let the projectile follow vanilla can-damage-anything rules. This is what happens by default.
		/// </summary>
		public virtual bool? CanDamage() {
			return null;
		}

		/// <summary>
		/// Whether or not this minion can damage NPCs by touching them. Returns false by default. Note that this will only be used if this projectile is considered a pet.
		/// </summary>
		public virtual bool MinionContactDamage() {
			return false;
		}

		/// <summary>
		/// Allows you to change the hitbox used by this projectile for damaging players and NPCs.
		/// </summary>
		/// <param name="hitbox"></param>
		public virtual void ModifyDamageHitbox(ref Rectangle hitbox) {
		}

		/// <summary>
		/// Allows you to determine whether this projectile can hit the given NPC. Return true to allow hitting the target, return false to block this projectile from hitting the target, and return null to use the vanilla code for whether the target can be hit. Returns null by default.
		/// </summary>
		/// <param name="target">The target.</param>
		public virtual bool? CanHitNPC(NPC target) {
			return null;
		}

		/// <summary>
		/// Allows you to modify the damage, knockback, etc., that this projectile does to an NPC. This method is only called for the owner of the projectile, meaning that in multi-player, projectiles owned by a player call this method on that client, and projectiles owned by the server such as enemy projectiles call this method on the server.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="damage">The modifiable damage.</param>
		/// <param name="knockback">The modifiable knockback.</param>
		/// <param name="crit">The modifiable crit.</param>
		/// <param name="hitDirection">The modifiable hit direction.</param>
		public virtual void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
		}

		/// <summary>
		/// Allows you to create special effects when this projectile hits an NPC (for example, inflicting debuffs). This method is only called for the owner of the projectile, meaning that in multi-player, projectiles owned by a player call this method on that client, and projectiles owned by the server such as enemy projectiles call this method on the server.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="damage">The damage.</param>
		/// <param name="knockback">The knockback.</param>
		/// <param name="crit">The critical hit.</param>
		public virtual void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether this projectile can hit the given opponent player. Return false to block this projectile from hitting the target. Returns true by default.
		/// </summary>
		/// <param name="target">The target</param>
		public virtual bool CanHitPvp(Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that this projectile does to an opponent player.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="damage">The modifiable damage.</param>
		/// <param name="crit">The modifiable crit.</param>
		public virtual void ModifyHitPvp(Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this projectile hits an opponent player.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="damage">The damage.</param>
		/// <param name="crit">The critical hit.</param>
		public virtual void OnHitPvp(Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to determine whether this hostile projectile can hit the given player. Return false to block this projectile from hitting the target. Returns true by default.
		/// </summary>
		/// <param name="target">The target.</param>
		public virtual bool CanHitPlayer(Player target) {
			return true;
		}

		/// <summary>
		/// Allows you to modify the damage, etc., that this hostile projectile does to a player.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="damage">The modifiable damage.</param>
		/// <param name="crit">The modifiable crit.</param>
		public virtual void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
		}

		/// <summary>
		/// Allows you to create special effects when this hostile projectile hits a player.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="damage">The damage.</param>
		/// <param name="crit">The critical hit.</param>
		public virtual void OnHitPlayer(Player target, int damage, bool crit) {
		}

		/// <summary>
		/// Allows you to use custom collision detection between this projectile and a player or NPC that this projectile can damage. Useful for things like diagonal lasers, projectiles that leave a trail behind them, etc.
		/// </summary>
		/// <param name="projHitbox">The hitbox of the projectile.</param>
		/// <param name="targetHitbox">The hitbox of the target.</param>
		/// <returns>Whether they collide or not.</returns>
		public virtual bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return null;
		}

		/// <summary>
		/// If this projectile is a bobber, allows you to modify the origin of the fisihing line that's connecting to the fishing pole, as well as the fishing line's color.
		/// </summary>
		/// <param name="lineOriginOffset"> The offset of the fishing line's origin from the player's center. </param>
		/// <param name="lineColor"> The fishing line's color, before being overridden by string color accessories. </param>
		public virtual void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor) {
		}

		/// <summary>
		/// Allows you to determine the color and transparency in which this projectile is drawn. Return null to use the default color (normally light and buff color). Returns null by default.
		/// </summary>
		public virtual Color? GetAlpha(Color lightColor) {
			return null;
		}

		/// <summary>
		/// Allows you to draw things behind this projectile. Use the Main.EntitySpriteDraw method for drawing. Returns false to stop the game from drawing extras textures related to the projectile (for example, the chains for grappling hooks), useful if you're manually drawing the extras. Returns true by default.
		/// </summary>
		public virtual bool PreDrawExtras() {
			return true;
		}

		/// <summary>
		/// Allows you to draw things behind this projectile, or to modify the way it is drawn. Use the Main.EntitySpriteDraw method for drawing. Return false to stop the vanilla projectile drawing code (useful if you're manually drawing the projectile). Returns true by default.
		/// </summary>
		/// <param name="lightColor"> The color of the light at the projectile's center. </param>
		public virtual bool PreDraw(ref Color lightColor) {
			return true;
		}

		/// <summary>
		/// Allows you to draw things in front of this projectile. Use the Main.EntitySpriteDraw method for drawing. This method is called even if PreDraw returns false.
		/// </summary>
		/// <param name="lightColor"> The color of the light at the projectile's center, after being modified by vanilla and other mods. </param>
		public virtual void PostDraw(Color lightColor) {
		}

		/// <summary>
		/// This code is called whenever the player uses a grappling hook that shoots this type of projectile. Use it to change what kind of hook is fired (for example, the Dual Hook does this), to kill old hook projectiles, etc.
		/// </summary>
		public virtual bool? CanUseGrapple(Player player) {
			return null;
		}

		/// <summary>
		/// Whether or not a grappling hook can only have one hook per player in the world at a time. Return null to use the vanilla code. Returns null by default.
		/// </summary>
		public virtual bool? SingleGrappleHook(Player player) {
			return null;
		}

		/// <summary>
		/// This code is called whenever the player uses a grappling hook that shoots this type of projectile. Use it to change what kind of hook is fired (for example, the Dual Hook does this), to kill old hook projectiles, etc.
		/// </summary>
		public virtual void UseGrapple(Player player, ref int type) {
		}

		/// <summary>
		/// How far away this grappling hook can travel away from its player before it retracts.
		/// </summary>
		public virtual float GrappleRange() {
			return 300f;
		}

		/// <summary>
		/// How many of this type of grappling hook the given player can latch onto blocks before the hooks start disappearing. Change the numHooks parameter to determine this; by default it will be 3.
		/// </summary>
		public virtual void NumGrappleHooks(Player player, ref int numHooks) {
		}

		/// <summary>
		/// The speed at which the grapple retreats back to the player after not hitting anything. Defaults to 11, but vanilla hooks go up to 24.
		/// </summary>
		public virtual void GrappleRetreatSpeed(Player player, ref float speed) {
		}

		/// <summary>
		/// The speed at which the grapple pulls the player after hitting something. Defaults to 11, but the Bat Hook uses 16.
		/// </summary>
		public virtual void GrapplePullSpeed(Player player, ref float speed) {
		}

		/// <summary>
		/// The location that the grappling hook pulls the player to. Defaults to the center of the hook projectile.
		/// </summary>
		public virtual void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) {
		}

		/// <summary>
		/// When used in conjunction with "projectile.hide = true", allows you to specify that this projectile should be drawn behind certain elements. Add the index to one and only one of the lists. For example, the Nebula Arcanum projectile draws behind NPCs and tiles.
		/// </summary>
		public virtual void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
		}
	}
}
