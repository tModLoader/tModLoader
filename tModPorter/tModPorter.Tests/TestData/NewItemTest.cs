using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

public class NewItemTest
{
	IEntitySource source;
	NPC npc;
	NPC other;
	Item item;
	bool flag;
	int i, j;
	Point point;
	Point16 point16;

	void Method() {
		// (int X, int Y, int Width, int Height) collapses to a center
		Item.NewItem(source, 100, 200, 16, 32, ItemID.DirtBlock);
		Item.NewItem(source, 100, 200, npc.width * 2, npc.height * 2, ItemID.DirtBlock);
		Item.NewItem(source, 100, 200, 0, 0, ItemID.DirtBlock);
		Item.NewItem(source, (int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.Gel);
		Item.NewItem(source, (int)npc.Center.X, (int)npc.Center.Y, 0, 0, ItemID.Gel);
		Item.NewItem(source, (int)npc.position.X, (int)other.position.Y, npc.width, npc.height, ItemID.Gel);

		// a tile sized box at tile coordinates collapses to ToWorldCoordinates
		Item.NewItem(source, i * 16, j * 16, 16, 16, ItemID.Gel);
		Item.NewItem(source, i * 16, j * 16, 32, 32, ItemID.Gel);
		Item.NewItem(source, point.X * 16, point.Y * 16, 16, 16, ItemID.Gel);
		Item.NewItem(source, point16.X * 16, point16.Y * 16, 32, 32, item);
		Item.NewItem(source, i * 16, j * 16, 0, 0, ItemID.Gel);

		// the other position forms
		Item.NewItem(source, npc.Center, ItemID.Gel); // unchanged
		Item.NewItem(source, npc.position, npc.Size, ItemID.Gel); // unchanged
		Item.NewItem(source, npc.position, npc.width, npc.height, ItemID.Gel);
		Item.NewItem(source, npc.Hitbox, ItemID.Gel); // unchanged

		// stack, prefix and noBroadcast
		Item.NewItem(source, npc.Center, ItemID.Gel, 5); // unchanged
		Item.NewItem(source, npc.Center, ItemID.Gel, 5, false, 0);
		Item.NewItem(source, npc.Center, ItemID.Gel, 5, false, PrefixID.Legendary);
		Item.NewItem(source, npc.Hitbox, ItemID.Gel, 5, false, PrefixID.Legendary);
		Item.NewItem(source, npc.Center, ItemID.Gel, 5, true, PrefixID.Legendary);
		Item.NewItem(source, npc.Center, ItemID.Gel, 5, flag);
		Item.NewItem(source, npc.position, npc.Size, ItemID.Gel, 5, true, PrefixID.Legendary);
		Item.NewItem(source, npc.position, npc.width, npc.height, ItemID.Gel, 5, false, PrefixID.Legendary);

		// noGrabDelay becomes NewItemOwnership
		Item.NewItem(source, npc.Center, ItemID.Gel, 1, false, 0, true);
		Item.NewItem(source, npc.Center, ItemID.Gel, 1, false, 0, false);
		Item.NewItem(source, npc.Center, ItemID.Gel, 1, false, 0, flag);
		Item.NewItem(source, npc.Hitbox, ItemID.Gel, 5, false, 0, true);

		// reverseLookup is dropped
		Item.NewItem(source, npc.Center, ItemID.Gel, 1, false, 0, false, true);
		Item.NewItem(source, 100, 200, 0, 0, ItemID.DirtBlock, 5, true, PrefixID.Legendary, true, false);

		// 1.4.4 parameter names
		Item.NewItem(source, npc.Center, Type: ItemID.Gel, Stack: 3);
		Item.NewItem(source, npc.Center, ItemID.Gel, pfix: PrefixID.Legendary);
		Item.NewItem(source, npc.Center, ItemID.Gel, prefixGiven: PrefixID.Legendary);
		Item.NewItem(source, npc.Center, ItemID.Gel, noGrabDelay: true, noBroadcast: true);

		// the Item overloads
		Item.NewItem(source, 100, 200, 16, 32, item);
		Item.NewItem(source, npc.Center, item); // unchanged
		Item.NewItem(source, npc.position, npc.Size, item); // unchanged
		Item.NewItem(source, npc.position, npc.width, npc.height, item);
		Item.NewItem(source, npc.Hitbox, item); // unchanged
		Item.NewItem(source, npc.Center, item, true, true);
		Item.NewItem(source, npc.position, npc.Size, item, false, true);
		Item.NewItem(source, 100, 200, 16, 32, item, true, true);
		Item.NewItem(source, npc.position, npc.width, npc.height, item, false, true);
		Item.NewItem(source, npc.Hitbox, item, true, true, true);

		// Not handled. Uncommon named positional argument forms. Handling these would significantly complicate the rewriter
		Item.NewItem(source, pos: npc.position, randomBox: npc.Size, Type: ItemID.Gel);
		Item.NewItem(source, npc.position, randomBox: npc.Size, Type: ItemID.Gel);
		Item.NewItem(source, npc.position, Width: npc.width, Height: npc.height, Type: ItemID.Gel);
	}
}
