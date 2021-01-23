using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Projectile
	{
		public ModProjectile ModProjectile { get; internal set; }

		internal GlobalProjectile[] globalProjectiles = new GlobalProjectile[0];

		private DamageClass _damageClass = DamageClass.Generic;
		/// <summary>
		/// The damage type of this Projectile.
		/// Assign to DamageClass.Melee/Ranged/Magic/Summon/Throwing for vanilla classes, or ModContent.GetInstance<T>() for custom damage types.
		/// Set to DamageClass.Generic to not have any one damage class, but still have the weapon be boosted by universal bonuses.
		/// </summary>
		public DamageClass DamageType {
			get => _damageClass;
			set => _damageClass = value;
		}

		/// <summary> Gets the instance of the specified GlobalProjectile type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetGlobalProjectile<T>() where T : GlobalProjectile
			=> GetGlobalProjectile(ModContent.GetInstance<T>());

		/// <summary> Gets the local instance of the type of the specified GlobalProjectile instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetGlobalProjectile<T>(T baseInstance) where T : GlobalProjectile
			=> baseInstance.Instance(this) as T ?? throw new KeyNotFoundException($"Instance of '{typeof(T).Name}' does not exist on the current projectile.");

		/*
		// TryGet

		/// <summary> Gets the instance of the specified GlobalProjectile type. </summary>
		public bool TryGetGlobalProjectile<T>(out T result) where T : GlobalProjectile
			=> TryGetGlobalProjectile(ModContent.GetInstance<T>(), out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified GlobalProjectile instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetGlobalProjectile<T>(T baseInstance, out T result) where T : GlobalProjectile {
			if (baseInstance == null || baseInstance.index < 0 || baseInstance.index >= globalProjectiles.Length) {
				result = default;

				return false;
			}

			result = baseInstance.Instance(this) as T;

			return result != null;
		}
		*/

		public bool CountsAsClass(DamageClass damageClass) {
			if (DamageType != null)
				return DamageClassLoader.countsAs[DamageType.Type, damageClass.Type];

			return false;
		}
	}
}
