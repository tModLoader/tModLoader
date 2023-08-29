using System;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI.Config.Elements;
public abstract class UINumericElement<T> : UIConfigElement<T>
{
	// TODO: change method, add ui for slider or input range
	public new T Value {
		get => base.Value;
		set => Clamp(Round(base.Value = value, Step), Min, Max);
	}

	public T Step => (T)(IncrementAttribute?.Increment ?? FromInt(1));
	public T Min => (T)(RangeAttribute?.Min ?? FromInt(0));
	public T Max => (T)(RangeAttribute?.Max ?? FromInt(20));

	private UIImageButton _incrementButton;

	// Overridden to implement increasing or decresing the value
	public abstract void Change(T amount);

	// Overriden to implement rounding to nearest step
	public abstract T Round(T value, T nearest);

	// Overriden to implement clamping to min and max
	public abstract T Clamp(T value, T min, T max);

	// Overriden to implement getting the a number of type T
	public abstract T FromInt(int value);

	// TODO: UI
	public override void OnInitialize()
	{
		if (SliderAttribute != null)
			CreateSliderUI();
		else
			CreateInputUI();
	}

	private void CreateSliderUI()
	{

	}

	private void CreateInputUI()
	{
		var inputBackground = new UIPanel {
			Width = { Pixels = 200 },
			Height = { Pixels = 25 },
			Left = { Pixels = -UpDownTexture.Width() - 5 },
			HAlign = 1f,
			VAlign = 0.5f,
		};
		Append(inputBackground);

		var inputField = new UIFocusInputTextField("") {
			Width = { Percent = 1f },
			Height = { Percent = 1f },
		};
		inputBackground.Append(inputField);

		_incrementButton = new UIImageButton(UpDownTexture) {
			HAlign = 1f,
			VAlign = 0.5f,
		};
		_incrementButton.OnLeftClick += (_, _) => {
			var dimensions = GetDimensions();

			if (Main.MouseScreen.Y >= dimensions.Y + dimensions.Height / 2f)
				Change(FromInt(-1));
			else
				Change(FromInt(1));
		};
		_incrementButton.SetVisibility(1f, 1f);
		Append(_incrementButton);
	}

	public override string GetLabel() => base.GetLabel() + ": " + Value.ToString();

	public override string GetTooltip()
	{
		string tooltip = base.GetTooltip();

		if (!_incrementButton.IsMouseHovering)
			return tooltip;

		var dimensions = _incrementButton.GetDimensions();
		return Main.MouseScreen.Y >= dimensions.Y + dimensions.Height / 2f ? "-1" : "+1";
	}
}

#region Numeric Elements
public class UIByteElement : UINumericElement<byte>
{
	public override void Change(byte amount) => Value += amount;
	public override byte Clamp(byte value, byte min, byte max) => Math.Clamp(value, min, max);
	public override byte Round(byte value, byte nearest) => (byte)(value - value % nearest);// TODO: test overflowing
	public override byte FromInt(int value) => (byte)value;
}
public class UISByteElement : UINumericElement<sbyte>
{
	public override void Change(sbyte amount) => Value += amount;
	public override sbyte Clamp(sbyte value, sbyte min, sbyte max) => Math.Clamp(value, min, max);
	public override sbyte Round(sbyte value, sbyte nearest) => (sbyte)(value - value % nearest);// TODO: test overflowing
	public override sbyte FromInt(int value) => (sbyte)value;
}

public class UIShortElement : UINumericElement<short>
{
	public override void Change(short amount) => Value += amount;
	public override short Clamp(short value, short min, short max) => Math.Clamp(value, min, max);
	public override short Round(short value, short nearest) => (short)(value - value % nearest);// TODO: test overflow
	public override short FromInt(int value) => (short)value;
}
public class UIUShortElement : UINumericElement<ushort>
{
	public override void Change(ushort amount) => Value += amount;
	public override ushort Clamp(ushort value, ushort min, ushort max) => Math.Clamp(value, min, max);
	public override ushort Round(ushort value, ushort nearest) => (ushort)(value - value % nearest);// TODO: test overflow
	public override ushort FromInt(int value) => (ushort)value;
}

public class UIIntElement : UINumericElement<int>
{
	public override void Change(int amount) => Value += amount;
	public override int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
	public override int Round(int value, int nearest) => value - value % nearest;
	public override int FromInt(int value) => value;
}
public class UIUIntElement : UINumericElement<uint>
{
	public override void Change(uint amount) => Value += amount;
	public override uint Clamp(uint value, uint min, uint max) => Math.Clamp(value, min, max);
	public override uint Round(uint value, uint nearest) => value - value % nearest;
	public override uint FromInt(int value) => (uint)value;
}

public class UILongElement : UINumericElement<long>
{
	public override void Change(long amount) => Value += amount;
	public override long Clamp(long value, long min, long max) => Math.Clamp(value, min, max);
	public override long Round(long value, long nearest) => value - value % nearest;
	public override long FromInt(int value) => (long)value;
}
public class UIULongElement : UINumericElement<ulong>
{
	public override void Change(ulong amount) => Value += amount;
	public override ulong Clamp(ulong value, ulong min, ulong max) => Math.Clamp(value, min, max);
	public override ulong Round(ulong value, ulong nearest) => value - value % nearest;
	public override ulong FromInt(int value) => (ulong)value;
}

public class UIFloatElement : UINumericElement<float>
{
	public override void Change(float amount) => Value += amount;
	public override float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
	public override float Round(float value, float nearest) => value - value % nearest;
	public override float FromInt(int value) => (float)value;
}
public class UIDoubleElement : UINumericElement<double>
{
	public override void Change(double amount) => Value += amount;
	public override double Clamp(double value, double min, double max) => Math.Clamp(value, min, max);
	public override double Round(double value, double nearest) => value - value % nearest;
	public override double FromInt(int value) => (double)value;
}
public class UIDecimalElement : UINumericElement<decimal>
{
	public override void Change(decimal amount) => Value += amount;
	public override decimal Clamp(decimal value, decimal min, decimal max) => Math.Clamp(value, min, max);
	public override decimal Round(decimal value, decimal nearest) => value - value % nearest;
	public override decimal FromInt(int value) => (decimal)value;
}
#endregion