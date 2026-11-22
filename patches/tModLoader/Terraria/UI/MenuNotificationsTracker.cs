using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;

namespace Terraria.UI;

// Mostly duplicated from InGameNotificationsTracker
public class MenuNotificationsTracker
{
	private static List<IInGameNotification> _notifications = new List<IInGameNotification>();

	/// <summary>
	/// Adds a menu notification to the tracker.
	/// </summary>
	/// <param name="notification">The notification to add.</param>
	public static void AddNotification(IInGameNotification notification)
	{
		_notifications.Add(notification);
	}

	public static void DrawInMenu(SpriteBatch sb)
	{
		float num = Main.screenHeight - 40;
		if (PlayerInput.UsingGamepad)
			num -= 25f;

		Vector2 positionAnchorBottom = new Vector2(Main.screenWidth - 40, num);
		foreach (IInGameNotification notification in _notifications) {
			notification.DrawInGame(sb, positionAnchorBottom);
			notification.PushAnchor(ref positionAnchorBottom);
			if (positionAnchorBottom.Y < -100f)
				break;
		}
	}

	public static void Clear()
	{
		_notifications.Clear();
	}

	public static void Update()
	{
		for (int i = 0; i < _notifications.Count; i++) {
			_notifications[i].Update();
			if (_notifications[i].ShouldBeRemoved) {
				_notifications.Remove(_notifications[i]);
				i--;
			}
		}
	}
}
