using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.Prefix)]
#endif
partial class PrefixID
{
	public static readonly IdDictionary Search = IdDictionary.Create<PrefixID, int>();
}
