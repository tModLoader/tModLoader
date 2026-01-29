using System;

namespace UnifiedTests.Optimizations;

[TestFixture]
public static class UnifiedRandomTests
{
	// This is the version of UnifiedRandom used by vanilla Terraria.
	[Serializable]
	private sealed class UnifiedRandomVanilla
	{
		private const int MBIG = int.MaxValue;

		private const int MSEED = 161803398;

		private const int MZ = 0;

		private int inext;

		private int inextp;

		private int[] SeedArray = new int[56];

		public UnifiedRandomVanilla()
			: this(Environment.TickCount) { }

		public UnifiedRandomVanilla(int Seed)
		{
			SetSeed(Seed);
		}

		public void SetSeed(int Seed)
		{
			for (int i = 0; i < SeedArray.Length; i++) {
				SeedArray[i] = 0;
			}

			int num = ((Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed));
			int num2 = 161803398 - num;
			SeedArray[55] = num2;
			int num3 = 1;
			for (int j = 1; j < 55; j++) {
				int num4 = 21 * j % 55;
				SeedArray[num4] = num3;
				num3 = num2 - num3;
				if (num3 < 0) {
					num3 += int.MaxValue;
				}

				num2 = SeedArray[num4];
			}

			for (int k = 1; k < 5; k++) {
				for (int l = 1; l < 56; l++) {
					SeedArray[l] -= SeedArray[1 + (l + 30) % 55];
					if (SeedArray[l] < 0) {
						SeedArray[l] += int.MaxValue;
					}
				}
			}

			inext = 0;
			inextp = 21;
		}

		protected double Sample()
		{
			return (double)InternalSample() * 4.656612875245797E-10;
		}

		private int InternalSample()
		{
			int num = inext;
			int num2 = inextp;
			if (++num >= 56) {
				num = 1;
			}

			if (++num2 >= 56) {
				num2 = 1;
			}

			int stopwatch = SeedArray[num] - SeedArray[num2];
			if (stopwatch == int.MaxValue) {
				stopwatch--;
			}

			if (stopwatch < 0) {
				stopwatch += int.MaxValue;
			}

			SeedArray[num] = stopwatch;
			inext = num;
			inextp = num2;
			return stopwatch;
		}

		public int Peek()
		{
			return SeedArray[inext] - SeedArray[inextp];
		}

		public int Next()
		{
			return InternalSample();
		}

		private double GetSampleForLargeRange()
		{
			int num = InternalSample();
			if (InternalSample() % 2 == 0) {
				num = -num;
			}

			return ((double)num + 2147483646.0) / 4294967293.0;
		}

		public int Next(int minValue, int maxValue)
		{
			if (minValue > maxValue) {
				throw new ArgumentOutOfRangeException("minValue", "minValue must be less than maxValue");
			}

			long num = (long)maxValue - (long)minValue;
			if (num <= int.MaxValue) {
				return (int)(Sample() * (double)num) + minValue;
			}

			return (int)((long)(GetSampleForLargeRange() * (double)num) + minValue);
		}

		public int Next(int maxValue)
		{
			if (maxValue < 0) {
				throw new ArgumentOutOfRangeException("maxValue", "maxValue must be positive.");
			}

			return (int)(Sample() * (double)maxValue);
		}

		public double NextDouble()
		{
			return Sample();
		}

