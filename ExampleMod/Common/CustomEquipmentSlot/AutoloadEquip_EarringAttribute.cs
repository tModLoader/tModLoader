using System;

namespace ExampleMod.Common.CustomEquipmentSlot
{
	/// <summary>
	/// This attribute will cause an annotated ModItem class to attempt to autoload an equipment texture. This works even in other mods. This is similar to how AutoloadEquip is used to autoload textures for vanilla equipment slots.
	/// <para/> The autoloaded texture's path will be Texture + _Earrings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class AutoloadEquip_EarringAttribute : Attribute
	{
	}
}
