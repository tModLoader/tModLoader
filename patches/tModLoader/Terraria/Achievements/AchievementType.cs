namespace Terraria.Achievements
{
	/// <summary>
	///		Achievement saving type.
	/// </summary>
	public enum AchievementType
	{
		/// <summary>
		///		Globally-saved, not attached to a player.
		/// </summary>
		Global,

		/// <summary>
		///		Saved to the player, allows for an achievement to be achieved on different players.
		/// </summary>
		Player
	}
}