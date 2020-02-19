using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class IntInputElement : ConfigElement
	{
		public IList<int> intList;
		public int min = 0;
		public int max = 100;
		public int increment = 1;

		public override void OnBind() {
			base.OnBind();
			this.intList = (IList<int>)list;

			if (intList != null) {
				TextDisplayFunction = () => index + 1 + ": " + intList[index];
			}

			if (rangeAttribute != null && rangeAttribute.min is int && rangeAttribute.max is int) {
				min = (int)rangeAttribute.min;
				max = (int)rangeAttribute.max;
			}
			if (incrementAttribute != null && incrementAttribute.increment is int) {
				this.increment = (int)incrementAttribute.increment;
			}

			UIPanel textBoxBackground = new UIPanel();
			textBoxBackground.SetPadding(0);
			UIFocusInputTextField uIInputTextField = new UIFocusInputTextField("Type here");
			textBoxBackground.Top.Set(0f, 0f);
			textBoxBackground.Left.Set(-190, 1f);
			textBoxBackground.Width.Set(180, 0f);
			textBoxBackground.Height.Set(30, 0f);
			Append(textBoxBackground);

			uIInputTextField.SetText(GetValue().ToString());
			uIInputTextField.Top.Set(5, 0f);
			uIInputTextField.Left.Set(10, 0f);
			uIInputTextField.Width.Set(-42, 1f); // allow space for arrows
			uIInputTextField.Height.Set(20, 0);
			uIInputTextField.OnTextChange += (a, b) => {
				if (Int32.TryParse(uIInputTextField.CurrentString, out int val)) {
					SetValue(val);
				}
				//else
				//{
				//	Interface.modConfig.SetMessage($"{uIInputTextField.currentString} isn't a valid value.", Color.Green);
				//}
			};
			uIInputTextField.OnUnfocus += (a, b) => uIInputTextField.SetText(GetValue().ToString());
			textBoxBackground.Append(uIInputTextField);

			UIModConfigHoverImageSplit upDownButton = new UIModConfigHoverImageSplit(upDownTexture, "+" + increment, "-" + increment);
			upDownButton.Recalculate();
			upDownButton.Top.Set(4f, 0f);
			upDownButton.Left.Set(-30, 1f);
			upDownButton.OnClick += (a, b) => {
				Rectangle r = b.GetDimensions().ToRectangle();
				if (a.MousePosition.Y < r.Y + r.Height / 2) {
					SetValue(Utils.Clamp(GetValue() + increment, min, max));
				}
				else {
					SetValue(Utils.Clamp(GetValue() - increment, min, max));
				}
				uIInputTextField.SetText(GetValue().ToString());
			};
			textBoxBackground.Append(upDownButton);
			Recalculate();
		}

		protected virtual int GetValue() => (int)GetObject();

		protected virtual void SetValue(int value) => SetObject(Utils.Clamp(value, min, max));
	}

	internal class IntRangeElement : PrimitiveRangeElement<int>
	{
		public override int NumberTicks => ((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		protected override float Proportion {
			get => (GetValue() - min) / (float)(max - min);
			set => SetValue((int)Math.Round((value * (max - min) + min) * (1f / increment)) * increment);
		}

		public IntRangeElement() {
			min = 0;
			max = 100;
			increment = 1;
		}
	}

	internal class UIntElement : PrimitiveRangeElement<uint>
	{
		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		protected override float Proportion {
			get => (GetValue() - min) / (float)(max - min);
			set => SetValue((uint)Math.Round((value * (max - min) + min) * (1f / increment)) * increment);
		}

		public UIntElement() { 
			min = 0;
			max = 100;
			increment = 1;
		}
	}

	internal class ByteElement : PrimitiveRangeElement<byte>
	{
		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		protected override float Proportion {
			get => (GetValue() - min) / (float)(max - min);
			set => SetValue(Convert.ToByte((int)Math.Round((value * (max - min) + min) * (1f / increment)) * increment));
		}

		public ByteElement() {
			min = 0;
			max = 255;
			increment = 1;
		}
	}
}
