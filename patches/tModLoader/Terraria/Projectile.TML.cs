using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Projectile : IEntityWithGlobals<GlobalProjectile>
	{
		public ModProjectile ModProjectile { get; internal set; }

		internal Instanced<GlobalProjectile>[] globalProjectiles = Array.Empty<Instanced<GlobalProjectile>>();

		public RefReadOnlyArray<Instanced<GlobalProjectile>> Globals => new RefReadOnlyArray<Instanced<GlobalProjectile>>(globalProjectiles);

		public static Projectile NewProjectileDirect(IEntitySource spawnSource, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = 255, float ai0 = 0f, float ai1 = 0f)
			=> Main.projectile[NewProjectile(spawnSource, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, owner, ai0, ai1)];

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
			set {
				if (value < 0)
					throw new ArgumentException($"{nameof(Projectile)}.{nameof(ArmorPenetration)} must be >= 0.");
				else
					_armorPenetration = value;
			}
		}

		private int _crit = 0;
		/// <summary>
		/// The critical strike chance modifier of this projectile. Cannot be set to negative values. Defaults to 0.
		/// On spawn, if this projectile was fired from a weapon, this value has the total critical strike chance of the weapon that made the projectile added to itself.
		/// </summary>
		public int CritChance {
			get => _crit;
			set {
				if (value < 0)
					throw new ArgumentException($"{nameof(Projectile)}.{nameof(CritChance)} must be >= 0.");
				else
					_crit = value;
			}
		}

		/* tML:
		this method is used to set the critical strike chance of a projectile based on the environment in which it was fired
		this critical strike chance is then stored on the projectile and checked against for all critical strike calculations
		this, alongside a number of other changes, is part of a massive list of fixes to critical strike chance made by tML

		- thomas
		*/
		private static void HandlePlayerStatModifiers(IEntitySource spawnSource, Projectile projectile) {
			if (spawnSource is null || (!(spawnSource is EntitySource_ItemUse) && !(spawnSource is EntitySource_ItemUse_WithAmmo)))
				return;

			EntitySource_ItemUse actualSpawnSource = spawnSource as EntitySource_ItemUse;
			if (actualSpawnSource.Entity is not Player)
				return;

			Player player = actualSpawnSource.Entity as Player;
			projectile.CritChance += player.GetWeaponCrit(actualSpawnSource.Item);
			projectile.ArmorPenetration += player.GetWeaponArmorPenetration(actualSpawnSource.Item);
		}

		/// <summary> Gets the instance of the specified GlobalProjectile type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetGlobalProjectile<T>(bool exactType = true) where T : GlobalProjectile
			=> GlobalType.GetGlobal<Projectile, GlobalProjectile, T>(globalProjectiles, exactType);

		/// <summary> Gets the local instance of the type of the specified GlobalProjectile instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetGlobalProjectile<T>(T baseInstance) where T : GlobalProjectile
			=> GlobalType.GetGlobal<Projectile, GlobalProjectile, T>(globalProjectiles, baseInstance);

		/// <summary> Gets the instance of the specified GlobalProjectile type. </summary>
		public bool TryGetGlobalProjectile<T>(out T result, bool exactType = true) where T : GlobalProjectile
			=> GlobalType.TryGetGlobal<GlobalProjectile, T>(globalProjectiles, exactType, out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified GlobalProjectile instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetGlobalProjectile<T>(T baseInstance, out T result) where T : GlobalProjectile
			=> GlobalType.TryGetGlobal<GlobalProjectile, T>(globalProjectiles, baseInstance, out result);

		public bool CountsAsClass<T>() where T : DamageClass
			=> CountsAsClass(ModContent.GetInstance<T>());

		public bool CountsAsClass(DamageClass damageClass)
			=> DamageClassLoader.effectInheritanceCache[DamageType.Type, damageClass.Type];
	}
}
