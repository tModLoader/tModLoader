using Terraria.Achievements;

namespace ExampleMod.Content.Achievements.Conditions;

public class ExampleAchievementCondition : AchievementCondition
{
	// This needs to be declared to set a name for the achievement condition class to be found when searching to set it.
	public ExampleAchievementCondition() : base("EXAMPLE_BOOLEAN_CONDITION") {

	}
}