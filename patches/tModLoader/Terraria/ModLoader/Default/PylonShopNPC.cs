namespace Terraria.ModLoader.Default
{
	/// <summary>
	/// This is a GlobalNPC native to tML that handles adding Pylon items to NPC's shops, to save on patch size within vanilla.
	/// </summary>
	public sealed class PylonShopNPC : GlobalNPC
	{
		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			foreach (ModPylon pylon in PylonLoader.modPylons) {
				int? pylonReturn = pylon.IsPylonForSale(type, Main.LocalPlayer, Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421);
				if (pylonReturn.HasValue && nextSlot < shop.item.Length) {
					shop.item[nextSlot++].SetDefaults(pylonReturn.Value);
				}
				else if (pylonReturn.HasValue && nextSlot >= shop.item.Length) {
					Logging.tML.Warn($"Ran out of NPC shop space for {pylon.Mod.Name}'s pylon item {pylon.Name}");
				}
			}
		}
	}
}
