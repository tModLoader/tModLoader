using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.CustomEquipmentSlot
{
	/// <summary>
	/// EarringsGlobalItem manages registering and assigning earring equipment slot IDs.
	/// </summary>
	public class EarringsGlobalItem : GlobalItem
	{
		internal static int nextEarringSlot = 1; // 0 is empty slot.
		internal static readonly Dictionary<int, int> earringItemToSlot = new();
		internal static readonly Dictionary<int, Asset<Texture2D>> earringSlotToTexture = new(); // or array?

		public override bool InstancePerEntity => true;

		public int earringSlot = -1;

		public override void SetStaticDefaults() {
			// Add an earring equipment slot to a vanilla item.
			AddEarringTexture(ItemID.AnglerEarring, "ExampleMod/Common/CustomEquipmentSlot/AnglerEarring_Earrings");
		}

		public override void SetDefaults(Item entity) {
			if (entity.ModItem is ModItem modItem && modItem.GetType().GetAttribute<AutoloadEquip_EarringAttribute>() != null) {
				//EquipLoader.AddEquipTexture(Mod, $"{Texture}_{equip}", equip, this);
				// Issue, no suitable hook for this. Called multiple times.
				AddEarringTexture(entity.type, $"{modItem.Texture}_Earrings");
			}

			// Assign earringSlot to the new Item instance if it has a registered earring equipment slot ID. 
			if (earringItemToSlot.TryGetValue(entity.type, out int equipmentSlot)) {
				earringSlot = equipmentSlot;
			}
		}

		public override void UpdateVisibleAccessory(Item item, Player player, bool vanity, int itemSlot, bool hideVisual) {
			// UpdateVisibleAccessory is called even when not visible, so we need this check
			if (hideVisual) {
				return;
			}

			if (earringSlot > 0) {
				player.GetModPlayer<EarringsPlayer>().earring = earringSlot;
			}
		}

		public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual) {
			// UpdateItemDye is called even when not visible to allow for some advanced usages, so we need this check
			if (hideVisual) {
				return;
			}

			if (earringSlot > 0) {
				player.GetModPlayer<EarringsPlayer>().earringShader = dye;
			}
		}

		internal void AddEarringTexture(int type, string texture) {
			if (earringItemToSlot.ContainsKey(type))
				return; // Hack to not auto-register multiple times.

			var asset = ModContent.Request<Texture2D>(texture);
			earringItemToSlot[type] = nextEarringSlot;
			earringSlotToTexture[nextEarringSlot] = asset;
			nextEarringSlot++;
		}
	}
}
