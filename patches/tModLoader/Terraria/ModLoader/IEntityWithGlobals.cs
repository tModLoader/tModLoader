using System;

namespace Terraria.ModLoader
{
	public interface IEntityWithGlobals<T> where T : GlobalType
	{
		Instanced<T>[] Globals { get; }
	}
}
