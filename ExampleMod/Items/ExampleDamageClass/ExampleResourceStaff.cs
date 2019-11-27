using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.ExampleDamageClass
{
    public class ExampleResourceStaff : ExampleDamageItem
    {
        // This is a staff that uses the example damage class stuff you've set up before, but uses exampleResource instead of mana.
        // This is a very simple way of doing it, and if you plan on multiple items using exampleResource then I'd suggest making a new abstract ModItem class that inherits ExampleDamageItem,
        // and doing the CanUseItem and UseItem in a more generalized way there, so you can just define the resource usage in SetDefaults and it'll do it automatically for you.
        public override void SafeSetDefaults()
        {
            item.CloneDefaults(ItemID.AmethystStaff);
            item.Size = new Vector2(28, 36);
            item.damage = 32;
            item.knockBack = 3;
            item.rare = 10;
            item.mana = 0; // Make sure to nullify the mana usage of the staff here, as it still copies the setdefaults of the amethyst staff.
            item.useStyle = ItemUseStyleID.HoldingOut;
        }
        // Make sure you can't use the item if you don't have enough resource and then use 10 resource otherwise.
        public override bool CanUseItem(Player player)
        {
            ExampleDamagePlayer modPlayer = ExampleDamagePlayer.ModPlayer(player);
            
            if (modPlayer.currentResource >= 10)
            {
                modPlayer.currentResource -= 10;
                return true;
            }
            return false;
        }
    }
}
