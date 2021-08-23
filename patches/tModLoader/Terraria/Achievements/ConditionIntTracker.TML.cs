namespace Terraria.Achievements
{
	partial class ConditionIntTracker
	{
		public override string GetProgressText() => $"{_value} / {_maxValue}";

		public override float GetProgress() => _value / _maxValue;
	}
}
