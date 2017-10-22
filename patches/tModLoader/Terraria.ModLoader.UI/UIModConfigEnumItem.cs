using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	class UIModConfigEnumItem : UIElement
	{
		private Color _color;
		private Func<string> _TextDisplayFunction;
		private Func<float> _GetProportion;
		private Action<float> _SetProportion;
		private Func<object> _GetValue;
		private Func<object> _GetValueString;
		private Func<int> _GetIndex;
		private Action<int> _SetValue;
		private int _sliderIDInPage;
		int max;
		private PropertyFieldWrapper variable;
		private ModConfig modConfig;
		string[] valueStrings;
		string[] valueEnums;

		public UIModConfigEnumItem(PropertyFieldWrapper variable, ModConfig modConfig, int sliderIDInPage)
		{
			this.variable = variable;
			this.modConfig = modConfig;
			Width.Set(0f, 1f);
			Height.Set(0f, 1f);

			valueStrings = Enum.GetNames(variable.Type);
			max = valueStrings.Length;

			//valueEnums = Enum.GetValues(variable.Type);

			this._color = Color.White;
			this._TextDisplayFunction = () => variable.Name + ": " + _GetValueString();
			this._GetValue = () => DefaultGetValue();
			_GetValueString = () => DefaultGetStringValue();
			_GetIndex = () => DefaultGetIndex();
			this._SetValue = (int value) => DefaultSetValue(value);
			this._sliderIDInPage = sliderIDInPage;

			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(variable.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				this._TextDisplayFunction = () => att.Label + ": " + _GetValueString();
			}

			this._GetProportion = () => DefaultGetProportion();
			this._SetProportion = (float proportion) => DefaultSetProportion(proportion);
		}

		void DefaultSetValue(int index)
		{
			variable.SetValue(modConfig, Enum.GetValues(variable.Type).GetValue(index));
			Interface.modConfig.SetPendingChanges();
		}

		object DefaultGetValue()
		{
			return variable.GetValue(modConfig);
		}
		int DefaultGetIndex()
		{
			return Array.IndexOf(Enum.GetValues(variable.Type), _GetValue());
		}
		string DefaultGetStringValue()
		{
			return valueStrings[_GetIndex()];
		}

		float DefaultGetProportion()
		{
			return _GetIndex() / (float)(max - 1);
		}

		void DefaultSetProportion(float proportion)
		{
			_SetValue((int)(Math.Round(proportion * (max - 1))));
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			float num = 6f;
			base.DrawSelf(spriteBatch);
			int num2 = 0;
			IngameOptions.rightHover = -1;
			if (!Main.mouseLeft)
			{
				IngameOptions.rightLock = -1;
			}
			if (IngameOptions.rightLock == this._sliderIDInPage)
			{
				num2 = 1;
			}
			else if (IngameOptions.rightLock != -1)
			{
				num2 = 2;
			}
			CalculatedStyle dimensions = base.GetDimensions();
			float num3 = dimensions.Width + 1f;
			Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
			bool flag = false;
			bool flag2 = base.IsMouseHovering;
			if (num2 == 1)
			{
				flag2 = true;
			}
			if (num2 == 2)
			{
				flag2 = false;
			}
			Vector2 baseScale = new Vector2(0.8f);
			Color color = flag ? Color.Gold : (flag2 ? Color.White : Color.Silver);
			color = Color.Lerp(color, Color.White, flag2 ? 0.5f : 0f);
			Color color2 = flag2 ? this._color : this._color.MultiplyRGBA(new Color(180, 180, 180));
			Vector2 vector2 = vector;
			Utils.DrawSettingsPanel(spriteBatch, vector2, num3, color2);
			vector2.X += 8f;
			vector2.Y += 2f + num;
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, this._TextDisplayFunction(), vector2, color, 0f, Vector2.Zero, baseScale, num3, 2f);
			vector2.X -= 17f;
			Main.colorBarTexture.Frame(1, 1, 0, 0);
			vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
			IngameOptions.valuePosition = vector2;
			float obj = IngameOptions.DrawValueBar(spriteBatch, 1f, this._GetProportion(), num2);
			if (IngameOptions.inBar || IngameOptions.rightLock == this._sliderIDInPage)
			{
				IngameOptions.rightHover = this._sliderIDInPage;
				if (PlayerInput.Triggers.Current.MouseLeft && IngameOptions.rightLock == this._sliderIDInPage)
				{
					this._SetProportion(obj);
				}
			}
			if (IngameOptions.rightHover != -1 && IngameOptions.rightLock == -1)
			{
				IngameOptions.rightLock = IngameOptions.rightHover;
			}
		}
	}
}
