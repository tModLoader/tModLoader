using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;
using static Terraria.Cloud;
using static Terraria.WorldGen;

namespace ExampleMod
{
	class ExampleCloud
	{
		public static UnifiedRandom Rand = new UnifiedRandom();

		/// <summary>
		/// As the name implies, clears all the draw data from any currently active clouds.
		/// </summary>
		public static void Reset()
			=> resetClouds();

		/// <summary>
		/// This encapsulates all the data needed to draw the clouds.
		/// </summary>
		/// <returns>A DrawData object that can be modified or drawn immediately.</returns>
		public static DrawData CloudToDrawData(Cloud C)
			=> new DrawData(Main.cloudTexture[C.type], C.position, null, C.cloudColor(Main.bgColor), C.rotation, Vector2.Zero, C.scale, C.spriteDir, 1);

		/// <summary>
		/// A simple command to let you add a bunch of vanilla clouds.
		/// </summary>
		/// <param name="Amount"></param>
		public static void AddClouds(int Amount)
		{
			for(int Index = 0; Index < Amount; Index++)
			{
				addCloud();
			}
		}


		

		/////  BONUS INFORMATION	//////////
		///
		///
		///   .Clouds are stored like entities and other objects in terraria in an array.
		///   
		///   .The vanilla limit for clouds in terraria is 200.
		///   .They have bool fields that when marked false can have their value overwritten.
		///   .There is also a field bool for a flag of whether or not the cloud is going to be removed this update cycle.
		///   
		///   .These concepts are important if you go further in video game design as it's the basic system of iterating
		///      through limited objects to conserve resources.
		///
		///   .That being said, lets make our own clouds.
		///   .Wind Speed is like the cloud's velocity for an entity.

	}
}
