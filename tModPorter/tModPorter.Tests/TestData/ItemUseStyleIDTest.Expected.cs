using Terraria;
using Terraria.ID;
using static Terraria.ID.ItemUseStyleID;

public class ItemUseStyleIDTest
{
	public void SetUseStyle(Item item) {
		item.useStyle = ItemUseStyleID.HoldUp;
		item.useStyle = ItemUseStyleID.Shoot;
		item.useStyle = ItemUseStyleID.Swing;
		item.useStyle = ItemUseStyleID.EatFood;
		item.useStyle = ItemUseStyleID.Thrust;

		item.useStyle = HoldUp;
		item.useStyle = Shoot;
		item.useStyle = Swing;
		item.useStyle = EatFood;
		item.useStyle = Thrust;

	}
}