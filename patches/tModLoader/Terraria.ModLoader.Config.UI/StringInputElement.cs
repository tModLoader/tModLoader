using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class StringInputElement : ConfigElement
	{
		private Func<string> _GetValue;
		private Action<string> _SetValue;

		public StringInputElement(PropertyFieldWrapper memberInfo, object item, IList<string> array, int index) : base(memberInfo, item, (IList)array)
		{
			_GetValue = () => DefaultGetValue();
			_SetValue = (string value) => DefaultSetValue(value);

			if (array != null)
			{
				_GetValue = () => array[index];
				_SetValue = (string value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				_TextDisplayFunction = () => index + 1 + ": " + array[index];
			}

			UIPanel textBoxBackground = new UIPanel();
			UIFocusInputTextField uIInputTextField = new UIFocusInputTextField("Type here");
			textBoxBackground.Top.Set(0f, 0f);
			textBoxBackground.Left.Set(-190, 1f);
			textBoxBackground.Width.Set(180, 0f);
			textBoxBackground.Height.Set(30, 0f);
			textBoxBackground.OnRightClick += (a, b) => uIInputTextField.SetText("");
			textBoxBackground.OnClick += (a, b) => uIInputTextField.Click(a);
			Append(textBoxBackground);

			uIInputTextField.SetText(_GetValue());
			uIInputTextField.Top.Set(6, 0f);
			uIInputTextField.Left.Set(-180, 1f);
			uIInputTextField.Width.Set(160, 0);
			uIInputTextField.Height.Set(20, 0);
			uIInputTextField.OnTextChange += (a, b) =>
			{
				_SetValue(uIInputTextField.currentString);
			};
			Append(uIInputTextField);
		}

		void DefaultSetValue(string text)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, text);
			Interface.modConfig.SetPendingChanges();
		}

		string DefaultGetValue()
		{
			return (string)memberInfo.GetValue(item);
		}
	}
}