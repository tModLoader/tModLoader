using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	internal class UIModConfigIntItem : UIConfigRangeItem
	{
		private Func<int> _GetValue;
		private Action<int> _SetValue;

		int min = 0;
		int max = 100;
		int increment = 1;

		public override int NumberTicks => ((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		public UIModConfigIntItem(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage, IList<int> array = null, int index = -1) : base(sliderIDInPage, memberInfo, item, (IList)array)
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

			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				this._TextDisplayFunction = () => att.Label + ": " + _GetValue();
			}

			RangeAttribute rangeAttribute = (RangeAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(RangeAttribute));
			IncrementAttribute incrementAttribute = (IncrementAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(IncrementAttribute));
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

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

		}
	}

	internal class UIModConfigUIntItem : UIConfigRangeItem
	{
		private Func<uint> _GetValue;
		private Action<uint> _SetValue;

		uint min = 0;
		uint max = 100;
		uint increment = 1;

		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		public UIModConfigUIntItem(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage, IList<uint> array = null, int index = -1) : base(sliderIDInPage, memberInfo, item, (IList)array)
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

			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				this._TextDisplayFunction = () => att.Label + ": " + _GetValue();
			}

			RangeAttribute rangeAttribute = (RangeAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(RangeAttribute));
			IncrementAttribute incrementAttribute = (IncrementAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(IncrementAttribute));
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

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
		}
	}

	internal class UIModConfigByteItem : UIConfigRangeItem
	{
		private Func<byte> _GetValue;
		private Action<byte> _SetValue;

		byte min = 0;
		byte max = 255;
		byte increment = 1;

		public override int NumberTicks => (int)((max - min) / increment) + 1;
		public override float TickIncrement => (float)(increment) / (max - min);

		public UIModConfigByteItem(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage, IList<byte> array = null, int index = -1) : base(sliderIDInPage, memberInfo, item, (IList)array)
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

			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				this._TextDisplayFunction = () => att.Label + ": " + _GetValue();
			}

			RangeAttribute rangeAttribute = (RangeAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(RangeAttribute));
			IncrementAttribute incrementAttribute = (IncrementAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(IncrementAttribute));
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

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
		}
	}
}
