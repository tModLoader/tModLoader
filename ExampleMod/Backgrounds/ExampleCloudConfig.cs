using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.Config;

namespace ExampleMod
{
	class ExampleCloudConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;


		[Label("Click this to enable custom wind speeds.")]
		public bool UseCustomWindSpeed;


		[Range(-1f, 1f)]
		[Increment(0.1f)]
		[Label("Use this to change the directiono the clouds in the sky are moving and the rate at which they move.")]
		public static float SetWindSpeed;

		[Label("Enable Clouds to take on custom color values below")]
		public static bool UseCustomCloudColor;

		[Range(0f, 255f)]
		[Increment(1f)]
		[Label("Shift the red value of the clouds.")]
		public static byte RedHueValue;

		[Range(0f, 255f)]
		[Increment(1f)]
		[Label("Shift the green value of the clouds.")]
		public static byte GreenHueValue;

		[Range(0f, 255f)]
		[Increment(1f)]
		[Label("Shift the blue value of the clouds.")]
		public static byte BlueHueValue;

		[Range(0f, 255f)]
		[Increment(1f)]
		[Label("Shift the transparency of the clouds.")]
		public static byte CloudOpacity;

		public static void SetAllCloudColor()
		{
			if (UseCustomCloudColor) {
				for (int Index = 0; Index < Main.cloudLimit; Index++) {
					Main.cloud[Index].cloudColor(new Color(RedHueValue, GreenHueValue, BlueHueValue, CloudOpacity));
				}
			}
		}
	}

}
