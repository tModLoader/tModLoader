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

		public static Projectile NewProjectileDirect(IProjectileSource spawnSource, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = 255, float ai0 = 0f, float ai1 = 0f)
			=> Main.projectile[NewProjectile(spawnSource, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, owner, ai0, ai1)];

		private DamageClass _damageClass = DamageClass.Generic;
		/// <summary>
		/// The damage type of this Projectile. Assign to DamageClass.Generic/Melee/Ranged/Magic/Summon/Throwing, or ModContent.GetInstance&lt;T&gt;() for custom damage types.
		/// </summary>
		public DamageClass DamageType {
			get => _damageClass;
			set => _damageClass = value ?? throw new ArgumentException("DamageType cannot be null");
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

		public bool CountsAsClass(DamageClass damageClass) => DamageClassLoader.countsAs[DamageType.Type, damageClass.Type];
	}
}
