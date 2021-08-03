namespace Terraria.Achievements
{
	partial interface IAchievementTracker
	{
		string GetProgressText();

		float GetProgress();
	}
}