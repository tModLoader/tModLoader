using System;
using System.Linq;

namespace Terraria.ModLoader;

/// <summary>
/// Allows for customizing if the annotated Type should be automatically loaded.
/// <para/> True to always autoload, which is the default behavior, or false to prevent autoloading this Type.
/// <para/> It is also possible to dictate if autoloading should conditionally only happen on a client or a server by setting <see cref="Side"/> to <see cref="ModSide.Client"/> or <see cref="ModSide.Server"/>. One use of this is to prevent graphics related classes from loading on the server.
/// <para/> Note that content with a non-default constructor or marked as abstract will automatically not be autoloaded, so this attribute is not needed for those.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
public sealed class AutoloadAttribute : Attribute
{
	private static readonly AutoloadAttribute Default = new AutoloadAttribute();

	public readonly bool Value;
	public ModSide Side { get; set; } = ModSide.Both;

	public bool NeedsAutoloading => Value && Core.ModOrganizer.LoadSide(Side);

	public AutoloadAttribute(bool value = true)
	{
		Value = value;
	}

	public static AutoloadAttribute GetValue(Type type)
	{
		//Get all AutoloadAttributes on the type.
		object[] all = type.GetCustomAttributes(typeof(AutoloadAttribute), true);
		//The first should be the most derived attribute.
		var mostDerived = (AutoloadAttribute)all.FirstOrDefault();
		//If there were no declarations, then return null.
		return mostDerived ?? Default;
	}
}
