using Terraria;
using Terraria.ModLoader;

public class GlobalItemTest : GlobalItem
{
	public override bool PreReforge(Item item) { return false; /* comment */ }
}