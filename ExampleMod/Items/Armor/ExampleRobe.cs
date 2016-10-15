using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items.Armor
{
    class ExampleRobe : ModItem
    {
        public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
        {
            equips.Add(EquipType.Body);
            return true;
        }

        public override void SetDefaults()
        {
            item.name = "Example Robe";
            item.width = 18;
            item.height = 14;
            item.rare = 1;
            item.vanity = true;
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            robes = true;
            // The equipSlot is added in ExampleMod.cs --> Load hook
            equipSlot = mod.GetEquipSlot("ExampleRobe_Legs");
        }
    }
}
