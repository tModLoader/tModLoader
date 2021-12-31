namespace Terraria.DataStructures
{
	// New class. *_Item was renamed to *_ItemUse, since it has a Player field. This one is just for items on the ground.
	public class EntitySource_Item : IEntitySource
	{
		public readonly Item Item;

		public EntitySource_Item(Item item) {
			Item = item;
		}
	}
}
