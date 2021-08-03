namespace Terraria.Achievements
{
	partial class ConditionFloatTracker
	{
		public override string GetProgressText() => $"{_value} / {_maxValue}";

		public override float GetProgress() => (int) _value / (int )_maxValue; // vanilla casts to an int, preserve that
	}
}
