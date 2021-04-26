using System;

namespace Terraria.ModLoader
{
	public interface IEntityWithGlobals<T> where T : GlobalType
	{
		ReadOnlySpan<Instanced<T>> Globals { get; }
	}
}
