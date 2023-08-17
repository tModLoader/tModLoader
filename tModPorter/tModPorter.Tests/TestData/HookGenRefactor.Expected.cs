using Terraria;
using Terraria.GameContent.Creative;
using Terraria.IO;

public class HookGenRefactor
{
	void Load()
	{
		Terraria.IL_Player.beeType += (il) => { };
		Terraria.GameContent.Creative.On_CreativeUI.SacrificeItem_refItem_refInt32_bool += OnSacrificeItem;
		On_PlayerFileData.SetAsActive += (On_PlayerFileData.orig_SetAsActive orig, Terraria.IO.PlayerFileData self) => { };
		On_PlayerFileData.SetAsActive += new On_PlayerFileData.hook_SetAsActive(SetAsActive);
	}

	private CreativeUI.ItemSacrificeResult OnSacrificeItem(Terraria.GameContent.Creative.On_CreativeUI.orig_SacrificeItem_refItem_refInt32_bool orig,
				ref Item item, out int amountWeSacrificed, bool returnRemainderToPlayer)
	{
		return orig(ref item, out amountWeSacrificed, returnRemainderToPlayer);
	}

	private void SetAsActive(On_PlayerFileData.orig_SetAsActive orig, Terraria.IO.PlayerFileData self) { }
}
