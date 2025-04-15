using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.CustomEquipmentSlot
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Common/CustomEquipmentSlot/README.md

	/// <summary>
	/// EarringsGlobalItem applies earring equipment slot IDs and the corresponding dye/shader.
	/// </summary>
	public class EarringsGlobalItem : GlobalItem
	{
		public override void UpdateVisibleAccessory(Item item, Player player, bool vanity, int itemSlot, bool hideVisual) {
			// UpdateVisibleAccessory is called even when not visible, so we need this check
			if (hideVisual) {
				return;
			}

			if (EarringsLoader.earringItemToTexture.ContainsKey(item.type)) {
				player.GetModPlayer<EarringsPlayer>().earring = item.type;
			}
		}

		public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual) {
			// UpdateItemDye is called even when not visible to allow for some advanced usages, so we need this check
			if (hideVisual) {
				return;
			}

			if (EarringsLoader.earringItemToTexture.ContainsKey(item.type)) { 
				player.GetModPlayer<EarringsPlayer>().earringShader = dye;
			}
		}
	}
}
