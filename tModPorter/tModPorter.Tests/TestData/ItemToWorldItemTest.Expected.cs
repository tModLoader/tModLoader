using Terraria;
using Terraria.ID;
//using Terraria.ModLoader;
using Microsoft.Xna.Framework;

public class ItemToWorldItemTest
{
	void Method()
	{
		foreach (WorldItem a in Main.ActiveItems) { }
		foreach (var b in Main.ActiveItems) { }
		foreach (WorldItem c in Main.item) { }
		foreach (var d in Main.item) { }

		for (int index = 0; index < Main.maxItems; index++) {
			WorldItem e = Main.item[index];
			var f = Main.item[index];
		}

		WorldItem g = Main.item[Item.NewItem(null, default(Rectangle), null)];
		var h = Main.item[Item.NewItem(null, default(Rectangle), null)];

		/* Wasn't able to implement:
		foreach (Terraria.Item a2 in Terraria.Main.ActiveItems) { }
		Terraria.Item e2 = Terraria.Main.item[index];
		*/

		// Verify that fields of Item that were moved to WorldItem don't get refactored when Item reference changed to WorldItem reference
		Rectangle hitbox = default;
		foreach (WorldItem item in Main.item) {
			if (item.active && !item.beingGrabbed && hitbox.Intersects(item.Hitbox)) {
#if COMPILE_ERROR
				item.active = false;
#endif

				NetMessage.SendData(MessageID.SyncItem, number: item.whoAmI);
			}
		}
	}
}