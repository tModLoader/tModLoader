using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class Vector2Element : ConfigElement
{
	private class Vector2Object
	{
		private readonly PropertyFieldWrapper memberInfo;
		private readonly object item;
		private readonly IList<Vector2> array;
		private readonly int index;

		private Vector2 current;

		[LabelKey("$Config.Vector2.X.Label")]
		public float X {
			get => current.X;
			set {
				current.X = value;
				Update();
			}
		}

		[LabelKey("$Config.Vector2.Y.Label")]
		public float Y {
			get => current.Y;
			set {
				current.Y = value;
				Update();
			}
		}

		private void Update()
		{
			if (array == null)
				memberInfo.SetValue(item, current);
			else
				array[index] = current;

			Interface.modConfig.SetPendingChanges();
		}

		public Vector2Object(PropertyFieldWrapper memberInfo, object item)
		{
			this.item = item;
			this.memberInfo = memberInfo;
			current = (Vector2)memberInfo.GetValue(item);
		}

		public Vector2Object(IList<Vector2> array, int index)
		{
			current = array[index];
			this.array = array;
			this.index = index;
		}
	}

	private int height;
	private Vector2Object c;
	private float min = 0;
	private float max = 1;
	private float increment = 0.01f;

	public IList<Vector2> Vector2List { get; set; }

	public override void OnBind()
	{
		base.OnBind();

		Vector2List = (IList<Vector2>)List;

		if (Vector2List != null) {
			DrawLabel = false;
			height = 30;
			c = new Vector2Object(Vector2List, Index);
		}
		else {
			height = 30;
			c = new Vector2Object(MemberInfo, Item);
		}

		if (RangeAttribute != null && RangeAttribute.Min is float && RangeAttribute.Max is float) {
			max = (float)RangeAttribute.Max;
			min = (float)RangeAttribute.Min;
		}

		if (IncrementAttribute != null && IncrementAttribute.Increment is float) {
			increment = (float)IncrementAttribute.Increment;
		}

		int order = 0;
		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c)) {
			var wrapped = UIModConfig.WrapIt(this, ref height, variable, c, order++);

			// Can X and Y inherit range and increment automatically? Pass in "fake PropertyFieldWrapper" to achieve? Real one desired for label.
			if (wrapped.Item2 is FloatElement floatElement) {
				floatElement.Min = min;
				floatElement.Max = max;
				floatElement.Increment = increment;
				floatElement.DrawTicks = Attribute.IsDefined(MemberInfo.MemberInfo, typeof(DrawTicksAttribute));
			}

			if (Vector2List != null) {
				wrapped.Item1.Left.Pixels -= 20;
				wrapped.Item1.Width.Pixels += 20;
			}
		}
	}

	// Draw axis? ticks?
	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		CalculatedStyle dimensions = base.GetInnerDimensions();
		Rectangle rectangle = dimensions.ToRectangle();
		rectangle = new Rectangle(rectangle.Right - 30, rectangle.Y, 30, 30);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.AliceBlue);

		float x = (c.X - min) / (max - min);
		float y = (c.Y - min) / (max - min);

		var position = rectangle.TopLeft();
		position.X += x * rectangle.Width;
		position.Y += y * rectangle.Height;
		var blipRectangle = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);

		if (x >= 0 && x <= 1 && y >= 0 && y <= 1)
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, blipRectangle, Color.Black);

		if (IsMouseHovering && rectangle.Contains((Main.MouseScreen).ToPoint()) && Main.mouseLeft) {
			float newPerc = (Main.mouseX - rectangle.X) / (float)rectangle.Width;
			newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
			c.X = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;

			newPerc = (Main.mouseY - rectangle.Y) / (float)rectangle.Height;
			newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
			c.Y = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;
		}
	}

	internal float GetHeight()
	{
		return height;
	}
}