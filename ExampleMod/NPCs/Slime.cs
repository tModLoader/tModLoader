using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.IO;

namespace ExampleMod.NPCs
{
	class AquaSlime : ModNPC
	{
		public override string Texture { get { return "Terraria/NPC_" + NPCID.BlueSlime; } }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aqua Slime");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.BlueSlime];
		}
		
		public override void SetDefaults()
		{
			npc.CloneDefaults(NPCID.BlueSlime);
			npc.aiStyle = 1;
			npc.color = Color.Aqua;
			animationType = NPCID.BlueSlime;
		}
		// Demonstrates loading custom bools from ExampleMod.cs for use with NPC Spawning
		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return ExampleMod.NoBiomeNormalSpawn(spawnInfo) ? SpawnCondition.OverworldDaySlime.Chance * 0.5f : 0f;
		}

	}
}
