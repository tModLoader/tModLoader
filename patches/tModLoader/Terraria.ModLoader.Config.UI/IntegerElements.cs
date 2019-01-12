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

			UIImageButton upButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonIncrement.png")));
			upButton.Recalculate();
			upButton.Top.Set(4f, 0f);
			upButton.Left.Set(-30, 1f);
			upButton.OnClick += (a, b) =>
			{
				_SetValue(Math.Min(_GetValue() + increment, max));
				uIInputTextField.SetText(_GetValue().ToString());
			};
			textBoxBackground.Append(upButton);
			UIImageButton downButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png")));
			downButton.Top.Set(16, 0f);
			downButton.Left.Set(-30, 1f);
			downButton.OnClick += (a, b) =>
			{
				_SetValue(Math.Max(_GetValue() - increment, min));
				uIInputTextField.SetText(_GetValue().ToString());
			};
			textBoxBackground.Append(downButton);
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
			this._GetProportion = () => DefaultGetProportion();
			this._SetProportion = (float proportion) => DefaultSetProportion(proportion);
		}

		//public UIModConfigIntItem(Func<int> _GetValue, Action<int> _SetValue, Func<string> text, int sliderIDInPage)
		//{
		//	this._GetValue = _GetValue;
		//	this._SetValue = _SetValue;
		//	this._TextDisplayFunction = text;
		//}

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

		float DefaultGetProportion()
		{
			return (_GetValue() - min) / (float)(max - min);
		}

		void DefaultSetProportion(float proportion)
		{
			_SetValue((int)Math.Round((proportion * (max - min) + min) * (1f / increment)) * increment);
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
			this._GetProportion = () => DefaultGetProportion();
			this._SetProportion = (float proportion) => DefaultSetProportion(proportion);
		}

		//public UIModConfigIntItem(Func<int> _GetValue, Action<int> _SetValue, Func<string> text, int sliderIDInPage)
		//{
		//	this._GetValue = _GetValue;
		//	this._SetValue = _SetValue;
		//	this._TextDisplayFunction = text;
		//}

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

		float DefaultGetProportion()
		{
			return (_GetValue() - min) / (float)(max - min);
		}

		void DefaultSetProportion(float proportion)
		{
			_SetValue((uint)Math.Round((proportion * (max - min) + min) * (1f / increment)) * increment);
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
			this._GetProportion = () => DefaultGetProportion();
			this._SetProportion = (float proportion) => DefaultSetProportion(proportion);
		}

		//public UIModConfigIntItem(Func<int> _GetValue, Action<int> _SetValue, Func<string> text, int sliderIDInPage)
		//{
		//	this._GetValue = _GetValue;
		//	this._SetValue = _SetValue;
		//	this._TextDisplayFunction = text;
		//}

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

		float DefaultGetProportion()
		{
			return (_GetValue() - min) / (float)(max - min);
		}

		void DefaultSetProportion(float proportion)
		{
			_SetValue(Convert.ToByte((int)Math.Round((proportion * (max - min) + min) * (1f / increment)) * increment));
		}
	}
}
