using Microsoft.Xna.Framework;
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

		private DamageClass _damageClass = DamageClass.Default;
		public static Projectile NewProjectileDirect(IProjectileSource spawnSource, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = 255, float ai0 = 0f, float ai1 = 0f)
			=> Main.projectile[NewProjectile(spawnSource, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, owner, ai0, ai1)];

		private DamageClass _damageClass = DamageClass.Generic;
		/// <summary>
		/// The damage type assigned to this projectile, represented as a DamageClass.
		/// Leave blank or use DamageClass.Default to prevent damage type scaling of any kind for this projectile.
		/// Use DamageClass.Generic/Melee/Ranged/Magic/Summon/Throwing for vanilla damage types.
		/// Refer to ExampleMod for more information on how to create and use your own damage types.
		/// </summary>
		public DamageClass DamageType {
			get => _damageClass;
			set => _damageClass = value ?? throw new ArgumentException("DamageType cannot be null");
		}

		private int _armorPenetration = 0;
		/// <summary>
		/// The number of defense points that this projectile can ignore on its own. Cannot be set to negative values. Defaults to 0.
		/// On spawn, if this projectile was fired frrom a weapon, this value has the total armor penetration of the weapon that made the projectile added to itself.
		/// </summary>
		public int ArmorPenetration {
			get => _armorPenetration;
			set => _armorPenetration = Math.Max(value, 0);
		}

		private int _crit = 0;
		/// <summary>
		/// The critical strike chance modifier of this projectile. Cannot be set to negative values. Defaults to 0.
		/// On spawn, if this projectile was fired frrom a weapon, this value has the total critical strike chance of the weapon that made the projectile added to itself.
		/// </summary>
		public int CritChance {
			get => _crit;
			set => _crit = Math.Max(value, 0);
		}

		private static void HandlePlayerStatModifiers(IProjectileSource spawnSource, Projectile projectile) {
			if (spawnSource is null || !(spawnSource is ProjectileSource_Item || spawnSource is ProjectileSource_Item_WithAmmo))
				return;

			if (spawnSource is ProjectileSource_Item) {
				ProjectileSource_Item actualSpawnSource = spawnSource as ProjectileSource_Item;
				projectile.CritChance += actualSpawnSource.Player.GetWeaponCrit(actualSpawnSource.Item);
				projectile.ArmorPenetration += actualSpawnSource.Player.GetWeaponArmorPenetration(actualSpawnSource.Item);
			}
			else
			{
				ProjectileSource_Item_WithAmmo actualSpawnSource = spawnSource as ProjectileSource_Item_WithAmmo;
				projectile.CritChance += actualSpawnSource.Player.GetWeaponCrit(actualSpawnSource.Item);
				projectile.ArmorPenetration += actualSpawnSource.Player.GetWeaponArmorPenetration(actualSpawnSource.Item);
			}
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

		/// <summary>
		/// Spawns a projectile based on the supplied parameters.
		/// </summary>
		/// <param name="spawnSource"></param>
		/// <param name="position"></param>
		/// <param name="velocity"></param>
		/// <param name="Type"></param>
		/// <param name="Damage"></param>
		/// <param name="KnockBack"></param>
		/// <param name="Owner"></param>
		/// <param name="ai0"></param>
		/// <param name="ai1"></param>
		/// <returns>The projectile spawned as a result of the method.</returns>
		// TO-DO: properly document what the hell spawnSource actually means both here and next to the actual methods so that the average modder can understand the parameter
		public static Projectile NewProjectileDirect(IProjectileSource spawnSource, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = 255, float ai0 = 0f, float ai1 = 0f)
			=> Main.projectile[Projectile.NewProjectile(spawnSource, position.X, position.Y, velocity.X, velocity.Y, Type, Damage, KnockBack, Owner, ai0, ai1)];

		public bool CountsAsClass<T>() where T : DamageClass
			=> CountsAsClass(ModContent.GetInstance<T>());

		public bool CountsAsClass(DamageClass damageClass)
			=> DamageClassLoader.countsAs[DamageType.Type, damageClass.Type];
	}
}