		public void NextBytes(byte[] buffer)
		{
			if (buffer == null) {
				throw new ArgumentNullException("buffer");
			}

			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (byte)(InternalSample() % 256);
			}
		}
	}

	// This is the version of UnifiedRandom refactored by CactusDuper.
	[Serializable]
	private sealed class UnifiedRandomFast
	{
		private const int MBIG = int.MaxValue;

		private const int MSEED = 161803398;

		private const int MZ = 0;

		private uint inext;

		private int[] SeedArray = new int[56];

		public UnifiedRandomFast()
			: this(Environment.TickCount) { }

		public UnifiedRandomFast(int Seed)
		{
			SetSeed(Seed);
		}

		public void SetSeed(int Seed)
		{
			for (int i = 0; i < SeedArray.Length; i++) {
				SeedArray[i] = 0;
			}

			int num = ((Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed));
			int num2 = 161803398 - num;
			SeedArray[55] = num2;
			int num3 = 1;
			for (int j = 1; j < 55; j++) {
				int num4 = 21 * j % 55;
				SeedArray[num4] = num3;
				num3 = num2 - num3;
				if (num3 < 0) {
					num3 += int.MaxValue;
				}

				num2 = SeedArray[num4];
			}

			for (int k = 1; k < 5; k++) {
				for (int l = 1; l < 56; l++) {
					SeedArray[l] -= SeedArray[1 + (l + 30) % 55];
					if (SeedArray[l] < 0) {
						SeedArray[l] += int.MaxValue;
					}
				}
			}

			inext = 0;
		}

		protected double Sample()
		{
			return (double)InternalSample() * 4.656612875245797E-10;
		}

		public int InternalSample()
		{
			var array = SeedArray;

			uint locINext = inext + 1;
			if (locINext > 55) {
				locINext = 1;
			}

			uint locINextp = locINext + 21;
			if (locINextp > 55) {
				locINextp -= 55;
			}

			int retVal = array[locINext] - array[locINextp];
			if (retVal == int.MaxValue) {
				retVal--;
			}

			retVal += (retVal >> 31) & int.MaxValue;

			array[locINext] = retVal;
			inext = locINext;
			return retVal;
		}

		public int Peek()
		{
			// NEW IN 1.4.5: return SeedArray[inext] - SeedArray[inextp];
			// Since inextp was removed, this we reuse logic from InternalSample:
			uint inextp = inext + 21;
			if (inextp > 55) {
				inextp -= 55;
			}

			return SeedArray[inext] - SeedArray[inextp];
		}

		public int Next()
		{
			return InternalSample();
		}

		private double GetSampleForLargeRange()
		{
			int num = InternalSample();
			if (InternalSample() % 2 == 0) {
				num = -num;
			}

			return ((double)num + 2147483646.0) / 4294967293.0;
		}

		public int Next(int minValue, int maxValue)
		{
			if (minValue > maxValue) {
				throw new ArgumentOutOfRangeException("minValue", "minValue must be less than maxValue");
			}

			long num = (long)maxValue - (long)minValue;
			if (num <= int.MaxValue) {
				return (int)(Sample() * (double)num) + minValue;
			}

			return (int)((long)(GetSampleForLargeRange() * (double)num) + minValue);
		}

		public int Next(int maxValue)
		{
			if (maxValue < 0) {
				throw new ArgumentOutOfRangeException("maxValue", "maxValue must be positive.");
			}

			return (int)(Sample() * (double)maxValue);
		}

		public double NextDouble()
		{
			return Sample();
		}

		public void NextBytes(byte[] buffer)
		{
			if (buffer == null) {
				throw new ArgumentNullException("buffer");
			}

			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = (byte)(InternalSample() % 256);
			}
		}
	}

	private const int num_instances = 100_000;
	private const int num_advances = 1_000;

	[Test]
	public static void EnsureNextIsTheSame()
	{
		DoTest(
			(v, f) => {
				int vNext = v.Next();
				int fNext = f.Next();
				Assert.That(vNext, Is.EqualTo(fNext));
			}
		);
	}

	[Test]
	public static void EnsureNextAndPeakAreTheSame()
	{
		DoTest(
			(v, f) => {
				int vNext = v.Next();
				int fNext = f.Next();
				Assert.That(vNext, Is.EqualTo(fNext));

				int vPeek = v.Peek();
				int fPeek = f.Peek();
				Assert.That(vPeek, Is.EqualTo(fPeek));
			}
		);
	}

	private static void DoTest(Action<UnifiedRandomVanilla, UnifiedRandomFast> callback)
	{
		DoTest(num_instances, num_advances, callback);
	}

	private static void DoTest(int instances, int advances, Action<UnifiedRandomVanilla, UnifiedRandomFast> callback)
	{
		for (int i = 0; i < instances; i++) {
			var vanilla = new UnifiedRandomVanilla(i);
			var fast = new UnifiedRandomFast(i);

			for (int j = 0; j < advances; j++) {
				callback(vanilla, fast);
			}
		}
	}
}