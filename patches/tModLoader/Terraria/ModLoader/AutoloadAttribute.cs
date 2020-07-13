using System;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Allows for types to be autoloaded and unloaded.
	/// True to always autoload, false to never autoload, null to use mod default.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, /*AllowMultiple = true,*/ Inherited = true)]
	public class AutoloadAttribute : Attribute
	{
		public readonly bool? Value;

		public AutoloadAttribute() : this(null) { }
		public AutoloadAttribute(bool? value) { Value = value; }
	}
}
