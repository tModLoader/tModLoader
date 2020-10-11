using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
		internal IList<string> usedMods;
		internal ModPlayer[] modPlayers = new ModPlayer[0];

		// Get

		/// <summary> Gets the instance of the specified ModPlayer type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetModPlayer<T>() where T : ModPlayer
			=> GetModPlayer(ModContent.GetInstance<T>());

		/// <summary> Gets the local instance of the type of the specified ModPlayer instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetModPlayer<T>(T baseInstance) where T : ModPlayer
			=> modPlayers[baseInstance.index] as T ?? throw new KeyNotFoundException($"Instance of '{typeof(T).Name}' does not exist on the current player.");

		/*
		// TryGet

		/// <summary> Gets the instance of the specified ModPlayer type. </summary>
		public bool TryGetModPlayer<T>(out T result) where T : ModPlayer
			=> TryGetModPlayer(ModContent.GetInstance<T>(), out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified ModPlayer instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetModPlayer<T>(T baseInstance, out T result) where T : ModPlayer {
			if (baseInstance == null || baseInstance.index < 0 || baseInstance.index >= modPlayers.Length) {
				result = default;

				return false;
			}

			result = modPlayers[baseInstance.index] as T;

			return result != null;
		}
		*/

		// Damage Classes

		private DamageClassData[] damageData;

		internal void ResetDamageClassData() {
			damageData = new DamageClassData[DamageClassLoader.DamageClassCount];

			for (int i = 0; i < damageData.Length; i++) {
				damageData[i] = new DamageClassData(Modifier.One, new Modifier(4f, 1f)); // Default values from vanilla - 4 crit, 0 add, 1x mult.
			}
		}

		/// <summary> Gets the reference to the crit modifier for this damage type on this player. Since this returns a reference, you can freely modify this method's return value with operators. <para/> Note that vanilla turns this to int before using it. </summary>
		public ref Modifier GetCrit<T>() where T : DamageClass => ref GetCrit(ModContent.GetInstance<T>());

		/// <summary> Gets the reference to the damage modifier for this damage type on this player. Since this returns a reference, you can freely modify this method's return value with operators. </summary>
		public ref Modifier GetDamage<T>() where T : DamageClass => ref GetDamage(ModContent.GetInstance<T>());

		/// <summary> Gets the reference to the crit modifier for this damage type on this player. Since this returns a reference, you can freely modify this method's return value with operators. <para/> Note that vanilla turns this to int before using it. </summary>
		public ref Modifier GetCrit(DamageClass damageClass) => ref damageData[damageClass.index].crit;

		/// <summary> Gets the reference to the damage modifier for this damage type on this player. Since this returns a reference, you can freely modify this method's return value with operators. </summary>
		public ref Modifier GetDamage(DamageClass damageClass) => ref damageData[damageClass.index].damage;
	}
}
