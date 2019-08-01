using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	class Vector2Element : ConfigElement
	{
		class Vector2Object
		{
			internal Vector2 current;
			PropertyFieldWrapper memberInfo;
			object item;
			IList<Vector2> array;
			int index;

			[Label("X")]
			public float X {
				get { return current.X; }
				set {
					current.X = value;
					Update();
				}
			}
			[Label("Y")]
			public float Y {
				get { return current.Y; }
				set {
					current.Y = value;
					Update();
				}
			}
			private void Update() {
				if (array == null)
					memberInfo.SetValue(item, current);
				else
					array[index] = current;
				Interface.modConfig.SetPendingChanges();
			}
			public Vector2Object(PropertyFieldWrapper memberInfo, object item) {
				this.item = item;
				this.memberInfo = memberInfo;
				current = (Vector2)memberInfo.GetValue(item);
			}
			public Vector2Object(IList<Vector2> array, int index) {
				current = array[index];
				this.array = array;
				this.index = index;
			}
		}

		int height;
		Vector2Object c;
		float min = 0;
		float max = 1;
		float increment = 0.01f;
		public IList<Vector2> vector2List;

		public override void OnBind() {
			base.OnBind();
			vector2List = (IList<Vector2>)list;
			if (vector2List != null) {
				drawLabel = false;
				height = 30;
				c = new Vector2Object(vector2List, index);
			}
			else {
				height = 30;
				c = new Vector2Object(memberInfo, item);
			}

			if (rangeAttribute != null && rangeAttribute.min is float && rangeAttribute.max is float) {
				max = (float)rangeAttribute.max;
				min = (float)rangeAttribute.min;
			}
			if (incrementAttribute != null && incrementAttribute.increment is float) {
				increment = (float)incrementAttribute.increment;
			}

			int order = 0;
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c)) {
				var wrapped = UIModConfig.WrapIt(this, ref height, variable, c, order++);

				// Can X and Y inherit range and increment automatically? Pass in "fake PropertyFieldWrapper" to achieve? Real one desired for label.
				if (wrapped.Item2 is FloatElement floatElement) {
					floatElement.min = min;
					floatElement.max = max;
					floatElement.increment = increment;
					floatElement.drawTicks = Attribute.IsDefined(memberInfo.MemberInfo, typeof(DrawTicksAttribute));
				}

				if (vector2List != null) {
					wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
			}
		}

		// Draw axis? ticks?
		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);

			CalculatedStyle dimensions = base.GetInnerDimensions();
			Rectangle rectangle = dimensions.ToRectangle();
			rectangle = new Rectangle(rectangle.Right - 30, rectangle.Y, 30, 30);
			spriteBatch.Draw(Main.magicPixel, rectangle, Color.AliceBlue);

			float x = (c.X - min) / (max - min);
			float y = (c.Y - min) / (max - min);

			var position = rectangle.TopLeft();
			position.X += x * rectangle.Width;
			position.Y += y * rectangle.Height;
			var blipRectangle = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);

			if (x >= 0 && x <= 1 && y >= 0 && y <= 1)
				spriteBatch.Draw(Main.magicPixel, blipRectangle, Color.Black);

			if (IsMouseHovering && rectangle.Contains((Main.MouseScreen).ToPoint()) && Main.mouseLeft) {
				float newPerc = (Main.mouseX - rectangle.X) / (float)rectangle.Width;
				newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
				c.X = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;

				newPerc = (Main.mouseY - rectangle.Y) / (float)rectangle.Height;
				newPerc = Utils.Clamp<float>(newPerc, 0f, 1f);
				c.Y = (float)Math.Round((newPerc * (max - min) + min) * (1 / increment)) * increment;
			}
		}

		internal float GetHeight() {
			return height;
		}
	}
}