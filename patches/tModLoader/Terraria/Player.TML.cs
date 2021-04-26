using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player : IEntityWithGlobals<ModPlayer>
	{
		internal IList<string> usedMods;
		internal Instanced<ModPlayer>[] modPlayers = Array.Empty<Instanced<ModPlayer>>();

		public int infoDisplayPage;
		public HashSet<int> nearbyModTorch = new HashSet<int>();

		public ReadOnlySpan<Instanced<ModPlayer>> Globals => modPlayers;

		// Get

		/// <summary> Gets the instance of the specified ModPlayer type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetModPlayer<T>(bool exactType = true) where T : ModPlayer
			=> GlobalType.GetGlobal<Player, ModPlayer, T>(modPlayers, exactType);

		/// <summary> Gets the local instance of the type of the specified ModPlayer instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetModPlayer<T>(T baseInstance) where T : ModPlayer
			=> GlobalType.GetGlobal<Player, ModPlayer, T>(modPlayers, baseInstance);

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
				damageData[i] = new DamageClassData(StatModifier.One, 0, StatModifier.One);
				DamageClassLoader.DamageClasses[i].SetDefaultStats(this);
			}
		}


		/// <summary>
		/// Gets the crit modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary> 
		public ref int GetCritChance<T>() where T : DamageClass => ref GetCritChance(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the damage modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetDamage<T>() where T : DamageClass => ref GetDamage(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the knockback modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetKnockback<T>() where T : DamageClass => ref GetKnockback(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the crit modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref int GetCritChance(DamageClass damageClass) => ref damageData[damageClass.Type].critChance;

		/// <summary>
		/// Gets the damage modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetDamage(DamageClass damageClass) => ref damageData[damageClass.Type].damage;

		/// <summary>
		/// Gets the knockback modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetKnockback(DamageClass damageClass) => ref damageData[damageClass.Type].knockback;
	}
}
