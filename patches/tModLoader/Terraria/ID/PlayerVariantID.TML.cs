using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.PlayerVariant)]
#endif
partial class PlayerVariantID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(PlayerVariantID), typeof(int));
}
