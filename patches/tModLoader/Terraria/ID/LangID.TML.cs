using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.Lang)]
#endif
partial class LangID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(LangID), typeof(int));
}
