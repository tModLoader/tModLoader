using System;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;

namespace Terraria.ModLoader.UI.Config.Elements;
// TODO: make this just have text input and slider
public abstract class UINumericElement<T> : UIConfigElement<T>
{
	public RangeAttribute RangeAttribute { get; internal set; }
	public IncrementAttribute IncrementAttribute { get; internal set; }
	public SliderAttribute SliderAttribute { get; internal set; }
	public DrawTicksAttribute DrawTicksAttribute { get; internal set; }
	public SliderColorAttribute SliderColorAttribute { get; internal set; }

	public new T Value {
		get => base.Value;
		set => base.Value = Clamp(Round(Clamp(value, Min, Max), Step), Min, Max); // TODO: Clamp twice to stop unsigned numbers from overflowing to the max value
	}

	public T Step => (T)(IncrementAttribute?.Increment ?? FromInt(1));
	public T Min => (T)(RangeAttribute?.Min ?? FromInt(0));
	public T Max => (T)(RangeAttribute?.Max ?? FromInt(20));

	private UIImageButton _incrementButton;
	private UIFocusInputTextField _inputField;

	// Overridden to implement increasing or decresing the value
	public abstract void Change(T amount);

	// Overridden to implement rounding to nearest step
	public abstract T Round(T value, T nearest);

	// Overridden to implement clamping to min and max
	public abstract T Clamp(T value, T min, T max);

	// Overridden to implement getting the a number of type T
	public abstract T FromInt(int value);

	// Overridden to implement negating a number
	public abstract T Negate(T value);

	// Overridden to implement parsing a number from a string
	public abstract bool TryParse(string text, out T value);

	// TODO: UI
	public override void OnInitialize()
	{
		base.OnInitialize();

		if (!MemberInfo.CanWrite)
			return;

		RangeAttribute = GetAttribute<RangeAttribute>();
		IncrementAttribute = GetAttribute<IncrementAttribute>();
		SliderAttribute = GetAttribute<SliderAttribute>();
		DrawTicksAttribute = GetAttribute<DrawTicksAttribute>();
		SliderColorAttribute = GetAttribute<SliderColorAttribute>();

		if (SliderAttribute != null)
			CreateSliderUI();
		else
			CreateInputUI();
	}

	private void CreateSliderUI()
	{
		CreateInputUI();// Temporary, could just make both uis be created
	}

	private void CreateInputUI()
	{
		var inputBackground = new UIPanel {
			Width = { Pixels = 100 },
			Height = { Pixels = 40 },
			Left = { Pixels = -UpDownTexture.Width() - PaddingLeft },
			HAlign = 1f,
			VAlign = 0.5f,
		}.WithPadding(0);
		Append(inputBackground);

		_inputField = new UIFocusInputTextField("Enter a value here") {
			Width = { Percent = 1f },
			Height = { Percent = 1f },
		};
		_inputField.OnTextChange += (_, _) => UpdateValueFromTextField();
		_inputField.OnUnfocus += (_, _) => UpdateValueFromTextField(clear: true);
		inputBackground.Append(_inputField);

		_incrementButton = new UIImageButton(UpDownTexture) {
			HAlign = 1f,
			VAlign = 0.5f,
		};
		_incrementButton.OnLeftClick += (_, _) => {
			var dimensions = GetDimensions();

			if (Main.MouseScreen.Y >= dimensions.Y + dimensions.Height / 2f)
				Change(Negate(Step));
			else
				Change(Step);
		};
		_incrementButton.SetVisibility(1f, 1f);
		Append(_incrementButton);
	}

	private void UpdateValueFromTextField(bool clear = false)
	{
		if (TryParse(_inputField.CurrentString, out var value))
			Value = value;

		if (clear)
			_inputField.SetText("");
	}

	public override void Recalculate()
	{
		_inputField?.SetText(Value.ToString());

		base.Recalculate();
	}

	public override string GetLabel() => base.GetLabel() + ": " + Value.ToString();

	// TODO: min and max values in tooltip?
	public override string GetTooltip()
	{
		string tooltip = base.GetTooltip();

		if (!_incrementButton?.IsMouseHovering ?? true)
			return tooltip;

		var dimensions = _incrementButton.GetDimensions();
		return Main.MouseScreen.Y >= dimensions.Y + dimensions.Height / 2f ? "-" + Step : "+" + Step;
	}
}

