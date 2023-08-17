using System;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader;

public class PreJITFilter
{
	public virtual bool ShouldJIT(MemberInfo member)
	{
		return member.GetCustomAttributes<MemberJitAttribute>().All(a => a.ShouldJIT(member));
	}
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
public abstract class MemberJitAttribute : Attribute
{
	public virtual bool ShouldJIT(MemberInfo member) => true;
}

public class NoJITAttribute : MemberJitAttribute
{
	public override bool ShouldJIT(MemberInfo member) => false;
}

public sealed class JITWhenModsEnabledAttribute : MemberJitAttribute
{
	public readonly string[] Names;

	public JITWhenModsEnabledAttribute(params string[] names)
	{
		Names = names ?? throw new ArgumentNullException(nameof(names));
	}

	public override bool ShouldJIT(MemberInfo member) => Names.All(ModLoader.HasMod);
}