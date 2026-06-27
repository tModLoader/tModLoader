using Terraria.ModLoader;

namespace Terraria;
public partial class WorldItem
{
	public bool master => inner.master;

	public ModItem ModItem => inner.ModItem;
}
