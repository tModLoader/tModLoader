using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader
{
	/// <summary> This readonly struct is a simple shortcut to add global drop rules to <see cref="ItemDropDatabase"/>'s methods. </summary>
	public readonly struct GlobalLoot
	{
		private readonly ItemDropDatabase itemDropDatabase;

		public GlobalLoot(ItemDropDatabase itemDropDatabase) {
			this.itemDropDatabase = itemDropDatabase;
		}

		public IItemDropRule Add(IItemDropRule entry) => itemDropDatabase.RegisterToGlobal(entry);
	}
}
