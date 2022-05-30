using Terraria;
using Terraria.ModLoader;

public class GlobalItemTest : GlobalItem
{
	public override bool NewPreReforge(Item item) { return false; /* comment */ }
}