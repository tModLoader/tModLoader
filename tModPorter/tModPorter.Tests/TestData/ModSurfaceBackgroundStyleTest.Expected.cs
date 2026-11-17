using System;
using System.Collections.Generic;
using System.Text;

namespace tModPorter.Tests.TestData;

public class ModSurfaceBackgroundStyleTest : ModSurfaceBackgroundStyle
{
	public override void ModifyFarFades(float[] fades, float transitionSpeed)
	{
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

	public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float verticalParallax, ref float verticalOffset)
	{
		return -1;
	}
}
