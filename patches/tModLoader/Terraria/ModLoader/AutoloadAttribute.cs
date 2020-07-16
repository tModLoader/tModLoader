using System;
using System.Linq;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Allows for types to be autoloaded and unloaded.
	/// True to always autoload, false to never autoload, null to use mod default.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public class AutoloadAttribute : Attribute
	{
		public readonly bool Value;

		public AutoloadAttribute(bool value = true) { Value = value; }

		public static bool? GetValue(Type type) {
			//Get all AutoloadAttributes on the type.
			object[] all = type.GetCustomAttributes(typeof(AutoloadAttribute), true);
			//The first should be the most derived attribute.
			var mostDerived = (AutoloadAttribute)all.FirstOrDefault();
			//If there were no declarations, then return null.
			return mostDerived?.Value;
		}
	}
}
