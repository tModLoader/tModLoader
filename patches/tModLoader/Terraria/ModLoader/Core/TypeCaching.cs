using System;

namespace Terraria.ModLoader.Core;

internal static class TypeCaching
{
	public static event Action OnClear;

	public static void Clear()
	{
		OnClear?.Invoke();
	}
}
