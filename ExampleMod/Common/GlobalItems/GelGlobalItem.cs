using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	// This is another part of the ExampleShiftClickSlotPlayer.cs that adds a tooltip line to the gel
	public class GelGlobalItem : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.Gel;

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			// Here we add a tooltip to the gel to let the player know what will happen
			tooltips.Add(new(Mod, "SpecialShiftClick", "Shift-click on this item from your inventory to get a random color and rarity!"));
		}
	}
}
