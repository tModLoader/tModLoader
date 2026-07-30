using Terraria;
using Terraria.ID;

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

		Player player = Main.LocalPlayer;
		player.GetItem(0, item, GetItemSettings.PickupItemFromWorld);
		player.GetItem(Main.myPlayer, item, GetItemSettings.PickupItemFromWorld);
		player.GetItem(plr: 0, newItem: item, settings: GetItemSettings.PickupItemFromWorld);

		player.AddBuff(BuffID.OnFire, 10, quiet: false, foodHack: true);
		player.AddBuff(BuffID.OnFire, 20, foodHack: false, quiet: true);
		player.AddBuff(BuffID.OnFire, 30, false, foodHack: false);
		player.AddBuff(BuffID.OnFire, 40, quiet: false);
		player.AddBuff(BuffID.OnFire, 50, foodHack: false);
		player.AddBuff(BuffID.OnFire, 60, true, false);
		player.AddBuff(BuffID.OnFire, 70, true);
	}
}