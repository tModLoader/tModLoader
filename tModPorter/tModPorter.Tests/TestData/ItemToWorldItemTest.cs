using Terraria;
using Terraria.ID;
//using Terraria.ModLoader;
using Microsoft.Xna.Framework;

public class ItemToWorldItemTest
{
	void Method()
	{
		foreach (Item a in Main.ActiveItems) { }
		foreach (var b in Main.ActiveItems) { }
		foreach (Item c in Main.item) { }
		foreach (var d in Main.item) { }

		for (int index = 0; index < Main.maxItems; index++) {
			Item e = Main.item[index];
			var f = Main.item[index];
		}

		Item g = Main.item[Item.NewItem(null, default(Rectangle), null)];
		var h = Main.item[Item.NewItem(null, default(Rectangle), null)];

		/* Wasn't able to implement:
		foreach (Terraria.Item a2 in Terraria.Main.ActiveItems) { }
		Terraria.Item e2 = Terraria.Main.item[index];
		*/

		// Verify that fields of Item that were moved to WorldItem don't get refactored when Item reference changed to WorldItem reference
		Rectangle hitbox = default;
		foreach (Item item in Main.item) {
			if (item.active && !item.beingGrabbed && hitbox.Intersects(item.Hitbox)) {
				item.active = false;

				NetMessage.SendData(MessageID.SyncItem, number: item.whoAmI);
			}
		}
	}
}