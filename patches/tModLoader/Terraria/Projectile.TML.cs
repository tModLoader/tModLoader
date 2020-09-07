using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Projectile
	{
		public ModProjectile modProjectile { get; internal set; }

		internal GlobalProjectile[] globalProjectiles = new GlobalProjectile[0];

		// Get

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
	}
}
