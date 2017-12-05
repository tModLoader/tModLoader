using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
		public override float TickIncrement => (float)(increment)/(max - min);

		public UIModConfigIntItem(PropertyFieldWrapper memberInfo, object item, int sliderIDInPage, IList<int> array = null, int index = -1) : base(sliderIDInPage, memberInfo, item)
		{
			this._TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			this._GetValue = () => DefaultGetValue();
			this._SetValue = (int value) => DefaultSetValue(value);

			if(array != null)
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
			_SetValue((int)Math.Round((proportion*(max-min) + min) * (1f / increment)) * increment);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			
		}
	}
}
