using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.CustomVisualEquipType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Common/CustomVisualEquipType/README.md

	/// <summary>
	/// EarringsGlobalItem applies the custom earring EquipType values of the accessory items and the corresponding dye/shader items to the player.
	/// </summary>
	public class EarringsGlobalItem : GlobalItem
	{
		public override void UpdateVisibleAccessory(Item item, Player player, bool hideVisual) {
			// UpdateVisibleAccessory is called even when not visible to allow for some advanced usages, so we need this check
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
