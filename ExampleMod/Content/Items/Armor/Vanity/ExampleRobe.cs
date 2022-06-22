using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Content.Items.Armor.Vanity
{
	[AutoloadEquip(EquipType.Body)]
	internal class ExampleRobe : ModItem
	{
		public override void Load() {
			// The code below runs only if we're not loading on a server
			if (Main.netMode == NetmodeID.Server)
				return;

			// Add this so we can reference it in SetMatch
			EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
		}

		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 14;
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}

		public override void SetMatch(bool male, ref int equipSlot, ref bool robes) {
			robes = true;
			equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
		}
	}
}