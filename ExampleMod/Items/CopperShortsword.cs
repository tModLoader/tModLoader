using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class CopperShortsword : GlobalItem
	{
		public override void SetDefaults(Item item)
		{
			if (item.type == ItemID.CopperShortsword)
			{
				item.damage = 50;		//Changed original CopperShortsword's damage to 50!
			}
		}
	}
}
