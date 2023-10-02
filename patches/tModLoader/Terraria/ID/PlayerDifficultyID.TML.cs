using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.PlayerDifficulty)]
#endif
partial class PlayerDifficultyID
{
	public static readonly IdDictionary Search = IdDictionary.Create<PlayerDifficultyID, byte>();
}
