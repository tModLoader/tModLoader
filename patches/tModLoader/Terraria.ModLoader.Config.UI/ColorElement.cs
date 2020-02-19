using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace Terraria.ModLoader.Config.UI
{
	class ColorElement : ConfigElement
	{
		class ColorObject
		{
			internal Color current;
			PropertyFieldWrapper memberInfo;
			object item;
			IList<Color> array;
			int index;

			[Label("Red")]
			public byte R
			{
				get { return current.R; }
				set
				{
					current.R = value;
					Update();
				}
			}
			[Label("Green")]
			public byte G
			{
				get { return current.G; }
				set
				{
					current.G = value;
					Update();
				}
			}
			[Label("Blue")]
			public byte B
			{
				get { return current.B; }
				set
				{
					current.B = value;
					Update();
				}
			}
			[Label("Hue")]
			public float Hue {
				get { return Main.rgbToHsl(current).X; }
				set {
					byte a = A;
					current = Main.hslToRgb(value, Saturation, Lightness);
					current.A = a;
					Update();
				}
			}
			[Label("Saturation")]
			public float Saturation {
				get { return Main.rgbToHsl(current).Y; }
				set {
					byte a = A;
					current = Main.hslToRgb(Hue, value, Lightness);
					current.A = a;
					Update();
				}
			}
			[Label("Lightness")]
			public float Lightness {
				get { return Main.rgbToHsl(current).Z; }
				set {
					byte a = A;
					current = Main.hslToRgb(Hue, Saturation, value);
					current.A = a;
					Update();
				}
			}
			[Label("Alpha")]
			public byte A
			{
				get { return current.A; }
				set
				{
					current.A = value;
					Update();
				}
			}
			private void Update()
			{
				if (array == null)
					memberInfo.SetValue(item, current);
				else
					array[index] = current;
			}
			public ColorObject(PropertyFieldWrapper memberInfo, object item)
			{
				this.item = item;
				this.memberInfo = memberInfo;
				current = (Color)memberInfo.GetValue(item);
			}
			public ColorObject(IList<Color> array, int index)
			{
				current = array[index];
				this.array = array;
				this.index = index;
			}
		}

		int height;
		ColorObject c;
		public IList<Color> colorList;

		public override void OnBind() {
			base.OnBind();
			colorList = (IList<Color>)list;
			if (colorList != null)
			{
				drawLabel = false;
				height = 30;
				c = new ColorObject(colorList, index);
			}
			else
			{
				height = 30;
				c = new ColorObject(memberInfo, item);
			}

			// TODO: Draw the sliders in the same manner as vanilla.
			var colorHSLSliderAttribute = ConfigManager.GetCustomAttribute<ColorHSLSliderAttribute>(memberInfo, item, list);
			bool useHue = colorHSLSliderAttribute != null;
			bool showSaturationAndLightness = colorHSLSliderAttribute?.showSaturationAndLightness ?? false;
			bool noAlpha = ConfigManager.GetCustomAttribute<ColorNoAlphaAttribute>(memberInfo, item, list) != null;

			List<string> skip = new List<string>();
			if (noAlpha) skip.Add(nameof(ColorObject.A));
			if (useHue)
				skip.AddRange(new[] { nameof(ColorObject.R), nameof(ColorObject.G), nameof(ColorObject.B) });
			else
				skip.AddRange(new[] { nameof(ColorObject.Hue), nameof(ColorObject.Saturation), nameof(ColorObject.Lightness) });
			if(useHue && !showSaturationAndLightness)
				skip.AddRange(new[] { nameof(ColorObject.Saturation), nameof(ColorObject.Lightness) });

			int order = 0;
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c))
			{
				if(skip.Contains(variable.Name))
					continue;

				var wrapped = UIModConfig.WrapIt(this, ref height, variable, c, order++);

				if (colorList != null)
				{
					wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			hitbox = new Rectangle(hitbox.X + hitbox.Width / 2, hitbox.Y, hitbox.Width / 2, 30);
			Main.spriteBatch.Draw(Main.magicPixel, hitbox, c.current);
		}

		internal float GetHeight()
		{
			return height;
		}
	}
}