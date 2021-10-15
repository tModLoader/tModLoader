using System;

namespace Terraria.ModLoader
{
	//TODO: When .NET 6 is out, make this generic and abuse that with 'ModContent.GetInstance<T>().Type'
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class ComponentDependencyAttribute : Attribute
	{
		public Type ComponentType { get; }

		public ComponentDependencyAttribute(Type componentType) {
			ComponentType = componentType;
		}
	}
}
