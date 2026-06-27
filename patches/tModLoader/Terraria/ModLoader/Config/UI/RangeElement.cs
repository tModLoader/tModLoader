using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

public abstract class PrimitiveRangeElement<T> : RangeElement where T : IComparable<T>
{
	public T Min { get; set; }
	public T Max { get; set; }
	public T Increment { get; set; }
	public IList<T> TList { get; set; }

	public override void OnBind()
	{
		base.OnBind();

		TList = (IList<T>)List;
		TextDisplayFunction = () => MemberInfo.Name + ": " + GetValue();

		if (TList != null) {
			TextDisplayFunction = () => Index + 1 + ": " + TList[Index];
		}

		if (Label != null) { // Problem with Lists using ModConfig Label.
			TextDisplayFunction = () => Label + ": " + GetValue();
		}

		if (RangeAttribute != null && RangeAttribute.Min is T && RangeAttribute.Max is T) {
			Min = (T)RangeAttribute.Min;
			Max = (T)RangeAttribute.Max;
		}
		if (IncrementAttribute != null && IncrementAttribute.Increment is T) {
			Increment = (T)IncrementAttribute.Increment;
		}
	}

	protected virtual T GetValue() => (T)GetObject();

	protected virtual void SetValue(object value)
	{
		if (value is T t)
			SetObject(Utils.Clamp(t, Min, Max));
	}
}

public abstract class RangeElement : ConfigElement
{
	private static RangeElement rightLock;
	private static RangeElement rightHover;

	protected Color SliderColor { get; set; } = Color.White;
	protected Utils.ColorLerpMethod ColorMethod { get; set; }

	internal bool DrawTicks { get; set; }

	public abstract int NumberTicks { get; }
	public abstract float TickIncrement { get; }

	protected abstract float Proportion { get; set; }

	public RangeElement()
	{
		ColorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.Black, SliderColor, percent));
	}

	public override void OnBind()
	{
		base.OnBind();

		DrawTicks = Attribute.IsDefined(MemberInfo.MemberInfo, typeof(DrawTicksAttribute));
		SliderColor = ConfigManager.GetCustomAttributeFromMemberThenMemberType<SliderColorAttribute>(MemberInfo, Item, List)?.Color ?? Color.White;
	}

	public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null)
	{
		perc = Utils.Clamp(perc, -.05f, 1.05f);

		if (colorMethod == null)
			colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);

		Texture2D colorBarTexture = TextureAssets.ColorBar.Value;
		Vector2 vector = new Vector2((float)colorBarTexture.Width, (float)colorBarTexture.Height) * scale;
		IngameOptions.valuePosition.X -= (float)((int)vector.X);
		Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2, (int)vector.X, (int)vector.Y);
		Rectangle destinationRectangle = rectangle;
		int num = 167;
		float num2 = rectangle.X + 5f * scale;
		float num3 = rectangle.Y + 4f * scale;

		if (DrawTicks) {
			int numTicks = NumberTicks;
			if (numTicks > 1) {
				for (int tick = 0; tick < numTicks; tick++) {
					float percent = tick * TickIncrement;

					if (percent <= 1f)
						sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
				}
			}
		}

		sb.Draw(colorBarTexture, rectangle, Color.White);

		for (float num4 = 0f; num4 < (float)num; num4 += 1f) {
			float percent = num4 / (float)num;
			sb.Draw(TextureAssets.ColorBlip.Value, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		rectangle.Inflate((int)(-5f * scale), 2);

		//rectangle.X = (int)num2;
		//rectangle.Y = (int)num3;

		bool flag = IsMouseHovering && rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

		if (lockState == 2) {
			flag = false;
		}

		if (flag || lockState == 1) {
			sb.Draw(TextureAssets.ColorHighlight.Value, destinationRectangle, Main.OurFavoriteColor);
		}

		var colorSlider = TextureAssets.ColorSlider.Value;

		sb.Draw(colorSlider, new Vector2(num2 + 167f * scale * perc, num3 + 4f * scale), null, Color.White, 0f, colorSlider.Size() * 0.5f, scale, SpriteEffects.None, 0f);

		if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width) {
			IngameOptions.inBar = flag;
			return (Main.mouseX - rectangle.X) / (float)rectangle.Width;
		}

		IngameOptions.inBar = false;

		if (rectangle.X >= Main.mouseX) {
			return 0f;
		}

		return 1f;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		float num = 6f;
		int num2 = 0;

		rightHover = null;

		if (!Main.mouseLeft) {
			rightLock = null;
		}

		if (rightLock == this) {
			num2 = 1;
		}
		else if (rightLock != null) {
			num2 = 2;
		}

		CalculatedStyle dimensions = GetDimensions();
		float num3 = dimensions.Width + 1f;
		Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
		bool flag2 = IsMouseHovering;

		if (num2 == 1) {
			flag2 = true;
		}

		if (num2 == 2) {
			flag2 = false;
		}

		Vector2 vector2 = vector;
		vector2.X += 8f;
		vector2.Y += 2f + num;
		vector2.X -= 17f;
		//TextureAssets.ColorBar.Value.Frame(1, 1, 0, 0);
		vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
		IngameOptions.valuePosition = vector2;
		float obj = DrawValueBar(spriteBatch, 1f, Proportion, num2, ColorMethod);

		if (IngameOptions.inBar || rightLock == this) {
			rightHover = this;
			if (PlayerInput.Triggers.Current.MouseLeft && rightLock == this) {
				Proportion = obj;
			}
		}

		if (rightHover != null && rightLock == null && PlayerInput.Triggers.JustPressed.MouseLeft) {
			rightLock = rightHover;
		}
	}
}
