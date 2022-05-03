using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
	public abstract class AMemberJitAttribute : Attribute
	{
		public virtual bool ShouldJITType(Type type, List<string> mods) => true;
		public virtual bool ShouldJITMethod(MethodInfo methodInfo, List<string> mods) => true;
	}

	public class NoJITAttribute : AMemberJitAttribute
	{
		public override bool ShouldJITType(Type type, List<string> mods) => false;
		public override bool ShouldJITMethod(MethodInfo methodInfo, List<string> mods) => false;
	}

	public sealed class JITWhenModsEnabledAttribute : AMemberJitAttribute
	{
		public readonly string[] Names;

		public JITWhenModsEnabledAttribute(params string[] names) {
			Names = names ?? throw new ArgumentNullException(nameof(names));
		}
		public override bool ShouldJITType(Type type, List<string> mods) {
			return !Names.Except(mods).Any();
		}

		public override bool ShouldJITMethod(MethodInfo methodInfo, List<string> mods) {
			return !Names.Except(mods).Any();
		}
	}
}