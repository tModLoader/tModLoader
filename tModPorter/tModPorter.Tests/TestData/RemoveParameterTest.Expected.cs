using Terraria;
using Terraria.ID;

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

		Player player = Main.LocalPlayer;
		player.GetItem(item, GetItemSettings.PickupItemFromWorld);
		player.GetItem(item, GetItemSettings.PickupItemFromWorld);
		player.GetItem(newItem: item, settings: GetItemSettings.PickupItemFromWorld);

		player.AddBuff(BuffID.OnFire, 10);
		player.AddBuff(BuffID.OnFire, 20);
		player.AddBuff(BuffID.OnFire, 30);
		player.AddBuff(BuffID.OnFire, 40);
		player.AddBuff(BuffID.OnFire, 50);
		player.AddBuff(BuffID.OnFire, 60);
		player.AddBuff(BuffID.OnFire, 70);
	}
}
