using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	public class ExampleRemoveShimmer : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			return entity.type == ItemID.RodofDiscord;
		}

		public override bool CanShimmer(Item item) {
			return false; // The rod of discord can no longer shimmer.
		}
	}
}