#region Numeric Elements
// TODO: when INumber comes around refactor all of this
public class UIByteElement : UINumericElement<byte>
{
	public override void Change(byte amount) => Value += amount;
	public override byte Clamp(byte value, byte min, byte max) => Math.Clamp(value, min, max);
	public override byte Round(byte value, byte nearest) => (byte)(value - value % nearest);
	public override byte FromInt(int value) => (byte)value;
	public override byte Negate(byte value) => (byte)-value;
	public override bool TryParse(string text, out byte value) => byte.TryParse(text, out value);
}
public class UISByteElement : UINumericElement<sbyte>
{
	public override void Change(sbyte amount) => Value += amount;
	public override sbyte Clamp(sbyte value, sbyte min, sbyte max) => Math.Clamp(value, min, max);
	public override sbyte Round(sbyte value, sbyte nearest) => (sbyte)(value - value % nearest);
	public override sbyte FromInt(int value) => (sbyte)value;
	public override sbyte Negate(sbyte value) => (sbyte)-value;
	public override bool TryParse(string text, out sbyte value) => sbyte.TryParse(text, out value);
}

public class UIShortElement : UINumericElement<short>
{
	public override void Change(short amount) => Value += amount;
	public override short Clamp(short value, short min, short max) => Math.Clamp(value, min, max);
	public override short Round(short value, short nearest) => (short)(value - value % nearest);
	public override short FromInt(int value) => (short)value;
	public override short Negate(short value) => (short)-value;
	public override bool TryParse(string text, out short value) => short.TryParse(text, out value);
}
public class UIUShortElement : UINumericElement<ushort>
{
	public override void Change(ushort amount) => Value += amount;
	public override ushort Clamp(ushort value, ushort min, ushort max) => Math.Clamp(value, min, max);
	public override ushort Round(ushort value, ushort nearest) => (ushort)(value - value % nearest);
	public override ushort FromInt(int value) => (ushort)value;
	public override ushort Negate(ushort value) => (ushort)-value;
	public override bool TryParse(string text, out ushort value) => ushort.TryParse(text, out value);
}

public class UIIntElement : UINumericElement<int>
{
	public override void Change(int amount) => Value += amount;
	public override int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
	public override int Round(int value, int nearest) => value - value % nearest;
	public override int FromInt(int value) => value;
	public override int Negate(int value) => -value;
	public override bool TryParse(string text, out int value) => int.TryParse(text, out value);
}
public class UIUIntElement : UINumericElement<uint>
{
	public override void Change(uint amount) => Value += amount;
	public override uint Clamp(uint value, uint min, uint max) => Math.Clamp(value, min, max);
	public override uint Round(uint value, uint nearest) => value - value % nearest;
	public override uint FromInt(int value) => (uint)value;
	public override uint Negate(uint value) => (uint)-value;
	public override bool TryParse(string text, out uint value) => uint.TryParse(text, out value);
}

public class UILongElement : UINumericElement<long>
{
	public override void Change(long amount) => Value += amount;
	public override long Clamp(long value, long min, long max) => Math.Clamp(value, min, max);
	public override long Round(long value, long nearest) => value - value % nearest;
	public override long FromInt(int value) => value;
	public override long Negate(long value) => -value;
	public override bool TryParse(string text, out long value) => long.TryParse(text, out value);
}
public class UIULongElement : UINumericElement<ulong>
{
	public override void Change(ulong amount) => Value += amount;
	public override ulong Clamp(ulong value, ulong min, ulong max) => Math.Clamp(value, min, max);
	public override ulong Round(ulong value, ulong nearest) => value - value % nearest;
	public override ulong FromInt(int value) => (ulong)value;
	public override ulong Negate(ulong value) => unchecked((ulong)-1) * value;
	public override bool TryParse(string text, out ulong value) => ulong.TryParse(text, out value);
}

public class UIFloatElement : UINumericElement<float>
{
	public override void Change(float amount) => Value += amount;
	public override float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
	public override float Round(float value, float nearest) => value - value % nearest;
	public override float FromInt(int value) => value;
	public override float Negate(float value) => -value;
	public override bool TryParse(string text, out float value) => float.TryParse(text, out value);
}
public class UIDoubleElement : UINumericElement<double>
{
	public override void Change(double amount) => Value += amount;
	public override double Clamp(double value, double min, double max) => Math.Clamp(value, min, max);
	public override double Round(double value, double nearest) => value - value % nearest;
	public override double FromInt(int value) => value;
	public override double Negate(double value) => -value;
	public override bool TryParse(string text, out double value) => double.TryParse(text, out value);
}
public class UIDecimalElement : UINumericElement<decimal>
{
	public override void Change(decimal amount) => Value += amount;
	public override decimal Clamp(decimal value, decimal min, decimal max) => Math.Clamp(value, min, max);
	public override decimal Round(decimal value, decimal nearest) => value - value % nearest;
	public override decimal FromInt(int value) => value;
	public override decimal Negate(decimal value) => -value;
	public override bool TryParse(string text, out decimal value) => decimal.TryParse(text, out value);
}
#endregion