using System;
using System.Reflection;

namespace Terraria.ModLoader;

/// <summary>
/// Provides a static abstract <see cref="Autoload(Mod)"/> method for mods to implement their own autoloading for types where <see cref="ILoadable"/> does not suffice.<br/>
/// Sample use cases:<br/>
/// <list type="bullet">
/// <item>When the type has multiple instances of the same class</item>
/// <item>To manually call some content registration methods during autoloading (eg <seealso cref="MusicLoader.AddMusic"/>)</item>
/// </list>
/// To implement completely custom loading behavior in a library, consider making an interface that extends from <see cref="IAutoload{TImpl}"/>
/// </summary>
public interface ICustomAutoload : IAutoload<ICustomAutoload.AutoloadImpl>
{
	public static abstract void Autoload(Mod mod);

	public class AutoloadImpl : IAutoloader
	{
		private static void Invoker<T>(Mod mod) where T : ICustomAutoload => T.Autoload(mod);
		private static readonly MethodInfo _invoker = typeof(AutoloadImpl).GetMethod(nameof(Invoker), BindingFlags.Static | BindingFlags.NonPublic);

		public static void Autoload(Mod mod, Type type) => _invoker.MakeGenericMethod(type).Invoke(null, [mod]);
	}
}
