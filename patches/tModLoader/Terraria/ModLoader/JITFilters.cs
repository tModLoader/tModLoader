using System;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader;

/// <summary>
/// Allows custom JIT logic to be applied to classes in this mod.
/// </summary>
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

/// <summary>
/// Excludes a class, method, or property from the load time JIT.
/// </summary>
public class NoJITAttribute : MemberJitAttribute
{
	public override bool ShouldJIT(MemberInfo member) => false;
}

/// <summary>
/// Excludes a class, method, or property from the <see href="https://github.com/tModLoader/tModLoader/wiki/JIT-Exception#what-are-jit-exceptions">load time JIT</see> unless the specified mods are also loaded. Use this on any member which directly references a Type from a <see href="https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content#weak-references-aka-weakreferences-expert">weakly referenced mod</see> that might not be present. See <see href="https://github.com/tModLoader/tModLoader/wiki/JIT-Exception#weak-references">this wiki page</see> for more information.
/// </summary>
public sealed class JITWhenModsEnabledAttribute : MemberJitAttribute
{
	public readonly string[] Names;

	public JITWhenModsEnabledAttribute(params string[] names)
	{
		Names = names ?? throw new ArgumentNullException(nameof(names));
	}

	public override bool ShouldJIT(MemberInfo member) => Names.All(ModLoader.HasMod);
}