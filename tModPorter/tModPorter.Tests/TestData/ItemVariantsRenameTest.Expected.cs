using Terraria.GameContent.Items;

public class ItemVariantsRenameTest
{
	void Method()
	{
		bool everything = ItemVariants.MechdusaWorld;
		bool qualified = Terraria.GameContent.Items.ItemVariants.MechdusaWorld;
		bool globalQualified = global::Terraria.GameContent.Items.ItemVariants.MechdusaWorld;
	}
}
