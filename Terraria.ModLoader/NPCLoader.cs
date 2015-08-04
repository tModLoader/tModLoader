using System;
using System.Collections.Generic;
using Terraria;

namespace Terraria.ModLoader {
public static class NPCLoader
{
    internal static readonly IList<GlobalNPC> globalNPCs = new List<GlobalNPC>();

    internal static void Unload()
    {
        globalNPCs.Clear();
    }

    //in Terraria.NPC.NPCLoot after hardmode meteor head check add
    //  if(!NPCLoader.PreNPCLoot(this)) { return; }
    internal static bool PreNPCLoot(NPC npc)
    {
        foreach(GlobalNPC globalNPC in globalNPCs)
        {
            if(!globalNPC.PreNPCLoot(npc))
            {
                return false;
            }
        }
        return true;
    }

    //in Terraria.NPC.NPCLoot before heart and star drops add NPCLoader.NPCLoot(this);
    internal static void NPCLoot(NPC npc)
    {
        foreach(GlobalNPC globalNPC in globalNPCs)
        {
            globalNPC.NPCLoot(npc);
        }
    }
}}
