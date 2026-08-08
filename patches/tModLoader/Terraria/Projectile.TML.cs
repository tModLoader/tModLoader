using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Terraria;

public partial class Projectile : IEntityWithGlobals<GlobalProjectile>
{
	/// <summary>
	/// The ModProjectile instance that controls the behavior of this projectile. This property is null if this is not a modded projectile.
	/// </summary>
	public ModProjectile ModProjectile { get; internal set; }

#region Globals
	int IEntityWithGlobals<GlobalProjectile>.Type => type;
	internal GlobalProjectile[] _globals;
	public RefReadOnlyArray<GlobalProjectile> EntityGlobals => _globals;
	public EntityGlobalsEnumerator<GlobalProjectile> Globals => new(this);

	/// <summary> Gets the instance of the specified GlobalProjectile type. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="IndexOutOfRangeException"/>
	public T GetGlobalProjectile<T>() where T : GlobalProjectile
		=> GlobalProjectile.GetGlobal<T>(type, EntityGlobals);

	/// <summary> Gets the local instance of the type of the specified GlobalProjectile instance. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="NullReferenceException"/>
	public T GetGlobalProjectile<T>(T baseInstance) where T : GlobalProjectile
		=> GlobalProjectile.GetGlobal(type, EntityGlobals, baseInstance);

	/// <summary> Gets the instance of the specified GlobalProjectile type. </summary>
	public bool TryGetGlobalProjectile<T>(out T result) where T : GlobalProjectile
		=> GlobalProjectile.TryGetGlobal(type, EntityGlobals, out result);

	/// <summary> Safely attempts to get the local instance of the type of the specified GlobalProjectile instance. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public bool TryGetGlobalProjectile<T>(T baseInstance, out T result) where T : GlobalProjectile
		=> GlobalProjectile.TryGetGlobal(type, EntityGlobals, baseInstance, out result);
#endregion

	/// <summary>
	/// <inheritdoc cref="Projectile.NewProjectile(IEntitySource, float, float, float, float, int, int, float, int, float, float, float)"/>
	/// <br/><br/>This particular overload uses a Vector2 instead of X and Y to determine the actual spawn position and a Vector2 to dictate the initial velocity. The return value is the actual Projectile instance rather than the index of the spawned Projectile within the <see cref="Main.projectile"/> array.
	/// <br/> A short-hand for <code> Main.projectile[Projectile.NewProjectile(...)] </code>
	/// </summary>
	public static Projectile NewProjectileDirect(IEntitySource spawnSource, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f)
		=> Main.projectile[NewProjectile(spawnSource, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, owner, ai0, ai1, ai2)];

	private DamageClass _damageClass = DamageClass.Default;
	/// <summary>
	/// The damage type assigned to this projectile, represented as a DamageClass.
	/// Leave blank or use DamageClass.Default to prevent damage type scaling of any kind for this projectile.
	/// Use DamageClass.Generic/Melee/Ranged/Magic/Summon/Throwing for vanilla damage types.
	/// Refer to ExampleMod for more information on how to create and use your own damage types.
	/// </summary>
	public DamageClass DamageType {
		get => _damageClass;
		set => _damageClass = value ?? throw new ArgumentException($"{nameof(Projectile)}.{nameof(DamageType)} cannot be null.");
	}

	/// <summary>
	/// The number of defense points that this projectile can ignore on its own. Cannot be set to negative values. Defaults to 0.
	/// On spawn, if this projectile was fired from a weapon, this value has the total armor penetration of the weapon that made the projectile added to itself.
	/// </summary>
	public int ArmorPenetration {
		get => armorPenetration;
		set => armorPenetration = Math.Max(0, value);
	}

	private int _crit = 0;
	/// <summary>
	/// The critical strike chance modifier of this projectile. Cannot be set to negative values. Defaults to 0.
	/// On spawn, if this projectile was fired from a weapon, this value has the total critical strike chance of the weapon that made the projectile added to itself.
	/// </summary>
	public int CritChance {
		get => _crit;
		set => _crit = Math.Max(0, value);
	}

	/// <summary>
	/// The crit chance of this projectile, without any player bonuses, similar to <see cref="originalDamage"/>
	/// <br/><br/> Used by <see cref="ContinuouslyUpdateDamageStats"/> to recalculate <see cref="CritChance"/> in combination with <see cref="Player.GetTotalCritChance(DamageClass)"/>
	/// </summary>
	public int OriginalCritChance { get; set; }

