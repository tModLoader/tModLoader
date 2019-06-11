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
		public IList<string> stringList;

		public override void OnBind() {
			base.OnBind();
			stringList = (IList<string>)list;
			_GetValue = () => DefaultGetValue();
			_SetValue = (string value) => DefaultSetValue(value);

			if (stringList != null)
			{
				_GetValue = () => stringList[index];
				_SetValue = (string value) => { this.stringList[index] = value; Interface.modConfig.SetPendingChanges(); };
				TextDisplayFunction = () => index + 1 + ": " + this.stringList[index];
			}

			UIPanel textBoxBackground = new UIPanel();
			textBoxBackground.SetPadding(0);
			UIFocusInputTextField uIInputTextField = new UIFocusInputTextField("Type here");
			textBoxBackground.Top.Set(0f, 0f);
			textBoxBackground.Left.Set(-190, 1f);
			textBoxBackground.Width.Set(180, 0f);
			textBoxBackground.Height.Set(30, 0f);
			Append(textBoxBackground);

			uIInputTextField.SetText(_GetValue());
			uIInputTextField.Top.Set(5, 0f);
			uIInputTextField.Left.Set(10, 0f);
			uIInputTextField.Width.Set(-20, 1f);
			uIInputTextField.Height.Set(20, 0);
			uIInputTextField.OnTextChange += (a, b) =>
			{
				_SetValue(uIInputTextField.currentString);
			};
			textBoxBackground.Append(uIInputTextField);
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