using System;

namespace ExampleMod
{
	//imported from my tAPI mod because it's useful
	public struct Angle
	{
		public float Value;

		public Angle(float angle)
		{
			Value = angle;
			float remainder = Value % (2f * (float)Math.PI);
			float rotations = Value - remainder;
			Value -= rotations;
			if (Value < 0f)
			{
				Value += 2f * (float)Math.PI;
			}
		}

		public static Angle operator +(Angle a1, Angle a2)
		{
			return new Angle(a1.Value + a2.Value);
		}

		public static Angle operator -(Angle a1, Angle a2)
		{
			return new Angle(a1.Value - a2.Value);
		}

		public Angle Opposite()
		{
			return new Angle(Value + (float)Math.PI);
		}

		public bool ClockwiseFrom(Angle other)
		{
			if (other.Value >= (float)Math.PI)
			{
				return this.Value < other.Value && this.Value >= other.Opposite().Value;
			}
			return this.Value < other.Value || this.Value >= other.Opposite().Value;
		}

		public bool Between(Angle cLimit, Angle ccLimit)
		{
			if (cLimit.Value < ccLimit.Value)
			{
				return this.Value >= cLimit.Value && this.Value <= ccLimit.Value;
			}
			return this.Value >= cLimit.Value || this.Value <= ccLimit.Value;
		}
	}
}