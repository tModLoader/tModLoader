using System.Collections.Generic;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static class DamageClassLoader
{
	public static int DamageClassCount => DamageClasses.Count;

	internal static bool[,] effectInheritanceCache;

	internal static readonly List<DamageClass> DamageClasses = new List<DamageClass>() {
		DamageClass.Default,
		DamageClass.Generic,
		DamageClass.Melee,
		DamageClass.MeleeNoSpeed,
		DamageClass.Ranged,
		DamageClass.Magic,
		DamageClass.Summon,
		DamageClass.SummonMeleeSpeed,
		DamageClass.MagicSummonHybrid,
		DamageClass.Throwing
	};

	private static readonly int DefaultClassCount = DamageClasses.Count;

	static DamageClassLoader()
	{
		RegisterDefaultClasses();
		RebuildEffectInheritanceCache();
	}

	internal static int Add(DamageClass damageClass)
	{
		DamageClasses.Add(damageClass);
		return DamageClasses.Count - 1;
	}

	internal static void ResizeArrays()
	{
		LoaderUtils.ResetStaticMembers(typeof(DamageClass.Sets));
		RebuildEffectInheritanceCache();
	}

	internal static void Unload()
	{
		DamageClasses.RemoveRange(DefaultClassCount, DamageClasses.Count - DefaultClassCount);
		DamageClass.Search = IdDictionary.Create<DamageClass, int>();
		foreach (var damageClass in DamageClasses)
			DamageClass.Search.Add(damageClass.FullName, damageClass.Type); // SetupContent isn't called on vanilla classes
	}

	private static void RebuildEffectInheritanceCache()
	{
		effectInheritanceCache = new bool[DamageClassCount, DamageClassCount];

		for (int i = 0; i < DamageClasses.Count; i++) {
			for (int j = 0; j < DamageClasses.Count; j++) {
				DamageClass damageClass = DamageClasses[i];
				if (damageClass == DamageClasses[j] || damageClass.GetEffectInheritance(DamageClasses[j]))
					effectInheritanceCache[i, j] = true;
			}
		}
	}

	internal static void RegisterDefaultClasses()
	{
		int i = 0;
		foreach (var damageClass in DamageClasses) {
			damageClass.Type = i++;
			ContentInstance.Register(damageClass);
			ModTypeLookup<DamageClass>.Register(damageClass);
		}
	}

	/// <summary>
	/// Gets the DamageClass instance corresponding to the specified type
	/// </summary>
	/// <param name="type">The <see cref="DamageClass.Type"/> of the damage class</param>
	/// <returns>The DamageClass instance, null if not found.</returns>
	public static DamageClass GetDamageClass(int type) => type < DamageClasses.Count ? DamageClasses[type] : null;
}
