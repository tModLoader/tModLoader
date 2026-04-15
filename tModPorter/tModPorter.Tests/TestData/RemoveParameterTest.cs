using Terraria;

public class RemoveParameterTest
{
	void Method()
	{
		Item item = new Item();
		item.SetDefaults(1);
		item.SetDefaults(2, false);
		item.SetDefaults(3, true, null);
		item.SetDefaults(4, noMatCheck: false);
		item.SetDefaults(5, variant: null, noMatCheck: false);
		item.SetDefaults(6, Main.rand.NextBool() ? false : true);
		item.SetDefaults(7, variant: null, noMatCheck: Main.rand.NextBool() ? false : true);
	}
}