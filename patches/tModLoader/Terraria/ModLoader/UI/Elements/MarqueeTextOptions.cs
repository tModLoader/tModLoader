using Microsoft.Xna.Framework;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Elements;

public struct MarqueeTextOptions()
{
	// TODO: document
	public bool IsScrolling { get; set; } = true;
	public float ScrollSpeed { get; set; } = 1.5f;
	public int ScrollDelay { get; set; } = 30;
	public bool AlignToStartWhenNotScrolling = false; // TODO: what should this default to?
	public float ClippingXPadding { get; set; } = 2f; // Many pieces of text have borders that get clipped
	public float ClippingYPadding { get; set; } = 0f;

	public float Scroll { get; private set; } = 0;

	private int scrollTimer = 0;
	private int scrollDirection = 1;

	public void Update(CalculatedStyle dims, Vector2 textSize, float textAlignX)
	{
		bool textIsClipping = textSize.X >= dims.Width;
		if (!IsScrolling || !textIsClipping) {
			ResetScroll(dims, textSize, textAlignX, textIsClipping);
			return;
		}

		scrollTimer--;
		if (scrollTimer > 0) {
			return;
		}

		// Each half of the text separated by the alignment
		float left =
			(textSize.X * textAlignX) -
			(dims.Width * textAlignX);

		float right =
			(textSize.X * (1f - textAlignX)) -
			(dims.Width * (1f - textAlignX));

		Scroll += ScrollSpeed * scrollDirection;

		if (Scroll >= right) {
			Scroll = right;
			scrollTimer = ScrollDelay;
			scrollDirection = -1;
		}
		else if (Scroll <= -left) {
			Scroll = -left;
			scrollTimer = ScrollDelay;
			scrollDirection = 1;
		}
	}


	public void ResetScroll(CalculatedStyle dims, Vector2 textSize, float textAlignX, bool textIsClipping)
	{
		Scroll = 0;
		scrollTimer = 0;
		scrollDirection = 1;

		if (AlignToStartWhenNotScrolling && textIsClipping) {
			Scroll = GetStartAlignment(textSize, dims.Width, textAlignX);
		}
	}

	// TODO: consider languages that read from right to left?
	public float GetStartAlignment(Vector2 textSize, float innerWidth, float textAlignX)
	{
		float align = 1f - textAlignX;
		return -((textSize.X * align) - (innerWidth * align));
	}

	public void SetScrollDelay(int scrollDelay)
	{
		scrollTimer = scrollDelay;
	}

	public void AdjustClippingRectangle(ref CalculatedStyle dims)
	{
		dims.X -= ClippingXPadding;
		dims.Y -= ClippingYPadding;
		dims.Width += ClippingXPadding * 2;
		dims.Height += ClippingYPadding * 2;
	}
}
