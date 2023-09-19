using System;

namespace tModCodeAssist.IDType.Sets;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AssociatedNameAttribute : Attribute
{
	public string Name { get; }

	public AssociatedNameAttribute(string name)
	{
		Name = name;
	}
}
