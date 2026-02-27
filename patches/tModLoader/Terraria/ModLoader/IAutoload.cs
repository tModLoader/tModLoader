using System;

namespace Terraria.ModLoader;

public interface IAutoload<TImpl> where TImpl : IAutoloader
{
}

public interface IAutoloader
{
	public static abstract void Autoload(Mod mod, Type type);
}
