using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.ModLoader {
public class GlobalNPC
{
    public Mod mod
    {
        get;
        internal set;
    }

    //(temporary) in Terraria.NPC.NPCLoot after hardmode meteor head check add
    //  foreach(Mod mod in ModLoader.ModLoader.mods.Values)
    //  {
    //      if(mod.globalNPC != null && !mod.globalNPC.PreNPCLoot(this))
    //      {
    //          return;
    //      }
    //  }
    public virtual bool PreNPCLoot(NPC npc)
    {
        return true;
    }

    //(temporary) in Terraria.NPC.NPCLoot before heart and star drops add
    //  foreach(Mod mod in ModLoader.ModLoader.mods.Values)
    //  {
    //      if(mod.globalNPC != null)
    //      {
    //          mod.globalNPC.NPCLoot(this);
    //      }
    //  }
    public virtual void NPCLoot(NPC npc) { }
}}
