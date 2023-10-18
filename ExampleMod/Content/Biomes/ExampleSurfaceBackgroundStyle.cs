using Terraria.ModLoader;

namespace ExampleMod.Backgrounds
{
	public class ExampleSurfaceBackgroundStyle : ModSurfaceBackgroundStyle
	{
		// Use this to keep far Backgrounds like the mountains.
		public override void ModifyStyleFade(float[] fades, float transitionSpeed) {
			for (int i = 0; i < fades.Length; i++) {
				if (i == Slot) {
					fades[i] += transitionSpeed;
					if (fades[i] > 1f) {
						fades[i] = 1f;
					}
				}
				else {
					fades[i] -= transitionSpeed;
					if (fades[i] < 0f) {
						fades[i] = 0f;
					}
				}
			}
		}

		public override int ChooseFarTexture(in BackgroundLayerParams layerParams) {
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/ExampleBiomeSurfaceFar");
		}

		private static int SurfaceFrameCounter;
		private static int SurfaceFrame;

		public override int ChooseMiddleTexture(in BackgroundLayerParams layerParams) {
			if (++SurfaceFrameCounter > 12) {
				SurfaceFrame = (SurfaceFrame + 1) % 4;
				SurfaceFrameCounter = 0;
			}
			switch (SurfaceFrame) {
				case 0:
					return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/ExampleBiomeSurfaceMid0");
				case 1:
					return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/ExampleBiomeSurfaceMid1");
				case 2:
					return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/ExampleBiomeSurfaceMid2");
				case 3:
					return BackgroundTextureLoader.GetBackgroundSlot("ExampleMod/Assets/Textures/Backgrounds/ExampleBiomeSurfaceMid3"); // You can use the full path version of GetBackgroundSlot too
				default:
					return -1;
			}
		}

		public override int ChooseCloseFarTexture(in BackgroundLayerParams layerParams) {
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/ExampleBiomeSurfaceCloseFar");
		}

		public override int ChooseCloseMidTexture(in BackgroundLayerParams layerParams) {
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/ExampleBiomeSurfaceCloseMid");
		}

		public override int ChooseCloseTexture(in BackgroundLayerParams layerParams) {
			return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Textures/Backgrounds/ExampleBiomeSurfaceClose");
		}
	}
}