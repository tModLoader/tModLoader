using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ExampleMod.Items.ExampleDamageClass
{
	public class ExampleResourceStaff : ExampleDamageItem
	{
		// This is a staff that uses the example damage class stuff you've set up before, but uses exampleResource instead of mana.
		// This is a very simple way of doing it, and if you plan on multiple items using exampleResource then I'd suggest making a new abstract ModItem class that inherits ExampleDamageItem,
		// and doing the CanUseItem and UseItem in a more generalized way there, so you can just define the resource usage in SetDefaults and it'll do it automatically for you.
		public override void SetStaticDefaults() {
			Item.staff[item.type] = true;
		}

		public override void SafeSetDefaults() {
			item.CloneDefaults(ItemID.AmethystStaff);
			item.Size = new Vector2(28, 36);
			item.damage = 32;
			item.knockBack = 3;
			item.rare = ItemRarityID.Red;
			item.mana = 0; // Make sure to nullify the mana usage of the staff here, as it still copies the setdefaults of the amethyst staff.
			item.useStyle = ItemUseStyleID.HoldingOut;

			// exampleResourceCost is a field in the base class ExampleDamageItem. This item consumes 10 Example Resource to use.
			exampleResourceCost = 10;
		}
	}
}
