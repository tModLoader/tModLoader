using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.GameMode)]
#endif
partial class GameModeID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(GameModeID), typeof(short));
}
