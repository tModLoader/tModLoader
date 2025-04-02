using System;
using Terraria.Graphics.CameraModifiers;
using Terraria;
using Microsoft.Xna.Framework;

namespace ExampleMod.Common
{
	// This example shows a Camera Modifier that pans the camera to a chosen point.
	// CameraModifierShowcase.cs shows how this can be used
	public class ExampleCameraModifier : ICameraModifier
	{
		private int _framesToLast;
		private int _framesLasted;
		public Vector2 _position;

		// This makes sure that other modifiers of the same identity don't run at the same time
		public string UniqueIdentity { get; private set; }
		public bool Finished { get; private set; }
		public ExampleCameraModifier(Vector2 position, int frames, string uniqueIdentity = null) {
			_position = position - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			_framesToLast = frames;
			UniqueIdentity = uniqueIdentity;
		}
		public void Update(ref CameraInfo cameraInfo) {
			float lerpT = MathHelper.Clamp(MathF.Sin(MathHelper.Pi * Utils.GetLerpValue(0, _framesToLast, _framesLasted)) * 2, 0, 1);

			// Smoothly pans the camera from the start position to the desired position, and back
			cameraInfo.CameraPosition = Vector2.Lerp(cameraInfo.CameraPosition, _position, lerpT);

			// Pauses the effect if the game is tabbed out or paused
			if (!Main.gameInactive && !Main.gamePaused)
				_framesLasted++;

			if (_framesLasted >= _framesToLast)
				Finished = true;
		}
	}
}