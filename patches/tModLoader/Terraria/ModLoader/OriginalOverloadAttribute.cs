using System;

namespace Terraria.ModLoader;

/// <summary>
/// Indicates that a method matching multiple overloads is the original method. This should be added to the original method when adding a 2nd overload to prevent breaking binary and source compatibility. The original method event will be generated as if there were not multiple overloads (in addition to the new fully qualified event), preserving the old simplified event name.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class OriginalOverloadAttribute : Attribute
{
}
