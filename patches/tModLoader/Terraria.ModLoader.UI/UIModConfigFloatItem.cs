using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	public class UIModConfigFloatItem : UIElement
	{
		private Color _color;
		private Func<string> _TextDisplayFunction;
		private Func<float> _GetStatusFunction;
		private Action<float> _SlideKeyboardAction;
		private Action _SlideGamepadAction;
		private int _sliderIDInPage;
		private Texture2D _toggleTexture;

		public UIModConfigFloatItem(Func<string> getText, Func<float> getStatus, Action<float> setStatusKeyboard, Action setStatusGamepad, int sliderIDInPage, Color color)
		{
			this._color = color;
			this._toggleTexture = TextureManager.Load("Images/UI/Settings_Toggle");
			Func<string> arg_42_1;
			if (getText == null)
			{
				arg_42_1 = (() => "???");
			}
			else
			{
				arg_42_1 = getText;
			}
			this._TextDisplayFunction = arg_42_1;
			Func<float> arg_6B_1;
			if (getStatus == null)
			{
				arg_6B_1 = (() => 0f);
			}
			else
			{
				arg_6B_1 = getStatus;
			}
			this._GetStatusFunction = arg_6B_1;
			Action<float> arg_94_1;
			if (setStatusKeyboard == null)
			{
				arg_94_1 = delegate(float s)
				{
				};
			}
			else
			{
				arg_94_1 = setStatusKeyboard;
			}
			this._SlideKeyboardAction = arg_94_1;
			Action arg_BF_1;
			if (setStatusGamepad == null)
			{
				arg_BF_1 = delegate
				{
				};
			}
			else
			{
				arg_BF_1 = setStatusGamepad;
			}
			this._SlideGamepadAction = arg_BF_1;
			this._sliderIDInPage = sliderIDInPage;
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
			float obj = IngameOptions.DrawValueBar(spriteBatch, 1f, this._GetStatusFunction(), num2);
			if (IngameOptions.inBar || IngameOptions.rightLock == this._sliderIDInPage)
			{
				IngameOptions.rightHover = this._sliderIDInPage;
				if (PlayerInput.Triggers.Current.MouseLeft && PlayerInput.CurrentProfile.AllowEditting && !PlayerInput.UsingGamepad && IngameOptions.rightLock == this._sliderIDInPage)
				{
					this._SlideKeyboardAction(obj);
				}
			}
			if (IngameOptions.rightHover != -1 && IngameOptions.rightLock == -1)
			{
				IngameOptions.rightLock = IngameOptions.rightHover;
			}
			if (base.IsMouseHovering && PlayerInput.CurrentProfile.AllowEditting)
			{
				this._SlideGamepadAction();
			}
		}
	}
}
