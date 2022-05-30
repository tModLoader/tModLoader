using Terraria;
using Terraria.ID;
using static Terraria.ID.ItemUseStyleID;

public class ItemUseStyleIDTest
{
	public void SetUseStyle(Item item) {
		item.useStyle = ItemUseStyleID.HoldingUp;
		item.useStyle = ItemUseStyleID.HoldingOut;
		item.useStyle = ItemUseStyleID.SwingThrow;
		item.useStyle = ItemUseStyleID.EatingUsing;
		item.useStyle = ItemUseStyleID.Stabbing;

		item.useStyle = HoldingUp;
		item.useStyle = HoldingOut;
		item.useStyle = SwingThrow;
		item.useStyle = EatingUsing;
		item.useStyle = Stabbing;

	}
}