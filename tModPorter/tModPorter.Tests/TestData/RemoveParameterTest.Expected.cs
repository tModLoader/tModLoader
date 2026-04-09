using Terraria;

public class RemoveParameterTest
{
	void Method()
	{
		Item item = new Item();
		item.SetDefaults(1);
		item.SetDefaults(2);
		item.SetDefaults(3, null);
		item.SetDefaults(4);
		item.SetDefaults(5, variant: null);
		item.SetDefaults(6);
		item.SetDefaults(7, variant: null);
	}
}
