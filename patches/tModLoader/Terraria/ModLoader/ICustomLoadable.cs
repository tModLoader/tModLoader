using System;

namespace Terraria.ModLoader;
public interface ICustomLoadable
{
	public static abstract void CustomLoad(Mod mod, Type type);
}
