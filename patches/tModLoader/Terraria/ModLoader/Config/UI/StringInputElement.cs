using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI;

internal class StringInputElement : ConfigElement<string>
{
	public override void OnBind()
	{
		base.OnBind();

		UIPanel textBoxBackground = new UIPanel();
		textBoxBackground.SetPadding(0);
		UIFocusInputTextField uIInputTextField = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigTypeHere"));
		textBoxBackground.Top.Set(0f, 0f);
		textBoxBackground.Left.Set(-190, 1f);
		textBoxBackground.Width.Set(180, 0f);
		textBoxBackground.Height.Set(30, 0f);

		Append(textBoxBackground);

		uIInputTextField.SetText(Value);
		uIInputTextField.Top.Set(5, 0f);
		uIInputTextField.Left.Set(10, 0f);
		uIInputTextField.Width.Set(-20, 1f);
		uIInputTextField.Height.Set(20, 0);
		uIInputTextField.OnTextChange += (a, b) => {
			Value = uIInputTextField.CurrentString;
		};

		textBoxBackground.Append(uIInputTextField);
	}
}