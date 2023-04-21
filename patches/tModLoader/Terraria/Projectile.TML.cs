using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
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
	/// If set, Projectile.damage will be recalculated based on Projectile.originalDamage, Projectile.DamageType and the owning player, just like minions and sentries.
	/// This has no effect if Projectile.minion or Projectile.sentry is set.
	/// </summary>
	public bool ContinuouslyUpdateDamage { get; set; }

	/* tML:
	this method is used to set the critical strike chance of a projectile based on the environment in which it was fired
	this critical strike chance is then stored on the projectile and checked against for all critical strike calculations
	this, alongside a number of other changes, is part of a massive list of fixes to critical strike chance made by tML

	- thomas
	*/
	private static void HandlePlayerStatModifiers(IEntitySource spawnSource, Projectile projectile)
	{
		// to-do: make this less ugly and more easily extensible to modded sources
		// (requires substantial changes, at minimum, to how entity sources are handled)
		if (spawnSource is EntitySource_ItemUse { Entity: Player player, Item: Item item }) {
			projectile.originalDamage = item.damage;
			projectile.CritChance += player.GetWeaponCrit(item);
			projectile.ArmorPenetration += player.GetWeaponArmorPenetration(item);
		}
		else if (spawnSource is EntitySource_Parent { Entity: Projectile parentProjectile }) {
			projectile.originalDamage = parentProjectile.originalDamage;
			projectile.CritChance += parentProjectile.CritChance;
			projectile.ArmorPenetration += parentProjectile.ArmorPenetration;
		}
	}

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
	/// <returns><see langword="true"/> if this projectiles's <see cref="DamageClass"/> matches <paramref name="damageClass"/>, <see langword="false"/> otherwise</returns>
	/// <seealso cref="CountsAsClass{T}"/>
	public bool CountsAsClass(DamageClass damageClass)
		=> DamageClassLoader.effectInheritanceCache[DamageType.Type, damageClass.Type];

	/// <summary>
	/// Checks if the projectile is a minion, sentry, minion shot, or sentry shot. <br/>
	/// </summary>
	public bool IsMinionOrSentryRelated => minion || ProjectileID.Sets.MinionShot[type] || sentry || ProjectileID.Sets.SentryShot[type];
}
