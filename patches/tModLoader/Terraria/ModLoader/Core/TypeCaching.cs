using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Core;

internal static class TypeCaching
{
	public static event Action OnClear;

	public static void Clear()
	{
		OnClear?.Invoke();
	}

	private static HashSet<Type> _resetsRegistered = new();
	internal static void ResetStaticMembersOnClear(Type type)
	{
		if (_resetsRegistered.Add(type))
			OnClear += () => LoaderUtils.ResetStaticMembers(type);
	}
}
