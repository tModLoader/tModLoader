using System;

namespace Terraria.ModLoader
{
	[AttributeUsage(AttributeTargets.Class)]
	public class AutoloadEquip : Attribute
	{
		public readonly EquipType[] equipTypes;

		public AutoloadEquip(params EquipType[] equipTypes) {
			this.equipTypes = equipTypes;
		}
	}
}
