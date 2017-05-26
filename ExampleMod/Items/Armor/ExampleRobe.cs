using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	class ExampleRobe : ModItem
	{
		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 14;
			item.rare = 1;
			item.vanity = true;
		}

		public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
		{
			robes = true;
			// The equipSlot is added in ExampleMod.cs --> Load hook
			equipSlot = mod.GetEquipSlot("ExampleRobe_Legs", EquipType.Legs);
		}
		
		public override void DrawHands(ref bool drawHands, ref bool drawArms)
		{
			drawHands = true;
		}
	}
}
