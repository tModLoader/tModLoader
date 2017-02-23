using System;
using Terraria;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// A struct which can hold various spawn information of an npc
	/// </summary>
	public struct NPCSpawnInfo
	{
		public int spawnTileX;
		//num
		public int spawnTileY;
		//num2
		public int spawnTileType;
		//num3
		public Player player;
		//Main.player[j]
		public int playerFloorX;
		//num25
		public int playerFloorY;
		//num26
		public bool sky;
		//flag
		public bool lihzahrd;
		//flag2
		public bool playerSafe;
		//flag3
		public bool invasion;
		//flag4
		public bool water;
		//flag5
		public bool granite;
		//flag7
		public bool marble;
		//flag8
		public bool spiderCave;
		//flag9
		public bool playerInTown;
		//flag10
		public bool desertCave;
		//flag11
		public bool planteraDefeated;
		//flag12
		public bool safeRangeX;
		//flag14
	}
}
