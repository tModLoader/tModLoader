using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Terraria.ModLoader;

/// <summary>
/// Works with <see cref="Mod.AddContent"/> to provide Load and Unload callbacks.<br/>
/// All non-abstract types extending this class which have a default constructor will be autoloaded and passed to <see cref="Mod.AddContent"/><br/>
/// For autoloading types which do not have a default constructor, see <see cref="ICustomAutoload"/>
/// </summary>
public interface ILoadable : IAutoload<ILoadable.AutoloadImpl>
{
	/// <summary>
	/// Called when loading the type.
	/// </summary>
	/// <param name="mod">The mod instance associated with this type.</param>
	public abstract void Load(Mod mod);

	/// <summary>
	/// Whether or not this type should be loaded when it's told to. Returning false disables <see cref="Mod.AddContent"/> from actually loading this type.
	/// </summary>
	/// <param name="mod">The mod instance trying to add this content</param>
	public virtual bool IsLoadingEnabled(Mod mod) => true;

	/// <summary>
	/// Called during unloading when needed.
	/// </summary>
	public abstract void Unload();

	public class AutoloadImpl : IAutoloader
	{
		public static void Autoload(Mod mod, Type type)
		{
			if (type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) is { } ctor)
				mod.AddContent((ILoadable)ctor.Invoke(null));
		}
	}
}
