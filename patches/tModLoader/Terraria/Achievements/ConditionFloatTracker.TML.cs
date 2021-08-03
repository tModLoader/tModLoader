namespace Terraria.Achievements
{
	partial class ConditionFloatTracker
	{
		public override string GetProgressText() => $"{(int) _value} / {(int )_maxValue}"; // vanilla casts to an int, preserve that

		public override float GetProgress() => _value / _maxValue;
	}
}
