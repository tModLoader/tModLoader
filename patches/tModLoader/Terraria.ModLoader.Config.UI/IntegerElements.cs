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
		private Func<int> _GetValue;
		private Action<int> _SetValue;
		int min = 0;
		int max = 100;
		int increment = 1;

		public IntInputElement(PropertyFieldWrapper memberInfo, object item, IList<int> array, int index) : base(memberInfo, item, (IList)array)
		{
			_GetValue = () => DefaultGetValue();
			_SetValue = (int value) => DefaultSetValue(value);

			if (array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (int value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			if (rangeAttribute != null && rangeAttribute.min is int && rangeAttribute.max is int)
			{
				min = (int)rangeAttribute.min;
				max = (int)rangeAttribute.max;
			}
			if (incrementAttribute != null && incrementAttribute.increment is int)
			{
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

			uIInputTextField.SetText(_GetValue().ToString());
			uIInputTextField.Top.Set(5, 0f);
			uIInputTextField.Left.Set(10, 0f);
			uIInputTextField.Width.Set(-42, 1f); // allow space for arrows
			uIInputTextField.Height.Set(20, 0);
			uIInputTextField.OnTextChange += (a, b) =>
			{
				if (Int32.TryParse(uIInputTextField.currentString, out int val))
				{
					_SetValue(val);
				}
				//else
				//{
				//	Interface.modConfig.SetMessage($"{uIInputTextField.currentString} isn't a valid value.", Color.Green);
				//}
			};
			uIInputTextField.OnUnfocus += (a, b) => uIInputTextField.SetText(_GetValue().ToString());
			textBoxBackground.Append(uIInputTextField);

			UIModConfigHoverImageSplit upDownButton = new UIModConfigHoverImageSplit(upDownTexture, "+" + increment, "-" + increment);
			upDownButton.Recalculate();
			upDownButton.Top.Set(4f, 0f);
			upDownButton.Left.Set(-30, 1f);
			upDownButton.OnClick += (a, b) =>
			{
				Rectangle r = b.GetDimensions().ToRectangle();
				if(a.MousePosition.Y < r.Y + r.Height / 2) {
					_SetValue(Utils.Clamp(_GetValue() + increment, min, max));
				}
				else {
					_SetValue(Utils.Clamp(_GetValue() - increment, min, max));
				}
				uIInputTextField.SetText(_GetValue().ToString());
			};
			textBoxBackground.Append(upDownButton);
			Recalculate();
		}

		void DefaultSetValue(int text)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, text);
			Interface.modConfig.SetPendingChanges();
		}

		int DefaultGetValue()
		{
			return (int)memberInfo.GetValue(item);
		}
	}

	internal class IntRangeElement : RangeElement
	{
		private Func<int> _GetValue;
		private Action<int> _SetValue;

		int min = 0;
		int max = 100;
		int increment = 1;

		public override int NumberTicks => ((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		protected override float Proportion {
			get => (_GetValue() - min) / (float)(max - min);
			set => _SetValue((int)Math.Round((value * (max - min) + min) * (1f / increment)) * increment);
		}

		public IntRangeElement(PropertyFieldWrapper memberInfo, object item, IList<int> array = null, int index = -1) : base(memberInfo, item, (IList)array)
		{
			this._TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			this._GetValue = () => DefaultGetValue();
			this._SetValue = (int value) => DefaultSetValue(value);

			if (array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (int value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			if (labelAttribute != null) // Problem with Lists using ModConfig Label.
			{
				this._TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValue();
			}

			if (rangeAttribute != null && rangeAttribute.min is int && rangeAttribute.max is int)
			{
				min = (int)rangeAttribute.min;
				max = (int)rangeAttribute.max;
			}
			if (incrementAttribute != null && incrementAttribute.increment is int)
			{
				this.increment = (int)incrementAttribute.increment;
			}
		}

		void DefaultSetValue(int value)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, Utils.Clamp(value, min, max));
			Interface.modConfig.SetPendingChanges();
		}

		int DefaultGetValue()
		{
			return (int)memberInfo.GetValue(item);
		}
	}

	internal class UIntElement : RangeElement
	{
		private Func<uint> _GetValue;
		private Action<uint> _SetValue;

		uint min = 0;
		uint max = 100;
		uint increment = 1;

		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		protected override float Proportion {
			get => (_GetValue() - min) / (float)(max - min);
			set => _SetValue((uint)Math.Round((value * (max - min) + min) * (1f / increment)) * increment);
		}

		public UIntElement(PropertyFieldWrapper memberInfo, object item, IList<uint> array = null, int index = -1) : base(memberInfo, item, (IList)array)
		{
			this._TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			this._GetValue = () => DefaultGetValue();
			this._SetValue = (uint value) => DefaultSetValue(value);

			if (array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (uint value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			if (labelAttribute != null)
			{
				this._TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValue();
			}

			if (rangeAttribute != null && rangeAttribute.min is uint && rangeAttribute.max is uint)
			{
				min = (uint)rangeAttribute.min;
				max = (uint)rangeAttribute.max;
			}
			if (incrementAttribute != null && incrementAttribute.increment is uint)
			{
				this.increment = (uint)incrementAttribute.increment;
			}
		}

		void DefaultSetValue(uint value)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, Utils.Clamp(value, min, max));
			Interface.modConfig.SetPendingChanges();
		}

		uint DefaultGetValue()
		{
			return (uint)memberInfo.GetValue(item);
		}
	}

	internal class ByteElement : RangeElement
	{
		private Func<byte> _GetValue;
		private Action<byte> _SetValue;

		byte min = 0;
		byte max = 255;
		byte increment = 1;

		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		protected override float Proportion {
			get => (_GetValue() - min) / (float)(max - min);
			set => _SetValue(Convert.ToByte((int)Math.Round((value * (max - min) + min) * (1f / increment)) * increment));
		}

		public ByteElement(PropertyFieldWrapper memberInfo, object item, IList<byte> array = null, int index = -1) : base(memberInfo, item, (IList)array)
		{
			this._TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			this._GetValue = () => DefaultGetValue();
			this._SetValue = (byte value) => DefaultSetValue(value);

			if (array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (byte value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			if (labelAttribute != null)
			{
				this._TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValue();
			}

			if (rangeAttribute != null && rangeAttribute.min is byte && rangeAttribute.max is byte)
			{
				min = (byte)rangeAttribute.min;
				max = (byte)rangeAttribute.max;
			}
			if (incrementAttribute != null && incrementAttribute.increment is byte)
			{
				this.increment = (byte)incrementAttribute.increment;
			}
		}
		void DefaultSetValue(byte value)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, Utils.Clamp(value, min, max));
			Interface.modConfig.SetPendingChanges();
		}

		byte DefaultGetValue()
		{
			return (byte)memberInfo.GetValue(item);
		}
	}
}
