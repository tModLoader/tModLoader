namespace Terraria.UI;

partial class InGameNotificationsTracker
{
	// /// <summary>
	// ///	The list of all in-game notifications being tracked.
	// /// </summary>
	// /// <remarks>
	// /// You can clear this collection with either <see cref="Clear"/> or <see cref="List{T}.Clear"/>, this property has no setter and thus does not allow you to directly change the collection instance.
	// /// </remarks>
	// public static List<IInGameNotification> Notifications => _notifications;

	/// <summary>
	/// Adds an in-game notification to the tracker.
	/// </summary>
	/// <param name="notification">The notification to add.</param>
	public static void AddNotification(IInGameNotification notification)
	{
		_notifications.Add(notification);
	}
}