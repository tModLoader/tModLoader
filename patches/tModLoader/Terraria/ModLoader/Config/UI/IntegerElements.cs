using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI;

internal class IntInputElement : ConfigElement
{
	public IList<int> IntList { get; set; }
	public int Min { get; set; } = 0;
	public int Max { get; set; } = 100;
	public int Increment { get; set; } = 1;

	public override void OnBind()
	{
		base.OnBind();

		IntList = (IList<int>)List;

		if (IntList != null) {
			TextDisplayFunction = () => Index + 1 + ": " + IntList[Index];
		}

		if (RangeAttribute != null && RangeAttribute.Min is int && RangeAttribute.Max is int) {
			Min = (int)RangeAttribute.Min;
			Max = (int)RangeAttribute.Max;
		}
		if (IncrementAttribute != null && IncrementAttribute.Increment is int) {
			Increment = (int)IncrementAttribute.Increment;
		}

		UIPanel textBoxBackground = new UIPanel();
		textBoxBackground.SetPadding(0);
		UIFocusInputTextField uIInputTextField = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigTypeHere"));
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
			if (int.TryParse(uIInputTextField.CurrentString, out int val)) {
				SetValue(val);
			}
			//else /{
			//	Interface.modConfig.SetMessage($"{uIInputTextField.currentString} isn't a valid value.", Color.Green);
			//}
		};
		uIInputTextField.OnUnfocus += (a, b) => uIInputTextField.SetText(GetValue().ToString());
		textBoxBackground.Append(uIInputTextField);

		UIModConfigHoverImageSplit upDownButton = new UIModConfigHoverImageSplit(UpDownTexture, "+" + Increment, "-" + Increment);
		upDownButton.Recalculate();
		upDownButton.Top.Set(4f, 0f);
		upDownButton.Left.Set(-30, 1f);
		upDownButton.OnLeftClick += (a, b) => {
			Rectangle r = b.GetDimensions().ToRectangle();
			if (a.MousePosition.Y < r.Y + r.Height / 2) {
				SetValue(Utils.Clamp(GetValue() + Increment, Min, Max));
			}
			else {
				SetValue(Utils.Clamp(GetValue() - Increment, Min, Max));
			}
			uIInputTextField.SetText(GetValue().ToString());
		};
		textBoxBackground.Append(upDownButton);
		Recalculate();
	}

	protected virtual int GetValue() => (int)GetObject();

	protected virtual void SetValue(int value) => SetObject(Utils.Clamp(value, Min, Max));
}

internal class IntRangeElement : PrimitiveRangeElement<int>
{
	public override int NumberTicks => ((Max - Min) / Increment) + 1;
	public override float TickIncrement => (float)(Increment) / (Max - Min);

	protected override float Proportion {
		get => (GetValue() - Min) / (float)(Max - Min);
		set => SetValue((int)Math.Round((value * (Max - Min) + Min) * (1f / Increment)) * Increment);
	}

	public IntRangeElement()
	{
		Min = 0;
		Max = 100;
		Increment = 1;
	}
}

internal class UIntElement : PrimitiveRangeElement<uint>
{
	public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
	public override float TickIncrement => (float)(Increment) / (Max - Min);

	protected override float Proportion {
		get => (GetValue() - Min) / (float)(Max - Min);
		set => SetValue((uint)Math.Round((value * (Max - Min) + Min) * (1f / Increment)) * Increment);
	}

	public UIntElement()
	{
		Min = 0;
		Max = 100;
		Increment = 1;
	}
}

internal class ByteElement : PrimitiveRangeElement<byte>
{
	public override int NumberTicks => (int)((Max - Min) / Increment) + 1;
	public override float TickIncrement => (float)(Increment) / (Max - Min);

	protected override float Proportion {
		get => (GetValue() - Min) / (float)(Max - Min);
		set => SetValue(Convert.ToByte((int)Math.Round((value * (Max - Min) + Min) * (1f / Increment)) * Increment));
	}

	public ByteElement()
	{
		Min = 0;
		Max = 255;
		Increment = 1;
	}
}
