using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.ItemRarity)]
#endif
partial class ItemRarityID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(ItemRarityID), typeof(int));
}
