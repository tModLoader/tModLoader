using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace ExampleMod.Common
{
	// This example shows a Camera Modifier that pans the camera to a chosen point.
	// CameraModifierShowcase.cs shows how this can be used
	// Also see how MinionBossBody uses an existing ICameraModifier implementation (PunchCameraModifier) to do a simple screenshake
	public class ExampleCameraModifier : ICameraModifier
	{
		private int framesToLast;
		private int framesElapsed;
		public Vector2 targetPosition;

		// This makes sure that other modifiers of the same identity don't run at the same time
		public string UniqueIdentity { get; private set; }
		public bool Finished { get; private set; }

		public ExampleCameraModifier(Vector2 position, int frames, string uniqueIdentity = null) {
			targetPosition = position - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
			framesToLast = frames;
			UniqueIdentity = uniqueIdentity;
		}

		public void Update(ref CameraInfo cameraInfo) {
			// Smoothly pans the camera from the start position to the desired position and then back:

			// We will use progress to determine where the camera should be with respect to how much time has passed.
			float progress = Utils.GetLerpValue(0, framesToLast, framesElapsed); // Equivalent to "(float)framesElapsed / framesToLast"

			// There are many approaches to interpolating between 2 values, such as using any of these methods:
			// MathF.Sin, MathHelper.Lerp, MathHelper.SmoothStep, Utils.MultiLerp, Utils.Turn01ToCyclic010
			// Each of these will result in different movement behaviors, such as how they accelerate.
			// In this example, however, we will use the Remap method and a switch expression to implement a linear interpolation split into 3 separate phases/segments
			// For the fist 50% of the animation time the value will ramp from 0 to 1,
			// then hold at 1 for 30% of the time,
			// then quickly travel back to 0 in the last 20% of the animation time
			float lerpAmount = progress switch {
				< 0.5f => Utils.Remap(progress, 0, 0.5f, 0, 1),
				> 0.8f => Utils.Remap(progress, 0.8f, 1f, 1, 0),
				_ => 1, // progress is between 0.5 and 0.8
			};

			cameraInfo.CameraPosition = Vector2.Lerp(cameraInfo.CameraPosition, targetPosition, lerpAmount);

			// Pauses the effect if the game is tabbed out or paused
			if (!Main.gameInactive && !Main.gamePaused) {
				framesElapsed++;
			}

			if (framesElapsed >= framesToLast) {
				Finished = true;
			}
		}
	}
}