	/// <summary>
	/// The armor penetration of this projectile, without any player bonuses, similar to <see cref="originalDamage"/>
	/// <br/><br/> Used by <see cref="ContinuouslyUpdateDamageStats"/> to recalculate <see cref="ArmorPenetration"/> in combination with <see cref="Player.GetTotalArmorPenetration(DamageClass)"/>
	/// </summary>
	public int OriginalArmorPenetration { get; set; }

	/// <summary>
	/// If set <see cref="damage"/> will be recalculated based on <see cref="originalDamage"/>, <see cref="DamageType"/> and the <see cref="owner"/> player, just like minions and sentries. <br/>
	/// Similarly for <see cref="CritChance"/> and <see cref="ArmorPenetration"/>.
	/// <br/><br/> No need to set this if <see cref="minion"/> or <see cref="sentry"/> is set.
	/// </summary>
	public bool ContinuouslyUpdateDamageStats { get; set; }

	[Obsolete("Use ContinuouslyUpdateDamageStats", error: true)]
	public bool ContinuouslyUpdateDamage { get => ContinuouslyUpdateDamageStats; set => ContinuouslyUpdateDamageStats = value; }

	/// <summary>
	/// Attempts to get the owner player of this projectile. Returns null for projectiles spawned by TownNPC (<see cref="npcProj"/>) and trap projectiles (<see cref="trap"/>). Returns <c>Main.player[owner]</c> otherwise.
	/// <para/> Note that this logic assumes that projectiles have the correct fields set, which might not always be true. Also note that in single player enemy projectiles are also "owned" by the player, so this alone isn't sufficient to know which projectiles were spawned by the player. Additional <see cref="friendly"/> checks would be needed for that.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public bool TryGetOwner([NotNullWhen(true)] out Player? player)
	{
		player = null;
		if (npcProj || trap)
			return false;

		player = Main.player[owner];
		return player.active;
	}

	/// <summary>
	/// Will drop loot the same way as when <see cref="ProjectileID.Geode"/> is cracked open.
	/// </summary>
	/// <param name="entity">The entity the loot originates from</param>
	public static void DropGeodeLoot(Entity entity)
	{
		var dict = ItemID.Sets.GeodeDrops;
		var list = dict.Keys.ToList();

		int attempts = 0;
		while (attempts < 2 && list.Count > 0) {
			attempts++;

			int item = Main.rand.Next(list);
			list.Remove(item);
			int stack = Main.rand.Next(dict[item].minStack, dict[item].maxStack);
			int num = Item.NewItem(new EntitySource_Loot(entity), entity.position, entity.Size, item, stack);
			Main.item[num].noGrabDelay = 0;
			if (Main.netMode == 1)
				NetMessage.SendData(21, -1, -1, null, num, 1f);
		}
	}

	/// <inheritdoc cref="CountsAsClass(DamageClass)"/>
	public bool CountsAsClass<T>() where T : DamageClass
		=> CountsAsClass(ModContent.GetInstance<T>());

	/// <summary>
	/// This is used to check if the projectile is considered to be a member of a specified <see cref="DamageClass"/>.
	/// </summary>
	/// <param name="damageClass">The DamageClass to compare with the one assigned to this projectile.</param>
	/// <returns><see langword="true"/> if this projectile's <see cref="DamageClass"/> matches <paramref name="damageClass"/>, <see langword="false"/> otherwise</returns>
	/// <seealso cref="CountsAsClass{T}"/>
	public bool CountsAsClass(DamageClass damageClass)
		=> DamageClassLoader.effectInheritanceCache[DamageType.Type, damageClass.Type];

	/// <summary>
	/// Checks if the projectile is a minion, sentry, minion shot, or sentry shot. <br/>
	/// </summary>
	public bool IsMinionOrSentryRelated => minion || ProjectileID.Sets.MinionShot[type] || sentry || ProjectileID.Sets.SentryShot[type];

