using Terraria;
using Terraria.GameContent.Creative;
using On.Terraria.IO;

public class HookGenRefactor
{
	void Load()
	{
		IL.Terraria.Player.beeType += (il) => { };
		On.Terraria.GameContent.Creative.CreativeUI.SacrificeItem_refItem_refInt32_bool += OnSacrificeItem;
		PlayerFileData.SetAsActive += (PlayerFileData.orig_SetAsActive orig, Terraria.IO.PlayerFileData self) => { };
		PlayerFileData.SetAsActive += new PlayerFileData.hook_SetAsActive(SetAsActive);
	}

	private CreativeUI.ItemSacrificeResult OnSacrificeItem(On.Terraria.GameContent.Creative.CreativeUI.orig_SacrificeItem_refItem_refInt32_bool orig,
				ref Item item, out int amountWeSacrificed, bool returnRemainderToPlayer)
	{
		return orig(ref item, out amountWeSacrificed, returnRemainderToPlayer);
	}

	private void SetAsActive(PlayerFileData.orig_SetAsActive orig, Terraria.IO.PlayerFileData self) { }
}
