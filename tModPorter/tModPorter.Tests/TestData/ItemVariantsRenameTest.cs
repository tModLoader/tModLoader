using Terraria.GameContent.Items;

public class ItemVariantsRenameTest
{
	void Method()
	{
		bool everything = ItemVariants.EverythingWorld;
		bool qualified = Terraria.GameContent.Items.ItemVariants.EverythingWorld;
		bool globalQualified = global::Terraria.GameContent.Items.ItemVariants.EverythingWorld;
	}
}
