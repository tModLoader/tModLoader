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
		Item.NewItem(source, new Vector2(100 + 16 / 2, 200 + 32 / 2), ItemID.DirtBlock);
		Item.NewItem(source, new Vector2(100 + (npc.width * 2) / 2, 200 + (npc.height * 2) / 2), ItemID.DirtBlock);
		Item.NewItem(source, new Vector2(100, 200), ItemID.DirtBlock);
		Item.NewItem(source, npc.Center, ItemID.Gel);
		Item.NewItem(source, npc.Center, ItemID.Gel);
		Item.NewItem(source, new Vector2(npc.position.X + npc.width / 2, other.position.Y + npc.height / 2), ItemID.Gel);

		// a tile sized box at tile coordinates collapses to ToWorldCoordinates
		Item.NewItem(source, new Point(i, j).ToWorldCoordinates(), ItemID.Gel);
		Item.NewItem(source, new Point(i, j).ToWorldCoordinates(16, 16), ItemID.Gel);
		Item.NewItem(source, point.ToWorldCoordinates(), ItemID.Gel);
		Item.NewItem(source, point16.ToWorldCoordinates(16, 16), item);
		Item.NewItem(source, new Vector2(i * 16, j * 16), ItemID.Gel);

		// the other position forms
		Item.NewItem(source, npc.Center, ItemID.Gel); // unchanged
		Item.NewItem(source, npc.position, npc.Size, ItemID.Gel); // unchanged
		Item.NewItem(source, npc.position, new Vector2(npc.width, npc.height), ItemID.Gel);
		Item.NewItem(source, npc.Hitbox, ItemID.Gel); // unchanged

		// stack, prefix and noBroadcast
		Item.NewItem(source, npc.Center, ItemID.Gel, 5); // unchanged
		Item.NewItem(source, npc.Center, ItemID.Gel, 5);
		Item.NewItem(source, npc.Center, ItemID.Gel, 5, prefix: PrefixID.Legendary);
		Item.NewItem(source, npc.Hitbox, ItemID.Gel, 5, PrefixID.Legendary);
		Item.NewItem(source, npc.Center, ItemID.Gel, 5, prefix: PrefixID.Legendary, noBroadcast: true);
		Item.NewItem(source, npc.Center, ItemID.Gel, 5, noBroadcast: flag);
		Item.NewItem(source, npc.position, npc.Size, ItemID.Gel, 5, PrefixID.Legendary, noBroadcast: true);
		Item.NewItem(source, npc.position, new Vector2(npc.width, npc.height), ItemID.Gel, 5, PrefixID.Legendary);

		// noGrabDelay becomes NewItemOwnership
		Item.NewItem(source, npc.Center, ItemID.Gel, 1, ownership: NewItemOwnership.GrabDelayForAllPlayers);
		Item.NewItem(source, npc.Center, ItemID.Gel, 1);
		Item.NewItem(source, npc.Center, ItemID.Gel, 1, ownership: flag ? NewItemOwnership.GrabDelayForAllPlayers : NewItemOwnership.None);
		Item.NewItem(source, npc.Hitbox, ItemID.Gel, 5, ownership: NewItemOwnership.GrabDelayForAllPlayers);

		// reverseLookup is dropped
		Item.NewItem(source, npc.Center, ItemID.Gel, 1);
		Item.NewItem(source, new Vector2(100, 200), ItemID.DirtBlock, 5, PrefixID.Legendary, NewItemOwnership.GrabDelayForAllPlayers, noBroadcast: true);

		// 1.4.4 parameter names
		Item.NewItem(source, npc.Center, ItemID.Gel, 3);
		Item.NewItem(source, npc.Center, ItemID.Gel, prefix: PrefixID.Legendary);
		Item.NewItem(source, npc.Center, ItemID.Gel, prefix: PrefixID.Legendary);
		Item.NewItem(source, npc.Center, ItemID.Gel, ownership: NewItemOwnership.GrabDelayForAllPlayers, noBroadcast: true);

		// the Item overloads
		Item.NewItem(source, new Vector2(100 + 16 / 2, 200 + 32 / 2), item);
		Item.NewItem(source, npc.Center, item); // unchanged
		Item.NewItem(source, npc.position, npc.Size, item); // unchanged
		Item.NewItem(source, npc.position, new Vector2(npc.width, npc.height), item);
		Item.NewItem(source, npc.Hitbox, item); // unchanged
		Item.NewItem(source, npc.Center, item, NewItemOwnership.GrabDelayForAllPlayers, noBroadcast: true);
		Item.NewItem(source, npc.position, npc.Size, item, NewItemOwnership.GrabDelayForAllPlayers);
		Item.NewItem(source, new Vector2(100 + 16 / 2, 200 + 32 / 2), item, NewItemOwnership.GrabDelayForAllPlayers, noBroadcast: true);
		Item.NewItem(source, npc.position, new Vector2(npc.width, npc.height), item, NewItemOwnership.GrabDelayForAllPlayers);
		Item.NewItem(source, npc.Hitbox, item, NewItemOwnership.GrabDelayForAllPlayers, noBroadcast: true);

		// Not handled. Uncommon named positional argument forms. Handling these would significantly complicate the rewriter
#if COMPILE_ERROR
		Item.NewItem(source, pos: npc.position, randomBox: npc.Size, Type: ItemID.Gel);
		Item.NewItem(source, npc.position, randomBox: npc.Size, Type: ItemID.Gel);
		Item.NewItem(source, npc.position, Width: npc.width, Height: npc.height, Type: ItemID.Gel);
#endif
	}
}
