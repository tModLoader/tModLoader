using ReLogic.Reflection;

namespace Terraria.ID;

partial class AchievementHelperID
{
#if TMLCODEASSIST
	[tModCodeAssist.IDType.Sets.AssociatedName(Terraria.ModLoader.Annotations.IDTypeAttribute.AchievementHelper_Events)]
#endif
	partial class Events
	{
		public static readonly IdDictionary Search = IdDictionary.Create<Events, int>();
	}

#if TMLCODEASSIST
	[tModCodeAssist.IDType.Sets.AssociatedName(Terraria.ModLoader.Annotations.IDTypeAttribute.AchievementHelper_Special)]
#endif
	partial class Special
	{
		public static readonly IdDictionary Search = IdDictionary.Create<Special, int>();
	}
}
