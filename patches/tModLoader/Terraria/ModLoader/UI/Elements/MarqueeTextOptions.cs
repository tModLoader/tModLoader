using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Elements;

public struct MarqueeTextOptions()
{
	/// <summary>
	/// Whether overflowing text should marquee.
	/// <br/><br/> Note that text that does not overflow will not marquee.
	/// </summary>
	public bool IsScrolling { get; set; } = true;
	/// <summary>
	/// The number of pixels scrolled every frame while the text is scrolling.
	/// </summary>
	public float ScrollSpeed { get; set; } = 1.5f;
	/// <summary>
	/// The number of frames waited before changing the scroll direction when the start or end of the text is scrolled to.
	/// </summary>
	public int ScrollDelay { get; set; } = 30;
	/// <summary>
	/// Whether <see cref="Scroll"/> should be set to align the text to the start of the text when the text isn't scrolling.
	/// </summary>
	public bool AlignToStartWhenNotScrolling = true;
	/// <summary>
	/// The horizontal padding added to the clipping rectangle for the text.
	/// <br/><br/> Defaults to 2f because text often has borders that get clipped otherwise.
	/// </summary>
	public float ClippingXPadding { get; set; } = 2f;
	/// <summary>
	/// The vertical padding added to the clipping rectangle for the text.
	/// <br/><br/> Defaults to 1000f because otherwise descenders can get unintentionally clipped.
	/// </summary>
	public float ClippingYPadding { get; set; } = 1000f;

	public float Scroll { get; private set; } = 0;
	public readonly Vector2 TextOffset => -Scroll * Vector2.UnitX;

	private int scrollTimer = 0;
	private int scrollDirection = 1;

	public void Update(CalculatedStyle dims, Vector2 textSize, float textAlignX)
	{
		if (!IsScrolling || textSize.X < dims.Width) {
			ResetScroll(dims, textSize, textAlignX);
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

	public void ResetScroll(CalculatedStyle dims, Vector2 textSize, float textAlignX)
	{
		Scroll = 0;
		scrollTimer = 0;
		scrollDirection = 1;

		if (AlignToStartWhenNotScrolling && textSize.X >= dims.Width) {
			Scroll = GetStartAlignment(textSize, dims.Width, textAlignX);
		}
	}

	public float GetStartAlignment(Vector2 textSize, float innerWidth, float textAlignX)
	{
		return -((textSize.X * textAlignX) - (innerWidth * textAlignX));
	}

	public void SetScrollDelay(int scrollDelay)
	{
		scrollTimer = scrollDelay;
	}

	// Below 2 methods are modified from UIElement.Draw()

	public void BeginDraw(SpriteBatch spriteBatch, UIElement element, out RasterizerState rasterizerState, out Rectangle scissorRectangle)
	{
		rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
		scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;

		spriteBatch.End();

		var dims = element.GetInnerDimensions();
		dims.X -= ClippingXPadding;
		dims.Y -= ClippingYPadding;
		dims.Width += ClippingXPadding * 2;
		dims.Height += ClippingYPadding * 2;

		spriteBatch.GraphicsDevice.RasterizerState = UIElement.OverflowHiddenRasterizerState;
		spriteBatch.GraphicsDevice.ScissorRectangle = UIElement.CalculateClippingRectangle(dims, spriteBatch);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, UIElement.OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);
	}

	public void EndDraw(SpriteBatch spriteBatch, UIElement element, RasterizerState rasterizerState, Rectangle scissorRectangle)
	{
		spriteBatch.End();
		spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;
		spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);
	}
}
