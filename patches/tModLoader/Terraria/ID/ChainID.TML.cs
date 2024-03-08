using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.Chain)]
#endif
partial class ChainID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(ChainID), typeof(short));
}
