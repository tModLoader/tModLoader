using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;

namespace Terraria.UI;

/// <summary>
/// Manages in-game notifications, such as achievements and join requests. Custom notifications can be added via <see cref="AddNotification(IInGameNotification)"/>. <see cref="MenuNotificationsTracker"/> is a similar class for menu notifications.
/// </summary>
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
