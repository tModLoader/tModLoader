using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
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

	private int _armorPenetration = 0;
	/// <summary>
	/// The number of defense points that this projectile can ignore on its own. Cannot be set to negative values. Defaults to 0.
	/// On spawn, if this projectile was fired from a weapon, this value has the total armor penetration of the weapon that made the projectile added to itself.
	/// </summary>
	public int ArmorPenetration {
		get => _armorPenetration;
		set => _armorPenetration = Math.Max(0, value);
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
	/// Transfers stat modifiers from the spawn source to the projectile. <br/>
	/// Adds <see cref="CritChance"/> and <see cref="ArmorPenetration"/> bonuses from players (<see cref="EntitySource_Parent"/>), weapons (<see cref="EntitySource_ItemUse"/>)<br/>
	/// If the source is a <see cref="EntitySource_Parent"/> projectile, <c>CritChance</c> and <c>ArmorPenetration</c> from the parent will be added, in order to transfer the original item/player bonus values.<br/><br/>
	/// <br/>
	/// To support minions, sentries and <see cref="ContinuouslyUpdateDamageStats"/>, <see cref="OriginalCritChance"/> and <see cref="OriginalArmorPenetration"/> are also copied from item sources and parent projectiles.
	/// </summary>
	/// <param name="spawnSource"></param>
	public void ApplyStatsFromSource(IEntitySource spawnSource)
	{
		originalDamage = damage;
		OriginalCritChance = CritChance;
		OriginalArmorPenetration = ArmorPenetration;

		 if (spawnSource is EntitySource_Parent { Entity: Player player }) {
			if (spawnSource is IEntitySource_WithStatsFromItem { Item: Item item }) {
				// Apply the weapon and player bonuses to the base stats
				CritChance += player.GetWeaponCrit(item);
				ArmorPenetration += player.GetWeaponArmorPenetration(item);

				// Apply original stats, so that ContinuouslyUpdateDamageStats can correctly scale the base values
				// originalDamage is set to item.damage as a convenience.
				if (item.damage >= 0)
					originalDamage = item.damage;

				OriginalCritChance += item.crit;
				OriginalArmorPenetration += item.ArmorPenetration;
			}
			else {
				// Apply player bonuses to the base stats
				CritChance += (int)(player.GetTotalCritChance(DamageType) + 5E-06f);
				ArmorPenetration += (int)(player.GetTotalArmorPenetration(DamageType) + 5E-06f);
			}
		}
		else if (spawnSource is EntitySource_Parent { Entity: Projectile parentProjectile }) {
			// This doesn't offer enough control, there's no way to determine if the parent originalDamage property should overwrite the child or not.
			// In the case of parent.originalDamage = item.damage, it could be helpful, but the caller of NewProjectile could also just pass originalDamage as the dmg param and get the same effective result.
			// In general, it is the responsibility of the creator of a minion or ContinuouslyUpdateDamageStats projectile to configure the child correctly.
			// originalDamage = parentProjectile.originalDamage;

			// To ensure snapshotted bonuses are passed on from parent to child, we just stack any parent CritChance/ArmorPenetration with the child default values
			// This is a pattern that mods can safely follow for their own stats, matches vanilla non-snapshotting behavior, and is easy to use.
			CritChance += parentProjectile.CritChance;
			ArmorPenetration += parentProjectile.ArmorPenetration;

			// In case this projectile is a minion or continuously updates damage (long running projectiles spawned by minions or sentries perhaps, maybe a laser for eg)
			// We want to pass on the OriginalCrit and OriginalArmorPenetration values from the parent, so that item.crit and item.ArmorPenetration can affect the child.
			OriginalCritChance += parentProjectile.OriginalCritChance;
			OriginalArmorPenetration += parentProjectile.OriginalArmorPenetration;
		}
	}

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
		if (targetPlayer.Hurt(damageSource, damageVariation, direction, pvp: true, quiet: false, Crit: false, -1, dodgeable: IsDamageDodgable(), armorPenetration: ArmorPenetration) > 0.0 && !targetPlayer.dead)
			StatusPlayer(targetPlayer.whoAmI);

		if (trap) {
			targetPlayer.trapDebuffSource = true;
			if (targetPlayer.dead)
				AchievementsHelper.HandleSpecialEvent(targetPlayer, 4);
		}
	}
}
