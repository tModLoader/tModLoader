using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.Lang)]
#endif
public static class LangID
{
	public const int English = 1;
	public const int German = 2;
	public const int Italian = 3;
	public const int French = 4;
	public const int Spanish = 5;
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(LangID), typeof(int));
}
