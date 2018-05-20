using System;

namespace Terraria.Utilities
{
	/// <summary>
	/// A random class with a luck modifier.
	/// </summary>
	public class LuckBasedRandom
	{
		/// <summary>
		/// Luck Percentage
		/// <para>The higher this value, the higher the chance of the outcome being true or an outcome with higher numbers.</para>
		/// <para>The default value is 100</para>
		/// </summary>
		public int Luck
		{
			get;
			set;
		}

		/// <summary>
		/// The underlying Random class instance.
		/// </summary>
		protected readonly Random Random;

		/// <summary>
		/// Initializes a new instance of the LuckyRandom class with a random seed.
		/// </summary>
		public LuckBasedRandom()
		{
			Random = new Random();
			Luck = 100;
		}

		/// <summary>
		/// Initializes a new instance of the LuckyRandom class with a specified seed.
		/// </summary>
		/// <param name="seed">Seed for the random number generator</param>
		public LuckBasedRandom(int seed)
		{
			Random = new Random(seed);
			Luck = 100;
		}

		/// <summary>
		/// Gets a true or false value by chance.
		/// </summary>
		/// <param name="chance">The chance of the result being true (100 would be 1 in 100)</param>
		/// <returns>A random value which can be either true or false.</returns>
		public bool NextBool(int chance)
		{
			if (Luck == 0)
				return false;
			// Calculate new chance based on luck modifier
			chance = (int)Math.Round((double)chance / (Math.Abs((double)Luck / 100)));

			// Chance can't be less than 1 or it will thow an exception
			if (chance < 1)
				chance = 1;
			
			// Return true if the random value is zero
			return Random.Next(chance) == 0;
		}

		/// <summary>
		/// Gets a true or false value by chance. (Inverse of NextBool)
		/// </summary>
		/// <param name="chance">The chance of the result being true (100 would be 1 in 100)</param>
		/// <returns>A random value which can be either true or false.</returns>
		public bool NextBoolInverted(int chance)
		{
			if (Luck == 0)
				return false;
			// Calculate new chance based on luck modifier
			chance = (int)Math.Round((double)chance / (Math.Abs((double)Luck / 100)));

			// Chance can't be less than 1 or it will thow an exception
			if (chance < 1)
				chance = 1;

			// Return true if the random value is zero
			return Random.Next(chance) != 0;
		}

		/// <summary>
		/// Gets a random integer between minValue and maxValue
		/// </summary>
		/// <param name="minValue">minimum value</param>
		/// <param name="maxValue">maximum value</param>
		/// <returns>A random integer between the specified minimum and maximum values</returns>
		public int NextInt(int minValue, int maxValue)
		{
			minValue = (int)Math.Round((double)minValue + ((double)minValue * 0.4) * (Math.Abs(Luck / 100)));
			maxValue = (int)Math.Round((double)maxValue + ((double)maxValue * 0.4) * (Math.Abs(Luck / 100)));

			if (minValue == maxValue)
				return minValue;

			return Random.Next(minValue, maxValue);
		}

		/// <summary>
		/// Gets a random integer between 0 and maxValue
		/// </summary>
		/// <param name="maxValue">maximum value</param>
		/// <returns>A random integer between 0 and the specified maximum value</returns>
		public int NextInt(int maxValue)
		{
			return NextInt(0, maxValue);
		}
	}
}
