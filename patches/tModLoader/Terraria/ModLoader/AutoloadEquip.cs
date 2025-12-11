using System;

namespace Terraria.ModLoader;

/// <summary>
/// This attribute annotates a <see cref="ModItem"/> class to indicate that the game should autoload the specified equipment texture or textures and assign it to this item. The equipment texture is the texture that appears on the player itself when the item is worn and visible. Armor and most accessories will set at least one <see cref="EquipType"/>, but not every accessory needs on-player visuals.
/// <para/> The equipment texture will be loaded from the path made from appending "_EquipTypeNameHere" to <see cref="ModItem.Texture"/>. An error will be thrown during mod loading if the texture can't be found. For example, a helmet item named "ExampleHelmet" annotated with <c>[AutoloadEquip(EquipType.Head)]</c> will need both a "ExampleHelmet.png" and "ExampleHelmet_Head.png" to work. Note that equipment textures must follow specific layouts, see <see href="https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/Items/Armor">ExampleMod examples</see> of similar items to use as a guide.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoloadEquip : Attribute
{
	public readonly EquipType[] equipTypes;

	public AutoloadEquip(params EquipType[] equipTypes)
	{
		this.equipTypes = equipTypes;
	}
}
