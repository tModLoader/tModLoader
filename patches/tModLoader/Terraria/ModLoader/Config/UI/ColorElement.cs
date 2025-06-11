using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.GameContent;

namespace Terraria.ModLoader.Config.UI;

internal class ColorElement : ConfigElement
{
	private class ColorObject
	{
		private readonly PropertyFieldWrapper memberInfo;
		private readonly object item;
		private readonly IList<Color> array;
		private readonly int index;

		internal Color current;

		[LabelKey("$Config.Color.Red.Label")]
		public byte R {
			get => current.R;
			set {
				current.R = value;
				Update();
			}
		}

		[LabelKey("$Config.Color.Green.Label")]
		public byte G {
			get => current.G;
			set {
				current.G = value;
				Update();
			}
		}

		[LabelKey("$Config.Color.Blue.Label")]
		public byte B {
			get => current.B;
			set {
				current.B = value;
				Update();
			}
		}

		[LabelKey("$Config.Color.Hue.Label")]
		public float Hue {
			get => Main.rgbToHsl(current).X;
			set {
				byte a = A;
				current = Main.hslToRgb(value, Saturation, Lightness);
				current.A = a;
				Update();
			}
		}

		[LabelKey("$Config.Color.Saturation.Label")]
		public float Saturation {
			get => Main.rgbToHsl(current).Y;
			set {
				byte a = A;
				current = Main.hslToRgb(Hue, value, Lightness);
				current.A = a;
				Update();
			}
		}

		[LabelKey("$Config.Color.Lightness.Label")]
		public float Lightness {
			get => Main.rgbToHsl(current).Z;
			set {
				byte a = A;
				current = Main.hslToRgb(Hue, Saturation, value);
				current.A = a;
				Update();
			}
		}

		[LabelKey("$Config.Color.Alpha.Label")]
		public byte A {
			get => current.A;
			set {
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

	private int height;
	private ColorObject c;

	public IList<Color> ColorList { get; set; }

	public override void OnBind()
	{
		base.OnBind();

		ColorList = (IList<Color>)List;

		if (ColorList != null) {
			DrawLabel = false;
			height = 30;
			c = new ColorObject(ColorList, Index);
		}
		else {
			height = 30;
			c = new ColorObject(MemberInfo, Item);
		}

		// TODO: Draw the sliders in the same manner as vanilla.
		var colorHSLSliderAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ColorHSLSliderAttribute>(MemberInfo, Item, List);
		bool useHue = colorHSLSliderAttribute != null;
		bool showSaturationAndLightness = colorHSLSliderAttribute?.ShowSaturationAndLightness ?? false;
		bool noAlpha = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ColorNoAlphaAttribute>(MemberInfo, Item, List) != null;

		var skip = new List<string>();

		if (noAlpha)
			skip.Add(nameof(ColorObject.A));
		if (useHue)
			skip.AddRange(new[] { nameof(ColorObject.R), nameof(ColorObject.G), nameof(ColorObject.B) });
		else
			skip.AddRange(new[] { nameof(ColorObject.Hue), nameof(ColorObject.Saturation), nameof(ColorObject.Lightness) });

		if (useHue && !showSaturationAndLightness)
			skip.AddRange(new[] { nameof(ColorObject.Saturation), nameof(ColorObject.Lightness) });

		int order = 0;

		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c)) {
			if (skip.Contains(variable.Name))
				continue;

			var wrapped = UIModConfig.WrapIt(this, ref height, variable, c, order++);

			if (ColorList != null) {
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
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, c.current);
	}

	internal float GetHeight()
	{
		return height;
	}
}