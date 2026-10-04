using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameInput;

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

	private static List<IInGameNotification> menuNotifications = new List<IInGameNotification>();
	internal static void AddMenuNotification(IInGameNotification notification)
	{
		menuNotifications.Add(notification);
	}

	public static void DrawInMenu(SpriteBatch sb)
	{
		float num = Main.screenHeight - 40;
		if (PlayerInput.UsingGamepad)
			num -= 25f;

		Vector2 positionAnchorBottom = new Vector2(Main.screenWidth - 40, num);
		foreach (IInGameNotification notification in menuNotifications) {
			notification.DrawInGame(sb, positionAnchorBottom);
			notification.PushAnchor(ref positionAnchorBottom);
			if (positionAnchorBottom.Y < -100f)
				break;
		}
	}
}