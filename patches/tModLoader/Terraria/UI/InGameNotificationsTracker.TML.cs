namespace Terraria.UI;

partial class InGameNotificationsTracker
{
	/// <summary>
	/// Adds an in-game notification to the tracker.
	/// </summary>
	/// <param name="notification">The notification to add.</param>
	public static void AddNotification(IInGameNotification notification)
	{
		_notifications.Add(notification);
	}
}