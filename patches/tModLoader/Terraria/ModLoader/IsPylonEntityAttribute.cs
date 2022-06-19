using System;
using Terraria.GameContent;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Use this attribute on Tile Entities that you want to be included in vanilla's pylon list. What this means, in short,
	/// is that whenever all of the pylons are refreshed by vanilla, Tile Entities with this attribute will get their own
	/// <seealso cref="TeleportPylonInfo"/> instance and it will be included along-side all of vanilla's pylons and any
	/// other modded pylons.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class IsPylonEntityAttribute : Attribute { }
}
