using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.RecipeGroup)]
#endif
partial class RecipeGroupID
{
	public static readonly IdDictionary Search = IdDictionary.Create<RecipeGroupID, int>();
}