	// Simplified version of Projectile.BombsHurtPlayers
	/// <summary>
	/// Hurts the local player if the player intersects the specified hitbox.
	/// </summary>
	/// <param name="hitbox">Typically the <see cref="Entity.Hitbox"/>, but any other Rectangle can be passed.</param>
	public void HurtPlayer(Rectangle hitbox)
	{
		Player targetPlayer = Main.LocalPlayer;
		// Check that the player should receive damage in the first place. If not, return.
		if (!targetPlayer.active || targetPlayer.dead || targetPlayer.immune) {
			return;
		}

		// Check that the hitbox radius intersects the player's hitbox. If not, return.
		if (!hitbox.Intersects(targetPlayer.Hitbox)) {
			return;
		}

		// Set the direction of the projectile so the knockback is always in the correct direction.
		direction = (targetPlayer.Center.X > Center.X).ToDirectionInt();

		int damageVariation = Main.DamageVar(damage, 0f - targetPlayer.luck); // Get the damage variation (affected by luck).
		PlayerDeathReason damageSource = PlayerDeathReason.ByProjectile(owner, whoAmI); // Get the death message.

		// Apply damage to the player.
		if (targetPlayer.Hurt(damageSource, damageVariation, direction, pvp: true, quiet: false, Crit: false, -1, dodgeable: IsDamageDodgeable(), armorPenetration: ArmorPenetration) > 0.0 && !targetPlayer.dead)
			StatusPlayer(targetPlayer);

		if (trap) {
			targetPlayer.trapDebuffSource = true;
			if (targetPlayer.dead)
				AchievementsHelper.HandleSpecialEvent(targetPlayer, 4);
		}
	}

	/// <summary>
	/// Calculates the default drawing parameters for this projectile. This can be used to easily implement custom drawing without reimplementing the vanilla drawing logic, usually in <see cref="ModProjectile.PreDraw(Player, ref Color)"/> or <see cref="ModProjectile.PostDraw(Player, Color)"/>.
	/// <para/> Note that this is only valid for modded projectiles and does not replicate any custom logic that would adjust the drawing parameters for vanilla projectiles.
	/// <para/> This also does not apply to projectiles drawn with specialized logic, such as spears and chain-drawn projectiles, or projectiles with hardcoded type-specific draw parameters, like golf balls.
	/// <para/> The returned <see cref="DrawData"/> can be adjusted and then drawn with <see cref="Main.EntitySpriteDraw(DrawData)"/>:
	/// <code>
	/// var drawData = Projectile.GetDefaultDrawData(player, lightColor);
	/// var adjustedDrawData = drawData with { color = Color.Blue }; // Adjust any parameter here
	/// Main.EntitySpriteDraw(adjustedDrawData);
	/// </code>
	/// </summary>
	/// <param name="player">Owner of the projectile. For non-player-owned projectiles, pass <c>Main.player[Projectile.owner]</c></param>
	/// <param name="lightColor">The lighting color for this projectile. Use the Color value passed into <see cref="ModProjectile.PreDraw(Player, ref Color)"/> or <see cref="ModProjectile.PostDraw(Player, Color)"/>. </param>
	/// <returns>Vanilla's default draw parameters for this projectile</returns>
	public DrawData GetDefaultDrawData(Player player, Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[type].Value;

		// Mirrors the offset setup at the top of Main.DrawProj_DrawNormalProjs
		// Vanilla starts at zero and applies specific overrides depending on the type, then calls DrawOffset
		// The type-specific overrides are intentionally skipped here.
		int drawOffsetX = 0;
		int originOffsetY = 0;

		float originX = (texture.Width - width) * 0.5f + width * 0.5f; // Vanilla computes: (texWidth - width) * 0.5f + width * 0.5f

		// Same hook vanilla calls, so ModProjectile's DrawOffsetX/DrawOriginOffsetY/DrawOriginOffsetX applies
		ProjectileLoader.DrawOffset(this, ref drawOffsetX, ref originOffsetY, ref originX);

		SpriteEffects effects = spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		// Vanilla splits these into two separate paths in DrawProj_DrawNormalProjs:
		// Animated projectiles draw a single frame while everything else draws the whole texture
		Rectangle sourceRectangle;

		if (Main.projFrames[type] > 1) {
			int frameHeight = texture.Height / Main.projFrames[type];
			sourceRectangle = new Rectangle(0, frameHeight * frame, texture.Width, frameHeight - 1);
		}
		else {
			sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);

			if (ownerHitCheck && player.gravDir == -1f) {
				if (player.direction == 1)
					effects = SpriteEffects.FlipHorizontally;
				else if (player.direction == -1)
					effects = SpriteEffects.None;
			}
		}

		return new DrawData(
			texture,
			new Vector2(
				position.X - Main.screenPosition.X + originX + drawOffsetX,
				position.Y - Main.screenPosition.Y + height / 2 + gfxOffY
			),
			sourceRectangle,
			GetAlpha(lightColor),
			rotation,
			new Vector2(originX, height / 2 + originOffsetY),
			scale,
			effects
		);
	}
}
